using System.Windows;

namespace VerificationPictureMD5
{
    /// <summary>
    /// Interaction logic for PleaseWaitWindow.xaml
    /// </summary>
    public partial class PleaseWaitWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the PleaseWaitWindow class
        /// </summary>
        public PleaseWaitWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the status text displayed in the window
        /// </summary>
        /// <param name="message">The status message to display</param>
        public void UpdateStatus(string message)
        {
            StatusTextBlock.Text = message;
        }
    }
}
