using System.Collections.Generic;

namespace PiNotifications.Models
{
    public interface INotificationRepository
    {
        IEnumerable<AnalysisModel> GetAllEvents();
        IEnumerable<AnalysisModel> GetAllBackupGenEvents();
        IEnumerable<AnalysisModel> GetAllOakdaleEvents();
        IEnumerable<AnalysisModel> GetAllPiInterfaceEvents();
        IEnumerable<AnalysisModel> GetAllPPTagEvents();

        IDictionary<string, EventFrameModel> GetActiveEvents();
        IDictionary<string, EventFrameModel> GetActiveBackupGenEvents();
        IDictionary<string, EventFrameModel> GetActiveOakdaleEvents();
        IDictionary<string, EventFrameModel> GetActivePiInterfaceEvents();
        IDictionary<string, EventFrameModel> GetActivePPTagEvents();
    }
}
