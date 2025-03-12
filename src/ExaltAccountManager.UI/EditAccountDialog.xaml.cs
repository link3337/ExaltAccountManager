using System.Windows;
using ExaltAccountManager.Core.Settings;
using ExaltAccountManager.Core.Util;

namespace ExaltAccountManager.UI
{
    /// <summary>
    /// Interaction logic for EditAccountDialog.xaml
    /// </summary>
    public partial class EditAccountDialog : Window
    {
        public string AccountName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string Password { get; private set; } = default!;

        public EditAccountDialog(Account? account)
        {
            InitializeComponent();

            if (account != null)
            {
                Title = "Edit Account";
                txtName.Text = account.Name;
                txtEmail.Text = Helper.Base64Decode(account.Base64EMail);
                txtPassword.Password = Helper.Base64Decode(account.Base64Password);
            }
            else
            {
                Title = "Add Account";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("All fields are required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccountName = txtName.Text;
            Email = txtEmail.Text;
            Password = txtPassword.Password;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
