using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Argent.SetupUi.Dialogs;
using Argent.SetupUi.Wizard;

namespace Argent.SetupUi
{
    public partial class MainWindow : Window
    {
        private readonly SetupState _state = new SetupState();
        private WizardStep _step = WizardStep.Welcome;
        private DispatcherTimer _progressTimer;
        private bool _odbcConfigured;
        private bool _simulateOdbcFailure = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _state;
            RefreshUi();
        }

        private void RefreshUi()
        {
            PageHost.Content = PageFactory.Create(_step, _state);
            RefreshStepList();
            RefreshButtons();
            InlineValidation.Text = _state.ValidationMessage;
        }

        private void RefreshStepList()
        {
            var items = new List<StepItem>();
            for (var i = 0; i < WizardStepNames.SidebarLabels.Length; i++)
            {
                var stepIndex = (WizardStep)i;
                items.Add(new StepItem
                {
                    IndexDisplay = (i + 1).ToString(),
                    Title = WizardStepNames.SidebarLabels[i],
                    CircleBrush = GetCircleBrush(stepIndex),
                    IndexForeground = GetIndexForeground(stepIndex),
                    TitleBrush = GetTitleBrush(stepIndex)
                });
            }
            StepList.ItemsSource = items;
        }

        private Brush GetCircleBrush(WizardStep step)
        {
            if (step == _step)
                return (Brush)FindResource("PrimaryBrush");
            if ((int)step < (int)_step)
                return new SolidColorBrush(Color.FromRgb(16, 185, 129));
            return new SolidColorBrush(Color.FromRgb(229, 231, 235));
        }

        private Brush GetIndexForeground(WizardStep step)
        {
            if (step == _step || (int)step < (int)_step)
                return Brushes.White;
            return (Brush)FindResource("TextSecondaryBrush");
        }

        private Brush GetTitleBrush(WizardStep step)
        {
            if (step == _step)
                return (Brush)FindResource("TextPrimaryBrush");
            if ((int)step < (int)_step)
                return (Brush)FindResource("TextPrimaryBrush");
            return (Brush)FindResource("TextSecondaryBrush");
        }

        private void RefreshButtons()
        {
            BackButton.IsEnabled = _step > WizardStep.Welcome && _step != WizardStep.Progress;
            CancelButton.IsEnabled = _step != WizardStep.Progress;

            if (_step == WizardStep.Complete)
            {
                NextButton.Content = "Finish";
                BackButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
            }
            else if (_step == WizardStep.Progress)
            {
                NextButton.Content = "Next";
                NextButton.IsEnabled = false;
                BackButton.IsEnabled = false;
            }
            else
            {
                NextButton.Content = "Next";
                NextButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _state.ValidationMessage = string.Empty;
            if (_step > WizardStep.Welcome)
            {
                _step--;
                RefreshUi();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _state.ValidationMessage = string.Empty;

            if (_step == WizardStep.Complete)
            {
                Close();
                return;
            }

            if (!ValidateCurrentStep())
            {
                InlineValidation.Text = _state.ValidationMessage;
                return;
            }

            if (_step == WizardStep.ServiceAndDatabase)
            {
                if (!TryCompleteDatabaseStep())
                    return;
            }

            if (_step == WizardStep.CustomerInfo)
            {
                _step = WizardStep.Progress;
                RefreshUi();
                StartProgressSimulation();
                return;
            }

            _step++;
            RefreshUi();
        }

        private bool ValidateCurrentStep()
        {
            switch (_step)
            {
                case WizardStep.License:
                    if (!_state.LicenseAccepted)
                    {
                        _state.ValidationMessage = "You must accept the license agreement to continue.";
                        return false;
                    }
                    return true;

                case WizardStep.InstallRemove:
                    if (!_state.InstallScheduler && !_state.InstallQueueEngine)
                    {
                        _state.ValidationMessage = "Select at least one component to install or remove.";
                        return false;
                    }
                    return true;

                case WizardStep.ServiceAndDatabase:
                    if (!string.IsNullOrEmpty(_state.Password) &&
                        !string.Equals(_state.Password, _state.ConfirmPassword, StringComparison.Ordinal))
                    {
                        _state.ValidationMessage = "Password and confirm password do not match.";
                        return false;
                    }
                    return true;

                case WizardStep.CustomerInfo:
                    if (string.IsNullOrWhiteSpace(_state.Email))
                    {
                        _state.ValidationMessage = "Email address is required.";
                        return false;
                    }
                    if (!Regex.IsMatch(_state.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        _state.ValidationMessage = "Enter a valid email address.";
                        return false;
                    }
                    return true;

                default:
                    return true;
            }
        }

        /// <summary>
        /// UI prototype: simulates ODBC browse failure and the fixed "No" behavior (stay on screen, SQL unchecked).
        /// </summary>
        private bool TryCompleteDatabaseStep()
        {
            if (!_state.UseSqlServer)
            {
                _odbcConfigured = false;
                return true;
            }

            if (_odbcConfigured && !string.IsNullOrEmpty(_state.SelectedOdbcDsn))
                return true;

            if (_simulateOdbcFailure)
            {
                var dialog = new OdbcRetryDialog { Owner = this };
                dialog.ShowDialog();
                if (dialog.RetrySelected)
                {
                    _state.ValidationMessage = "Select an ODBC DSN (prototype — wire to legacy BrowseODBC).";
                    InlineValidation.Text = _state.ValidationMessage;
                    return false;
                }

                _state.UseSqlServer = false;
                _odbcConfigured = false;
                _state.ValidationMessage =
                    "SQL Server storage was turned off. You can re-enable it after configuring ODBC.";
                RefreshUi();
                return false;
            }

            _state.SelectedOdbcDsn = "ArgentScheduler_DSN";
            _odbcConfigured = true;
            return true;
        }

        private void StartProgressSimulation()
        {
            _state.SimulatedProgress = 0;
            _state.ProgressStatus = "Updating system registry information…";

            _progressTimer?.Stop();
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (PageHost.Content is FrameworkElement fe && fe.Tag is PageFactory.ProgressRefs refs)
            {
                if (_state.SimulatedProgress < 90)
                    _state.SimulatedProgress += 2;
                else if (_state.SimulatedProgress < 100)
                    _state.SimulatedProgress += 1;

                if (_state.SimulatedProgress >= 50 && _state.SimulatedProgress < 85)
                    _state.ProgressStatus = "Creating Queue Engine program items and links…";
                else if (_state.SimulatedProgress >= 85)
                    _state.ProgressStatus = "Updating system registry information…";

                refs.Bar.Value = _state.SimulatedProgress;
                refs.Pct.Text = $"{_state.SimulatedProgress}%";
                refs.Status.Text = _state.ProgressStatus;

                if (_state.SimulatedProgress >= 100)
                {
                    _progressTimer.Stop();
                    _step = WizardStep.Complete;
                    RefreshUi();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Quit Setup?",
                "Argent Job Scheduler Setup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Close();
        }

        private sealed class StepItem
        {
            public string IndexDisplay { get; set; }
            public string Title { get; set; }
            public Brush CircleBrush { get; set; }
            public Brush IndexForeground { get; set; }
            public Brush TitleBrush { get; set; }
        }
    }
}
