namespace WpfApp10
{
    public static class AppState
    {
        public static string Role { get; set; }

        public static bool IsAdmin => Role == "Admin";
        public static bool IsManagerOrAdmin => Role == "Admin" || Role == "Manager";
    }
}
