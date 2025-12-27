using System;
using System.Data.SQLite;
using System.IO;

namespace WpfApp10
{
    public static class Database
    {
        private static readonly string DbPath = "taskplanner.db";
        private static readonly string Cs =
            "Data Source=taskplanner.db;Version=3;foreign keys=true;";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(Cs);
        }

        public static void Initialize()
        {
            if (!File.Exists(DbPath))
                SQLiteConnection.CreateFile(DbPath);

            CreateTables();
            InsertTestData();
        }

        private static void CreateTables()
        {
            Exec(@"
CREATE TABLE IF NOT EXISTS Users(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Login TEXT UNIQUE,
    Password TEXT,
    Role TEXT,
    FullName TEXT
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS Employees(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Position TEXT
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS Projects(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT,
    Description TEXT,
    Owner TEXT,
    Deadline TEXT,
    Progress INTEGER
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS Tasks(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId INTEGER,
    Title TEXT,
    Description TEXT,
    Status TEXT,
    Priority TEXT,
    Progress INTEGER,
    Deadline TEXT
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS TaskEmployees(
    TaskId INTEGER,
    EmployeeId INTEGER,
    PRIMARY KEY(TaskId, EmployeeId)
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS Documents(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT,
    Type TEXT,
    Author TEXT,
    FilePath TEXT,
    CreatedDate TEXT
);");

            Exec(@"
CREATE TABLE IF NOT EXISTS Notifications(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Message TEXT,
    Time TEXT
);");
        }

        private static void InsertTestData()
        {
            using (var con = GetConnection())
            {
                con.Open();

                // 🔴 ВАЖНО: проверяем USERS, а не Tasks
                var check = new SQLiteCommand("SELECT COUNT(*) FROM Users", con);
                if ((long)check.ExecuteScalar() > 0)
                    return;
            }

            // ===== USERS =====
            Exec(@"
INSERT INTO Users (Login, Password, Role, FullName) VALUES
('admin','admin','Admin','Администратор'),
('ivan','123','Manager','Иванов И.И.'),
('petr','123','Employee','Петров П.П.'),
('sidor','123','Employee','Сидоров С.С.'),
('guest','guest','Guest','Гость');
");

            // ===== EMPLOYEES =====
            Exec(@"
INSERT INTO Employees (FullName, Position) VALUES
('Иванов И.И.','Backend'),
('Петров П.П.','Frontend'),
('Сидоров С.С.','Analyst'),
('Орлов О.О.','DevOps'),
('Кузнецов К.К.','Tester'),
('Смирнов С.С.','Designer'),
('Фёдоров Ф.Ф.','QA');
");

            // ===== PROJECTS =====
            Exec(@"
INSERT INTO Projects (Title, Description, Owner, Deadline, Progress) VALUES
('CRM','Продажи','Иванов И.И.','2026-01-15',20),
('Сайт','Корпоративный','Петров П.П.','2026-01-20',50),
('Мобильное приложение','Android/iOS','Сидоров С.С.','2026-02-10',10);
");

            // ===== TASKS =====
            Exec(@"
INSERT INTO Tasks (ProjectId, Title, Description, Status, Priority, Progress, Deadline) VALUES
(1,'Тест БД','Проверка SQLite','Новая','Высокий',0,'2026-01-15'),
(1,'API','REST','В работе','Высокий',40,'2026-01-18'),
(2,'Верстка','HTML/CSS','В работе','Средний',30,'2026-01-18'),
(3,'Прототип','UI/UX','Новая','Низкий',0,'2026-02-01');
");

            // ===== TASK ↔ EMPLOYEES =====
            Exec(@"
INSERT INTO TaskEmployees (TaskId, EmployeeId) VALUES
(1,1),
(1,3),
(2,1),
(3,2),
(4,3);
");

            // ===== DOCUMENTS =====
            Exec(@"
INSERT INTO Documents (Title, Type, Author, CreatedDate) VALUES
('ТЗ','PDF','Иванов И.И.','2026-01-10'),
('Макеты','Figma','Петров П.П.','2026-01-12'),
('API Doc','DOCX','Сидоров С.С.','2026-01-15');
");

            // ===== NOTIFICATIONS =====
            Exec(@"
INSERT INTO Notifications (Message, Time) VALUES
('Добавлена новая задача','2026-01-01 10:00'),
('Проект CRM обновлён','2026-01-05 12:30');
");
        }

        private static void Exec(string sql)
        {
            using (var con = GetConnection())
            {
                con.Open();
                new SQLiteCommand(sql, con).ExecuteNonQuery();
            }
        }
    }
}
