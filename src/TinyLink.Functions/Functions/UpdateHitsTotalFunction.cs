using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TinyLink.Core.Commands.CommandMessages;
using TinyLink.Core.Helpers;
using TinyLink.ShortLinks.TableStorage;
using TinyLink.ShortLinks.TableStorage.Entities;

namespace TinyLink.Functions.Functions
{
    public class UpdateHitsTotalFunction
    {
        private readonly ILogger<UpdateHitsTotalFunction> _logger;

        public UpdateHitsTotalFunction(ILogger<UpdateHitsTotalFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(UpdateHitsTotalFunction))]
        public async Task Run([ServiceBusTrigger("hitscalculationscompleted")] TotalHitsChangedCommand message)
        {

            var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            var identity = CloudIdentity.GetChainedTokenCredential();
            var storageAccountUrl = new Uri($"https://{storageAccountName}.table.core.windows.net");
            var tableClient = new TableClient(storageAccountUrl, ShortLinksRepository.TableName, identity);


            var entityQuery = await tableClient.GetEntityAsync<ShortLinkTableEntity>(message.OwnerId, message.Id.ToString());
            var entity = entityQuery.Value;
            var updatedEntity = entity with { Hits = message.TotalHits };
            await tableClient.UpdateEntityAsync(updatedEntity, ETag.All, TableUpdateMode.Replace);


        }
    }
}
