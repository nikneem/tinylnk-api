using TinyLink.Hits.TableStorage;

static async Task Main()
{
    Console.WriteLine("Starting the hits total accumulation job");

    var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    var hitsRepository = new HitsRepository(storageAccountName);
    var rawHitsData = await hitsRepository.GetAllAsync();


    Console.WriteLine("Accumulating hits per ten minutes");
    var accumulatePerTenMinutes = await hitsRepository.AccumulateRawHitsAsync(rawHitsData);
    Console.WriteLine($"Accumulating hits per ten minutes succeeded: {accumulatePerTenMinutes}");


    Console.WriteLine("Totaling hits in a grant total");
    var updateTotals = await hitsRepository.TotalRawHitsAsync(rawHitsData);
    Console.WriteLine($"Totaling hits in a grant total succeeded: {updateTotals}");

    Console.WriteLine("Moving raw data to long term persistence");
    var persistRawDataAsCopy = await hitsRepository.DuplicateRawDataAsProcessedAsync(rawHitsData);
    Console.WriteLine($"Moving raw data to long term persistence succeeded: {persistRawDataAsCopy}");

    Console.WriteLine("Removing processed raw data");
    var removeRawData = await hitsRepository.DeleteRawHitsAsync(rawHitsData);
    Console.WriteLine($"Removing processed raw data succeeded: {removeRawData}");
}

await Main();