using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Linq;

namespace WpfApp10.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // ===== СЧЁТЧИКИ =====
        public int ProjectsCount { get; private set; }
        public int TasksCount { get; private set; }
        public int DocumentsCount { get; private set; }

        // ===== ДИАГРАММЫ =====
        public SeriesCollection TasksByStatusSeries { get; private set; }
        public SeriesCollection ProjectsByOwnerSeries { get; private set; }
        public ObservableCollection<string> OwnersLabels { get; private set; }

        // ===== ПОИСК =====
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (Set(ref _searchText, value))
                    UpdateSearch();
            }
        }

        public ObservableCollection<SearchResultItem> SearchResults { get; private set; }

        public bool HasSearchResults
        {
            get { return SearchResults.Count > 0; }
        }

        private SearchResultItem _selectedSearchResult;
        public SearchResultItem SelectedSearchResult
        {
            get { return _selectedSearchResult; }
            set
            {
                if (Set(ref _selectedSearchResult, value) && value != null)
                {
                    MainViewModel.Instance.NavigateFromSearch(value);
                    SelectedSearchResult = null;
                }
            }
        }

        public DashboardViewModel()
        {
            SearchResults = new ObservableCollection<SearchResultItem>();

            LoadCounts();
            LoadCharts();
        }

        private void LoadCounts()
        {
            ProjectsCount = Repositories.GetProjects().Count;
            TasksCount = Repositories.GetTasks().Count;
            DocumentsCount = Repositories.GetDocuments().Count;

            OnPropertyChanged(nameof(ProjectsCount));
            OnPropertyChanged(nameof(TasksCount));
            OnPropertyChanged(nameof(DocumentsCount));
        }

        private void LoadCharts()
        {
            TasksByStatusSeries = new SeriesCollection();

            var taskGroups = Repositories.GetTasks()
                .GroupBy(t => t.Status)
                .ToList();

            foreach (var group in taskGroups)
            {
                TasksByStatusSeries.Add(new PieSeries
                {
                    Title = group.Key,
                    Values = new ChartValues<int> { group.Count() },
                    DataLabels = true
                });
            }

            var projectGroups = Repositories.GetProjects()
                .GroupBy(p => p.Owner)
                .ToList();

            OwnersLabels = new ObservableCollection<string>(
                projectGroups.Select(p => p.Key)
            );

            ProjectsByOwnerSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = new ChartValues<int>(
                        projectGroups.Select(p => p.Count())
                    )
                }
            };

            OnPropertyChanged(nameof(TasksByStatusSeries));
            OnPropertyChanged(nameof(ProjectsByOwnerSeries));
            OnPropertyChanged(nameof(OwnersLabels));
        }

        private void UpdateSearch()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                OnPropertyChanged(nameof(HasSearchResults));
                return;
            }

            string text = SearchText.ToLower();

            foreach (var t in Repositories.GetTasks()
                .Where(x => x.Title.ToLower().Contains(text)))
                SearchResults.Add(new SearchResultItem("Задача", t.Title));

            foreach (var p in Repositories.GetProjects()
                .Where(x => x.Title.ToLower().Contains(text)))
                SearchResults.Add(new SearchResultItem("Проект", p.Title));

            foreach (var d in Repositories.GetDocuments()
                .Where(x => x.Title.ToLower().Contains(text)))
                SearchResults.Add(new SearchResultItem("Документ", d.Title));

            OnPropertyChanged(nameof(HasSearchResults));
        }
    }
}
