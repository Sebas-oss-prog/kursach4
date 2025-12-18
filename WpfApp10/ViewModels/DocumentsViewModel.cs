using System.Collections.ObjectModel;
using System.Data.SQLite;
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

            AddCommand = new RelayCommand(_ => AddDocument());
            EditCommand = new RelayCommand(_ => EditDocument(), _ => SelectedDocument != null);
            DeleteCommand = new RelayCommand(_ => DeleteDocument(), _ => SelectedDocument != null);
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
                var cmd = new SQLiteCommand("SELECT * FROM Documents", con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Documents.Add(new DocumentModel
                    {
                        Id = r.GetInt32(0),
                        Title = r.GetString(1),
                        Type = r.GetString(2),
                        Author = r.GetString(3),
                        FilePath = r.IsDBNull(4) ? null : r.GetString(4),
                        CreatedDate = r.GetString(5)
                    });
                }
            }
        }

        private void AddDocument()
        {
            _isEditMode = false;
            SelectedDocument = new DocumentModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditDocument()
        {
            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void DeleteDocument()
        {
            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("DELETE FROM Documents WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", SelectedDocument.Id);
                cmd.ExecuteNonQuery();
            }

            Documents.Remove(SelectedDocument);
            SelectedDocument = null;
        }

        private void SaveDocument()
        {
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
