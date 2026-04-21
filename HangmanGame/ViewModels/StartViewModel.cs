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

            string usernameToDelete = SelectedUser.Username; 
            Users.Remove(SelectedUser);
            SaveUsers();

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
            var lines = Users.Select(u => $"{u.Username}|{u.ImagePath}");
            File.WriteAllLines(UsersFile, lines);
        }

        private void LoadUsers()
        {
            if (!File.Exists(UsersFile)) return;

            var lines = File.ReadAllLines(UsersFile);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    Users.Add(new UserModel { Username = parts[0], ImagePath = parts[1] });
                }
            }
        }
    }
}