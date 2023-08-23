using Azure.Messaging.ServiceBus;
using Azure;
using System.Diagnostics;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using Newtonsoft.Json;
using TinyLink.Core.Commands.CommandMessages;
using TinyLink.Hits.TableStorage;

const string sourceQueueName = "hits";

static async Task Main()
{
    Console.WriteLine("Starting the hits processor job");

    var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
    var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
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

            Console.WriteLine("Created entity instance");
            var hitsRepository = new HitsRepository(storageAccountName);
            Console.WriteLine("Saving entity in table storage");
            await hitsRepository.CreateAsync(payload.OwnerId, payload.ShortCode, payload.CreatedOn);

            Console.WriteLine("Completing original message in service bus");
            await receiver.CompleteMessageAsync(receivedMessage);
            Console.WriteLine("All good, process complete");
        }
    }
}

await Main();