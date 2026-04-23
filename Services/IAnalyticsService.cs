using LampStoreProjects.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Services
{
    public interface IAnalyticsService
    {
        Task TrackVisitAsync(string sessionId, string ipAddress, string path, Guid? productId);
        Task<object> GetDashboardOverviewAsync();
        Task<object> GetSalesOverviewAsync();
    }
}
