using HangmanGame.Models;
using HangmanGame.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Text.Json;

namespace HangmanGame.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private UserModel _currentPlayer;
        private const string WordsFile = "words.txt";
        private const string UsersFile = "users.txt";
        private Dictionary<string, List<string>> _wordsByCategory;
        private List<string> _usedWords = new List<string>();
        private string _currentCategory = "All categories";
        private string _wordToGuess = "";
        private Random _random = new Random();
        private int _mistakes;
        private List<char> _guessedLetters;
        private DispatcherTimer _timer;
        private int _timeLeft;
        private string _currentHangmanImage;
        private string _displayedWord;

        public UserModel CurrentPlayer
        {
            get => _currentPlayer;
            set { _currentPlayer = value; OnPropertyChanged(); }
        }

        public string CurrentHangmanImage
        {
            get => _currentHangmanImage;
            set { _currentHangmanImage = value; OnPropertyChanged(); }
        }

        public string DisplayedWord
        {
            get => _displayedWord;
            set { _displayedWord = value; OnPropertyChanged(); }
        }

        public string CurrentCategory
        {
            get => _currentCategory;
            set { _currentCategory = value; OnPropertyChanged(); }
        }
        public int TimeLeft
        {
            get => _timeLeft;
            set { _timeLeft = value; OnPropertyChanged(); }
        }

        public bool IsAllCategories { get => _isAllCategories; set { _isAllCategories = value; OnPropertyChanged(); } }
        private bool _isAllCategories = true;
        public bool IsCars { get => _isCars; set { _isCars = value; OnPropertyChanged(); } }
        private bool _isCars;
        public bool IsMovies { get => _isMovies; set { _isMovies = value; OnPropertyChanged(); } }
        private bool _isMovies;
        public bool IsRivers { get => _isRivers; set { _isRivers = value; OnPropertyChanged(); } }
        private bool _isRivers;
        public bool IsCountries { get => _isCountries; set { _isCountries = value; OnPropertyChanged(); } }
        private bool _isCountries;
        public bool IsFlowers { get => _isFlowers; set { _isFlowers = value; OnPropertyChanged(); } }
        private bool _isFlowers;
        public bool IsInstruments { get => _isInstruments; set { _isInstruments = value; OnPropertyChanged(); } }
        private bool _isInstruments;

        public ICommand CategoryCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand GuessCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand LoadGameCommand { get; }

        public GameViewModel(UserModel player)
        {
            CurrentPlayer = player;

            CategoryCommand = new RelayCommand(ChangeCategory);
            NewGameCommand = new RelayCommand(ResetToZeroAndStart);
            GuessCommand = new RelayCommand(GuessLetter, CanGuessLetter);
            AboutCommand = new RelayCommand(OnAbout);
            SaveGameCommand = new RelayCommand(OnSaveGame);
            StatisticsCommand = new RelayCommand(OnStatistics);
            LoadGameCommand = new RelayCommand(OnLoadGame);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            LoadWords();
            StartNewGame(null);
        }

        private List<UserModel> GetAllUsers()
        {
            var users = new List<UserModel>();
            if (!File.Exists(UsersFile)) return users;

            var lines = File.ReadAllLines(UsersFile);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    var user = new UserModel
                    {
                        Username = parts[0],
                        ImagePath = parts[1],
                        CurrentLevel = (parts.Length >= 3) ? int.Parse(parts[2]) : 0
                    };

                    if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]))
                    {
                        var categories = parts[3].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var catData in categories)
                        {
                            var details = catData.Split(':');
                            if (details.Length == 2)
                            { 
                                var scores = details[1].Split(',');
                                if (scores.Length == 2)
                                {
                                    user.CategoryStats[details[0]] = new CategoryStats
                                    {
                                        Played = int.Parse(scores[0]),
                                        Won = int.Parse(scores[1])
                                    };
                                }
                            }
                        }
                    }

                    users.Add(user);
                }
            }
            return users;
        }

        private void UpdateUserStatsInFile(bool isWin)
        {
            var allUsers = GetAllUsers();
            var currentUser = allUsers.FirstOrDefault(u => u.Username == CurrentPlayer.Username);

            if (currentUser != null)
            {
                if (!currentUser.CategoryStats.ContainsKey(_currentCategory))
                    currentUser.CategoryStats[_currentCategory] = new CategoryStats();

                currentUser.CategoryStats[_currentCategory].Played++;
                if (isWin) currentUser.CategoryStats[_currentCategory].Won++;

                currentUser.CurrentLevel = CurrentPlayer.CurrentLevel;

                var lines = allUsers.Select(u => {
                    string statsStr = string.Join(";", u.CategoryStats.Select(kvp => $"{kvp.Key}:{kvp.Value.Played},{kvp.Value.Won}"));
                    return $"{u.Username}|{u.ImagePath}|{u.CurrentLevel}|{statsStr}";
                });

                File.WriteAllLines(UsersFile, lines);
            }
        }

        private void LoadWords()
        {
            _wordsByCategory = new Dictionary<string, List<string>>();
            if (!File.Exists(WordsFile)) return;
            var lines = File.ReadAllLines(WordsFile);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    string cat = parts[0].Trim();
                    string word = parts[1].Trim().ToUpper();
                    if (!_wordsByCategory.ContainsKey(cat)) _wordsByCategory[cat] = new List<string>();
                    _wordsByCategory[cat].Add(word);
                }
            }
        }

        private void ChangeCategory(object parameter)
        {
            if (parameter is string category)
            {
                CurrentCategory = category; 
                IsAllCategories = IsCars = IsMovies = IsRivers = IsCountries = IsFlowers = IsInstruments = false;
                CurrentPlayer.CurrentLevel = 0;

                switch (category)
                {
                    case "All categories": IsAllCategories = true; break;
                    case "Cars": IsCars = true; break;
                    case "Movies": IsMovies = true; break;
                    case "Rivers": IsRivers = true; break;
                    case "Countries": IsCountries = true; break;
                    case "Flowers": IsFlowers = true; break;
                    case "Instruments": IsInstruments = true; break;
                }
                _usedWords.Clear();
                StartNewGame(null);
            }
        }

        private void StartNewGame(object obj)
        {
           
            _timer.Stop(); 
            _mistakes = 0;
            _guessedLetters = new List<char>();
            CurrentHangmanImage = "/Images/hg0.png";
            TimeLeft = 30;

            List<string> wordPool = new List<string>();
            if (_currentCategory == "All categories")
            {
                foreach (var list in _wordsByCategory.Values) wordPool.AddRange(list);
            }
            else if (_wordsByCategory.ContainsKey(_currentCategory))
            {
                wordPool = _wordsByCategory[_currentCategory];
            }

            var availableWords = wordPool.Where(w => !_usedWords.Contains(w)).ToList();
            if (availableWords.Count == 0)
            {
                _usedWords.Clear();
                availableWords = wordPool;
            }

            if (availableWords.Count > 0)
            {
                _wordToGuess = availableWords[_random.Next(availableWords.Count)];
                _usedWords.Add(_wordToGuess);
                UpdateDisplayedWord();
            }

            _timer.Start();
            CommandManager.InvalidateRequerySuggested();
        }

        private void GuessLetter(object parameter)
        {
            if (string.IsNullOrEmpty(_wordToGuess)) return;
            if (parameter is string letterStr && letterStr.Length == 1)
            {
                char letter = letterStr[0];
                _guessedLetters.Add(letter);
                CommandManager.InvalidateRequerySuggested();

                if (_wordToGuess.Contains(letter))
                {
                    UpdateDisplayedWord();
                    CheckWin();
                }
                else
                {
                    _mistakes++;
                    CurrentHangmanImage = $"/Images/hg{_mistakes}.png";
                    CheckLoss();
                }
            }
        }

        private bool CanGuessLetter(object parameter)
        {
            if (string.IsNullOrEmpty(_wordToGuess)) return false;
            if (parameter is string letterStr && letterStr.Length == 1 && _guessedLetters != null)
                return !_guessedLetters.Contains(letterStr[0]);
            return false;
        }

        private void UpdateDisplayedWord()
        {
            if (string.IsNullOrEmpty(_wordToGuess)) return;
            var display = _wordToGuess.Select(c => _guessedLetters.Contains(c) ? c : '_');
            DisplayedWord = string.Join(" ", display);
        }

        private void CheckWin()
        {
            if (!DisplayedWord.Contains('_'))
            {
                _timer.Stop();
                CurrentPlayer.CurrentLevel++;

                if (CurrentPlayer.CurrentLevel >= 3)
                {
                    UpdateUserStatsInFile(true);
                    CurrentPlayer.CurrentLevel = 0;
                    MessageBox.Show($"Congratulation! You guessed 3 words", "Game Won");
                }
                StartNewGame(null);
            }
        }

        private void CheckLoss()
        {
            if (_mistakes >= 6)
            {
                _timer.Stop();

                UpdateUserStatsInFile(false); 
                CurrentPlayer.CurrentLevel = 0; 

                string revealed = _wordToGuess;
                _wordToGuess = "";
                CommandManager.InvalidateRequerySuggested();

                MessageBox.Show($"You lost! The series was interrupted. The word was: {revealed}", "Game Over");
                StartNewGame(null);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_wordToGuess))
            {
                _timer.Stop();
                return;
            }

            if (TimeLeft > 0)
            {
                TimeLeft--;
            }
            else 
            {
                _timer.Stop();

                string wordToDisplay = _wordToGuess;
                UpdateUserStatsInFile(false);
                CurrentPlayer.CurrentLevel = 0;

                MessageBox.Show($"Time over! The word was : {wordToDisplay}", "Game Over");

                StartNewGame(null);
            }
        }
        public void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        private void ResetToZeroAndStart(object obj)
        {
            CurrentPlayer.CurrentLevel = 0;
            _usedWords.Clear();
            StartNewGame(null);
        }

        private void OnAbout(object obj)
        {
            new AboutWindow().ShowDialog();
        }

        private void OnSaveGame(object obj)
        {
            string customName = Microsoft.VisualBasic.Interaction.InputBox("Name", "Save Game", $"Save_{_currentCategory}");
            if (string.IsNullOrEmpty(customName)) return;

            try
            {
                var save = new GameSave
                {
                    SaveName = $"{CurrentPlayer.Username}_{customName}",
                    PlayerUsername = CurrentPlayer.Username,
                    Category = _currentCategory,
                    WordToGuess = _wordToGuess,
                    GuessedLetters = new List<char>(_guessedLetters),
                    Mistakes = _mistakes,
                    TimeLeft = TimeLeft,
                    Level = CurrentPlayer.CurrentLevel,
                    SaveDate = DateTime.Now
                };
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, $"{save.SaveName}.json"), JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("Game saved!");
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void OnLoadGame(object obj)
        {
            _timer.Stop();
            var loadWin = new LoadGameWindow(CurrentPlayer.Username);
            if (loadWin.ShowDialog() == true)
            {
                var save = loadWin.SelectedSave;

                _currentCategory = save.Category;
                _wordToGuess = save.WordToGuess;
                _guessedLetters = save.GuessedLetters;
                _mistakes = save.Mistakes;
                TimeLeft = save.TimeLeft;
                CurrentPlayer.CurrentLevel = save.Level;

                CurrentHangmanImage = $"/Images/hg{_mistakes}.png";
                UpdateDisplayedWord();
                UpdateCategoryChecks(_currentCategory);

                _timer.Start();
                MessageBox.Show("Game loaded successfully!");
            }
            else
                _timer.Start();

        }

        private void UpdateCategoryChecks(string category)
        {
            IsAllCategories = IsCars = IsMovies = IsRivers = IsCountries = IsFlowers = IsInstruments = false;
            if (category == "All categories") IsAllCategories = true;
            else if (category == "Cars") IsCars = true;
            else if (category == "Movies") IsMovies = true;
            else if (category == "Rivers") IsRivers = true;
            else if (category == "Countries") IsCountries = true;
            else if (category == "Flowers") IsFlowers = true;
            else if (category == "Instruments") IsInstruments = true;
        }

        private void OnStatistics(object obj)
        {
            var allUsers = GetAllUsers();
            StatisticsWindow statsWin = new StatisticsWindow(allUsers, CurrentPlayer.Username);

            statsWin.ShowDialog();
        }
    }
}