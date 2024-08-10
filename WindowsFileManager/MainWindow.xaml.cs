using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace WindowsFileManager
{
    public partial class MainWindow : Window
    {
        private FileWatcher fileWatcher;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                PathTextBox.Text = dialog.SelectedPath;
                fileWatcher = new FileWatcher(dialog.SelectedPath);
                fileWatcher.OnFileChanged += (s, ev) => Dispatcher.Invoke(() =>
                {
                    LogListBox.Items.Add($"{DateTime.Now}: {ev.ChangeType} - {ev.FullPath}");
                });

                UpdateFileCount();
            }
        }

        private void UpdateFileCount()
        {
            if (fileWatcher == null || string.IsNullOrWhiteSpace(fileWatcher.Path))
            {
                FileCountLabel.Content = "Silinecek Dosya Sayısı: 0";
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var filesToDelete = Directory.GetFiles(fileWatcher.Path)
                                          .Where(file => File.GetLastWriteTime(file) >= startDate && File.GetLastWriteTime(file) <= endDate)
                                          .ToList();

            FileCountLabel.Content = $"Silinecek Dosya Sayısı: {filesToDelete.Count}";
        }

        private void FilterFiles_Click(object sender, RoutedEventArgs e)
        {
            if (fileWatcher == null)
            {
                MessageBox.Show("Lütfen önce bir yol seçin.");
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            if (startDate > endDate)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
                return;
            }

            var fileNameFilter = FileNameFilterTextBox.Text.Trim();

            var filteredFiles = Directory.GetFiles(fileWatcher.Path)
                                         .Where(file => File.GetLastWriteTime(file) >= startDate && File.GetLastWriteTime(file) <= endDate)
                                         .Where(file => string.IsNullOrEmpty(fileNameFilter) || Path.GetFileName(file).IndexOf(fileNameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                                         .ToList();

            LogListBox.Items.Clear();
            foreach (var file in filteredFiles)
            {
                LogListBox.Items.Add(file);
            }

            FileCountLabel.Content = $"Silinecek Dosya Sayısı: {filteredFiles.Count}";
        }

        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (fileWatcher == null)
            {
                MessageBox.Show("Lütfen önce bir yol seçin.");
                return;
            }

            var filteredFiles = LogListBox.Items.Cast<string>().ToList();

            if (filteredFiles.Count == 0)
            {
                MessageBox.Show("Silinecek dosya bulunamadı.");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Bu işlem {filteredFiles.Count} dosyayı silecek. Devam etmek istediğinize emin misiniz?",
                "Dosyaları Sil",
                MessageBoxButton.YesNo);

            if (confirmResult == MessageBoxResult.Yes)
            {
                foreach (var file in filteredFiles)
                {
                    try
                    {
                        File.Delete(file);
                        DatabaseLogger.Log("Deleted", file, Environment.UserName, "Dosya başarıyla silindi.", "Info");
                    }
                    catch (Exception ex)
                    {
                        DatabaseLogger.Log("Error", file, Environment.UserName, ex.Message, "Error");
                    }
                }

                MessageBox.Show("Dosyalar başarıyla silindi.");
                FilterFiles_Click(sender, e); // Silme sonrası listeyi güncelle
            }
        }
    }
}
