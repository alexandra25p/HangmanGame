using System;
using System.Collections.Generic;
using System.IO;
using HangmanGame.ViewModels;

namespace HangmanGame.Models
{
    public class CategoryStats
    {
        public int Played { get; set; }
        public int Won { get; set; }
        public double WinRate => Played > 0 ? (double)Won / Played * 100 : 0;
    }

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
            set { _imagePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullImagePath)); }
        }

        public string FullImagePath => string.IsNullOrEmpty(ImagePath) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath);

        private int _currentLevel = 0;
        public int CurrentLevel
        {
            get => _currentLevel;
            set { _currentLevel = value; OnPropertyChanged(); }
        }

        private Dictionary<string, CategoryStats> _categoryStats = new Dictionary<string, CategoryStats>();
        public Dictionary<string, CategoryStats> CategoryStats
        {
            get => _categoryStats;
            set { _categoryStats = value; OnPropertyChanged(); }
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