using System;
using System.Collections.ObjectModel;
using WpfApp10.ViewModels;

namespace WpfApp10.Models
{
    public class TaskModel : BaseViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        // ================= TITLE =================
        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        // ================= DESCRIPTION =================
        private string _description;
        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        // ================= PRIORITY =================
        private string _priority;
        public string Priority
        {
            get => _priority;
            set => Set(ref _priority, value);
        }

        // ================= STATUS =================
        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        // ================= DEADLINE (STRING / DB) =================
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

        // ================= DEADLINE (DatePicker) =================
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
                Deadline = value?.ToString("yyyy-MM-dd");
                OnPropertyChanged(nameof(DeadlineDate));
            }
        }

        // ================= PROGRESS =================
        public int Progress { get; set; }

        // ================= EMPLOYEES =================
        public ObservableCollection<EmployeeModel> Employees { get; }
            = new ObservableCollection<EmployeeModel>();
    }
}
