using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using SimpleResults;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared.Logger;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.Services {
    public interface IAggregateRepositoryResolver {
        IAggregateRepository<TAggregate, TEvent> Resolve<TAggregate, TEvent>() where TAggregate : Aggregate where TEvent : DomainEvent;
    }

    public class AggregateRepositoryResolver : IAggregateRepositoryResolver {
        private readonly IServiceProvider _provider;

        public AggregateRepositoryResolver(IServiceProvider provider) {
            _provider = provider;
        }

        public IAggregateRepository<TAggregate, TEvent> Resolve<TAggregate, TEvent>() where TAggregate : Aggregate where TEvent : DomainEvent {
            Type repoType = typeof(IAggregateRepository<,>).MakeGenericType(typeof(TAggregate), typeof(TEvent));
            return (IAggregateRepository<TAggregate, TEvent>)_provider.GetRequiredService(repoType);
        }
    }

    public interface IAggregateService {
        Task<Result<TAggregate>> Get<TAggregate>(Guid aggregateId,
                                         CancellationToken cancellationToken) where TAggregate : Aggregate, new();

        Task<SimpleResults.Result<TAggregate>> GetLatest<TAggregate>(Guid aggregateId,
                                                                     CancellationToken cancellationToken) where TAggregate : Aggregate, new();

        Task<SimpleResults.Result<TAggregate>> GetLatestFromLastEvent<TAggregate>(Guid aggregateId,
                                                                                  CancellationToken cancellationToken) where TAggregate : Aggregate, new();

        Task<SimpleResults.Result<TAggregate>> GetLatestAggregateAsync<TAggregate>(Guid aggregateId,
                                                                                   Func<IAggregateRepository<TAggregate, DomainEvent>, Guid, CancellationToken, Task<SimpleResults.Result<TAggregate>>> getLatestVersionFunc,
                                                                                   CancellationToken cancellationToken) where TAggregate : Aggregate, new();

        Task<Result> Save<TAggregate>(TAggregate aggregate,
                                      CancellationToken cancellationToken) where TAggregate : Aggregate, new();
    }

    public class AggregateService : IAggregateService {
        private readonly IAggregateRepositoryResolver AggregateRepositoryResolver;
        private readonly AggregateMemoryCache Cache;
        private readonly List<(Type, MemoryCacheEntryOptions, Object)> AggregateTypes;

        public AggregateService(IAggregateRepositoryResolver aggregateRepositoryResolver,
                                IMemoryCache cache) {
            this.AggregateRepositoryResolver = aggregateRepositoryResolver;
            this.Cache = new AggregateMemoryCache(cache);

            //We update this list to contain MemoryCacheEntryOptions
            // TODO: We might make this configurable in the future
            this.AggregateTypes = new();

            // Set default caching options
            MemoryCacheEntryOptions memoryCacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .RegisterPostEvictionCallback(AggregateService.EvictionCallback);

            this.AggregateTypes.Add((typeof(EstateAggregate), memoryCacheEntryOptions, new Object()));
            this.AggregateTypes.Add((typeof(ContractAggregate), memoryCacheEntryOptions, new Object()));
            this.AggregateTypes.Add((typeof(OperatorAggregate), memoryCacheEntryOptions, new Object()));
        }

        internal static void EvictionCallback(Object key,
                                              Object value,
                                              EvictionReason reason,
                                              Object state)
        {
            Logger.LogWarning($"Key [{key}] of type [{value.GetType()}] removed from the cache {reason.ToString()}");
        }

        internal (Type, MemoryCacheEntryOptions, Object) GetAggregateType<TAggregate>() where TAggregate : Aggregate, new()
        {
            return this.AggregateTypes.SingleOrDefault(a => a.Item1 == typeof(TAggregate));
        }

        internal void SetCache<TAggregate>((Type, MemoryCacheEntryOptions, Object) aggregateType,
                                           Aggregate aggregate) where TAggregate : Aggregate, new()
        {
            //Changed the trace here. 
            //We have at least one scenario where something in aggregateType is null, and stopped us actually setting the cache!
            //This approach should be safer.
            if (aggregate == null)
            {
                Logger.LogWarning($"aggregate is null");
            }

            Logger.LogWarning($"About to save to cache.");

            String g = typeof(TAggregate).Name;
            String key = $"{g}-{aggregate.AggregateId}";

            this.Cache.Set<TAggregate>(key, aggregate, aggregateType.Item2);
        }

        public async Task<Result<TAggregate>> Get<TAggregate>(Guid aggregateId,
                                                              CancellationToken cancellationToken) where TAggregate : Aggregate, new()
        {
            Debug.WriteLine("In Get");
            (Type, MemoryCacheEntryOptions, Object) at = GetAggregateType<TAggregate>();
            TAggregate aggregate = default;
            String g = typeof(TAggregate).Name;
            String key = $"{g}-{aggregateId}";

            // Check the cache
            if (at != default && this.Cache.TryGetValue(key, out aggregate))
            {
                return Result.Success(aggregate);
            }

            if (at == default)
            {
                // We don't use caching for this aggregate so just hit GetLatest
                Result<TAggregate> getResult = await this.GetLatest<TAggregate>(aggregateId, cancellationToken);

                if (getResult.IsFailed) {
                    return getResult;
                }

                return Result.Success(getResult.Data);
            }

            try
            {
                // Lock
                Monitor.Enter(at.Item3);

                if (this.Cache.TryGetValueWithMetrics<TAggregate>(key, out TAggregate cachedAggregate))
                {
                    return Result.Success(cachedAggregate);
                }
                else
                {
                    // Not found in cache so call GetLatest
                    SimpleResults.Result<TAggregate> aggregateResult = this.GetLatest<TAggregate>(aggregateId, cancellationToken).Result;

                    if (aggregateResult.IsSuccess)
                    {
                        aggregate = aggregateResult.Data;
                        this.SetCache<TAggregate>(at, aggregateResult.Data);
                        return Result.Success(aggregate);
                    }
                    else
                    {
                        Logger.LogWarning($"aggregateResult failed {aggregateResult.Message}");
                        return aggregateResult;
                    }
                }
            }
            finally
            {
                // Release
                Monitor.Exit(at.Item3);
            }
        }

        public async Task<SimpleResults.Result<TAggregate>> GetLatest<TAggregate>(Guid aggregateId,
                                                                                              CancellationToken cancellationToken) where TAggregate : Aggregate, new() {
            return await this.GetLatestAggregateAsync<TAggregate>(aggregateId, (repo,
                                                                                id,
                                                                                cancellation) => repo.GetLatestVersion(id, cancellation), cancellationToken);
        }

        public async Task<SimpleResults.Result<TAggregate>> GetLatestFromLastEvent<TAggregate>(Guid aggregateId,
                                                                                               CancellationToken cancellationToken) where TAggregate : Aggregate, new() {
            return await this.GetLatestAggregateAsync<TAggregate>(aggregateId, (repo,
                                                                                id,
                                                                                cancellation) => repo.GetLatestVersionFromLastEvent(id, cancellation), cancellationToken);
        }

        public async Task<SimpleResults.Result<TAggregate>> GetLatestAggregateAsync<TAggregate>(Guid aggregateId,
                                                                                                Func<IAggregateRepository<TAggregate, DomainEvent>, Guid, CancellationToken, Task<SimpleResults.Result<TAggregate>>> getLatestVersionFunc,
                                                                                                CancellationToken cancellationToken) where TAggregate : Aggregate, new() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IAggregateRepository<TAggregate, DomainEvent> repository = this.AggregateRepositoryResolver.Resolve<TAggregate, DomainEvent>();

            String g = typeof(TAggregate).Name;
            String m = $"AggregateService";
            Counter counterCalls = AggregateService.GetCounterMetric($"{m}_{g}_times_rehydrated");
            Histogram histogramMetric = AggregateService.GetHistogramMetric($"{m}_{g}_rehydrated");

            counterCalls.Inc();
            TAggregate aggregate = null;
            try {
                var aggregateResult = await getLatestVersionFunc(repository, aggregateId, cancellationToken);
                if (aggregateResult.IsFailed)
                    return aggregateResult;
                aggregate = aggregateResult.Data;
            }
            catch (Exception ex) {
                return Result.Failure(ex.Message);
            }

            stopwatch.Stop();
            histogramMetric.Observe(stopwatch.Elapsed.TotalSeconds);

            return Result.Success(aggregate);
        }


        public async Task<Result> Save<TAggregate>(TAggregate aggregate,
                                                           CancellationToken cancellationToken) where TAggregate : Aggregate, new() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IAggregateRepository<TAggregate, DomainEvent> repository = this.AggregateRepositoryResolver.Resolve<TAggregate, DomainEvent>();

            String g = typeof(TAggregate).Name;
            String m = $"AggregateService";
            Counter counterCalls = AggregateService.GetCounterMetric($"{m}_{g}_times_saved");
            Histogram histogramMetric = AggregateService.GetHistogramMetric($"{m}_{g}_saved");

            counterCalls.Inc();

            // TODO: Check the pending events so dont save blindly, this would need a change to the base aggregate ?
            Result result = await repository.SaveChanges(aggregate, cancellationToken);

            stopwatch.Stop();

            histogramMetric.Observe(stopwatch.Elapsed.TotalSeconds);

            if (result.IsFailed)
            {
                // Get out before any caching
                return result;
            }
            
            (Type, MemoryCacheEntryOptions, Object) at = GetAggregateType<TAggregate>();

            if (at != default) {
                this.SetCache<TAggregate>(at, aggregate);
            }

            return result;
        }

        public static readonly ConcurrentDictionary<String, Counter> DynamicCounter = new();

        public static readonly ConcurrentDictionary<String, Histogram> DynamicHistogram = new();

        private static readonly Func<String, String, String> FormatMetricName = (methodName,
                                                                                 metricType) => $"{methodName}_{metricType}";

        public static Histogram GetHistogramMetric(String methodName)
        {
            String n = AggregateService.FormatMetricName(methodName, nameof(Histogram).ToLower());

            HistogramConfiguration histogramConfiguration = new()
            {
                Buckets = new[] { 1.0, 2.0, 5.0, 10.0, Double.PositiveInfinity }
            };

            var histogram = AggregateService.DynamicHistogram.GetOrAdd(methodName,
                name => Metrics.CreateHistogram(name: n,
                    help: $"Histogram of the execution time for {n}",
                    histogramConfiguration));

            return histogram;
        }

        public static Counter GetCounterMetric(String methodName)
        {
            String n = AggregateService.FormatMetricName(methodName, nameof(Counter).ToLower());

            var counter = AggregateService.DynamicCounter.GetOrAdd(methodName, name => Metrics.CreateCounter(name: n, help: $"Total number times executed {n}"));

            return counter;
        }
    }
}


