using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class DocumentsViewModel : BaseViewModel
    {
        public ObservableCollection<DocumentModel> Documents { get; }
            = new ObservableCollection<DocumentModel>();

        public ObservableCollection<EmployeeModel> Employees { get; }
            = new ObservableCollection<EmployeeModel>();

        private DocumentModel _selectedDocument;
        public DocumentModel SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                Set(ref _selectedDocument, value);
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isEditorVisible;
        public bool IsEditorVisible
        {
            get => _isEditorVisible;
            set => Set(ref _isEditorVisible, value);
        }

        private bool _isEditMode;
        public string PanelTitle =>
            _isEditMode ? "Редактирование документа" : "Создание документа";

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        public DocumentsViewModel()
        {
            LoadEmployees();
            LoadDocuments();

            AddCommand = new RelayCommand(_ => AddDocument(), _ => MainViewModel.Instance.CanAdd);
            EditCommand = new RelayCommand(_ => EditDocument(), _ => SelectedDocument != null && MainViewModel.Instance.CanEdit);
            DeleteCommand = new RelayCommand(_ => DeleteDocument(), _ => SelectedDocument != null && MainViewModel.Instance.CanDelete);
            SaveCommand = new RelayCommand(_ => SaveDocument());
            CloseCommand = new RelayCommand(_ => CloseEditor());
        }

        private void LoadEmployees()
        {
            Employees.Clear();

            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("SELECT Id, FullName, Position FROM Employees", con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Employees.Add(new EmployeeModel
                    {
                        Id = r.GetInt32(0),
                        FullName = r.GetString(1),
                        Position = r.GetString(2)
                    });
                }
            }
        }

        private void LoadDocuments()
        {
            Documents.Clear();

            using (var con = Database.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Documents", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Documents.Add(new DocumentModel
                        {
                            Id = r.GetInt32(r.GetOrdinal("Id")),
                            Title = r.IsDBNull(r.GetOrdinal("Title"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Title")),

                            Type = r.IsDBNull(r.GetOrdinal("Type"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Type")),

                            Author = r.IsDBNull(r.GetOrdinal("Author"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Author")),

                            FilePath = r.IsDBNull(r.GetOrdinal("FilePath"))
                                ? ""
                                : r.GetString(r.GetOrdinal("FilePath")),

                            CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate"))
                                ? ""
                                : r.GetString(r.GetOrdinal("CreatedDate"))
                        });
                    }
                }
            }
        }

        private void AddDocument()
        {
            if (!MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление документов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = false;
            SelectedDocument = new DocumentModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditDocument()
        {
            if (!MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование документов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void DeleteDocument()
        {
            if (!MainViewModel.Instance.CanDelete)
            {
                MessageBox.Show("У вас нет прав на удаление документов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDocument == null)
                return;

            var result = MessageBox.Show($"Удалить документ \"{SelectedDocument.Title}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Repositories.DeleteDocument(SelectedDocument.Id);
                Documents.Remove(SelectedDocument);
                SelectedDocument = null;
            }
        }

        private void SaveDocument()
        {
            if (SelectedDocument == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedDocument.Title))
            {
                MessageBox.Show("Введите название документа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при создании нового документа
            if (!_isEditMode && !MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление документов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при редактировании документа
            if (_isEditMode && !MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование документов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var con = Database.GetConnection())
            {
                con.Open();
                SQLiteCommand cmd;

                if (SelectedDocument.Id == 0)
                {
                    cmd = new SQLiteCommand(
                        "INSERT INTO Documents (Title, Type, Author, CreatedDate) VALUES (@t,@ty,@a,@d)", con);
                }
                else
                {
                    cmd = new SQLiteCommand(
                        "UPDATE Documents SET Title=@t, Type=@ty, Author=@a, CreatedDate=@d WHERE Id=@id", con);
                    cmd.Parameters.AddWithValue("@id", SelectedDocument.Id);
                }

                cmd.Parameters.AddWithValue("@t", SelectedDocument.Title);
                cmd.Parameters.AddWithValue("@ty", SelectedDocument.Type);
                cmd.Parameters.AddWithValue("@a", SelectedDocument.Author);
                cmd.Parameters.AddWithValue("@d", SelectedDocument.CreatedDate);

                cmd.ExecuteNonQuery();

                // Уведомление
                string action = SelectedDocument.Id == 0 ? "Добавлен" : "Обновлён";
                Repositories.AddNotification($"{action} документ: {SelectedDocument.Title}");
            }

            LoadDocuments();
            CloseEditor();
        }

        private void CloseEditor()
        {
            IsEditorVisible = false;
            SelectedDocument = null;
        }
    }
}