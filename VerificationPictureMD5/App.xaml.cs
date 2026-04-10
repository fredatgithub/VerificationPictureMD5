using System;
using System.IO;
using System.Windows;
using System.Text.Json;

namespace VerificationPictureMD5
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string SettingsFileName = "appsettings.json";
        private static readonly string _settingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            SettingsFileName);

        /// <summary>
        /// Gets the application settings file path
        /// </summary>
        public static string SettingsFilePath => _settingsFilePath;

        /// <summary>
        /// Handles the application exit event to save settings
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The exit event arguments</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save window settings when application exits
            if (VerificationPictureMD5.MainWindow.Current != null)
            {
                VerificationPictureMD5.MainWindow.Current.SaveWindowSettings();
            }
        }

        /// <summary>
        /// Loads application settings from file
        /// </summary>
        /// <returns>The application settings or null if file doesn't exist</returns>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            return new AppSettings();
        }

        /// <summary>
        /// Saves application settings to file
        /// </summary>
        /// <param name="settings">The settings to save</param>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
