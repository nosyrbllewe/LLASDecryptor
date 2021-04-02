using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LLASDecryptor;
using LLASDecryptor.Core;

namespace LLASDecryptor.Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string InputPath { get; set; } = @"X:\SIFAS\files";

        public string OutputPath { get; set; } = @"X:\SIFAS\output";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = new Decryptor(InputPath, OutputPath);
            decryptor.DecryptFiles("member_model");
        }

        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            Decryptor decryptor = new Decryptor(InputPath, OutputPath);
            decryptor.DecryptDatabase();
        }
    }
}