namespace WpfApp10.ViewModels
{
    public class SearchResultItem
    {
        public string Type { get; }
        public string Title { get; }

        public SearchResultItem(string type, string title)
        {
            Type = type;
            Title = title;
        }
    }
}
