using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Amqp.Framing;
using TinyLink.Hits.Abstractions.Repositories;
using TinyLink.Hits.TableStorage.Entities;
using TinyLink.ShortLinks.Abstractions.DataTransferObjects;
using TinyLink.ShortLinks.Abstractions.Exceptions;

namespace TinyLink.Hits.TableStorage;

public class HitsRepository : IHitsRepository
{

    private const string TotalHitsPartitionKey = "total";
    private const string ProcessedPartitionKey = "processed";
    private const string HitPartitionKey = "hit";
    private const string AccumulationPartitionKey = "accumulation";
    private const string TableName = "hits";
    private readonly TableClient _tableClient;

    public async Task<bool> CreateAsync(string ownerId, string shortCode, DateTimeOffset createdOn)
    {
        var voteEntity = new HitTableEntity
        {
            PartitionKey = HitPartitionKey,
            RowKey = Guid.NewGuid().ToString(),
            ShortCode = shortCode,
            OwnerId = ownerId,
            Hits = 1,
            Timestamp = createdOn,
            ETag = ETag.All
        };
        var response = await _tableClient.UpsertEntityAsync(voteEntity);
        return !response.IsError;
    }

    public async Task<List<HitTableEntity>> GetAllAsync()
    {
        var hitsQuery = _tableClient.QueryAsync<HitTableEntity>($"{nameof(HitTableEntity.PartitionKey)} eq ${HitPartitionKey}");

        var entities = new List<HitTableEntity>();
        await foreach (var queryPage in hitsQuery.AsPages().WithCancellation(CancellationToken.None))
        {
            entities.AddRange(queryPage.Values.Where(ent => ent.Timestamp.HasValue));
        }
        Console.WriteLine($"Downloaded {entities.Count} entities for accumulation");
        return entities;
    }

    public async Task<bool> AccumulateRawHitsAsync(List<HitTableEntity> rawHits)
    {
        bool allGood;
        var minDate = GetMinDateForBatch(rawHits);
        do
        {
            var maxDate = minDate.AddMinutes(10);
            var currentBatch = rawHits.Where(ent => ent.Timestamp >= minDate && ent.Timestamp <= maxDate);

            var accumulatedEntities = currentBatch.GroupBy(ent => ent.ShortCode).Select(ent =>
                new HitTableEntity
                {
                    PartitionKey = AccumulationPartitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    ShortCode = ent.First().ShortCode,
                    OwnerId = ent.First().OwnerId,
                    Hits = ent.Count(),
                    Timestamp = minDate
                });

            var insertAccumulatedEntities = accumulatedEntities.Select(
                ent => new TableTransactionAction(TableTransactionActionType.Add, ent)
            );

            var response = await _tableClient.SubmitTransactionAsync(insertAccumulatedEntities);
            allGood = response.HasValue && response.Value.All(x => !x.IsError);
            minDate = minDate.AddMinutes(10);
        } while (minDate < DateTimeOffset.UtcNow);

        return allGood;
    }

    public async Task<bool> TotalRawHitsAsync(List<HitTableEntity> rawHits)
    {
        var totalledHits = rawHits.GroupBy(ent => ent.ShortCode).Select(ent =>
            new
            {
                ShortCode = ent.First().ShortCode,
                OwnerId = ent.First().OwnerId,
                Hits = ent.Sum(x => x.Hits)
            });

        var transactionsList = new List<TableTransactionAction>();
        foreach (var totalHit in totalledHits)
        {
            var totalHitEntity = await GetSingleTotalHitEntity(totalHit.OwnerId, totalHit.ShortCode) ??
                                 CreateNewTotalHitsEntity(totalHit.OwnerId, totalHit.ShortCode);
            totalHitEntity.Hits += totalHit.Hits;
            transactionsList.Add(  new TableTransactionAction(TableTransactionActionType.UpsertReplace, totalHitEntity));
        }

        var result = await _tableClient.SubmitTransactionAsync(transactionsList);
        return result.HasValue && result.Value.All(r => !r.IsError);
    }

    public async Task<bool> DuplicateRawDataAsProcessedAsync(List<HitTableEntity> rawHits)
    {
        var processedHits = rawHits.Select(ent =>
            new HitTableEntity
            {
                PartitionKey = ProcessedPartitionKey,
                RowKey = ent.RowKey,
                ShortCode = ent.ShortCode,
                OwnerId = ent.OwnerId,
                Hits = ent.Hits,
                Timestamp = ent.Timestamp
            });

        var insertAccumulatedEntities = processedHits.Select(
            ent => new TableTransactionAction(TableTransactionActionType.Add, ent)
        );

        var response = await _tableClient.SubmitTransactionAsync(insertAccumulatedEntities);
        return response.HasValue && response.Value.All(x => !x.IsError);
    }

    public async Task<bool> DeleteRawHitsAsync(List<HitTableEntity> rawHits)
    {
        var insertAccumulatedEntities = rawHits.Select(
            ent => new TableTransactionAction(TableTransactionActionType.Delete, ent)
        );

        var response = await _tableClient.SubmitTransactionAsync(insertAccumulatedEntities);
        return response.HasValue && response.Value.All(x => !x.IsError);
    }

    private DateTimeOffset GetMinDateForBatch(List<HitTableEntity> batch)
    {
        var dataMinDate = batch.Min(ent => ent.Timestamp);
        return dataMinDate!.Value;
    }

    private async Task<HitTableEntity?> GetSingleTotalHitEntity(string ownerId, string shortCode, CancellationToken cancellationToken = default)
    {
            var pollsQuery = _tableClient.QueryAsync<HitTableEntity>($"{nameof(HitTableEntity.PartitionKey)} eq '{TotalHitsPartitionKey}' and {nameof(HitTableEntity.OwnerId)} eq '{ownerId}' and {nameof(HitTableEntity.ShortCode)} eq '{shortCode}'");
            await foreach (var queryPage in pollsQuery.AsPages().WithCancellation(cancellationToken))
            {
                return queryPage.Values.FirstOrDefault();
            }
            return null;
    }
    private HitTableEntity CreateNewTotalHitsEntity(string ownerId, string shortCode)
    {
        return new HitTableEntity
        {
            PartitionKey = TotalHitsPartitionKey,
            RowKey = Guid.NewGuid().ToString(),
            Hits = 0,
            OwnerId = ownerId,
            ShortCode = shortCode,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    public HitsRepository(string storageAccountName)
    {
        var identity = new ManagedIdentityCredential();
        var storageAccountUrl = new Uri($"https://{storageAccountName}.table.core.windows.net");
        _tableClient = new TableClient(storageAccountUrl, TableName, identity); ;
    }
}