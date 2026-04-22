using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HangmanGame.Models;
using HangmanGame.View;

namespace HangmanGame.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        private const string UsersFile = "users.txt";

        public ObservableCollection<UserModel> Users { get; set; }

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        public ICommand NewUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand PlayCommand { get; }

        public StartViewModel()
        {
            Users = new ObservableCollection<UserModel>();

            LoadUsers();

            NewUserCommand = new RelayCommand(OnNewUser);
            DeleteUserCommand = new RelayCommand(OnDeleteUser, _ => SelectedUser != null);
            PlayCommand = new RelayCommand(OnPlay, _ => SelectedUser != null);
        }

        private void OnNewUser(object obj)
        {
            NewUserWindow win = new NewUserWindow();
            if (win.ShowDialog() == true)
            {
                if (win.CreatedUser != null)
                {
                    Users.Add(win.CreatedUser);
                    SaveUsers();
                }
            }
        }

        private void OnDeleteUser(object obj)
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete user {SelectedUser.Username}? All statistics and saved games will be lost.",
                                         "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var userToDelete = SelectedUser;
                string usernameToDelete = userToDelete.Username;
                string imagePathToDelete = userToDelete.ImagePath;

                //eliberam resursa img din ui
                SelectedUser = null;

                try
                {
                    string savesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");

                    if (Directory.Exists(savesPath))
                    {
                        var files = Directory.GetFiles(savesPath, $"{usernameToDelete}_*.json");
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting saved games: {ex.Message}");
                }

                try
                {
                    if (!string.IsNullOrEmpty(imagePathToDelete) && !imagePathToDelete.StartsWith("/Images/Avatars/"))
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePathToDelete.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the user image: {ex.Message}");
                }

                Users.Remove(userToDelete);
                SaveUsers();

                MessageBox.Show("User deleted successfully!");
            }
        }

        private void OnPlay(object obj)
        {
            if (SelectedUser != null)
            {
                var gameWindow = new HangmanGame.View.GameWindow();

                gameWindow.DataContext = new HangmanGame.ViewModels.GameViewModel(SelectedUser);

                Window currentWindow = Application.Current.MainWindow;
                gameWindow.Show();

                if (currentWindow != null)
                {
                    currentWindow.Hide();
                }
                gameWindow.Closed += (s, e) => {
                    if (currentWindow != null) currentWindow.Show();
                };
            }
        }

        private void SaveUsers()
        {
            var lines = Users.Select(u => {
                string statsStr = string.Join(";", u.CategoryStats.Select(kvp => $"{kvp.Key}:{kvp.Value.Played},{kvp.Value.Won}"));

                return $"{u.Username}|{u.ImagePath}|{u.CurrentLevel}|{statsStr}";
            });

            File.WriteAllLines(UsersFile, lines);
        }

        private void LoadUsers()
        {
            Users.Clear();
            if (!File.Exists(UsersFile)) return;

            try
            {
                var lines = File.ReadAllLines(UsersFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        var user = new UserModel
                        {
                            Username = parts[0],
                            ImagePath = parts[1],
                            CurrentLevel = 0,
                            CategoryStats = new System.Collections.Generic.Dictionary<string, CategoryStats>()
                        };

                        if (parts.Length >= 3 && int.TryParse(parts[2], out int level))
                        {
                            user.CurrentLevel = level;
                        }

                        if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                        {
                            var categories = parts[3].Split(';'); 
                            foreach (var catEntry in categories)
                            {
                                var catParts = catEntry.Split(':'); 
                                if (catParts.Length == 2)
                                {
                                    string catName = catParts[0];
                                    var scores = catParts[1].Split(','); 

                                    if (scores.Length == 2 &&
                                        int.TryParse(scores[0], out int played) &&
                                        int.TryParse(scores[1], out int won))
                                    {
                                         
                                            user.CategoryStats[catName] = new CategoryStats { Played = played, Won = won };
                                       
                                    }
                                }
                            }
                        }

                        Users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error loading users: {ex.Message}", "File Error");
            }
        }
    }
}