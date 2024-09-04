using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace Asynchronous
{
    internal abstract class Program
    {
        private static async Task Main(string[] args)
        {
            var settings =
                EventStoreClientSettings.Create("esdb://admin:changeit@localhost:2113?tls=true&tlsVerifyCert=false");
            settings.OperationOptions.ThrowOnAppendFailure = true;
            var client = new EventStoreClient(settings);

            var streamName = "example-stream";
            var eventType = "sample-event";

            var eventData = new EventData(
                Uuid.NewUuid(),
                eventType,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Message = "Hello, EventStore!" }))
            );

            await client.AppendToStreamAsync(
                streamName,
                StreamState.Any,
                [eventData]
            );
            Console.WriteLine("Event appended successfully.");
        }
    }
}