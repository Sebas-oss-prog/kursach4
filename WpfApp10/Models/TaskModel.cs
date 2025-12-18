using System.Collections.Generic;

namespace WpfApp10.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string Priority { get; set; }

        // основной ответственный (автоматически)
        public string AssignedTo { get; set; }

        public string Deadline { get; set; }
        public string Status { get; set; }

        // ❗ редактировать нельзя
        public int Progress { get; set; }

        public int ProjectId { get; set; }

        // команда задачи
        public List<EmployeeModel> Employees { get; set; }
            = new List<EmployeeModel>();
    }
}
