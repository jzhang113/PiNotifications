using System.Collections.Generic;

namespace PiNotifications.Models
{
    public interface INotificationRepository
    {
        IDictionary<string, EventFrameModel> GetActiveEvents();
        IEnumerable<AnalysisModel> GetAllEvents();
        IDictionary<string, EventFrameModel> GetActiveBackupGenEvents();
        IEnumerable<AnalysisModel> GetAllBackupGenEvents();
    }
}
