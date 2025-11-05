using DataProcessor.Core.Models;

namespace DataProcessor.Core.Writers;

public interface IReportExcelWriter
{
    byte[] Write(ProcessedReportSummary summary);
}
