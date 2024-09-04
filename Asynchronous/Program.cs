using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace Testing;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        // Connection settings
        var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
        var client = new EventStoreClient(settings);

        var streamName = "example-stream";
        var eventType = "sample-event";

        // Create an event to append
        var eventData = new EventData(
            Uuid.NewUuid(),
            eventType,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Message = "Hello, EventStore!" }))
        );

        // Append the event to the stream
        await client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
        Console.WriteLine("Event appended successfully.");


        await using var subscription = client.SubscribeToStream(
            streamName,
            FromStream.Start);
        await foreach (var message in subscription.Messages)
        {
            switch (message)
            {
                case StreamMessage.Event(var evnt):
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}");
                    break;
                case StreamMessage.CaughtUp _:
                    Console.WriteLine("Caught up to the stream.");
                    break;
            }
        }
    }
}