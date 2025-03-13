using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExaltAccountManager.Core.Settings;
using ExaltAccountManager.Core.Util;
using ExaltAccountManager.UI.Controls;
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
        private Point _dragStartPoint;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Snackbar Snackbar => SnackbarElement;

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
                Snackbar.Show($"Failed to load settings: {ex.Message}");
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

                Snackbar.Show("Settings saved successfully");
            }
            catch (Exception ex)
            {
                Snackbar.Show($"Failed to save settings: {ex.Message}");
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
                        Snackbar.Show("An account with this name already exists");
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

                    Snackbar.Show("Account added successfully");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Show($"Failed to add account: {ex.Message}");
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

                    Snackbar.Show("Account edited successfully");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Show($"Failed to edit account: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAccount is null)
                {
                    Snackbar.Show("Please select an account to delete");
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete account '{SelectedAccount.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Settings.Accounts!.Remove(SelectedAccount);
                    _settingsManager.SaveSettings(Settings);
                    ApplySettings(Settings);
                    Snackbar.Show("Account deleted successfully");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Show($"Failed to delete account: {ex.Message}");
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
                Snackbar.Show("Exalt path cannot be empty");
                return false;
            }

            if (!Directory.Exists(ExaltPath))
            {
                Snackbar.Show("Selected Exalt path does not exist");
                return false;
            }

            return true;
        }

        private bool ValidateExaltLaunch()
        {
            if (SelectedAccount is null)
            {
                Snackbar.Show("Please select an account");
                return false;
            }

            if (string.IsNullOrEmpty(Settings.ExaltPath))
            {
                Snackbar.Show("Please set the Exalt path in settings");
                return false;
            }

            return true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void AccountsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void AccountsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            Vector diff = _dragStartPoint - e.GetPosition(null);
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            // Find the DataGridRow where dragging started.
            DataGridRow? row = FindVisualParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row == null || row.Item is not Account draggedAccount)
            {
                return;
            }

            DataObject data = new("DraggedAccount", draggedAccount);
            DragDrop.DoDragDrop(row, data, DragDropEffects.Move);
        }

        private void AccountsGrid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("DraggedAccount"))
            {
                return;
            }

            // Identify target row.
            DataGridRow? targetRow = FindVisualParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (targetRow == null || targetRow.Item is not Account targetAccount)
            {
                return;
            }

            // Get the dropped item.
            if (e.Data.GetData("DraggedAccount") is not Account draggedAccount || draggedAccount == targetAccount)
            {
                return;
            }

            int indexDragged = Settings.Accounts!.IndexOf(draggedAccount);
            int indexTarget = Settings.Accounts.IndexOf(targetAccount);

            // Update order.
            if (indexDragged < indexTarget)
            {
                Settings.Accounts.Insert(indexTarget + 1, draggedAccount);
                Settings.Accounts.RemoveAt(indexDragged);
            }
            else
            {
                Settings.Accounts.Insert(indexTarget, draggedAccount);
                Settings.Accounts.RemoveAt(indexDragged + 1);
            }

            _settingsManager.SaveSettings(Settings);
            ApplySettings(Settings);
            Snackbar.Show("Accounts reordered successfully");
        }

        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T correctlyTyped)
                {
                    return correctlyTyped;
                }
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
