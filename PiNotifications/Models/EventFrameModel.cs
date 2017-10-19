using System;

namespace PiNotifications.Models
{
    public class EventFrameModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public dynamic Value { get; set; }
    }
}
