using System;
using System.Collections.ObjectModel;
using WpfApp10.Helpers;
using WpfApp10.ViewModels;

namespace WpfApp10.Models
{
    public class ProjectModel : BaseViewModel
    {
        public int Id { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        // ================== OWNER ==================

        private string _owner;
        public string Owner
        {
            get => _owner;
            set => Set(ref _owner, value);
        }

        // Для ComboBox
        public EmployeeModel OwnerEmployee
        {
            get => string.IsNullOrEmpty(Owner)
                ? null
                : new EmployeeModel { FullName = Owner };

            set => Owner = value?.FullName;
        }

        // ================== DEADLINE ==================

        private string _deadline;
        public string Deadline
        {
            get => _deadline;
            set
            {
                Set(ref _deadline, value);
                OnPropertyChanged(nameof(DeadlineDate));
            }
        }

        // 👉 Для DatePicker
        public DateTime? DeadlineDate
        {
            get
            {
                if (DateTime.TryParse(Deadline, out var d))
                    return d;
                return null;
            }
            set
            {
                Deadline = value.HasValue
                    ? value.Value.ToString("yyyy-MM-dd")
                    : null;
            }
        }

        // ================== ПРОЧЕЕ ==================

        public int Progress { get; set; }

        // Участники (ТОЛЬКО UI)
        public ObservableCollection<EmployeeModel> Members { get; set; }
            = new ObservableCollection<EmployeeModel>();
    }
}
