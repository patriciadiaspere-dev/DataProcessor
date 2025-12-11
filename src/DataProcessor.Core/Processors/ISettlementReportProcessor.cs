using DataProcessor.Core.Models.Settlement;

namespace DataProcessor.Core.Processors;

public interface ISettlementReportProcessor
{
    SettlementReportData Process(Stream stream);
}
