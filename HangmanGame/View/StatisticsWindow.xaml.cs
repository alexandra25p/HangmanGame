using HangmanGame.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HangmanGame.View
{
    public partial class StatisticsWindow : Window
    {
        private List<UserModel> _allUsers;
        private string _loggedUser;

        public StatisticsWindow(List<UserModel> users, string currentUsername)
        {
            InitializeComponent();
            _allUsers = users;
            _loggedUser = currentUsername;

            LoadCategories();
            ShowAllData();
        }

        private void LoadCategories()
        {
            var categories = _allUsers
                .SelectMany(u => u.CategoryStats.Keys)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            categories.Insert(0, "All"); 
            CategoryFilterCombo.ItemsSource = categories;
            CategoryFilterCombo.SelectedIndex = 0; 
        }

        private void ShowAllData()
        {
            var displayData = _allUsers.SelectMany(u => u.CategoryStats.Select(s => new
            {
                User = u.Username,
                Category = s.Key,
                Played = s.Value.Played,
                Won = s.Value.Won,
                WinPercent = s.Value.WinRate
            })).ToList();

            StatsDataGrid.ItemsSource = displayData;
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilterCombo.SelectedItem is string selectedCat)
            {
                if (selectedCat == "All")
                {
                    ShowAllData();
                    return;
                }

                var filtered = _allUsers.SelectMany(u => u.CategoryStats
                    .Where(s => s.Key == selectedCat)
                    .Select(s => new
                    {
                        User = u.Username,
                        Category = s.Key,
                        Played = s.Value.Played,
                        Won = s.Value.Won,
                        WinPercent = s.Value.WinRate
                    })).ToList();

                StatsDataGrid.ItemsSource = filtered;
            }
        }

        private void MyStats_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilterCombo.SelectedIndex = -1; 

            var user = _allUsers.FirstOrDefault(u => u.Username == _loggedUser);
            if (user != null)
            {
                var myData = user.CategoryStats.Select(s => new
                {
                    User = user.Username,
                    Category = s.Key,
                    Played = s.Value.Played,
                    Won = s.Value.Won,
                    WinPercent = s.Value.WinRate
                }).ToList();

                StatsDataGrid.ItemsSource = myData;
            }
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilterCombo.SelectedIndex = 0;
            ShowAllData();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}