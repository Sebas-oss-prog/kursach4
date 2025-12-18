using System;
using System.Data.SQLite;
using System.IO;

namespace WpfApp10
{
    public static class Database
    {
        private static readonly string _dbPath = "taskplanner.db";
        private static readonly string _connectionString =
            "Data Source=taskplanner.db;Version=3;foreign keys=true;";

        public static SQLiteConnection GetConnection()
            => new SQLiteConnection(_connectionString);

        public static void Initialize()
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            CreateTables();
            InsertUsers();
            InsertEmployees();
            InsertTestData();
        }

        // ================== TABLES ==================
        private static void CreateTables()
        {
            Execute(@"
CREATE TABLE IF NOT EXISTS Documents(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Type TEXT,
    Author TEXT,
    FilePath TEXT,
    CreatedDate TEXT,
    Deadline TEXT
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS Projects(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    Owner TEXT,
    Deadline TEXT,
    Progress INTEGER
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS Tasks(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId INTEGER,
    Title TEXT NOT NULL,
    Description TEXT,
    Status TEXT,
    Priority TEXT,
    Progress INTEGER,
    Deadline TEXT,
    FOREIGN KEY(ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS Employees(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Position TEXT
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS TaskEmployees(
    TaskId INTEGER,
    EmployeeId INTEGER,
    PRIMARY KEY (TaskId, EmployeeId),
    FOREIGN KEY(TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
    FOREIGN KEY(EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS Notifications(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Message TEXT,
    Time TEXT
);");

            Execute(@"
CREATE TABLE IF NOT EXISTS Users(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Login TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Role TEXT NOT NULL,
    FullName TEXT
);");
        }

        // ================== USERS ==================
        private static void InsertUsers()
        {
            if (HasData("Users")) return;

            Execute(@"
INSERT INTO Users (Login,Password,Role,FullName) VALUES
('admin','admin','Admin','Администратор'),
('manager','manager','Manager','Менеджер'),
('user','123','User','Пользователь');
");
        }

        // ================== EMPLOYEES ==================
        private static void InsertEmployees()
        {
            if (HasData("Employees")) return;

            Execute(@"
INSERT INTO Employees (FullName, Position) VALUES
('Иванов И.И.', 'Backend'),
('Петров П.П.', 'Frontend'),
('Сидоров С.С.', 'Analyst'),
('Кузнецов К.К.', 'Tester'),
('Орлов О.О.', 'DevOps');
");
        }

        // ================== TEST DATA ==================
        private static void InsertTestData()
        {
            if (HasData("Projects")) return;

            // -------- PROJECTS --------
            Execute(@"
INSERT INTO Projects (Title, Description, Owner, Deadline, Progress) VALUES
('CRM система','Разработка CRM','Иванов','2025-03-01',20),
('Сайт компании','Редизайн сайта','Петров','2025-02-15',60),
('Мобильное приложение','Приложение для клиентов','Орлов','2025-04-10',10),
('Автоматизация отдела','Внутренняя система','Сидоров','2025-05-05',40),
('Интеграция API','Связь с сервисами','Белов','2025-02-20',80);
");

            // -------- TASKS --------
            Execute(@"
INSERT INTO Tasks (ProjectId, Title, Description, Status, Priority, Progress, Deadline) VALUES
(1,'Проектирование БД','Схема данных','В работе','Высокий',30,'2025-01-25'),
(1,'API авторизации','JWT + роли','Не начато','Высокий',0,'2025-02-05'),
(2,'Главная страница','UI и верстка','Завершена','Средний',100,'2025-01-10'),
(3,'Push-уведомления','Firebase','В работе','Низкий',20,'2025-03-05'),
(4,'Сбор требований','Интервью','Завершена','Высокий',100,'2025-01-15');
");

            // -------- DOCUMENTS --------
            Execute(@"
INSERT INTO Documents (Title, Type, Author, CreatedDate, Deadline) VALUES
('Техническое задание','PDF','Иванов','2025-01-01','2025-01-10'),
('Отчет по проекту','DOCX','Петров','2025-01-03','2025-01-20'),
('График работ','XLSX','Сидоров','2025-01-05','2025-01-25'),
('Финансовый отчет','PDF','Орлов','2025-01-07','2025-02-01'),
('Презентация проекта','PPTX','Кузнецов','2025-01-09','2025-01-30');
");

            // -------- NOTIFICATIONS --------
            Execute(@"
INSERT INTO Notifications (Message, Time) VALUES
('Проект CRM обновлен','2025-01-10'),
('Задача завершена','2025-01-11'),
('Добавлен документ','2025-01-12'),
('Изменен срок проекта','2025-01-13'),
('Создан новый пользователь','2025-01-14');
");
        }

        // ================== HELPERS ==================
        private static bool HasData(string table)
        {
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {table}", con);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void Execute(string sql)
        {
            using (var con = GetConnection())
            {
                con.Open();
                new SQLiteCommand(sql, con).ExecuteNonQuery();
            }
        }
    }
}
