using DataProcessor.Core.Models.Settlement;
using DataProcessor.Data.DTOs.Settlement;
using DataProcessor.Data.Entities;

namespace DataProcessor.Data.Services;

public interface ISettlementReportService
{
    Task<SettlementUpload> SaveAsync(User user, string accountType, string fileName, SettlementReportData data, CancellationToken cancellationToken = default);
    Task<SettlementReconciliationResponse> BuildReconciliationAsync(User user, string accountType, string salesFileName, Stream salesStream, CancellationToken cancellationToken = default);
}
using System.IO;
using System.Threading;
using System.Threading.Tasks;
