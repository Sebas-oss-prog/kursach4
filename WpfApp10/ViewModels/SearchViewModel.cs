using System.Collections.ObjectModel;
using System.ComponentModel;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                Set(ref _query, value);
                UpdateResults();
            }
        }

        public ObservableCollection<SearchResultModel> Results { get; set; }
            = new ObservableCollection<SearchResultModel>();

        public SearchViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return;
        }

        private void UpdateResults()
        {
            Results.Clear();

            if (string.IsNullOrWhiteSpace(Query))
                return;

            var list = WpfApp10.Repositories.Search(Query);
            foreach (var r in list)
            {
                Results.Add(new SearchResultModel
                {
                    Title = r.Title,
                    Type = r.Type,
                    Info = r.Info
                });
            }
        }
    }
}
