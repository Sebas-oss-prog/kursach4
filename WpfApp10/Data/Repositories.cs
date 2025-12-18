using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WpfApp10.Models;

namespace WpfApp10
{
    public static class Repositories
    {
        private static SQLiteConnection GetConnection()
        {
            return Database.GetConnection();
        }

        // -------------------------- USERS --------------------------
        // 🔹 ДОБАВЛЕНО ДЛЯ АВТОРИЗАЦИИ

        public static UserModel GetUser(string login, string password)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Login, Role, FullName FROM Users WHERE Login=@l AND Password=@p;",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@l", login);
                    cmd.Parameters.AddWithValue("@p", password);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new UserModel
                            {
                                Id = r.GetInt32(0),
                                Login = r.GetString(1),
                                Role = r.GetString(2),
                                FullName = r.IsDBNull(3) ? "" : r.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // -------------------------- TASKS --------------------------

        public static List<TaskModel> GetTasks()
        {
            var list = new List<TaskModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Title, Description, Status, Priority, ProjectId FROM Tasks;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var t = new TaskModel();
                        t.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                        t.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                        t.Description = r.IsDBNull(2) ? "" : r.GetString(2);
                        t.Status = r.IsDBNull(3) ? "" : r.GetString(3);
                        t.Priority = r.IsDBNull(4) ? "" : r.GetString(4);
                        t.ProjectId = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public static void AddTask(string title, string description, string status, string priority, int projectId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Tasks (Title, Description, Status, Priority, ProjectId) VALUES (@t,@d,@s,@p,@pid);", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@s", status ?? "");
                    cmd.Parameters.AddWithValue("@p", priority ?? "");
                    cmd.Parameters.AddWithValue("@pid", projectId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddTask(TaskModel task)
        {
            if (task == null) return;
            AddTask(task.Title, task.Description, task.Status, task.Priority, task.ProjectId);
        }

        public static void AddTask(string title, string description, string status, string priority)
        {
            AddTask(title, description, status, priority, 0);
        }

        public static void UpdateTask(int id, string title, string description, string status, string priority, int projectId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Tasks SET Title=@t, Description=@d, Status=@s, Priority=@p, ProjectId=@pid WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@s", status ?? "");
                    cmd.Parameters.AddWithValue("@p", priority ?? "");
                    cmd.Parameters.AddWithValue("@pid", projectId);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTask(TaskModel task)
        {
            if (task == null) return;
            UpdateTask(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId);
        }

        public static void UpdateTask(int id, string title, string description, string status, string priority)
        {
            UpdateTask(id, title, description, status, priority, 0);
        }

        public static void DeleteTask(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Tasks WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------- PROJECTS --------------------------

        public static List<ProjectModel> GetProjects()
        {
            var list = new List<ProjectModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Title, Description, Owner, Deadline, Progress FROM Projects;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var p = new ProjectModel();
                        p.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                        p.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                        p.Description = r.IsDBNull(2) ? "" : r.GetString(2);
                        p.Owner = r.IsDBNull(3) ? "" : r.GetString(3);
                        p.Deadline = r.IsDBNull(4) ? "" : r.GetString(4);
                        p.Progress = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                        list.Add(p);
                    }
                }
            }
            return list;
        }

        public static void AddProject(string title, string description, string owner, string deadline)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Projects (Title, Description, Owner, Deadline, Progress) VALUES (@t,@d,@o,@dl,0);", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@o", owner ?? "");
                    cmd.Parameters.AddWithValue("@dl", deadline ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // overload: AddProject from model
        public static void AddProject(ProjectModel project)
        {
            if (project == null) return;
            AddProject(project.Title, project.Description, project.Owner, project.Deadline);
        }

        public static void UpdateProject(int id, string title, string description, string owner, string deadline)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Projects SET Title=@t, Description=@d, Owner=@o, Deadline=@dl WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@o", owner ?? "");
                    cmd.Parameters.AddWithValue("@dl", deadline ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // overload: UpdateProject from model
        public static void UpdateProject(ProjectModel project)
        {
            if (project == null) return;
            UpdateProject(project.Id, project.Title, project.Description, project.Owner, project.Deadline);
        }

        public static void DeleteProject(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Projects WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------- DOCUMENTS --------------------------

        public static List<DocumentModel> GetDocuments()
        {
            var list = new List<DocumentModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Title, Type, Author, CreatedDate, FilePath FROM Documents;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var d = new DocumentModel();
                        d.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                        d.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                        d.Type = r.IsDBNull(2) ? "" : r.GetString(2);
                        d.Author = r.IsDBNull(3) ? "" : r.GetString(3);
                        d.CreatedDate = r.IsDBNull(4) ? "" : r.GetString(4);
                        d.FilePath = r.IsDBNull(5) ? "" : r.GetString(5);
                        list.Add(d);
                    }
                }
            }
            return list;
        }

        public static void AddDocument(string title, string type, string author, string createdDate, string filePath)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Documents (Title, Type, Author, CreatedDate, FilePath) VALUES (@t,@tp,@a,@cd,@fp);", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@tp", type ?? "");
                    cmd.Parameters.AddWithValue("@a", author ?? "");
                    cmd.Parameters.AddWithValue("@cd", createdDate ?? "");
                    cmd.Parameters.AddWithValue("@fp", filePath ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // overload: AddDocument from model
        public static void AddDocument(DocumentModel doc)
        {
            if (doc == null) return;
            AddDocument(doc.Title, doc.Type, doc.Author, doc.CreatedDate, doc.FilePath);
        }

        public static void UpdateDocument(int id, string title, string type, string author, string createdDate, string filePath)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Documents SET Title=@t, Type=@tp, Author=@a, CreatedDate=@cd, FilePath=@fp WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@tp", type ?? "");
                    cmd.Parameters.AddWithValue("@a", author ?? "");
                    cmd.Parameters.AddWithValue("@cd", createdDate ?? "");
                    cmd.Parameters.AddWithValue("@fp", filePath ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // overload: UpdateDocument from model
        public static void UpdateDocument(DocumentModel doc)
        {
            if (doc == null) return;
            UpdateDocument(doc.Id, doc.Title, doc.Type, doc.Author, doc.CreatedDate, doc.FilePath);
        }

        public static void DeleteDocument(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Documents WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------- NOTIFICATIONS --------------------------

        public static List<NotificationModel> GetNotifications()
        {
            var list = new List<NotificationModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Message, Time FROM Notifications;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var n = new NotificationModel();
                        n.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                        n.Message = r.IsDBNull(1) ? "" : r.GetString(1);
                        n.Time = r.IsDBNull(2) ? "" : r.GetString(2);
                        list.Add(n);
                    }
                }
            }
            return list;
        }

