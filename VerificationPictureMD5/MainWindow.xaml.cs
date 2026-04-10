using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace VerificationPictureMD5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        public static MainWindow Current { get; private set; }

        /// <summary>
        /// Collection of image information items
        /// </summary>
        public ObservableCollection<ImageInfo> ImageItems { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Current = this;
            ImageItems = new ObservableCollection<ImageInfo>();
            ImagesDataGrid.ItemsSource = ImageItems;
            
            LoadSettings();
            DataContext = _settings;
        }

        /// <summary>
        /// Loads application settings and restores window state
        /// </summary>
        private void LoadSettings()
        {
            _settings = App.LoadSettings();
            
            // Restore window position and size
            Left = _settings.WindowLeft;
            Top = _settings.WindowTop;
            Width = _settings.WindowWidth;
            Height = _settings.WindowHeight;
            WindowState = _settings.WindowState;
            
            // Restore last directory
            DirectoryTextBox.Text = _settings.LastDirectory;
            
            // Restore splitter position after window is loaded
            Loaded += (s, e) => RestoreSplitterPosition();
        }

        /// <summary>
        /// Saves current window settings
        /// </summary>
        public void SaveWindowSettings()
        {
            if (WindowState == WindowState.Normal)
            {
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                _settings.WindowWidth = Width;
                _settings.WindowHeight = Height;
            }
            else
            {
                // If maximized, save the restore bounds
                _settings.WindowLeft = RestoreBounds.Left;
                _settings.WindowTop = RestoreBounds.Top;
                _settings.WindowWidth = RestoreBounds.Width;
                _settings.WindowHeight = RestoreBounds.Height;
            }
            
            _settings.WindowState = WindowState;
            _settings.LastDirectory = DirectoryTextBox.Text;
            _settings.SplitterPosition = CalculateSplitterPosition();
            
            App.SaveSettings(_settings);
        }

        /// <summary>
        /// Handles the browse button click event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The routed event arguments</param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing images";
                dialog.ShowNewFolderButton = false;
                
                if (!string.IsNullOrEmpty(DirectoryTextBox.Text) && Directory.Exists(DirectoryTextBox.Text))
                {
                    dialog.SelectedPath = DirectoryTextBox.Text;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Handles the find duplicate button click event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The routed event arguments</param>
        private void FindDuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageItems == null || ImageItems.Count == 0)
            {
                MessageBox.Show("No images loaded. Please load images first.", "No Images", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Clear previous highlights
            ClearHighlights();

            // Find duplicates by MD5 hash
            var duplicates = ImageItems
                .GroupBy(item => item.MD5Hash)
                .Where(group => group.Count() > 1)
                .ToList();

            if (duplicates.Count == 0)
            {
                MessageBox.Show("No duplicate MD5 hashes found.", "No Duplicates", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Highlight duplicate rows
            foreach (var duplicateGroup in duplicates)
            {
                foreach (var duplicateItem in duplicateGroup)
                {
                    var row = ImagesDataGrid.ItemContainerGenerator.ContainerFromItem(duplicateItem) as DataGridRow;
                    if (row != null)
                    {
                        row.Background = System.Windows.Media.Brushes.LightYellow;
                    }
                }
            }

            // Show results
            int totalDuplicates = duplicates.Sum(g => g.Count() - 1); // Count extra items beyond first in each group
            MessageBox.Show($"Found {duplicates.Count} groups of duplicates ({totalDuplicates} duplicate images).\n" +
                          $"Highlighted rows in yellow.", "Duplicates Found", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Clears all row highlights
        /// </summary>
        private void ClearHighlights()
        {
            foreach (var item in ImageItems)
            {
                var row = ImagesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    row.Background = System.Windows.Media.Brushes.Transparent;
                }
            }
        }

        /// <summary>
        /// Handles the load images button click event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The routed event arguments</param>
        private async void LoadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DirectoryTextBox.Text))
            {
                MessageBox.Show("The specified directory does not exist.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var waitWindow = new PleaseWaitWindow();
            waitWindow.Owner = this;
            waitWindow.Show();

            // Clear items before starting
            ImageItems.Clear();

            try
            {
                var images = await LoadImagesFromDirectoryAsync(DirectoryTextBox.Text, 
                    message => Dispatcher.Invoke(() => waitWindow.UpdateStatus(message)));

                // Debug: Log result from background
                System.Diagnostics.Debug.WriteLine($"Received {images.Count} images from background task");

                // Add items on UI thread
                foreach (var imageInfo in images)
                {
                    ImageItems.Add(imageInfo);
                }

                // Debug: Log final UI count
                System.Diagnostics.Debug.WriteLine($"UI ImageItems now has {ImageItems.Count} items");

                waitWindow.Close();
                MessageBox.Show($"Loaded {ImageItems.Count} images from the directory.", 
                    "Load Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                waitWindow.Close();
                MessageBox.Show($"Error loading images: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads all JPG images from the specified directory asynchronously
        /// </summary>
        /// <param name="directoryPath">The directory path to load images from</param>
        /// <param name="updateStatus">Action to update status message</param>
        /// <returns>List of processed image information</returns>
        private async Task<List<ImageInfo>> LoadImagesFromDirectoryAsync(string directoryPath, Action<string> updateStatus)
        {
            return await Task.Run(() =>
            {
                var jpgFiles = Directory.GetFiles(directoryPath, "*.jpg", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.GetFiles(directoryPath, "*.jpeg", SearchOption.TopDirectoryOnly));

                int processedCount = 0;
                int totalCount = jpgFiles.Count();
                var imagesToAdd = new List<ImageInfo>();

                // Debug: Log file count
                System.Diagnostics.Debug.WriteLine($"Found {totalCount} files in {directoryPath}");

                foreach (var filePath in jpgFiles)
                {
                    try
                    {
                        // Update status
                        updateStatus($"Processing {Path.GetFileName(filePath)}...");

                        var md5Hash = CalculateMD5Hash(filePath);
                        var imageInfo = new ImageInfo
                        {
                            Path = filePath,
                            MD5Hash = md5Hash
                        };
                        
                        imagesToAdd.Add(imageInfo);
                        processedCount++;
                        
                        // Debug: Log processing
                        System.Diagnostics.Debug.WriteLine($"Processed {processedCount}/{totalCount}: {filePath}");
                        
                        // Update progress
                        updateStatus($"Processed {processedCount} of {totalCount} images...");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing {filePath}: {ex.Message}");
                    }
                }

                // Debug: Log final result
                System.Diagnostics.Debug.WriteLine($"Returning {imagesToAdd.Count} images");

                return imagesToAdd;
            });
        }

        /// <summary>
        /// Calculates the MD5 hash of a file
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>The MD5 hash as a hexadecimal string</returns>
        private string CalculateMD5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        /// <summary>
        /// Handles the DataGrid selection changed event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The selection changed event arguments</param>
        private void ImagesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImagesDataGrid.SelectedItem is ImageInfo selectedImage)
            {
                try
                {
                    if (File.Exists(selectedImage.Path))
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(selectedImage.Path);
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        PreviewImage.Source = bitmap;
                    }
                    else
                    {
                        PreviewImage.Source = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading preview: {ex.Message}");
                    PreviewImage.Source = null;
                }
            }
            else
            {
                PreviewImage.Source = null;
            }
        }

        /// <summary>
        /// Restores the splitter position from settings
        /// </summary>
        private void RestoreSplitterPosition()
        {
            try
            {
                var mainGrid = ImagesDataGrid.Parent as Grid;
                if (mainGrid != null)
                {
                    var totalWidth = mainGrid.ActualWidth;
                    if (totalWidth > 0)
                    {
                        var splitterWidth = totalWidth * _settings.SplitterPosition;
                        var columnDefinition = mainGrid.ColumnDefinitions[1];
                        columnDefinition.Width = new GridLength(splitterWidth, GridUnitType.Pixel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring splitter position: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the current splitter position as a ratio
        /// </summary>
        /// <returns>The splitter position as a ratio (0.0 to 1.0)</returns>
        private double CalculateSplitterPosition()
        {
            try
            {
                var mainGrid = ImagesDataGrid.Parent as Grid;
                if (mainGrid != null)
                {
                    var totalWidth = mainGrid.ActualWidth;
                    if (totalWidth > 0)
                    {
                        var dataGridWidth = mainGrid.ColumnDefinitions[0].ActualWidth;
                        return dataGridWidth / totalWidth;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating splitter position: {ex.Message}");
            }
            return 0.7; // Default fallback value
        }

        /// <summary>
        /// Handles the GridSplitter drag completed event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The drag completed event arguments</param>
        private void MainGridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // Save the new splitter position immediately
            _settings.SplitterPosition = CalculateSplitterPosition();
        }
    }

    /// <summary>
    /// Represents information about an image file
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// Gets or sets the full path to the image file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the MD5 hash of the image file
        /// </summary>
        public string MD5Hash { get; set; }
    }
}
