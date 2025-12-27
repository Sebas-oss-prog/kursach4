namespace WpfApp10.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }

        // Свойства для удобства UI
        public bool IsAdmin => Role == "Admin";
        public bool IsManager => Role == "Manager";
        public bool IsUser => Role == "User";

        public override string ToString() => FullName;
    }
}