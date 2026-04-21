using HangmanGame.Models;
using HangmanGame.View; // Asigură-te că namespace-ul pentru AboutWindow este corect
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HangmanGame.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private UserModel _currentPlayer;
        private const string WordsFile = "words.txt";
        private Dictionary<string, List<string>> _wordsByCategory;
        private string _currentCategory = "All categories";
        private string _wordToGuess;
        private Random _random = new Random();
        private int _mistakes;
        private List<char> _guessedLetters;
        private int _consecutiveWins = 0; // Pentru cerința cu 3 cuvinte ghicite

        private string _displayedWord;
        public string DisplayedWord
        {
            get => _displayedWord;
            set { _displayedWord = value; OnPropertyChanged(); }
        }

        public UserModel CurrentPlayer
        {
            get => _currentPlayer;
            set { _currentPlayer = value; OnPropertyChanged(); }
        }


        private string _currentHangmanImage;
        public string CurrentHangmanImage
        {
            get => _currentHangmanImage;
            set { _currentHangmanImage = value; OnPropertyChanged(); }
        }

        // Proprietăți pentru bifele din meniu
        private bool _isAllCategories = true;
        public bool IsAllCategories { get => _isAllCategories; set { _isAllCategories = value; OnPropertyChanged(); } }

        private bool _isCars;
        public bool IsCars { get => _isCars; set { _isCars = value; OnPropertyChanged(); } }

        private bool _isMovies;
        public bool IsMovies { get => _isMovies; set { _isMovies = value; OnPropertyChanged(); } }

        private bool _isRivers;
        public bool IsRivers { get => _isRivers; set { _isRivers = value; OnPropertyChanged(); } }

        private bool _isCountries;
        public bool IsCountries { get => _isCountries; set { _isCountries = value; OnPropertyChanged(); } }

        private bool _isFlowers;
        public bool IsFlowers { get => _isFlowers; set { _isFlowers = value; OnPropertyChanged(); } }

        private bool _isInstruments;
        public bool IsInstruments { get => _isInstruments; set { _isInstruments = value; OnPropertyChanged(); } }

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
            NewGameCommand = new RelayCommand(StartNewGame);
            GuessCommand = new RelayCommand(GuessLetter, CanGuessLetter);
            AboutCommand = new RelayCommand(OnAbout);
            SaveGameCommand = new RelayCommand(OnSaveGame);
            StatisticsCommand = new RelayCommand(OnStatistics);

            LoadWords();
            StartNewGame(null);
        }

        private void LoadWords()
        {
            _wordsByCategory = new Dictionary<string, List<string>>();

            if (!File.Exists(WordsFile))
            {
                // Creează un fișier default dacă lipsește pentru a evita crash-ul la prima rulare
                File.WriteAllLines(WordsFile, new[] { "Cars|DACIA", "Movies|AVATAR", "Rivers|DUNARE" });
            }

            var lines = File.ReadAllLines(WordsFile);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    string cat = parts[0].Trim();
                    string word = parts[1].Trim().ToUpper();

                    if (!_wordsByCategory.ContainsKey(cat))
                        _wordsByCategory[cat] = new List<string>();

                    _wordsByCategory[cat].Add(word);
                }
            }
        }

        private void ChangeCategory(object parameter)
        {
            if (parameter is string category)
            {
                _currentCategory = category;

                IsAllCategories = IsCars = IsMovies = IsRivers = false;
                IsCountries = IsFlowers = IsInstruments = false;

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
                StartNewGame(null);
            }
        }

        private void StartNewGame(object obj)
        {
            _mistakes = 0;
            _guessedLetters = new List<char>();
            List<string> wordPool = new List<string>();
            CurrentHangmanImage = "/Images/hg0.png";

            CommandManager.InvalidateRequerySuggested();

            if (_currentCategory == "All categories")
            {
                foreach (var list in _wordsByCategory.Values)
                    wordPool.AddRange(list);
            }
            else if (_wordsByCategory.ContainsKey(_currentCategory))
            {
                wordPool = _wordsByCategory[_currentCategory];
            }

            if (wordPool.Count > 0)
            {
                _wordToGuess = wordPool[_random.Next(wordPool.Count)];
                UpdateDisplayedWord();
            }
        }

        private void GuessLetter(object parameter)
        {
            if (parameter is string letterStr && letterStr.Length == 1)
            {
                char letter = letterStr[0];
                if (_guessedLetters.Contains(letter)) return;

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
            if (parameter is string letterStr && letterStr.Length == 1 && _guessedLetters != null)
            {
                return !_guessedLetters.Contains(letterStr[0]);
            }
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
                _consecutiveWins++;
                if (_consecutiveWins >= 3)
                {
                    MessageBox.Show($"Felicitări! Ai câștigat nivelul ghicind 3 cuvinte la rând!", "Victorie Totală");
                    _consecutiveWins = 0;
                }
                else
                {
                    MessageBox.Show($"Bravo! Mai ai {3 - _consecutiveWins} cuvinte de ghicit pentru a termina jocul.", "Cuvânt Ghicit");
                }
                StartNewGame(null);
            }
        }

        private void CheckLoss()
        {
            if (_mistakes >= 6)
            {
                _consecutiveWins = 0; // Resetăm progresul la pierdere
                MessageBox.Show($"Ai pierdut! Cuvântul era: {_wordToGuess}", "Game Over");
                StartNewGame(null);
            }
        }

        private void OnAbout(object obj)
        {

            AboutWindow aboutWin = new AboutWindow();
            aboutWin.ShowDialog();
        }

        private void OnSaveGame(object obj)
        {
            MessageBox.Show("Funcția de salvare va folosi serializarea pentru utilizatorul " + CurrentPlayer.Username);
        }

        private void OnStatistics(object obj)
        {
            MessageBox.Show("Afișare statistici din fișier.");
        }
    }
}