using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ПрогЛабор.Models;
namespace ПрогЛабор.Services
{
    public class ProtocolGenerator : IProtocolGenerator
    {
        private readonly ILabRepository _labRepository;

        public ProtocolGenerator(ILabRepository labRepository)
        {
            _labRepository = labRepository;
        }

        public async Task<string> GenerateRawMaterialTestProtocolAsync(int testId, string outputPath)
        {
            var test = await _labRepository.GetRawMaterialTestByIdAsync(testId);
            if (test == null) throw new Exception("Испытание не найдено");

            var results = await _labRepository.GetRawMaterialTestResultsAsync(testId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Протокол испытаний");

            // Заголовок
            worksheet.Cell(1, 1).Value = "ПРОТОКОЛ ИСПЫТАНИЙ №";
            worksheet.Cell(1, 2).Value = test.TestNumber;
            worksheet.Cell(2, 1).Value = "Дата проведения:";
            worksheet.Cell(2, 2).Value = test.CreatedDate.ToString("dd.MM.yyyy");
            worksheet.Cell(3, 1).Value = "Тип испытания:";
            worksheet.Cell(3, 2).Value = test.TestType;
            worksheet.Cell(4, 1).Value = "Исполнитель:";
            worksheet.Cell(4, 2).Value = test.AssignedToName ?? "не указан";

            // Таблица параметров
            int startRow = 6;
            worksheet.Cell(startRow, 1).Value = "№";
            worksheet.Cell(startRow, 2).Value = "Параметр";
            worksheet.Cell(startRow, 3).Value = "Норматив";
            worksheet.Cell(startRow, 4).Value = "Ед. изм.";
            worksheet.Cell(startRow, 5).Value = "Фактическое значение";
            worksheet.Cell(startRow, 6).Value = "Результат";
            worksheet.Cell(startRow, 7).Value = "Комментарий";

            var headerRange = worksheet.Range(startRow, 1, startRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = startRow + 1;
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = r.ParameterName;
                // Формируем норматив
                string standard = "";
                if (r.StandardValueMin.HasValue && r.StandardValueMax.HasValue)
                    standard = $"{r.StandardValueMin} – {r.StandardValueMax}";
                else if (!string.IsNullOrEmpty(r.StandardText))
                    standard = r.StandardText;
                worksheet.Cell(row, 3).Value = standard;
                worksheet.Cell(row, 4).Value = r.Unit;
                worksheet.Cell(row, 5).Value = r.MeasuredValue?.ToString() ?? "—";
                worksheet.Cell(row, 6).Value = r.Result == "pass" ? "соответствует" : (r.Result == "fail" ? "не соответствует" : "не испытан");
                worksheet.Cell(row, 7).Value = r.AnalystComment ?? "";
                row++;
            }

            // Подписи
            int footerRow = row + 2;
            worksheet.Cell(footerRow, 1).Value = "Лаборант:";
            worksheet.Cell(footerRow, 2).Value = "__________________";
            worksheet.Cell(footerRow + 1, 1).Value = "Дата:";
            worksheet.Cell(footerRow + 1, 2).Value = DateTime.Now.ToString("dd.MM.yyyy");

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(outputPath);
            return outputPath;
        }

        public async Task<string> GenerateQualityTestProtocolAsync(int testId, string outputPath)
        {
            // Получаем данные об испытании
            var test = await _labRepository.GetQualityTestByIdAsync(testId);
            if (test == null)
                throw new Exception("Испытание не найдено");

            // Получаем результаты испытания
            var results = await _labRepository.GetQualityTestResultsAsync(testId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Протокол испытаний");

            // Заголовок
            worksheet.Cell(1, 1).Value = "ПРОТОКОЛ ИСПЫТАНИЙ №";
            worksheet.Cell(1, 2).Value = test.TestNumber;
            worksheet.Cell(2, 1).Value = "Дата проведения:";
            worksheet.Cell(2, 2).Value = test.CreatedDate.ToString("dd.MM.yyyy");
            worksheet.Cell(3, 1).Value = "Тип испытания:";
            worksheet.Cell(3, 2).Value = test.TestType;
            worksheet.Cell(4, 1).Value = "Исполнитель:";
            worksheet.Cell(4, 2).Value = test.AssignedToName ?? "не указан";

            // Таблица параметров
            int startRow = 6;
            worksheet.Cell(startRow, 1).Value = "№";
            worksheet.Cell(startRow, 2).Value = "Параметр";
            worksheet.Cell(startRow, 3).Value = "Норматив";
            worksheet.Cell(startRow, 4).Value = "Ед. изм.";
            worksheet.Cell(startRow, 5).Value = "Фактическое значение";
            worksheet.Cell(startRow, 6).Value = "Результат";
            worksheet.Cell(startRow, 7).Value = "Комментарий";

            var headerRange = worksheet.Range(startRow, 1, startRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = startRow + 1;
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = r.ParameterName;

                // Формируем норматив
                string standard = "";
                if (r.StandardValueMin.HasValue && r.StandardValueMax.HasValue)
                    standard = $"{r.StandardValueMin} – {r.StandardValueMax}";
                else if (!string.IsNullOrEmpty(r.StandardText))
                    standard = r.StandardText;

                worksheet.Cell(row, 3).Value = standard;
                worksheet.Cell(row, 4).Value = r.Unit;
                worksheet.Cell(row, 5).Value = r.MeasuredValue?.ToString() ?? "—";
                worksheet.Cell(row, 6).Value = r.Result == "pass" ? "соответствует" : (r.Result == "fail" ? "не соответствует" : "не испытан");
                worksheet.Cell(row, 7).Value = r.AnalystComment ?? "";
                row++;
            }

            // Подписи
            int footerRow = row + 2;
            worksheet.Cell(footerRow, 1).Value = "Лаборант:";
            worksheet.Cell(footerRow, 2).Value = "__________________";
            worksheet.Cell(footerRow + 1, 1).Value = "Дата:";
            worksheet.Cell(footerRow + 1, 2).Value = DateTime.Now.ToString("dd.MM.yyyy");

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(outputPath);

            return outputPath;  // возвращаем путь к сохранённому файлу
        }
    }
}