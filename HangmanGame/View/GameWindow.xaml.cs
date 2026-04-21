using System;
using System.Windows;
using System.Windows.Input;
using HangmanGame.ViewModels;

namespace HangmanGame.View
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is GameViewModel vm)
            {
                vm.StopTimer();
            }
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                string pressedLetter = e.Key.ToString();

                if (this.DataContext is GameViewModel vm)
                {
                    if (vm.GuessCommand != null && vm.GuessCommand.CanExecute(pressedLetter))
                    {
                        vm.GuessCommand.Execute(pressedLetter);
                    }
                }
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.DataContext is GameViewModel vm)
            {
                vm.StopTimer(); 
            }
        }
    }
}