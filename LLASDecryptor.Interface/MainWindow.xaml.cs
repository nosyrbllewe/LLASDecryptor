using LLASDecryptor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LLASDecryptor.Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string InputPath { get; set; } = @"L:\SIFAS\Memu\SIFAS\files";

        public string OutputPath { get; set; } = @"L:\SIFAS\output";

        private double currentProgress = 0;

        private object progressLock = new object();

        private List<string> fileTables = new List<string>()
        {
            "member_model",
            "navi_motion",
            "navi_timeline",
            "live_timeline",
            "stage",
            "stage_effect",
            "member_facial_animation",
            "member_facial",
        };

        public MainWindow()
        {
            InitializeComponent();
            DecryptProgress.Value = 0;
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            DatabaseTables.Tables
                .Select(t => t.TableName)
                .OrderBy(t => t)
                .ToList().
                ForEach(t => TablesList.Items.Add(t));
            dispatcherTimer.Start();
        }

        private async void FileButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = new Decryptor(InputPath, OutputPath);
            decryptor.ProgressChanged += ProgressChanged;

            var listBoxItems = new List<string>(TablesList.SelectedItems.Cast<string>());
            fileTables = listBoxItems;//.Select(lb => lb..ToString()).ToList();// DatabaseTables.Tables.Select(t => t.TableName).ToList();

            int tablesCompleted = 0;
            int totalTables = fileTables.Count;
            UpdateOverallProgress(tablesCompleted, totalTables);
            foreach (var table in fileTables)
            {
                ProgressText.Text = $"Processing '{table}'";
                await decryptor.DecryptFiles(table);
                tablesCompleted++;
                UpdateOverallProgress(tablesCompleted, totalTables);
            }
            ProgressText.Text = "Completed!";
        }

        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = new Decryptor(InputPath, OutputPath);
            decryptor.DecryptDatabase();
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
    }
}