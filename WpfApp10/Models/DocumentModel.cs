namespace WpfApp10.Models
{
    public class DocumentModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Type { get; set; }

        // ⚠️ ОСТАВЛЯЕМ
        public string Author { get; set; }

        public string CreatedDate { get; set; }
        public string FilePath { get; set; }

        public string Date
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }

        // ================== НОВОЕ ==================

        // Для ComboBox
        public EmployeeModel AuthorEmployee
        {
            get => string.IsNullOrEmpty(Author) ? null : new EmployeeModel { FullName = Author };
            set => Author = value?.FullName;
        }
    }
}
