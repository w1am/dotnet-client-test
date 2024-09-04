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

            var response = await client.AppendToStreamAsync(
                streamName,
                StreamState.Any,
                [
                    new EventData(
                        Uuid.NewUuid(),
                        eventType,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Message = "Hello, EventStore!" }))
                    ),
                    new EventData(
                        Uuid.NewUuid(),
                        eventType,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Message = "Hello, EventStore!" }))
                    )
                ]
            );
            Console.WriteLine($"Event appended successfully. {response.LogPosition}");

            var subscription = await client.SubscribeToStreamAsync(
                streamName,
                FromStream.Start,
                eventAppeared: async (subscription, evnt, cancellationToken) =>
                {
                    try
                    {
                        Console.WriteLine($"Received event: {evnt.OriginalEvent.EventType}");
                        await HandleEvent(evnt);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling event: {ex.Message}");
                    }
                },
                subscriptionDropped: SubscriptionDropped
            );

            Console.WriteLine("Subscription set up. Press any key to exit.");
            Console.ReadKey();

            subscription.Dispose();
        }

        private static void SubscriptionDropped(StreamSubscription subscription, SubscriptionDroppedReason reason,
            Exception? arg3)
        {
            Console.WriteLine($"Subscription dropped: {reason}");
        }

        static Task HandleEvent(ResolvedEvent evnt)
        {
            Console.WriteLine($"Handling event: {evnt.OriginalEvent.EventType}");
            return Task.CompletedTask;
        }
    }
}