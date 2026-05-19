using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ПрилТехно.Repositories;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    // Определения классов-строк (можно вынести в отдельные файлы)
    public class OrdersReportRow { /* ... как ранее */ }
    public class BatchesReportRow { /* ... */ }
    public class DeviationsReportRow { /* ... */ }
    public class RecipeUsageReportRow { /* ... */ }
    public class ExtruderReportRow { /* ... */ }
    public class LabBlockedReportRow { /* ... */ }
    public class SystemEventsReportRow { /* ... */ }

    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportRepository _reportRepository;
        private readonly IDialogService _dialogService;

        private const string ReportOrders = "Отчёт по заказам";
        private const string ReportBatches = "Отчёт по партиям за период";
        private const string ReportDeviations = "Отчёт по отклонениям";
        private const string ReportRecipeUsage = "Отчёт по использованию рецептур";
        private const string ReportExtruder = "Отчёт по событиям экструдера";
        private const string ReportLabBlocks = "Отчёт по лабораторным блокировкам";
        private const string ReportSystemEvents = "Журнал событий системы";

        [ObservableProperty]
        private string? _selectedReport;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private IEnumerable<object>? _reportData;

        [ObservableProperty]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;
        public bool CanExport => ReportData != null && ReportData.Any();

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportCommand { get; }

        public ReportsViewModel(IReportRepository reportRepository, IDialogService dialogService)
        {
            _reportRepository = reportRepository;
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
            OnPropertyChanged(nameof(IsNotLoading));
            try
            {
                switch (SelectedReport)
                {
                    case ReportOrders:
                        ReportData = await Task.Run(() => _reportRepository.GetOrdersReport(StartDate, EndDate));
                        break;
                    case ReportBatches:
                        ReportData = await Task.Run(() => _reportRepository.GetBatchesReport(StartDate, EndDate));
                        break;
                    case ReportDeviations:
                        ReportData = await Task.Run(() => _reportRepository.GetDeviationsReport(StartDate, EndDate));
                        break;
                    case ReportRecipeUsage:
                        ReportData = await Task.Run(() => _reportRepository.GetRecipeUsageReport());
                        break;
                    case ReportExtruder:
                        ReportData = await Task.Run(() => _reportRepository.GetExtruderReport(StartDate, EndDate));
                        break;
                    case ReportLabBlocks:
                        ReportData = await Task.Run(() => _reportRepository.GetLabBlockedReport(StartDate, EndDate));
                        break;
                    case ReportSystemEvents:
                        ReportData = await Task.Run(() => _reportRepository.GetSystemEventsReport(StartDate, EndDate));
                        break;
                    default:
                        _dialogService.ShowMessage("Неизвестный тип отчёта");
                        ReportData = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}");
                ReportData = null;
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(CanExport));
            }
        }

        private async Task ExportAsync()
        {
            if (ReportData == null || !ReportData.Any())
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
                await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Отчёт");
                    var firstRow = ReportData.First();
                    var props = firstRow.GetType().GetProperties();

                    for (int i = 0; i < props.Length; i++)
                        worksheet.Cell(1, i + 1).Value = props[i].Name;

                    int row = 2;
                    foreach (var item in ReportData)
                    {
                        for (int i = 0; i < props.Length; i++)
                        {
                            var val = props[i].GetValue(item);
                            worksheet.Cell(row, i + 1).Value = val?.ToString() ?? "";
                        }
                        row++;
                    }
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(saveDialog.FileName);
                });
                _dialogService.ShowMessage($"Экспорт завершён: {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка экспорта: {ex.Message}");
            }
        }
    }
}