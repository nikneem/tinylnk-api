using Azure.Messaging.ServiceBus;
using Azure;
using System.Diagnostics;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using Newtonsoft.Json;
using TinyLink.Core.Commands.CommandMessages;
using TinyLink.Jobs.HitsProcessor.Entities;

const string sourceQueueName = "hits";

const string storageTableName = "hits";
const string partitionKey = "hit";

static async Task Main()
{
    Console.WriteLine("Starting the hits processor job");

    var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
    var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");

    var identity = new ManagedIdentityCredential();
    var storageAccountUrl = new Uri($"https://{storageAccountName}.table.core.windows.net");

    var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
    var receiver = serviceBusClient.CreateReceiver(sourceQueueName);

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