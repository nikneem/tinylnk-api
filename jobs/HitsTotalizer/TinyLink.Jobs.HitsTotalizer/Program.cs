using Azure;
using System.Diagnostics;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using TinyLink.Jobs.HitsTotalizer.Entities;

const string storageTableName = "hits";
const string partitionKey = "hit";

static async Task Main()
{
    Console.WriteLine("Starting the hits total accumulation job");

    var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");

    var identity = new ManagedIdentityCredential();
    var storageAccountUrl = new Uri($"https://{storageAccountName}.table.core.windows.net");

    Console.WriteLine("Creating table client");
    var tableClient = new TableClient(storageAccountUrl, storageTableName, identity);

    var hitsQuery = tableClient.QueryAsync<ShortLinkHitEntity>($"{nameof(ShortLinkHitEntity.PartitionKey)} eq ${partitionKey}");

    var entities = new List<ShortLinkHitEntity>();
    await foreach (var queryPage in hitsQuery.AsPages().WithCancellation(CancellationToken.None))
    {
        entities.AddRange(queryPage.Values);
    }
    Console.WriteLine("Downloaded {entityCount} entities for accumulation", entities.Count);

    var withTimeStamp = entities.Where(ent => ent.Timestamp.HasValue).ToList()
    var dataMinDate = withTimeStamp.Min(ent => ent.Timestamp);
    if (!dataMinDate.HasValue)
    {
        Console.WriteLine("This can never happen but is to satisfy the compiler ;)");
        return;
    }

    var minDate = dataMinDate.Value;
    do
    {
        var maxDate = minDate.AddMinutes(10);
        var currentBatch = withTimeStamp.Where(ent => ent.Timestamp >= minDate && ent.Timestamp <= maxDate);

        var accumulatedEntities = currentBatch.GroupBy(ent => ent.ShortCode).Select(ent =>
            new AccumulationHitsTableEntity
            {
                ShortCode = ent.First().ShortCode,
                OwnerId = ent.First().OwnerId,
                CumulatedCount = ent.Count(),
                PartitionKey = "accumulation",
                RowKey = Guid.NewGuid().ToString()
            });

        var insertAccumulatedEntities = accumulatedEntities.Select(
            ent => new TableTransactionAction(TableTransactionActionType.Add, ent)
            );

        await tableClient.SubmitTransactionAsync(insertAccumulatedEntities);
        minDate = minDate.AddMinutes(10);
    } while (minDate < DateTimeOffset.UtcNow);



    var deleteTransactions = new List<TableTransactionAction>();
    foreach (var entity in entities)
    {
        deleteTransactions.Add( new TableTransactionAction( TableTransactionActionType.Delete, entity));
        await tableClient.SubmitTransactionAsync(deleteTransactions);
    }

    tableClient.SubmitTransactionAsync()

    Console.WriteLine("Receiving message from service bus");
    var receivedMessage = await receiver.ReceiveMessageAsync();

    if (receivedMessage != null)
    {
        Console.WriteLine("Got a message from the service bus");
        var payloadString = Encoding.UTF8.GetString(receivedMessage.Body);
        var payload = JsonConvert.DeserializeObject<ProcessHitCommand>(payloadString);
        if (payload != null)
        {
            Console.WriteLine("Processing hit command, persisting to storage");

            Activity.Current?.AddTag("ShortCode", payload.ShortCode);
            Activity.Current?.AddTag("CreatedOn", payload.CreatedOn.ToString());

            var voteEntity = new ShortLinkHitEntity
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                ShortCode = payload.ShortCode,
                Timestamp = payload.CreatedOn,
                ETag = ETag.All
            };

            Console.WriteLine("Created entity instance");
            var client = new TableClient(storageAccountUrl, storageTableName, identity);
            Console.WriteLine("Saving entity in table storage");
            await client.UpsertEntityAsync(voteEntity);

            Console.WriteLine("Completing original message in service bus");
            await receiver.CompleteMessageAsync(receivedMessage);
            Console.WriteLine("All good, process complete");
        }
    }
}

await Main();