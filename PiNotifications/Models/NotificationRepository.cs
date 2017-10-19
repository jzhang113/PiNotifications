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
        static string UrlFormat = "https://{0}/piwebapi/elements?path={1}";
        static string PiBatchRequest = $"https://{PiWebApiServer}/piwebapi/batch";
        static string Selector = "?selectedFields=Items.Name;Items.WebId";

        static dynamic notificationRoot;
        static List<AnalysisModel> EventList;

        private static Regex namePattern = new Regex(@".*(?= \d{4}-\d{2}-\d{2})"); // match event frame names up to the date

        static NotificationRepository()
        {
            string notificationUrl = string.Format(UrlFormat, PiWebApiServer, NotificationPath);
            notificationRoot = MakeRequest(notificationUrl);

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
                            new JProperty("Resource", "{0}" + Selector))));

            dynamic notificationData = MakePostRequest(request.ToString());

            // initializing EventList
            EventList = new List<AnalysisModel>();

            foreach (dynamic analysis in notificationData["Analyses"].Content.Items)
            {
                EventList.Add(new AnalysisModel
                {
                    Name = analysis.Name.Value,
                    Id = analysis.WebId.Value
                });
            }
        }
        
        // Get the current active event frames. An event frame is considered active when it has an end date of 9999
        public IDictionary<string, EventFrameModel> GetActiveEvents()
        {
            IDictionary<string, EventFrameModel> eventList = new Dictionary<string, EventFrameModel>();
            dynamic eventFrames = MakeRequest(notificationRoot.Links.EventFrames.Value + "?searchMode=BackwardInProgress&startTime=*-5s");

            foreach (dynamic ev in eventFrames.Items)
            {
                EventFrameModel item = new EventFrameModel()
                {
                    Name = ev.Name.Value,
                    Id = ev.Id.Value,
                    StartTime = ev.StartTime.Value.ToLocalTime(),
                    EndTime = ev.EndTime.Value.ToLocalTime()
                };

                dynamic val = MakeRequest(ev.Links.Value.Value);
                item.Value = (val.Items[0].Value.Good.Value) ? val.Items[0].Value.Value.Value : null;

                String name = namePattern.Match(ev.Name.Value).Value;
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

            return eventList;
        }

        public IEnumerable<AnalysisModel> GetAllEvents()
        {
            return EventList;
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