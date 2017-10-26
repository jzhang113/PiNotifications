using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiNotifications.Models
{
    public class NotificationRepository : INotificationRepository
    {
        static string PiWebApiServer = "pi-web-api.facilities.uiowa.edu";
        static string NotificationPath = "%5C%5Cpi-af.facilities.uiowa.edu%5CPIDB-AF%5CNotifications";
        static string BackupGenPath = "%5C%5Cpi-af.facilities.uiowa.edu%5CPIDB-AF%5CBackup Generators";

        static string UrlFormat = "https://{0}/piwebapi/elements?path={1}";
        static string PiBatchRequest = $"https://{PiWebApiServer}/piwebapi/batch";
        static string Selector = "?selectedFields=Items.Name;Items.WebId;";
        static string EventItemSelector = Selector + "Items.Links.Analyses;Items.Links.EventFrames;";

        static string NotificationEventFrames;
        static IDictionary<string, string> BackupGenEventFrames;
        static IDictionary<string, string> OakdaleEventFrames;
        static IDictionary<string, string> PiInterfaceEventFrames;
        static IDictionary<string, string> PPTagEventFrames;

        static ICollection<AnalysisModel> NotificationEventList;
        static ICollection<AnalysisModel> BackupGenEventList;
        static ICollection<AnalysisModel> OakdaleEventList;
        static ICollection<AnalysisModel> PiInterfaceEventList;
        static ICollection<AnalysisModel> PPTagEventList;

        private static Regex namePattern = new Regex(@".*(?= \d{4}-\d{2}-\d{2})"); // match event frame names up to the date

        static NotificationRepository()
        {
            string notificationUrl = string.Format(UrlFormat, PiWebApiServer, NotificationPath);
            string backupGenUrl = string.Format(UrlFormat, PiWebApiServer, BackupGenPath);

            JObject request =
                new JObject(
                    new JProperty("Notifications",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("Resource", notificationUrl))),
                    new JProperty("Analyses",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("Notifications"))),
                            new JProperty("Parameters",
                                new JArray(
                                    new JValue("$.Notifications.Content.Links.Analyses"))),
                            new JProperty("Resource", "{0}" + Selector))),
                    new JProperty("Subnotifications",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("Notifications"))),
                            new JProperty("Resource", "$.Notifications.Content.Links.Elements"))),
                    new JProperty("OakdaleElements",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("Subnotifications"))),
                            new JProperty("Parameters",
                                new JArray(
                                    new JValue("$.Subnotifications.Content.Items[0].Links.Elements"))),
                            new JProperty("Resource", "{0}" + EventItemSelector))),
                    new JProperty("PiInterfaceElements",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("Subnotifications"))),
                            new JProperty("Parameters",
                                new JArray(
                                    new JValue("$.Subnotifications.Content.Items[1].Links.Elements"))),
                            new JProperty("Resource", "{0}" + EventItemSelector))),
                    new JProperty("PPTagElements",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("Subnotifications"))),
                            new JProperty("Parameters",
                                new JArray(
                                    new JValue("$.Subnotifications.Content.Items[2].Links.Elements"))),
                            new JProperty("Resource", "{0}" + EventItemSelector))),
                    new JProperty("BackupGenRoot",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("Resource", backupGenUrl))),
                    new JProperty("BackupGenElements",
                        new JObject(
                            new JProperty("Method", "GET"),
                            new JProperty("ParentIds",
                                new JArray(
                                    new JValue("BackupGenRoot"))),
                            new JProperty("Parameters",
                                new JArray(
                                    new JValue("$.BackupGenRoot.Content.Links.Elements"))),
                            new JProperty("Resource", "{0}" + EventItemSelector))));

            dynamic notificationData = MakePostRequest(request.ToString());

            NotificationEventFrames = notificationData["Notifications"].Content.Links.EventFrames.Value;
            NotificationEventList = new List<AnalysisModel>();

            foreach (dynamic analysis in notificationData["Analyses"].Content.Items)
            {
                NotificationEventList.Add(new AnalysisModel
                {
                    Name = analysis.Name.Value,
                    Id = analysis.WebId.Value
                });
            }

            BackupGenEventFrames = FillEventFrames(notificationData, "BackupGenElements");
            BackupGenEventList = FillEventList(notificationData, "BackupGenElements");

            OakdaleEventFrames = FillEventFrames(notificationData, "OakdaleElements");
            OakdaleEventList = FillEventList(notificationData, "OakdaleElements");

            PiInterfaceEventFrames = FillEventFrames(notificationData, "PiInterfaceElements");
            PiInterfaceEventList = FillEventList(notificationData, "PiInterfaceElements");

            PPTagEventFrames = FillEventFrames(notificationData, "PPTagElements");
            PPTagEventList = FillEventList(notificationData, "PPTagElements");
        }

        private static IDictionary<string, string> FillEventFrames(dynamic notificationData, string itemName)
        {
            IDictionary<string, string> eventFrames = new Dictionary<string, string>();

            foreach (dynamic elements in notificationData[itemName].Content.Items)
            {
                eventFrames.Add(elements.Name.Value, elements.Links.EventFrames.Value);
            }

            return eventFrames;
        }

        private static IEnumerable<AnalysisModel> FillEventList(dynamic notificationData, string itemName)
        {
            ICollection<AnalysisModel> eventList = new List<AnalysisModel>();

            foreach (dynamic item in notificationData[itemName].Content.Items)
            {
                string elementName = item.Name.Value;
                dynamic obj = MakeRequest(item.Links.Analyses.Value + Selector + "Items.AnalysisRulePlugInName");

                foreach (dynamic analysis in obj.Items)
                {
                    if (analysis.AnalysisRulePlugInName.Value.Equals("EventFrame"))
                    {
                        eventList.Add(new AnalysisModel
                        {
                            Name = elementName + " " + analysis.Name.Value,
                            Id = analysis.WebId.Value
                        });
                    }
                }
            }

            return eventList;
        }
        
        // Get the current active event frames. An event frame is considered active when it has an end date of 9999
        public IDictionary<string, EventFrameModel> GetActiveEvents()
        {
            IDictionary<string, EventFrameModel> eventList = new Dictionary<string, EventFrameModel>();
            dynamic eventFrames = MakeRequest(NotificationEventFrames + "?searchMode=BackwardInProgress&startTime=*-5s");
            AddToEventList(eventFrames, eventList);

            return eventList;
        }

        public IDictionary<string, EventFrameModel> GetActiveBackupGenEvents()
        {
            return ProcessActiveEvents(BackupGenEventFrames);
        }

        public IDictionary<string, EventFrameModel> GetActiveOakdaleEvents()
        {
            return ProcessActiveEvents(OakdaleEventFrames);
        }

        public IDictionary<string, EventFrameModel> GetActivePiInterfaceEvents()
        {
            return ProcessActiveEvents(PiInterfaceEventFrames);
        }

        public IDictionary<string, EventFrameModel> GetActivePPTagEvents()
        {
            return ProcessActiveEvents(PPTagEventFrames);
        }

        private static IDictionary<string, EventFrameModel> ProcessActiveEvents(IDictionary<string, string> elementEventFrames)
        {
            IDictionary<string, EventFrameModel> eventList = new Dictionary<string, EventFrameModel>();

            foreach (KeyValuePair<string, string> entry in elementEventFrames)
            {
                dynamic eventFrames = MakeRequest(entry.Value + "?searchMode=BackwardInProgress&startTime=*-5s");
                AddToEventList(eventFrames, eventList, entry.Key);
            }

            return eventList;
        }

        private static void AddToEventList(dynamic eventData, IDictionary<string, EventFrameModel> eventList, string namePrefix = "")
        {
            foreach (dynamic ev in eventData.Items)
            {
                EventFrameModel item = new EventFrameModel()
                {
                    Name = ev.Name.Value,
                    Id = ev.Id.Value,
                    StartTime = ev.StartTime.Value.ToLocalTime(),
                    EndTime = ev.EndTime.Value.ToLocalTime()
                };

                dynamic val = MakeRequest(ev.Links.Value.Value);
                try
                {
                    item.Value = (val.Items[0].Value.Good.Value) ? val.Items[0].Value.Value.Value : val.Items[0].Value.Value.Name.Value;
                }
                catch
                {
                    item.Value = "Error";
                }

                String name = namePattern.Match(ev.Name.Value).Value;
                if (!namePrefix.Equals(""))
                    name = namePrefix + " " + name;

                EventFrameModel prev;
                if (eventList.TryGetValue(name, out prev))
                {
                    // keep the earlier event frame
                    if (item.StartTime < prev.StartTime)
                    {
                        eventList[name] = item;
                    }
                }
                else
                {
                    eventList.Add(name, item);
                }
            }
        }

        public IEnumerable<AnalysisModel> GetAllEvents()
        {
            return NotificationEventList;
        }

        public IEnumerable<AnalysisModel> GetAllBackupGenEvents()
        {
            return BackupGenEventList;
        }

        public IEnumerable<AnalysisModel> GetAllOakdaleEvents()
        {
            return OakdaleEventList;
        }

        public IEnumerable<AnalysisModel> GetAllPiInterfaceEvents()
        {
            return PiInterfaceEventList;
        }

        public IEnumerable<AnalysisModel> GetAllPPTagEvents()
        {
            return PPTagEventList;
        }

        // Return a dynamic object representing the JSON data from the given post request
        internal static dynamic MakePostRequest(string data)
        {
            string json = MakePostAsync(PiBatchRequest, data).Result;
            dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
            return results;
        }

        // Return a dynamic object representing the JSON data from the given URL
        internal static dynamic MakeRequest(string url)
        {
            string json = MakeRequestAsync(url).Result; //returns results of MakeRequestAsync
            dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
            return results;
        }

        // Issue a post request and return JSON
        internal static async Task<string> MakePostAsync(string url, string json)
        {
            //Create Request
            WebRequest request = WebRequest.Create(url);

            //Format Request
            string authInfo = @"IOWA\fm-pictoapi:H1LGvgGnJs!N";
            authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
            request.Headers["Authorization"] = $"Basic {authInfo}";
            request.Method = "POST";
            request.ContentType = "application/json";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            //Return response
            WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        //Return JSON from requested URL
        internal static async Task<string> MakeRequestAsync(string url)
        {
            //Create Request
            WebRequest request = WebRequest.Create(url);

            //Format Request
            string authInfo = @"IOWA\fm-pictoapi:H1LGvgGnJs!N";
            authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
            request.Headers["Authorization"] = $"Basic {authInfo}";

            //Return response
            WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}