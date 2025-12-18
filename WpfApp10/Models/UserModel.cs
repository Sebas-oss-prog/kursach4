namespace WpfApp10.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        // Логин пользователя
        public string Login { get; set; }

        // Хеш или обычный пароль (позже можно заменить на хеш)
        public string Password { get; set; }

        // Admin / Manager / User
        public string Role { get; set; }

        // Отображаемое имя (для меню в боковой панели)
        public string FullName { get; set; }
    }
}
