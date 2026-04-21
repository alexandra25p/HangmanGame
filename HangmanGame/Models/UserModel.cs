using System;
using System.IO;
using HangmanGame.ViewModels;

namespace HangmanGame.Models
{
    public class UserModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _imagePath;
        public string ImagePath 
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullImagePath));
            }
        }

        public string FullImagePath => string.IsNullOrEmpty(ImagePath)  ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath);

        private int _currentLevel = 1;
        public int CurrentLevel
        {
            get => _currentLevel;
            set { _currentLevel = value; OnPropertyChanged(); }
        }

        private int _gamesPlayed;
        public int GamesPlayed
        {
            get => _gamesPlayed;
            set { _gamesPlayed = value; OnPropertyChanged(); }
        }

        private int _gamesWon;
        public int GamesWon
        {
            get => _gamesWon;
            set { _gamesWon = value; OnPropertyChanged(); }
        }
    }
}