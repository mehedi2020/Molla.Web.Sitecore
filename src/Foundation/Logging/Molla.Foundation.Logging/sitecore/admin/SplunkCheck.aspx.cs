using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using log4net.helpers;
using log4net.spi;
using Nito.AsyncEx.Synchronous;
using Sitecore.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Molla.Foundation.Logging
{
    public partial class SplunkCheck : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


        } 

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string AzureEventHubConnectionString = Settings.GetConnectionString("AzureEventHubConnectionString");
            EventHubProducerClient producerClient = new EventHubProducerClient(AzureEventHubConnectionString);
           
            try
            {
                LoggingEventData loggingEventData = new LoggingEventData { Domain="Test Message", ExceptionString="No Exception", Identity= Convert.ToBase64String(Guid.NewGuid().ToByteArray()),  LoggerName = "DW Splunk Page", Message="This is Test message from DW Splunk Message.", Level= new Level(10,"System"), LocationInfo= new LocationInfo("Splunk.aspx.cs"),  ThreadName="Manually Pushed", TimeStamp= DateTime.Now, UserName="Manual", NDC="NDC"};
                 loggingEventData.Properties = new log4net.helpers.PropertiesCollection(); 
                LoggingEvent loggingEvent = new LoggingEvent(loggingEventData); 
                Sitecore.Diagnostics.Log.Info($"****EVENT HUB START*****", this);
                if (producerClient != null)
                {
                    var task = Task.Run(async () => await producerClient.CreateBatchAsync());
                    Sitecore.Diagnostics.Log.Info($"****EVENT HUB START Task *****", this);

                    using EventDataBatch eventBatch = task.WaitAndUnwrapException();

                    Sitecore.Diagnostics.Log.Info($"****EVENT HUB START Task eventBatch *****", this); 
                    

                    var eventDataType = Constants.APPLICATION_NAME;


                    //create all the properties required for the format for Event Hubs 
                    var eventData = new EventData(Encoding.UTF8.GetBytes($"{eventDataType} : {loggingEvent.MessageObject}")); 
                    eventData.Properties.Add("EventType", $"{loggingEvent.LoggerName}");
                    eventData.Properties.Add("Timestamp", $"{loggingEvent.TimeStamp}");
                    eventData.Properties.Add("Message", $"{loggingEvent.MessageObject}");
                    eventData.Properties.Add("RenderedMessage", $"{loggingEvent.RenderedMessage}");
                    eventData.Properties.Add("Properties", $"{loggingEvent.Properties.ToString()}");
                    eventData.Properties.Add("Identity", $"{loggingEvent.Identity}");
                    Sitecore.Diagnostics.Log.Info($"****EVENT HUB IN PROGRESS *****", this);

                    Response.Write("EventType:"+ loggingEvent.LoggerName+ "<br />");
                    Response.Write("Timestamp:" + loggingEvent.TimeStamp + "<br />");
                    Response.Write("RenderedMessage:" + loggingEvent.RenderedMessage + "<br />");
                    Response.Write("MessageObject:" + loggingEvent.MessageObject + "<br />");
                    Response.Write("Identity:" + loggingEvent.Identity + "<br />") ;
                    Response.Write("Properties:" + loggingEvent.Properties + "<br />");

                    if (!eventBatch.TryAdd(eventData))
                    {    // if it is too large for the batch
                        Sitecore.Diagnostics.Log.Error($"Event  is too large for the batch and cannot be sent.", this);
                        Response.Write("Event  is too large for the batch and cannot be sent.");
                    }


                    // Use the producer client to send the batch of events to the event hub
                    // 
                    var sendTask = producerClient.SendAsync(eventBatch);  
                    sendTask.WaitAndUnwrapException();
                    Sitecore.Diagnostics.Log.Info($"****End EVENT HUB IN PROGRESS *****", this);

                    Response.Write("Message Submitted. Identity:<b>"+ loggingEventData.Identity+ "</b>"); 
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error($"****EVENT HUB EXCEPTION*****", this);
                Sitecore.Diagnostics.Log.Error($"Issues while sending Messages to Azure Event Hubs : {ex.Message}", this);
                Sitecore.Diagnostics.Log.Error($"****EVENT HUB StackTrace*****" + ex.StackTrace.ToString(), this);
                Response.Write("Exception <br />."+ ex.Message + "<br />" +ex.StackTrace);
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);

            }

            Sitecore.Diagnostics.Log.Info($"****EVENT HUB END*****", this);
        } 
    }
         
}