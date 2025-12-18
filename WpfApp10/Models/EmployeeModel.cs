namespace WpfApp10.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }

        public override string ToString() => FullName;
    }
}
