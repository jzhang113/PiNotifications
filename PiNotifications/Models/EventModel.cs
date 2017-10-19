using System;

namespace PiNotifications.Models
{
    public class EventModel
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public dynamic Value { get; set; }
    }
}
