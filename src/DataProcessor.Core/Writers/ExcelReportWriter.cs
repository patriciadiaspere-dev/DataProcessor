using System.Linq;
using ClosedXML.Excel;
using DataProcessor.Core.Models;

namespace DataProcessor.Core.Writers;

public class ExcelReportWriter : IReportExcelWriter
{
    public byte[] Write(ProcessedReportSummary summary)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Resumo");

        worksheet.Cell(1, 1).Value = "Resumo";
        worksheet.Cell(2, 1).Value = "Produtos Únicos";
        worksheet.Cell(2, 2).Value = summary.TotalUniqueProducts;
        worksheet.Cell(3, 1).Value = "Unidades Vendidas";
        worksheet.Cell(3, 2).Value = summary.TotalUnitsSold;
        worksheet.Cell(4, 1).Value = "Faturamento Total";
        worksheet.Cell(4, 2).Value = summary.TotalRevenue;
        worksheet.Cell(4, 2).Style.NumberFormat.Format = "R$ #,##0.00";

        var detailSheet = workbook.Worksheets.Add("Itens");
        detailSheet.Cell(1, 1).Value = "ASIN";
        detailSheet.Cell(1, 2).Value = "Produto";
        detailSheet.Cell(1, 3).Value = "Preço Unitário";
        detailSheet.Cell(1, 4).Value = "Quantidade";
        detailSheet.Cell(1, 5).Value = "Valor Total";

        for (var i = 0; i < summary.Items.Count; i++)
        {
            var item = summary.Items.ElementAt(i);
            var row = i + 2;
            detailSheet.Cell(row, 1).Value = item.Asin;
            detailSheet.Cell(row, 2).Value = item.ProductName;
            detailSheet.Cell(row, 3).Value = item.UnitPrice;
            detailSheet.Cell(row, 3).Style.NumberFormat.Format = "R$ #,##0.00";
            detailSheet.Cell(row, 4).Value = item.TotalQuantity;
            detailSheet.Cell(row, 5).Value = item.TotalValue;
            detailSheet.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";
        }

        detailSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
