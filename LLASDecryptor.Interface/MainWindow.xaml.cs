using LLASDecryptor.Core;
using System;
using System.Windows;

namespace LLASDecryptor.Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string InputPath { get; set; } = @"L:\SIFAS\Memu\files";

        public string OutputPath { get; set; } = @"L:\SIFAS\output";

        private double currentProgress = 0;

        private object progressLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            DecryptProgress.Value = 0;
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private async void FileButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = new Decryptor(InputPath, OutputPath);
            decryptor.ProgressChanged += ProgressChanged;
            await decryptor.DecryptFiles("member_model");
            //decryptor.DecryptFiles("navi_motion");
            //decryptor.DecryptFiles("navi_timeline");
            //decryptor.DecryptFiles("live_timeline");
            //decryptor.DecryptFiles("stage");
            //decryptor.DecryptFiles("stage_effect");
            //decryptor.DecryptFiles("member_facial_animation");
            //decryptor.DecryptFiles("member_facial");
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
    }
}