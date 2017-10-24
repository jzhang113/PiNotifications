using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PiNotifications.Models;

namespace PiNotifications.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsController : Controller
    {
        private INotificationRepository _repository;

        public EventsController(INotificationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<EventModel> Get()
        {
            IEnumerable<AnalysisModel> events = _repository.GetAllEvents();
            IDictionary<string, EventFrameModel> active = _repository.GetActiveEvents();
            return ProcessEvents(events, active);
        }

        [Route("backup")]
        [HttpGet]
        public IEnumerable<EventModel> GetBackupGen()
        {
            IEnumerable<AnalysisModel> events = _repository.GetAllBackupGenEvents();
            IDictionary<string, EventFrameModel> active = _repository.GetActiveBackupGenEvents();
            return ProcessEvents(events, active);
        }

        private static ICollection<EventModel> ProcessEvents(IEnumerable<AnalysisModel> events, IDictionary<string, EventFrameModel> active)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            ICollection<EventModel> result = new List<EventModel>();

            foreach (AnalysisModel am in events)
            {
                EventFrameModel ev;
                if (active.TryGetValue(am.Name, out ev))
                {
                    result.Add(new EventModel
                    {
                        Name = am.Name,
                        Active = true,
                        StartTime = ev.StartTime,
                        EndTime = ev.EndTime,
                        Value = ev.Value
                    });
                }
                else
                {
                    result.Add(new EventModel
                    {
                        Name = am.Name,
                        Active = false
                    });
                }
            }

            timer.Stop();
            System.Console.WriteLine(timer.Elapsed);
            return result;
        }
    }
}
