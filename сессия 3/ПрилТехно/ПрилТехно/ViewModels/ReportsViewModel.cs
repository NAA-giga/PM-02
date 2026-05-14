using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        // Константы для типов отчётов (избегаем ошибок строк)
        private const string ReportOrders = "Отчёт по заказам";
        private const string ReportBatches = "Отчёт по партиям за период";
        private const string ReportDeviations = "Отчёт по отклонениям";
        private const string ReportRecipeUsage = "Отчёт по использованию рецептур";
        private const string ReportExtruder = "Отчёт по событиям экструдера";
        private const string ReportLabBlocks = "Отчёт по лабораторным блокировкам";
        private const string ReportSystemEvents = "Журнал событий системы";

        [ObservableProperty]
        private string? _selectedReport = ReportOrders;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private DataTable? _reportData;

        [ObservableProperty]
        private bool _isLoading;

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportCommand { get; }

        public ReportsViewModel(ApiClient apiClient, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);
        }

        private async Task GenerateReportAsync()
        {
            if (string.IsNullOrEmpty(SelectedReport))
            {
                _dialogService.ShowMessage("Выберите отчёт");
                return;
            }

            IsLoading = true;
            try
            {
                switch (SelectedReport)
                {
                    case ReportOrders:
                        await LoadOrdersReportAsync();
                        break;
                    case ReportBatches:
                        await LoadBatchReportAsync();
                        break;
                    case ReportDeviations:
                        await LoadDeviationReportAsync();
                        break;
                    case ReportRecipeUsage:
                        await LoadRecipeUsageReportAsync();
                        break;
                    case ReportExtruder:
                        await LoadExtruderReportAsync();
                        break;
                    case ReportLabBlocks:
                        await LoadLabBlockReportAsync();
                        break;
                    case ReportSystemEvents:
                        await LoadSystemEventsReportAsync();
                        break;
                    default:
                        _dialogService.ShowMessage("Неизвестный тип отчёта");
                        break;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Загрузка данных из API

        /// <summary>Отчёт по заказам (производственным заказам)</summary>
        private async Task LoadOrdersReportAsync()
        {
            var response = await _apiClient.GetAsync<List<ProductionOrderDto>>("/api/productionorders");
            if (response?.IsSuccess != true || response.Data == null)
            {
                _dialogService.ShowMessage("Не удалось загрузить заказы");
                return;
            }

            var filtered = response.Data
                .Where(o => o.PlannedStartDate.Date >= StartDate.Date && o.PlannedStartDate.Date <= EndDate.Date)
                .OrderBy(o => o.PlannedStartDate)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Номер заказа", typeof(string));
            table.Columns.Add("Продукт", typeof(string));
            table.Columns.Add("План, кг", typeof(decimal));
            table.Columns.Add("Статус", typeof(string));
            table.Columns.Add("Плановая дата", typeof(DateTime));
            table.Columns.Add("Факт. начало", typeof(DateTime?));
            table.Columns.Add("Факт. конец", typeof(DateTime?));

            foreach (var order in filtered)
            {
                table.Rows.Add(order.OrderNumber, order.ProductName, order.PlannedQuantityKg,
                               order.Status, order.PlannedStartDate, order.ActualStartDate, order.ActualEndDate);
            }
            ReportData = table;
        }

        /// <summary>Отчёт по производственным партиям за период</summary>
        private async Task LoadBatchReportAsync()
        {
            var response = await _apiClient.GetAsync<List<ProductionBatchDto>>("/api/productionbatches");
            if (response?.IsSuccess != true || response.Data == null)
            {
                _dialogService.ShowMessage("Не удалось загрузить партии");
                return;
            }

            var filtered = response.Data
                .Where(b => b.StartTime.HasValue && b.StartTime.Value.Date >= StartDate.Date && b.StartTime.Value.Date <= EndDate.Date)
                .OrderBy(b => b.StartTime)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Номер партии", typeof(string));
            table.Columns.Add("Продукт", typeof(string));
            table.Columns.Add("Дата запуска", typeof(DateTime?));
            table.Columns.Add("Статус", typeof(string));
            table.Columns.Add("План, кг", typeof(decimal));
            table.Columns.Add("Факт, кг", typeof(decimal?));
            table.Columns.Add("Лаб. решение", typeof(string));

            foreach (var b in filtered)
            {
                table.Rows.Add(b.BatchNumber, b.ProductName, b.StartTime, b.Status,
                               b.PlannedQuantityKg, b.ActualQuantityKg, b.LabDecision);
            }
            ReportData = table;
        }

        /// <summary>Отчёт по отклонениям</summary>
        private async Task LoadDeviationReportAsync()
        {
            var response = await _apiClient.GetAsync<List<DeviationDto>>("/api/deviations");
            if (response?.IsSuccess != true || response.Data == null)
            {
                _dialogService.ShowMessage("Не удалось загрузить отклонения");
                return;
            }

            var filtered = response.Data
                .Where(d => d.CreatedAt.Date >= StartDate.Date && d.CreatedAt.Date <= EndDate.Date)
                .OrderBy(d => d.CreatedAt)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Партия", typeof(string));
            table.Columns.Add("Шаг", typeof(string));
            table.Columns.Add("Параметр", typeof(string));
            table.Columns.Add("План", typeof(string));
            table.Columns.Add("Факт", typeof(string));
            table.Columns.Add("Тип", typeof(string));
            table.Columns.Add("Серьёзность", typeof(string));
            table.Columns.Add("Дата", typeof(DateTime));

            foreach (var d in filtered)
            {
                table.Rows.Add(d.BatchNumber, d.StepName, d.ParameterName, d.PlannedValue,
                               d.ActualValue, d.DeviationType, d.Severity, d.CreatedAt);
            }
            ReportData = table;
        }

        /// <summary>Отчёт по использованию рецептур</summary>
        private async Task LoadRecipeUsageReportAsync()
        {
            var recipesTask = _apiClient.GetAsync<List<RecipeDto>>("/api/recipes");
            var batchesTask = _apiClient.GetAsync<List<ProductionBatchDto>>("/api/productionbatches");
            await Task.WhenAll(recipesTask, batchesTask);

            var recipes = recipesTask.Result?.Data ?? new List<RecipeDto>();
            var batches = batchesTask.Result?.Data ?? new List<ProductionBatchDto>();

            var usage = batches
                .Where(b => b.Status == "completed" || b.Status == "quality_control")
                .GroupBy(b => b.RecipeId)
                .Select(g => new
                {
                    RecipeId = g.Key,
                    Recipe = recipes.FirstOrDefault(r => r.Id == g.Key),
                    BatchCount = g.Count(),
                    TotalQuantity = g.Sum(b => b.ActualQuantityKg ?? 0)
                })
                .Where(x => x.Recipe != null)
                .OrderBy(x => x.Recipe.ProductName)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Продукт", typeof(string));
            table.Columns.Add("Рецептура", typeof(string));
            table.Columns.Add("Версия", typeof(int));
            table.Columns.Add("Кол-во партий", typeof(int));
            table.Columns.Add("Объём, кг", typeof(decimal));

            foreach (var item in usage)
            {
                table.Rows.Add(item.Recipe.ProductName, item.Recipe.Name, item.Recipe.Version,
                               item.BatchCount, item.TotalQuantity);
            }
            ReportData = table;
        }

        /// <summary>Отчёт по событиям экструдера (телеметрия)</summary>
        private async Task LoadExtruderReportAsync()
        {
            var batchesResp = await _apiClient.GetAsync<List<ProductionBatchDto>>("/api/productionbatches");
            if (batchesResp?.IsSuccess != true || batchesResp.Data == null) return;

            var batches = batchesResp.Data
                .Where(b => b.StartTime.HasValue && b.StartTime.Value.Date >= StartDate.Date && b.StartTime.Value.Date <= EndDate.Date)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Партия", typeof(string));
            table.Columns.Add("Зона", typeof(int));
            table.Columns.Add("Температура,°C", typeof(decimal?));
            table.Columns.Add("Давление, бар", typeof(decimal?));
            table.Columns.Add("Скорость, об/мин", typeof(int?));
            table.Columns.Add("Время записи", typeof(DateTime));

            foreach (var batch in batches)
            {
                var telemetryResp = await _apiClient.GetAsync<List<ExtruderTelemetryDto>>($"/api/extrudertelemetry?batchId={batch.Id}");
                if (telemetryResp?.IsSuccess == true && telemetryResp.Data != null)
                {
                    foreach (var t in telemetryResp.Data)
                    {
                        table.Rows.Add(batch.BatchNumber, t.ZoneNumber, t.TemperatureC,
                                       t.PressureBar, t.ScrewSpeedRpm, t.RecordedAt);
                    }
                }
            }
            ReportData = table;
        }

        /// <summary>Отчёт по лабораторным блокировкам</summary>
        private async Task LoadLabBlockReportAsync()
        {
            var response = await _apiClient.GetAsync<List<ProductionBatchDto>>("/api/productionbatches");
            if (response?.IsSuccess != true || response.Data == null) return;

            var blocked = response.Data
                .Where(b => b.LabDecision == "blocked" && b.LabDecisionDate.HasValue &&
                            b.LabDecisionDate.Value.Date >= StartDate.Date && b.LabDecisionDate.Value.Date <= EndDate.Date)
                .OrderBy(b => b.LabDecisionDate)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Партия", typeof(string));
            table.Columns.Add("Продукт", typeof(string));
            table.Columns.Add("Дата блокировки", typeof(DateTime?));
            table.Columns.Add("Причина", typeof(string));
            table.Columns.Add("Ответственный", typeof(string));

            foreach (var b in blocked)
            {
                table.Rows.Add(b.BatchNumber, b.ProductName, b.LabDecisionDate,
                               b.LabDecisionReason, b.LabDecisionBy);
            }
            ReportData = table;
        }

        /// <summary>Журнал событий системы (например, уведомления)</summary>
        private async Task LoadSystemEventsReportAsync()
        {
            var response = await _apiClient.GetAsync<List<EventDto>>("/api/events/unread"); // или /api/events?from=...&to=...
            if (response?.IsSuccess != true || response.Data == null)
            {
                _dialogService.ShowMessage("Не удалось загрузить события");
                return;
            }

            var filtered = response.Data
                .Where(e => e.CreatedAt.Date >= StartDate.Date && e.CreatedAt.Date <= EndDate.Date)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            var table = new DataTable();
            table.Columns.Add("Тип", typeof(string));
            table.Columns.Add("Источник", typeof(string));
            table.Columns.Add("Сообщение", typeof(string));
            table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Пользователь", typeof(string));

            foreach (var e in filtered)
            {
                table.Rows.Add(e.EventType, e.SourceType, e.Message, e.CreatedAt, e.UserName);
            }
            ReportData = table;
        }

        #endregion

        #region Экспорт в Excel

        private async Task ExportAsync()
        {
            if (ReportData == null || ReportData.Rows.Count == 0)
            {
                _dialogService.ShowMessage("Нет данных для экспорта");
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"{SelectedReport}_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            if (saveDialog.ShowDialog() != true) return;

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Отчёт");
                worksheet.Cell(1, 1).InsertTable(ReportData);
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveDialog.FileName);
                _dialogService.ShowMessage($"Отчёт сохранён: {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка экспорта: {ex.Message}");
            }
        }

        #endregion
    }
}