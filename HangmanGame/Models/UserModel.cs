using System;
using System.IO;

namespace HangmanGame.Models
{
    public class UserModel
    {
        public string Username { get; set; } 
        public string ImagePath { get; set; } //cale relativa
        public string FullImagePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath);
    }
}