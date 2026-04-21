using System;
using System.Collections.Generic;

namespace HangmanGame.Models
{
    [Serializable] //permite salvarea obiectului pe disc
    public class GameSave
    {
        public string SaveName { get; set; }
        public string PlayerUsername { get; set; }
        public string Category { get; set; }
        public string WordToGuess { get; set; }
        public List<char> GuessedLetters { get; set; }
        public int Mistakes { get; set; }
        public int TimeLeft { get; set; }
        public int Level { get; set; }
        public DateTime SaveDate { get; set; }
    }
}