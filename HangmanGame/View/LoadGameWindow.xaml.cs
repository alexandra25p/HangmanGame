using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using HangmanGame.Models;

namespace HangmanGame.View
{
    public partial class LoadGameWindow : Window
    {
        public GameSave SelectedSave { get; private set; }
        private string _username;

        public LoadGameWindow(string username)
        {
            InitializeComponent();
            _username = username;
            LoadSavesList();
        }

        private void LoadSavesList()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            if (!Directory.Exists(directory)) return;

            var files = Directory.GetFiles(directory, $"{_username}_*.json");
            var saveList = new List<SaveFile>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var displayName = fileName.Replace($"{_username}_", "").Replace(".json", "");

                saveList.Add(new SaveFile
                {
                    FileName = file,
                    DisplayName = displayName,
                    SaveDate = File.GetCreationTime(file).ToString("dd/MM/yyyy HH:mm")
                });
            }

            SavesListBox.ItemsSource = saveList.OrderByDescending(s => s.SaveDate).ToList();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (SavesListBox.SelectedItem is SaveFile selected)
            {
                try
                {
                    string jsonString = File.ReadAllText(selected.FileName);
                    SelectedSave = JsonSerializer.Deserialize<GameSave>(jsonString);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la încărcarea fișierului: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Te rugăm să selectezi o salvare!");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}