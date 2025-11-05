using DataProcessor.Core.Models;

namespace DataProcessor.Core.Processors;

public interface IReportProcessor
{
    ProcessedReportSummary Process(Stream stream);
}
