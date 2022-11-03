using LLASDecryptor.Core;
using LLASDecryptor.Interface.Properties;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LLASDecryptor.Interface
{
    public partial class MainWindow : Window
    {
        public readonly DependencyProperty InputProp = DependencyProperty.Register(nameof(InputPath), typeof(string), typeof(MainWindow));
        public readonly DependencyProperty OutputProp = DependencyProperty.Register(nameof(OutputPath), typeof(string), typeof(MainWindow));
        public readonly DependencyProperty PlayerPrefsProp = DependencyProperty.Register(nameof(PlayerPrefsKey), typeof(string), typeof(MainWindow));

        public string InputPath { get => (string)GetValue(InputProp); set => SetValue(InputProp, value); }
        public string OutputPath { get => (string)GetValue(OutputProp); set => SetValue(OutputProp, value); }
        public string PlayerPrefsKey { get => (string)GetValue(PlayerPrefsProp); set => SetValue(PlayerPrefsProp, value); }

        private List<string> fileTables = new List<string>();

        public MainWindow()
        {
            LoadSettings();
            InitializeComponent();
            DecryptProgress.Value = 0;
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            DatabaseTables.Tables
                .Select(t => t.DisplayName)
                .OrderBy(t => t)
                .ToList().
                ForEach(t => TablesList.Items.Add(t));
            dispatcherTimer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        void LoadSettings()
        {
            InputPath = Settings.Default.InputFiles;
            OutputPath = Settings.Default.OutputFiles;
            PlayerPrefsKey = Settings.Default.PlayerPrefsKey;
        }

        void SaveSettings()
        {
            Settings.Default.InputFiles = InputPath;
            Settings.Default.OutputFiles = OutputPath;
            Settings.Default.PlayerPrefsKey = PlayerPrefsKey;
            Settings.Default.Save();
        }

        private async void FileButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = BuildDecryptor();
            decryptor.ProgressChanged += ProgressChanged;
            decryptor.ConsoleLog += LogConsole;

            var listBoxItems = new List<string>(TablesList.SelectedItems.Cast<string>());
            fileTables = listBoxItems;

            if(fileTables.Count == 0)
            {
                LogConsole("No tables selected to decrypt");
                return;
            }


            int tablesCompleted = 0;
            int totalTables = fileTables.Count;
            UpdateOverallProgress(tablesCompleted, totalTables);
            foreach (var tableDisplayName in fileTables)
            {
                ProgressText.Text = $"Processing '{tableDisplayName}'";

                var table = DatabaseTables.Tables.Single(t => t.DisplayName == tableDisplayName);
                try
                {
                    await decryptor.DecryptFiles(table);
                }
                catch(Exception ex)
                {
                    LogConsole(ex.Message + ":" + ex.StackTrace);
                }
                tablesCompleted++;
                UpdateOverallProgress(tablesCompleted, totalTables);
            }
            ProgressText.Text = "Completed!";
        }

        private void LogConsole(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleUI.Text += $"\n{message}";
            });
        }

        private void ProgressChanged(double progress)
        {
            Dispatcher.Invoke(() =>
            {
                DecryptProgress.Value = progress;
            });
        }

        private void UpdateOverallProgress(int tablesCompleted, int totalTables)
        {
            OverallProgress.Value = (double)tablesCompleted / totalTables;
        }

        private Decryptor BuildDecryptor()
        {
            return new Decryptor(InputPath, OutputPath, PlayerPrefsKey);
        }

        private void InputFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Input Folder";
            dialog.IsFolderPicker = true;
            dialog.ShowDialog();
            InputPath = dialog.FileName;
        }

        private void OutputFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Output Folder";
            dialog.IsFolderPicker = true;
            dialog.ShowDialog();
            OutputPath = dialog.FileName;
        }
    }
}