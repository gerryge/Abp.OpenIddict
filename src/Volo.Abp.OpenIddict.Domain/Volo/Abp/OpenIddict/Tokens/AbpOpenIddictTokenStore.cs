﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Volo.Abp.OpenIddict.Tokens;

/// <inheritdoc/>
public class AbpOpenIddictTokenStore : OpenIddictTokenStoreBase
{
    protected IOpenIddictTokenRepository TokenRepository { get; }

    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    protected AbpOpenIddictCleanupOptions Options { get; }

    public AbpOpenIddictTokenStore(
        IGuidGenerator guidGenerator,
        IOpenIddictTokenRepository tokenRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpOpenIddictCleanupOptions> options) : base(guidGenerator)
    {
        TokenRepository = tokenRepository;
        UnitOfWorkManager = unitOfWorkManager;
        Options = options.Value;
    }

    /// <inheritdoc/>
    public override async ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        return await TokenRepository.LongCountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<long> CountAsync<TResult>(
        Func<IQueryable<OpenIddictToken>, IQueryable<TResult>> query,
        CancellationToken cancellationToken)
    {
        Check.NotNull(query, nameof(query));

        var queryable = await TokenRepository.GetQueryableAsync();
        return await TokenRepository.AsyncExecuter
            .LongCountAsync(query(queryable), cancellationToken);
    }


    /// <inheritdoc/>
    //[UnitOfWork]
    public override async ValueTask CreateAsync(OpenIddictToken token, CancellationToken cancellationToken)
    {
        Check.NotNull(token, nameof(token));

        using var unitOfWork = UnitOfWorkManager.Begin();

        await TokenRepository.InsertAsync(token, true, cancellationToken);

        await unitOfWork.CompleteAsync();
    }

    /// <inheritdoc/>
    //[UnitOfWork]
    public override async ValueTask UpdateAsync(OpenIddictToken token, CancellationToken cancellationToken)
    {
        Check.NotNull(token, nameof(token));

        using var unitOfWork = UnitOfWorkManager.Begin();

        await TokenRepository.UpdateAsync(token, true, cancellationToken);

        await unitOfWork.CompleteAsync();
    }

    /// <inheritdoc/>
    //[UnitOfWork]
    public override async ValueTask DeleteAsync(OpenIddictToken token, CancellationToken cancellationToken)
    {
        Check.NotNull(token, nameof(token));

        using var unitOfWork = UnitOfWorkManager.Begin();

        await TokenRepository.DeleteAsync(token, true, cancellationToken);

        await unitOfWork.CompleteAsync();
    }

    /// <inheritdoc/>
    //[UnitOfWork]
    public override async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        List<Exception> exceptions = null;

        using var unitOfWork = UnitOfWorkManager.Begin();

        for (var index = 0; index < Options.CleanupLoopCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var date = threshold.UtcDateTime;
            var tokens = await TokenRepository.GetPruneListAsync(date, Options.CleanupBatchSize, cancellationToken);

            if (tokens.Count == 0)
            {
                break;
            }

            try
            {
                await TokenRepository.DeleteManyAsync(tokens, true, cancellationToken);
            }
            catch (Exception exception)
            {
                exceptions ??= new List<Exception>(capacity: 1);
                exceptions.Add(exception);
            }
        }

        if (exceptions is not null)
        {
            throw new AggregateException(SR.GetResourceString(SR.ID0249), exceptions);
        }

        await unitOfWork.CompleteAsync();
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindAsync(
        string subject,
        string client,
        CancellationToken cancellationToken)
    {
        if (subject.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (client.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        var key = ConvertIdentifierFromString(client);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindAsync(subject, key, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindAsync(
        string subject,
        string client,
        string status,
        CancellationToken cancellationToken)
    {
        if (subject.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (client.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        if (status.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
        }

        var key = ConvertIdentifierFromString(client);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindAsync(subject, key, status, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindAsync(
        string subject,
        string client,
        string status,
        string type,
        CancellationToken cancellationToken)
    {
        if (subject.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        if (client.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(client));
        }

        if (status.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
        }

        if (type.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0200), nameof(type));
        }

        var key = ConvertIdentifierFromString(client);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindAsync(subject, key, status, type, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindByApplicationIdAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        if (identifier.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindByApplicationIdAsync(key, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindByAuthorizationIdAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        if (identifier.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindByAuthorizationIdAsync(key, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OpenIddictToken> FindByIdAsync(
        string identifier, CancellationToken cancellationToken)
    {
        if (identifier.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        var key = ConvertIdentifierFromString(identifier);

        return await TokenRepository.FindAsync(key, false, cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<OpenIddictToken> FindByReferenceIdAsync(
        string identifier, CancellationToken cancellationToken)
    {
        if (identifier.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0195), nameof(identifier));
        }

        return await TokenRepository.FindByReferenceIdAsync(identifier, cancellationToken);
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> FindBySubjectAsync(
        string subject,
        CancellationToken cancellationToken)
    {
        if (subject.IsNullOrWhiteSpace())
        {
            throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(subject));
        }

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository.FindBySubjectAsync(subject, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<TResult> GetAsync<TState, TResult>(
        Func<IQueryable<OpenIddictToken>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
    {
        Check.NotNull(query, nameof(query));

        var queryable = await TokenRepository.GetQueryableAsync();
        return await TokenRepository.AsyncExecuter
            .FirstOrDefaultAsync(query(queryable, state), cancellationToken);
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<OpenIddictToken> ListAsync(
        int? count,
        int? offset,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<OpenIddictToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var authorizations = (await TokenRepository
                .GetListAsync(count, offset, cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var authorization in authorizations)
            {
                yield return authorization;
            }
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<OpenIddictToken>, TState, IQueryable<TResult>> query,
        TState state,
        CancellationToken cancellationToken)
    {
        Check.NotNull(query, nameof(query));

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TResult> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var queryable = await TokenRepository.GetQueryableAsync();

            var results = (await TokenRepository.AsyncExecuter
                .ToListAsync(query(queryable, state), cancellationToken))
                .AsAsyncEnumerable();

            await foreach (var result in results)
            {
                yield return result;
            }
        }
    }
}