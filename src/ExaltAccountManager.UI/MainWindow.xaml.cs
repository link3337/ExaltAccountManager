using ExaltAccountManager.Core.Exceptions;
using ExaltAccountManager.Core.Settings;
using ExaltAccountManager.Core.Util;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ExaltAccountManager.UI
{
    public partial class MainWindow : Window
    {
        private readonly SettingsManager<UserSettings> _settingsManager;
        private readonly UserSettings _userSettings;

        public MainWindow()
        {
            InitializeComponent();
            _settingsManager = new SettingsManager<UserSettings>("settings.json");
            _userSettings = _settingsManager.LoadSettings() ?? new UserSettings();
            // if account list null initialize empty list
            _userSettings.Accounts ??= new List<Account>();
            // apply settings
            Apply(_userSettings);
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _userSettings.ExaltPath = txtExaltPath.Text;
            _userSettings.DeviceToken = txtDeviceToken.Password;
            _settingsManager.SaveSettings(_userSettings);
            MessageBox.Show("Settings saved");
        }

        private void BtnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            if (_userSettings.Accounts.Any(x => x.Name == name))
            {
                MessageBox.Show("An account with name already exists. Not saved.");
                return;
            }
            if (string.IsNullOrEmpty(txtEMail.Text) || string.IsNullOrEmpty(txtPassword.Password))

            {
                MessageBox.Show("E-Mail / Password cannot be empty.");
                return;
            }

            string base64EMail = Helper.Base64Encode(txtEMail.Text);
            string base64Password = Helper.Base64Encode(txtPassword.Password);

            _userSettings.Accounts.Add(
                new Account
                {
                    Base64EMail = base64EMail,
                    Base64Password = base64Password,
                    Name = name
                });

            _settingsManager.SaveSettings(_userSettings);

            MessageBox.Show("Added account.");
            txtEMail.Text = "";
            txtName.Text = "";
            txtPassword.Password = "";
            Apply(_userSettings);
        }

        private void Apply(UserSettings userSettings)
        {
            txtExaltPath.Text = userSettings?.ExaltPath ?? "";
            txtDeviceToken.Password = userSettings?.DeviceToken ?? "";
            lbAccountList.ItemsSource = userSettings.Accounts;
            lbAccountList.Items.Refresh();
        }

        private void BtnOpenSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbAccountList.SelectedItem is null)
            {
                MessageBox.Show("Select an item.");
                return;
            }
            Account account = lbAccountList.SelectedItem as Account;

            Helper.LaunchExaltClient(_userSettings.ExaltPath, Helper.Base64Decode(account.Base64EMail), Helper.Base64Decode(account.Base64Password), _userSettings.DeviceToken);
        }

        private void BtnSelectLast_Click(object sender, RoutedEventArgs e)
        {
            lbAccountList.SelectedIndex = lbAccountList.Items.Count - 1;
        }

        private void BtnSelectNext_Click(object sender, RoutedEventArgs e)
        {
            int nextIndex = 0;
            if ((lbAccountList.SelectedIndex >= 0) && (lbAccountList.SelectedIndex < (lbAccountList.Items.Count - 1)))
            {
                nextIndex = lbAccountList.SelectedIndex + 1;
            }
            lbAccountList.SelectedIndex = nextIndex;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbAccountList.SelectedItem is null)
            {
                MessageBox.Show("Select an item.");
                return;
            }
            Account Account = lbAccountList.SelectedItem as Account;
            _userSettings.Accounts.Remove(Account);
            _settingsManager.SaveSettings(_userSettings);
            Apply(_userSettings);
        }

        private void TxtExaltPath_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new();
            if (dialog.ShowDialog().Value)
            {
                txtExaltPath.Text = dialog.SelectedPath;
            }
        }
    }
}
