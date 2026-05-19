using System.Data;
using ПрилТехно.Models;

namespace ПрилТехно.Repositories
{
    public interface IReportRepository
    {
        List<OrdersReportRow> GetOrdersReport(DateTime start, DateTime end);
        List<BatchesReportRow> GetBatchesReport(DateTime start, DateTime end);
        List<DeviationsReportRow> GetDeviationsReport(DateTime start, DateTime end);
        List<RecipeUsageReportRow> GetRecipeUsageReport();
        List<ExtruderReportRow> GetExtruderReport(DateTime start, DateTime end);
        List<LabBlockedReportRow> GetLabBlockedReport(DateTime start, DateTime end);
        List<SystemEventsReportRow> GetSystemEventsReport(DateTime start, DateTime end);
    }
}