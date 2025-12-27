using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using WpfApp10.Models;

namespace WpfApp10
{
    public static class Repositories
    {
        private static SQLiteConnection GetConnection()
        {
            return Database.GetConnection();
        }

        // ========================== USERS ==========================

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

        // ========================== TASKS ==========================

        public static List<TaskModel> GetTasks()
        {
            var list = new List<TaskModel>();

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            Id,
                            Title,
                            Description,
                            Status,
                            Priority,
                            ProjectId,
                            Progress,
                            Deadline
                        FROM Tasks
                    ";

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var task = new TaskModel
                            {
                                Id = Convert.ToInt32(r["Id"]),
                                Title = r["Title"]?.ToString(),
                                Description = r["Description"]?.ToString(),
                                Status = r["Status"]?.ToString(),
                                Priority = r["Priority"]?.ToString(),
                                ProjectId = r["ProjectId"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(r["ProjectId"]),
                                Progress = r["Progress"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(r["Progress"]),
                                Deadline = r["Deadline"]?.ToString()
                            };

                            // ===== ЗАГРУЗКА ИСПОЛНИТЕЛЕЙ =====
                            using (var ecmd = new SQLiteCommand(@"
                                SELECT e.Id, e.FullName, e.Position
                                FROM Employees e
                                JOIN TaskEmployees te ON te.EmployeeId = e.Id
                                WHERE te.TaskId = @tid;", conn))
                            {
                                ecmd.Parameters.AddWithValue("@tid", task.Id);
                                using (var er = ecmd.ExecuteReader())
                                {
                                    while (er.Read())
                                    {
                                        task.Employees.Add(new EmployeeModel
                                        {
                                            Id = er.GetInt32(0),
                                            FullName = er.GetString(1),
                                            Position = er.IsDBNull(2) ? "" : er.GetString(2)
                                        });
                                    }
                                }
                            }

                            list.Add(task);
                        }
                    }
                }
            }

            return list;
        }

        // ========================== SAVE TASK (ДОБАВЛЕНО) ==========================

        public static void SaveTask(TaskModel task)
        {
            if (task == null)
                return;

            using (var conn = GetConnection())
            {
                conn.Open();

                bool isNew = task.Id == 0;

                if (isNew)
                {
                    using (var cmd = new SQLiteCommand(@"
                        INSERT INTO Tasks
                        (Title, Description, Status, Priority, ProjectId, Progress, Deadline)
                        VALUES (@t,@d,@s,@p,@pid,@pr,@dl);
                        SELECT last_insert_rowid();", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", task.Title ?? "");
                        cmd.Parameters.AddWithValue("@d", task.Description ?? "");
                        cmd.Parameters.AddWithValue("@s", task.Status ?? "");
                        cmd.Parameters.AddWithValue("@p", task.Priority ?? "");
                        cmd.Parameters.AddWithValue("@pid", task.ProjectId);
                        cmd.Parameters.AddWithValue("@pr", task.Progress);
                        cmd.Parameters.AddWithValue("@dl", task.Deadline ?? "");

                        task.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Уведомление о новой задаче
                    AddNotification($"Добавлена новая задача: {task.Title}");
                }
                else
                {
                    UpdateTask(task);

                    // Уведомление об обновлении задачи
                    AddNotification($"Обновлена задача: {task.Title}");
                }

                // ===== СОХРАНЕНИЕ ИСПОЛНИТЕЛЕЙ =====
                SetTaskEmployees(task.Id, task.Employees.Select(e => e.Id).ToList());
            }
        }

        public static void AddTask(string title, string description, string status, string priority, int projectId, string deadline)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(@"
                    INSERT INTO Tasks
                    (Title, Description, Status, Priority, ProjectId, Progress, Deadline)
                    VALUES (@t,@d,@s,@p,@pid,0,@dl);", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@s", status ?? "");
                    cmd.Parameters.AddWithValue("@p", priority ?? "");
                    cmd.Parameters.AddWithValue("@pid", projectId);
                    cmd.Parameters.AddWithValue("@dl", deadline ?? "");
                    cmd.ExecuteNonQuery();
                }