public class AggregateMemoryCache
{
    private readonly IMemoryCache MemoryCache;

    private readonly ConcurrentDictionary<String, Aggregate> KeyTracker;

    public AggregateMemoryCache(IMemoryCache memoryCache)
    {
        this.MemoryCache = memoryCache;
        this.KeyTracker = new ConcurrentDictionary<String, Aggregate>();
    }

    private static readonly Dictionary<Type, Boolean> CallbackRegistered = new();

    private static readonly Object CallbackLock = new();

    public void Set<TAggregate>(String key,
                                Aggregate aggregate,
                                MemoryCacheEntryOptions memoryCacheEntryOptions) where TAggregate : Aggregate, new()
    {
        Type aggregateType = typeof(TAggregate);

        // Ensure the eviction callback is registered only once per TAggregate type
        if (!AggregateMemoryCache.CallbackRegistered.TryGetValue(aggregateType, out Boolean isRegistered) || !isRegistered)
        {
            Monitor.Enter(AggregateMemoryCache.CallbackLock);

            if (!AggregateMemoryCache.CallbackRegistered.TryGetValue(aggregateType, out isRegistered) || !isRegistered) // Double-check locking
            {
                // Register a callback to remove the item from our internal tracking
                memoryCacheEntryOptions.RegisterPostEvictionCallback((evictedKey,
                                                                      _, _, _) => {
                                                                          this.KeyTracker.TryRemove(evictedKey.ToString(), out _);
                                                                      });

                AggregateMemoryCache.CallbackRegistered[aggregateType] = true;
            }

            Monitor.Exit(AggregateMemoryCache.CallbackLock);
        }

        // Set the cache entry
        this.MemoryCache.Set(key, aggregate, memoryCacheEntryOptions);
        this.KeyTracker.TryAdd(key, aggregate);

        Counter counterCalls = AggregateService.GetCounterMetric($"AggregateService_{aggregateType.Name}_times_cache_saved");
        counterCalls.Inc();

        Counter counterItems = AggregateService.GetCounterMetric($"AggregateService_{aggregateType.Name}_total_cached_items");
        counterItems.IncTo(this.KeyTracker.Count);
    }

    public Boolean TryGetValueWithMetrics<TAggregate>(String key,
                                                      out TAggregate aggregate) where TAggregate : Aggregate, new()
    {
        String g = typeof(TAggregate).Name;

        var found = this.MemoryCache.TryGetValue(key, out aggregate);

        if (!found)
        {
            //TODO: Failed cache hit?
            Counter counterCalls = AggregateService.GetCounterMetric($"AggregateService_{g}_failed_cache_hit");
            counterCalls.Inc();
        }

        return found;
    }

    public Boolean TryGetValue<TAggregate>(String key,
                                           out TAggregate aggregate) where TAggregate : Aggregate, new()
    {
        return this.MemoryCache.TryGetValue(key, out aggregate);
    }
}