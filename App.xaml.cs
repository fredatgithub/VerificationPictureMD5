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
        private static readonly string SettingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            SettingsFileName);

        /// <summary>
        /// Gets the application settings file path
        /// </summary>
        public static string SettingsFilePath => SettingsFilePath;

        /// <summary>
        /// Handles the application exit event to save settings
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The exit event arguments</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save window settings when application exits
            if (MainWindow.Current != null)
            {
                MainWindow.Current.SaveWindowSettings();
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
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
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
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents application settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the window left position
        /// </summary>
        public double WindowLeft { get; set; } = 100;

        /// <summary>
        /// Gets or sets the window top position
        /// </summary>
        public double WindowTop { get; set; } = 100;

        /// <summary>
        /// Gets or sets the window width
        /// </summary>
        public double WindowWidth { get; set; } = 800;

        /// <summary>
        /// Gets or sets the window height
        /// </summary>
        public double WindowHeight { get; set; } = 600;

        /// <summary>
        /// Gets or sets the window state
        /// </summary>
        public WindowState WindowState { get; set; } = WindowState.Normal;

        /// <summary>
        /// Gets or sets the last selected directory path
        /// </summary>
        public string LastDirectory { get; set; } = string.Empty;
    }
}
