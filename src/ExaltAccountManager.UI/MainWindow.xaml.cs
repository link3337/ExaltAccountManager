using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ExaltAccountManager.Core.Settings;
using ExaltAccountManager.Core.Util;
using Ookii.Dialogs.Wpf;

namespace ExaltAccountManager.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly SettingsManager<UserSettings> _settingsManager;
        private UserSettings? _settings;
        private ObservableCollection<Account> _accounts;
        private Account? _selectedAccount;
        private string _exaltPath = string.Empty;
        private string _deviceToken = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set
            {
                _accounts = value;
                OnPropertyChanged();
            }
        }

        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
            }
        }
        public UserSettings Settings
        {
            get => _settings ?? throw new InvalidOperationException("Settings not initialized");
            private set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public string ExaltPath
        {
            get => _exaltPath;
            set
            {
                _exaltPath = value;
                OnPropertyChanged();
            }
        }

        public string DeviceToken
        {
            get => _deviceToken;
            set
            {
                _deviceToken = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _settingsManager = new SettingsManager<UserSettings>("settings.json");
            _accounts = [];
            DataContext = this;

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                Settings = _settingsManager.LoadSettings() ?? new UserSettings();
                Settings.Accounts ??= [];
                ApplySettings(Settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateSettings())
                {
                    return;
                }

                Settings.ExaltPath = ExaltPath;
                Settings.DeviceToken = DeviceToken;
                _settingsManager.SaveSettings(Settings);
                MessageBox.Show("Settings saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditAccountDialog dialog = new(null);

                if (dialog.ShowDialog() == true)
                {
                    if (Settings.Accounts!.Any(x => x.Name == dialog.AccountName))
                    {
                        MessageBox.Show("An account with this name already exists", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Account account = new()
                    {
                        Name = dialog.AccountName,
                        Base64EMail = Helper.Base64Encode(dialog.Email),
                        Base64Password = Helper.Base64Encode(dialog.Password)
                    };

                    Settings.Accounts!.Add(account);
                    _settingsManager.SaveSettings(Settings);
                    ApplySettings(Settings);

                    MessageBox.Show("Account added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((FrameworkElement)sender).DataContext is not Account account)
                {
                    return;
                }

                // Create edit dialog
                EditAccountDialog dialog = new(account);
                if (dialog.ShowDialog() == true)
                {
                    // Update account
                    account.Name = dialog.AccountName;
                    account.Base64EMail = Helper.Base64Encode(dialog.Email);
                    account.Base64Password = Helper.Base64Encode(dialog.Password);

                    // Save changes
                    _settingsManager.SaveSettings(Settings);
                    ApplySettings(Settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to edit account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAccount is null)
                {
                    MessageBox.Show("Please select an account to delete", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete account '{SelectedAccount.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    Settings.Accounts!.Remove(SelectedAccount);
                    _settingsManager.SaveSettings(Settings);
                    ApplySettings(Settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySettings(UserSettings userSettings)
        {
            ExaltPath = userSettings?.ExaltPath ?? "";
            DeviceToken = userSettings?.DeviceToken ?? "";
            Accounts = [.. userSettings?.Accounts ?? []];
        }

        private async void BtnOpenSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateExaltLaunch())
            {
                return;
            }

            await Helper.LaunchExaltClient(Settings.ExaltPath!, Helper.Base64Decode(SelectedAccount!.Base64EMail), Helper.Base64Decode(SelectedAccount.Base64Password), Settings.DeviceToken ?? "");
        }

        private void BtnSelectLast_Click(object sender, RoutedEventArgs e)
        {
            if (Accounts.Count > 0)
            {
                SelectedAccount = Accounts[^1];
            }
        }

        private void BtnSelectNext_Click(object sender, RoutedEventArgs e)
        {
            if (Accounts.Count == 0)
            {
                return;
            }

            int currentIndex = Accounts.IndexOf(SelectedAccount ?? Accounts[^1]);
            SelectedAccount = Accounts[(currentIndex + 1) % Accounts.Count];
        }

        private void TxtExaltPath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new();
            if (dialog.ShowDialog()!.Value)
            {
                ExaltPath = dialog.SelectedPath;
            }
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrEmpty(ExaltPath))
            {
                MessageBox.Show("Exalt path cannot be empty", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Directory.Exists(ExaltPath))
            {
                MessageBox.Show("Selected Exalt path does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateExaltLaunch()
        {
            if (SelectedAccount is null)
            {
                MessageBox.Show("Please select an account", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(Settings.ExaltPath))
            {
                MessageBox.Show("Please set the Exalt path in settings", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
