using System.Windows;
using System.Windows.Threading;
using ExaltAccountManager.Core.Exceptions;

namespace ExaltAccountManager.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is ExaltPathNotFoundException)
            {
                MessageBox.Show("Make sure to enter exalt path in settings tab.");
            }
            else if (e.Exception is AccessTokenParseFailedException)
            {
                MessageBox.Show("Failed to parse access token, try later again maybe.");
            }
            else if (e.Exception is AccessTokenRetrievalFailedException)
            {
                MessageBox.Show("Failed to retrieve access token, try later again maybe. Make sure the credentials of the account are valid.");
            }
            else if (e.Exception is ExaltExeNotFoundException)
            {
                MessageBox.Show("Couldn't start exalt. Make sure the path is correct and it contains the RotMG Exalt.exe file.");
            }
            else
            {
                MessageBox.Show("Something went wrong.", e.Exception.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            e.Handled = true;
        }
    }
}
