using ClosedXML.Excel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp10.Helpers;

namespace WpfApp10.ViewModels
{
    public enum AnalyticsMode
    {
        Year,
        Month,
        Today
    }

    public class AnalyticsViewModel : BaseViewModel
    {
        private SeriesCollection _seriesCollection;
        private AnalyticsMode _currentMode;

        public SeriesCollection SeriesCollection
        {
            get { return _seriesCollection; }
            set { Set(ref _seriesCollection, value); }
        }

        public string[] Labels
        {
            get
            {
                return new[]
                {
                    "Задачи",
                    "Проекты",
                    "Документы"
                };
            }
        }

        public ICommand ShowYearCommand { get; private set; }
        public ICommand ShowMonthCommand { get; private set; }
        public ICommand ShowTodayCommand { get; private set; }
        public ICommand ExportExcelCommand { get; private set; }

        public AnalyticsViewModel()
        {
            ShowYearCommand = new RelayCommand(o => LoadYear());
            ShowMonthCommand = new RelayCommand(o => LoadMonth());
            ShowTodayCommand = new RelayCommand(o => LoadToday());
            ExportExcelCommand = new RelayCommand(o => ExportToExcel());

            LoadYear();
        }

        // ================= ГОД =================
        private void LoadYear()
        {
            _currentMode = AnalyticsMode.Year;

            SeriesCollection = new SeriesCollection
            {
                BuildSeries("Всего", Brushes.SteelBlue, 70, 50, 60),
                BuildSeries("Завершено", Brushes.SeaGreen, 45, 40, 50),
                BuildSeries("Провалено", Brushes.IndianRed, 25, 10, 10)
            };
        }

        // ================= МЕСЯЦ =================
        private void LoadMonth()
        {
            _currentMode = AnalyticsMode.Month;

            SeriesCollection = new SeriesCollection
            {
                BuildSeries("Всего", Brushes.SteelBlue, 50, 40, 45),
                BuildSeries("Завершено", Brushes.SeaGreen, 30, 25, 35),
                BuildSeries("Провалено", Brushes.IndianRed, 20, 15, 10)
            };
        }

        // ================= СЕГОДНЯ =================
        private void LoadToday()
        {
            _currentMode = AnalyticsMode.Today;

            SeriesCollection = new SeriesCollection
            {
                BuildSeries("Всего", Brushes.SteelBlue, 5, 4, 6),
                BuildSeries("Завершено", Brushes.SeaGreen, 2, 3, 1),
                BuildSeries("В процессе", Brushes.Orange, 3, 1, 5)
            };
        }

        // ================= ЭКСПОРТ В EXCEL =================
        private void ExportToExcel()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel (*.xlsx)|*.xlsx";
            dialog.FileName = "Аналитика.xlsx";

            if (dialog.ShowDialog() != true)
                return;

            XLWorkbook workbook = new XLWorkbook();

            string sheetName;
            if (_currentMode == AnalyticsMode.Year)
                sheetName = "Год";
            else if (_currentMode == AnalyticsMode.Month)
                sheetName = "Месяц";
            else
                sheetName = "Сегодня";

            IXLWorksheet ws = workbook.Worksheets.Add(sheetName);

            // Заголовки
            ws.Cell(1, 1).Value = "Тип";
            ws.Cell(1, 2).Value = "Всего";
            ws.Cell(1, 3).Value = SeriesCollection[1].Title;
            ws.Cell(1, 4).Value = SeriesCollection[2].Title;

            ws.Range(1, 1, 1, 4).Style.Font.Bold = true;

            // Данные
            for (int i = 0; i < Labels.Length; i++)
            {
                ws.Cell(i + 2, 1).Value = Labels[i];
                ws.Cell(i + 2, 2).Value = (int)SeriesCollection[0].Values[i];
                ws.Cell(i + 2, 3).Value = (int)SeriesCollection[1].Values[i];
                ws.Cell(i + 2, 4).Value = (int)SeriesCollection[2].Values[i];
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
        }

        // ================= ВСПОМОГАТЕЛЬНОЕ =================
        private ColumnSeries BuildSeries(
            string title,
            Brush color,
            int tasks,
            int projects,
            int documents)
        {
            ColumnSeries series = new ColumnSeries();
            series.Title = title;
            series.Values = new ChartValues<int>
            {
                tasks,
                projects,
                documents
            };
            series.Fill = color;
            series.DataLabels = true;

            return series;
        }
    }
}
