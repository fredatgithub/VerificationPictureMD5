using System.Windows;

namespace VerificationPictureMD5
{
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
