using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PiNotifications.Models;

namespace PiNotifications.Controllers
{
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private INotificationRepository _repository;

        public EventsController(INotificationRepository repository)
        {
            _repository = repository;
        }

        [Produces("application/json")]
        [HttpGet]
        public IEnumerable<EventModel> Get()
        {
            IEnumerable<AnalysisModel> events = _repository.GetAllEvents();
            IDictionary<string, EventFrameModel> active = _repository.GetActiveEvents();
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
                } else
                {
                    result.Add(new EventModel
                    {
                        Name = am.Name,
                        Active = false
                    });
                }
            }
            return result;
        }
    }
}
