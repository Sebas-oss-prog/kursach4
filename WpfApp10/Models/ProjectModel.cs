using System.Collections.ObjectModel;

namespace WpfApp10.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        // ⚠️ ОСТАВЛЯЕМ (БД + старая логика)
        public string Owner { get; set; }

        public string Deadline { get; set; }
        public int Progress { get; set; }

        // ================== НОВОЕ ==================

        // Для UI (выбор владельца)
        public EmployeeModel OwnerEmployee
        {
            get => string.IsNullOrEmpty(Owner) ? null : new EmployeeModel { FullName = Owner };
            set => Owner = value?.FullName;
        }

        // Участники проекта (ТОЛЬКО UI, можно не сохранять в БД)
        public ObservableCollection<EmployeeModel> Members { get; set; }
            = new ObservableCollection<EmployeeModel>();
    }
}
