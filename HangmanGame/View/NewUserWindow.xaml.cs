using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HangmanGame.Models;

namespace HangmanGame.View
{
    public partial class NewUserWindow : Window
    {
        public UserModel CreatedUser { get; private set; }
        private string selectedAbsPath = null;
        private string predefinedSelection = null;

        public NewUserWindow()
        {
            InitializeComponent();
            PopulatePredefinedImages();
        }

        private void PopulatePredefinedImages()
        {
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(imagesFolder)) 
                return;

            //poze predefinite
            string[] fileNames = {
                "lamb.jpg",
                "leu.jpg",
                "panda.png",
                "rabbit.jpg",
                "shark.png",
                "bird.png",
                "dog.png",
                "fox.png",
                "hamster.png"

            }; 

            var fullPaths = new List<string>();

            //construim calea completa ca sa recunoasca wpf
            foreach (var name in fileNames)
            {
                string path = Path.Combine(imagesFolder, name);
                if (File.Exists(path))
                {
                    fullPaths.Add(path);
                }
            }

            LstPredefined.ItemsSource = fullPaths;
        }

        private void LstPredefined_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPredefined.SelectedItem != null)
            {
                string fullPath = LstPredefined.SelectedItem.ToString();
                predefinedSelection = Path.GetFileName(fullPath);

                selectedAbsPath = null;
                TxtPath.Text = "Selected avatar: " + predefinedSelection;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Images JPG/GIF|*.jpg;*.jpeg;*.gif;*.png" };
            if (dlg.ShowDialog() == true)
            {
                selectedAbsPath = dlg.FileName;
                predefinedSelection = null;
                LstPredefined.SelectedItem = null;

                TxtPath.Text = "External path: " + selectedAbsPath;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || username.Contains(" "))
            {
                MessageBox.Show("Name must be a single word!"); return;
            }

            string relativePathToSave = "";
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            if (!string.IsNullOrEmpty(selectedAbsPath))
            {
                string fileName = Path.GetFileName(selectedAbsPath);
                string destinationPath = Path.Combine(imagesFolder, fileName);
                File.Copy(selectedAbsPath, destinationPath, true);
                relativePathToSave = Path.Combine("Images", fileName);
            }
            else if (!string.IsNullOrEmpty(predefinedSelection))
            {
                relativePathToSave = Path.Combine("Images", predefinedSelection);
            }
            else
            {
                MessageBox.Show("Please select an image!"); return;
            }

            CreatedUser = new UserModel { Username = username, ImagePath = relativePathToSave };
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}