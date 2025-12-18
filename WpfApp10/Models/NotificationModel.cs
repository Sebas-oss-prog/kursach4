namespace WpfApp10.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }

        // Текст уведомления
        public string Message { get; set; }

        // Оригинальное поле — сейчас используется в проекте
        public string Time { get; set; }

        // Совместимое свойство для нового кода (проксирует Time)
        // Позволяет одновременно обращаться к .Date и к .Time без ошибок.
        public string Date
        {
            get { return Time; }
            set { Time = value; }
        }
    }
}