        public static void AddNotification(string message, string time)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("INSERT INTO Notifications (Message, Time) VALUES (@m,@t);", conn))
                {
                    cmd.Parameters.AddWithValue("@m", message ?? "");
                    cmd.Parameters.AddWithValue("@t", time ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // overload: AddNotification from model
        public static void AddNotification(NotificationModel n)
        {
            if (n == null) return;
            AddNotification(n.Message, n.Time);
        }

        public static void DeleteNotification(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Notifications WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------- SEARCH --------------------------

        public static List<SearchResultModel> Search(string query)
        {
            var results = new List<SearchResultModel>();
            if (string.IsNullOrWhiteSpace(query)) return results;

            using (var conn = GetConnection())
            {
                conn.Open();

                // TASKS
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Title, Description FROM Tasks WHERE Title LIKE @q OR Description LIKE @q;", conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var s = new SearchResultModel();
                            s.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            s.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                            s.Category = "Task";
                            s.Info = r.IsDBNull(2) ? "" : r.GetString(2);
                            results.Add(s);
                        }
                    }
                }

                // PROJECTS
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Title, Description FROM Projects WHERE Title LIKE @q OR Description LIKE @q;", conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var s = new SearchResultModel();
                            s.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            s.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                            s.Category = "Project";
                            s.Info = r.IsDBNull(2) ? "" : r.GetString(2);
                            results.Add(s);
                        }
                    }
                }

                // DOCUMENTS
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Title, FilePath FROM Documents WHERE Title LIKE @q;", conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var s = new SearchResultModel();
                            s.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            s.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                            s.Category = "Document";
                            s.Info = r.IsDBNull(2) ? "" : r.GetString(2);
                            results.Add(s);
                        }
                    }
                }

                // NOTIFICATIONS
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Message, Time FROM Notifications WHERE Message LIKE @q;", conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var s = new SearchResultModel();
                            s.Id = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            s.Title = r.IsDBNull(1) ? "" : r.GetString(1);
                            s.Category = "Notification";
                            s.Info = r.IsDBNull(2) ? "" : r.GetString(2);
                            results.Add(s);
                        }
                    }
                }
            }

            return results;
        }

        // ================= EMPLOYEES =================

        public static List<EmployeeModel> GetEmployees()
        {
            var list = new List<EmployeeModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT Id, FullName, Position FROM Employees", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new EmployeeModel
                        {
                            Id = r.GetInt32(0),
                            FullName = r.GetString(1),
                            Position = r.IsDBNull(2) ? "" : r.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        // ================= EMPLOYEES =================
        public static void SetTaskEmployees(int taskId, List<int> employeeIds)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var del = new SQLiteCommand("DELETE FROM TaskEmployees WHERE TaskId=@id", conn);
                del.Parameters.AddWithValue("@id", taskId);
                del.ExecuteNonQuery();

                foreach (var empId in employeeIds)
                {
                    var ins = new SQLiteCommand(
                        "INSERT INTO TaskEmployees (TaskId, EmployeeId) VALUES (@t,@e)", conn);
                    ins.Parameters.AddWithValue("@t", taskId);
                    ins.Parameters.AddWithValue("@e", empId);
                    ins.ExecuteNonQuery();
                }
            }
        }

    }
}
