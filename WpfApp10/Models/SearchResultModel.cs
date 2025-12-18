namespace WpfApp10.Models
{
    public class SearchResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // поддерживаем оба имени чтобы не ломать код
        public string Category { get; set; }

        // legacy — если где-то всё ещё используют Type
        public string Type
        {
            get { return Category; }
            set { Category = value; }
        }

        public string Info { get; set; }
    }
}