                // Уведомление
                AddNotification($"Добавлена новая задача: {title}");
            }
        }

        public static void AddTask(TaskModel task)
        {
            if (task == null) return;

            AddTask(
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.ProjectId,
                task.Deadline
            );
        }

        public static void AddTask(string title, string description, string status, string priority)
        {
            AddTask(title, description, status, priority, 0, "");
        }

        public static void UpdateTask(int id, string title, string description, string status, string priority, int projectId, string deadline)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(@"
                    UPDATE Tasks SET
                        Title=@t,
                        Description=@d,
                        Status=@s,
                        Priority=@p,
                        ProjectId=@pid,
                        Deadline=@dl
                    WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@s", status ?? "");
                    cmd.Parameters.AddWithValue("@p", priority ?? "");
                    cmd.Parameters.AddWithValue("@pid", projectId);
                    cmd.Parameters.AddWithValue("@dl", deadline ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTask(TaskModel task)
        {
            if (task == null) return;

            UpdateTask(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.ProjectId,
                task.Deadline
            );
        }

        public static void DeleteTask(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                new SQLiteCommand("DELETE FROM Tasks WHERE Id=@id", conn)
                {
                    Parameters = { new SQLiteParameter("@id", id) }
                }.ExecuteNonQuery();
            }
        }

        // ========================== PROJECTS ==========================

        public static List<ProjectModel> GetProjects()
        {
            var list = new List<ProjectModel>();

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Title, Description, Owner, Deadline, Progress FROM Projects;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ProjectModel
                        {
                            Id = r.GetInt32(0),
                            Title = r.GetString(1),
                            Description = r.GetString(2),
                            Owner = r.GetString(3),
                            Deadline = r.GetString(4),
                            Progress = r.GetInt32(5)
                        });
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

                // Уведомление
                AddNotification($"Добавлен новый проект: {title}");
            }
        }

        public static void UpdateProject(int id, string title, string description, string owner, string deadline, int progress)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Projects SET Title=@t, Description=@d, Owner=@o, Deadline=@dl, Progress=@p WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@d", description ?? "");
                    cmd.Parameters.AddWithValue("@o", owner ?? "");
                    cmd.Parameters.AddWithValue("@dl", deadline ?? "");
                    cmd.Parameters.AddWithValue("@p", progress);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteProject(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                new SQLiteCommand("DELETE FROM Projects WHERE Id=@id", conn)
                {
                    Parameters = { new SQLiteParameter("@id", id) }
                }.ExecuteNonQuery();
            }
        }

        // ========================== DOCUMENTS ==========================

        public static List<DocumentModel> GetDocuments()
        {
            var list = new List<DocumentModel>();

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Title, Type, Author, CreatedDate, FilePath FROM Documents;",
                    conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DocumentModel
                        {
                            Id = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            Title = r.IsDBNull(1) ? "" : r.GetString(1),
                            Type = r.IsDBNull(2) ? "" : r.GetString(2),
                            Author = r.IsDBNull(3) ? "" : r.GetString(3),
                            CreatedDate = r.IsDBNull(4) ? "" : r.GetString(4),
                            FilePath = r.IsDBNull(5) ? "" : r.GetString(5)
                        });
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
                    "INSERT INTO Documents (Title, Type, Author, CreatedDate, FilePath) VALUES (@t,@ty,@a,@d,@fp);", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@ty", type ?? "");
                    cmd.Parameters.AddWithValue("@a", author ?? "");
                    cmd.Parameters.AddWithValue("@d", createdDate ?? "");
                    cmd.Parameters.AddWithValue("@fp", filePath ?? "");
                    cmd.ExecuteNonQuery();
                }

                // Уведомление
                AddNotification($"Добавлен новый документ: {title}");
            }
        }

        public static void UpdateDocument(int id, string title, string type, string author, string createdDate, string filePath)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Documents SET Title=@t, Type=@ty, Author=@a, CreatedDate=@d, FilePath=@fp WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@t", title ?? "");
                    cmd.Parameters.AddWithValue("@ty", type ?? "");
                    cmd.Parameters.AddWithValue("@a", author ?? "");
                    cmd.Parameters.AddWithValue("@d", createdDate ?? "");
                    cmd.Parameters.AddWithValue("@fp", filePath ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteDocument(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                new SQLiteCommand("DELETE FROM Documents WHERE Id=@id", conn)
                {
                    Parameters = { new SQLiteParameter("@id", id) }
                }.ExecuteNonQuery();
            }
        }

        // ========================== NOTIFICATIONS ==========================

        public static List<NotificationModel> GetNotifications()
        {
            var list = new List<NotificationModel>();

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Message, Time FROM Notifications ORDER BY Id DESC;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new NotificationModel
                        {
                            Id = r.GetInt32(0),
                            Message = r.GetString(1),
                            Time = r.GetString(2)
                        });
                    }
                }
            }

            return list;
        }

        public static void AddNotification(string message)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Notifications (Message, Time) VALUES (@msg, @time);", conn))
                {
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ========================== SEARCH ==========================

        public static List<SearchResultModel> Search(string query)
        {
            var results = new List<SearchResultModel>();
            if (string.IsNullOrWhiteSpace(query)) return results;

            using (var conn = GetConnection())
            {
                conn.Open();

                void SearchTable(string sql, string category)
                {
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                results.Add(new SearchResultModel
                                {
                                    Id = r.GetInt32(0),
                                    Title = r.GetString(1),
                                    Category = category,
                                    Info = r.GetString(2)
                                });
                            }
                        }
                    }
                }

                SearchTable("SELECT Id, Title, Description FROM Tasks WHERE Title LIKE @q", "Task");
                SearchTable("SELECT Id, Title, Description FROM Projects WHERE Title LIKE @q", "Project");
                SearchTable("SELECT Id, Title, FilePath FROM Documents WHERE Title LIKE @q", "Document");
                SearchTable("SELECT Id, Message, Time FROM Notifications WHERE Message LIKE @q", "Notification");
            }

            return results;
        }

        // ========================== EMPLOYEES ==========================

        public static List<EmployeeModel> GetEmployees()
        {
            var list = new List<EmployeeModel>();

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, FullName, Position FROM Employees;", conn))
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

        public static void SetTaskEmployees(int taskId, List<int> employeeIds)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                new SQLiteCommand("DELETE FROM TaskEmployees WHERE TaskId=@id", conn)
                {
                    Parameters = { new SQLiteParameter("@id", taskId) }
                }.ExecuteNonQuery();

                foreach (var empId in employeeIds)
                {
                    new SQLiteCommand(
                        "INSERT INTO TaskEmployees (TaskId, EmployeeId) VALUES (@t,@e)", conn)
                    {
                        Parameters =
                        {
                            new SQLiteParameter("@t", taskId),
                            new SQLiteParameter("@e", empId)
                        }
                    }.ExecuteNonQuery();
                }
            }
        }

        // ========================== USERS MANAGEMENT ==========================

        public static List<UserModel> GetUsers()
        {
            var list = new List<UserModel>();

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, Login, Password, Role, FullName FROM Users ORDER BY Id;", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new UserModel
                        {
                            Id = r.GetInt32(0),
                            Login = r.GetString(1),
                            Password = r.GetString(2),
                            Role = r.GetString(3),
                            FullName = r.IsDBNull(4) ? "" : r.GetString(4)
                        });
                    }
                }
            }
            return list;
        }

        public static void AddUser(string login, string password, string role, string fullName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Users (Login, Password, Role, FullName) VALUES (@l,@p,@r,@fn);", conn))
                {
                    cmd.Parameters.AddWithValue("@l", login);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.Parameters.AddWithValue("@r", role);
                    cmd.Parameters.AddWithValue("@fn", fullName);
                    cmd.ExecuteNonQuery();
                }

                // Уведомление
                AddNotification($"Добавлен новый пользователь: {fullName}");
            }
        }

        public static void UpdateUser(int id, string login, string password, string role, string fullName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "UPDATE Users SET Login=@l, Password=@p, Role=@r, FullName=@fn WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@l", login);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.Parameters.AddWithValue("@r", role);
                    cmd.Parameters.AddWithValue("@fn", fullName);
                    cmd.ExecuteNonQuery();
                }

                // Уведомление
                AddNotification($"Обновлён пользователь: {fullName}");
            }
        }

        public static void DeleteUser(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                new SQLiteCommand("DELETE FROM Users WHERE Id=@id", conn)
                {
                    Parameters = { new SQLiteParameter("@id", id) }
                }.ExecuteNonQuery();
            }
        }
    }
}