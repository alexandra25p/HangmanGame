using HangmanGame.Models;
using HangmanGame.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HangmanGame.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private UserModel _currentPlayer;
        private const string WordsFile = "words.txt";
        private Dictionary<string, List<string>> _wordsByCategory;
        private List<string> _usedWords = new List<string>();
        private string _currentCategory = "All categories";
        private string _wordToGuess = "";
        private Random _random = new Random();
        private int _mistakes;
        private List<char> _guessedLetters;
        private int _consecutiveWins = 0;
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

        public GameViewModel(UserModel player)
        {
            CurrentPlayer = player;

            CategoryCommand = new RelayCommand(ChangeCategory);
            NewGameCommand = new RelayCommand(ResetToZeroAndStart);
            GuessCommand = new RelayCommand(GuessLetter, CanGuessLetter);
            AboutCommand = new RelayCommand(OnAbout);
            SaveGameCommand = new RelayCommand(OnSaveGame);
            StatisticsCommand = new RelayCommand(OnStatistics);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            LoadWords();
            StartNewGame(null);
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TimeLeft > 1)
            {
                TimeLeft--;
            }
            else
            {
                TimeLeft = 0;
                _timer.Stop();

                // Înregistrăm jocul jucat
                CurrentPlayer.GamesPlayed++;

                // RESETARE CONFORM CERINTEI: Pierdem seria si nivelul
                CurrentPlayer.CurrentLevel = 0;
                _consecutiveWins = 0;

                string revealedWord = _wordToGuess;
                _wordToGuess = "";
                CommandManager.InvalidateRequerySuggested();

                MessageBox.Show($"Timpul a expirat! Nivelul a fost resetat la 0.\nCuvântul era: {revealedWord}", "Game Over");
                StartNewGame(null);
            }
        }

        private void ChangeCategory(object parameter)
        {
            if (parameter is string category)
            {
                _currentCategory = category;
                IsAllCategories = IsCars = IsMovies = IsRivers = IsCountries = IsFlowers = IsInstruments = false;

                // Resetare nivel la schimbarea categoriei
                CurrentPlayer.CurrentLevel = 0;
                _consecutiveWins = 0;

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
            _mistakes = 0;
            _guessedLetters = new List<char>();
            CurrentHangmanImage = "/Images/hg0.png";
            TimeLeft = 30;
            _timer.Start();

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

                // Actualizăm statistica globală (nu se resetează niciodată)
                CurrentPlayer.GamesWon++;
                CurrentPlayer.GamesPlayed++;

                _consecutiveWins++;

                // Creștem nivelul la fiecare 3 victorii consecutive
                if (_consecutiveWins > 0 && _consecutiveWins % 3 == 0)
                {
                    CurrentPlayer.CurrentLevel++;
                    MessageBox.Show($"Nivel Nou! Esti la nivelul {CurrentPlayer.CurrentLevel}!", "Level Up");
                }
                else
                {
                    MessageBox.Show($"Cuvânt ghicit! Seria curentă: {_consecutiveWins % 3}/3 victorii.", "Bravo");
                }

                StartNewGame(null);
            }
        }

        private void CheckLoss()
        {
            if (_mistakes >= 6)
            {
                _timer.Stop();

                // Înregistrăm jocul în statistici
                CurrentPlayer.GamesPlayed++;

                // RESETARE CONFORM CERINTEI: Pierdem seria si nivelul la greseala fatală
                CurrentPlayer.CurrentLevel = 0;
                _consecutiveWins = 0;

                string revealed = _wordToGuess;
                _wordToGuess = "";
                CommandManager.InvalidateRequerySuggested();

                MessageBox.Show($"Ai pierdut! Nivelul a fost resetat la 0.\nCuvântul era: {revealed}", "Game Over");
                StartNewGame(null);
            }
        }

        private void ResetToZeroAndStart(object obj)
        {
            // Resetare manuală din butonul "New Game"
            CurrentPlayer.CurrentLevel = 0;
            _consecutiveWins = 0;
            _usedWords.Clear();
            StartNewGame(null);
        }

        private void OnAbout(object obj)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.ShowDialog();
        }

        private void OnSaveGame(object obj)
        {
            // Aici se va implementa serializarea profilului în fișier
            MessageBox.Show("Jocul a fost salvat!", "Save");
        }

        private void OnStatistics(object obj)
        {
            // Aici se va deschide fereastra de statistici
            MessageBox.Show($"Statistici pentru {CurrentPlayer.Username}:\n" +
                            $"- Victorii totale: {CurrentPlayer.GamesWon}\n" +
                            $"- Jocuri jucate: {CurrentPlayer.GamesPlayed}", "Statistici");
        }
    }
}