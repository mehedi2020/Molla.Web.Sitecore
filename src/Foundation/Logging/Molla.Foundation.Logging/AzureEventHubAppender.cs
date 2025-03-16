using Azure.Messaging.EventHubs.Producer;
using log4net.Appender;
using log4net.spi;
using System.Text;
using System;
using System.Threading.Tasks;
using EventData = Azure.Messaging.EventHubs.EventData;
using Sitecore.Configuration;
using Nito.AsyncEx.Synchronous;
using System.Web.Http;

namespace Molla.Foundation.Logging
{
    public class AzureEventHubAppender : AppenderSkeleton
    {
        private readonly Lazy<EventHubProducerClient> _client = new(() =>
            new EventHubProducerClient(Settings.GetConnectionString("AzureEventHubConnectionString")),
            System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        protected override void Append(LoggingEvent loggingEvent)
        {
             try
             {
                 Sitecore.Diagnostics.Log.Debug("****EVENT HUB START*****", this);
                 var task = Task.Run(async () => await _client.Value.CreateBatchAsync());

                 Sitecore.Diagnostics.Log.Debug("****EVENT HUB START Task *****", this);
                 using (EventDataBatch eventBatch = task.WaitAndUnwrapException())
                 {
                     Sitecore.Diagnostics.Log.Debug("****EVENT HUB START Task eventBatch *****", this);
                     //create all the properties required for the format for Event Hubs 
                     var eventData = new EventData(Encoding.UTF8.GetBytes($"{Constants.APPLICATION_NAME} : {loggingEvent.MessageObject}"));
                     eventData.Properties.Add("EventType", $"{loggingEvent.Level}");
                     eventData.Properties.Add("Timestamp", $"{loggingEvent.TimeStamp}");
                     eventData.Properties.Add("Message", $"{loggingEvent.MessageObject}");
                     eventData.Properties.Add("RenderedMessage", $"{loggingEvent.RenderedMessage}");
                     eventData.Properties.Add("Properties", $"{loggingEvent.Properties}");

                     Sitecore.Diagnostics.Log.Info(
                         $"****EVENT HUB {nameof(loggingEvent.RenderedMessage)}: {loggingEvent.RenderedMessage}", this);

                     if (!eventBatch.TryAdd(eventData))
                     {
                         Sitecore.Diagnostics.Log.Error("Event is too large for the batch and cannot be sent.", this);
                     }

                     // Use the producer client to send the batch of events to the event hub
                     var sendTask = Task.Run(async () => await _client.Value.SendAsync(eventBatch));
                     sendTask.WaitAndUnwrapException(new System.Threading.CancellationToken(false));
                 }
                 Sitecore.Diagnostics.Log.Debug("****End EVENT HUB IN PROGRESS *****", this);
             }
             catch (Exception ex)
             {
                 Sitecore.Diagnostics.Log.Error($"Error sending message to Azure Event Hub: {ex.Message}", ex, this);
                 throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
             }

             Sitecore.Diagnostics.Log.Debug($"****EVENT HUB END*****", this);
        }
    }
}
