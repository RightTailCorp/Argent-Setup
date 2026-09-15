using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Argent.SetupUi.Dialogs;
using Argent.SetupUi.Logging;
using Argent.SetupUi.Wizard;

namespace Argent.SetupUi
{
    public partial class MainWindow : Window
    {
        private readonly SetupState _state = new SetupState();
        private WizardStep _step = WizardStep.Start;
        private DispatcherTimer _progressTimer;
        private DispatcherTimer _scanTimer;
        private int _scanIndex;
        private bool _odbcConfigured;
        private bool _scanning;

        private static readonly Dictionary<string, PrereqStatus> ScanResults =
            new Dictionary<string, PrereqStatus>
            {
                { "dotnet", PrereqStatus.Found },
                { "vcredist", PrereqStatus.WillInstall },
                { "odbc", PrereqStatus.WillInstall },
                { "admin", PrereqStatus.Found },
                { "disk", PrereqStatus.Found }
            };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _state;
            SetupLog.Current.LogSessionStart();
            SetupLog.Current.Log("Wizard opened on step: Start");
            RefreshUi();
        }

        private void RefreshUi()
        {
            PageHost.Content = PageFactory.Create(_step, _state);
            RefreshStepList();
            RefreshButtons();
            InlineValidation.Text = _state.ValidationMessage;
            StepPill.Text = WizardStepNames.SidebarLabels[(int)_step];
            var pct = (int)Math.Round(((int)_step) * 100.0 / (WizardStepNames.SidebarLabels.Length - 1));
            ProgressMeta.Text = string.Format("Step {0} of {1} · {2}%",
                (int)_step + 1, WizardStepNames.SidebarLabels.Length, pct);

            if (_step == WizardStep.SystemCheck && !_state.ScanDone && !_scanning)
                StartSystemScan();
        }

        private void StartSystemScan()
        {
            _scanning = true;
            _scanIndex = 0;
            foreach (var p in _state.Prerequisites)
                p.Status = PrereqStatus.Pending;

            _scanTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            _scanTimer.Tick += ScanTimer_Tick;
            _scanTimer.Start();
            if (_state.Prerequisites.Count > 0)
                _state.Prerequisites[0].Status = PrereqStatus.Scanning;
            RefreshButtons();
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (_scanIndex >= _state.Prerequisites.Count)
            {
                _scanTimer.Stop();
                _scanning = false;
                _state.ScanDone = true;
                RefreshUi();
                return;
            }

            var item = _state.Prerequisites[_scanIndex];
            PrereqStatus result;
            if (!ScanResults.TryGetValue(item.Id, out result))
                result = PrereqStatus.Found;
            item.Status = result;
            SetupLog.Current.Log(string.Format("System check: {0} — {1}", item.Name, item.StatusLabel));
            _scanIndex++;

            if (_scanIndex < _state.Prerequisites.Count)
                _state.Prerequisites[_scanIndex].Status = PrereqStatus.Scanning;
            else
            {
                _scanTimer.Stop();
                _scanning = false;
                _state.ScanDone = true;
                RefreshUi();
            }
        }

        private void RefreshStepList()
        {
            var items = new List<StepItem>();
            for (var i = 0; i < WizardStepNames.SidebarLabels.Length; i++)
            {
                var stepIndex = (WizardStep)i;
                var current = stepIndex == _step;
                var done = (int)stepIndex < (int)_step;
                items.Add(new StepItem
                {
                    IndexDisplay = done && !current ? "✓" : (i + 1).ToString(),
                    Title = WizardStepNames.SidebarLabels[i],
                    CircleBrush = current
                        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
                        : done
                            ? new SolidColorBrush(Color.FromRgb(59, 158, 255))
                            : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    IndexForeground = current || done ? Brushes.White : new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                    TitleBrush = current || done ? Brushes.White : new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
                    RowBrush = current
                        ? new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
                        : Brushes.Transparent
                });
            }
            StepList.ItemsSource = items;
        }

        private void RefreshButtons()
        {
            BackButton.IsEnabled = _step > WizardStep.Start && _step != WizardStep.Installing && _step != WizardStep.Done;
            CancelButton.IsEnabled = _step != WizardStep.Installing && _step != WizardStep.Done;

            if (_step == WizardStep.Done)
                NextButton.Content = "Close";
            else if (_step == WizardStep.Start)
                NextButton.Content = "Get started";
            else if (_step == WizardStep.SystemCheck)
                NextButton.Content = "Looks good — continue";
            else if (_step == WizardStep.Account)
                NextButton.Content = "Install";
            else
                NextButton.Content = "Continue";

            NextButton.IsEnabled = _step != WizardStep.Installing &&
                                   !(_step == WizardStep.SystemCheck && (!_state.ScanDone || _scanning));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _state.ValidationMessage = string.Empty;
            if (_step > WizardStep.Start)
            {
                _step--;
                SetupLog.Current.Log("User clicked Back — step: " + WizardStepNames.SidebarLabels[(int)_step]);
                RefreshUi();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _state.ValidationMessage = string.Empty;

            if (_step == WizardStep.Done)
            {
                Close();
                return;
            }

            if (!ValidateCurrentStep())
            {
                SetupLog.Current.Log("Validation: " + _state.ValidationMessage);
                InlineValidation.Text = _state.ValidationMessage;
                return;
            }

            LogStepSummaryBeforeAdvance();

            if (_step == WizardStep.Account)
            {
                if (!TryCompleteDatabaseStep())
                    return;
                _step = WizardStep.Installing;
                SetupLog.Current.Log("Install started");
                RefreshUi();
                StartProgressSimulation();
                return;
            }

            _step++;
            SetupLog.Current.Log("Advanced to step: " + WizardStepNames.SidebarLabels[(int)_step]);
            RefreshUi();
        }

        private void LogStepSummaryBeforeAdvance()
        {
            switch (_step)
            {
                case WizardStep.Start:
                    SetupLog.Current.Log(_state.LicenseAccepted
                        ? "User accepted license agreement"
                        : "User declined license agreement");
                    break;
                case WizardStep.SystemCheck:
                    SetupLog.Current.Log("User continued after system check");
                    break;
                case WizardStep.Install:
                    SetupLog.Current.LogQuoted("Install Node", _state.InstallNode);
                    SetupLog.Current.Log("Install Scheduler: " + _state.InstallScheduler);
                    SetupLog.Current.Log("Install Queue Engine: " + _state.InstallQueueEngine);
                    SetupLog.Current.Log("Install operation index: " + _state.InstallOperationIndex);
                    break;
                case WizardStep.PathsLicense:
                    SetupLog.Current.LogQuoted("License file", _state.LicenseFilePath);
                    SetupLog.Current.LogQuoted("Scheduler path", _state.SchedulerOutputPath);
                    SetupLog.Current.LogQuoted("Queue path", _state.QueueOutputPath);
                    SetupLog.Current.Log("Standalone Queue Engine: " + _state.StandaloneQueueEngine);
                    break;
                case WizardStep.Account:
                    SetupLog.Current.LogQuoted("Service account", _state.ServiceAccount);
                    SetupLog.Current.Log("Use SQL Server: " + _state.UseSqlServer);
                    SetupLog.Current.LogQuoted("Email", _state.Email);
                    break;
            }
        }

        private bool ValidateCurrentStep()
        {
            switch (_step)
            {
                case WizardStep.Start:
                    if (!_state.LicenseAccepted)
                    {
                        _state.ValidationMessage = "Accept the license to continue";
                        return false;
                    }
                    return true;
                case WizardStep.SystemCheck:
                    if (!_state.ScanDone)
                    {
                        _state.ValidationMessage = "Wait for the system check to finish";
                        return false;
                    }
                    return true;
                case WizardStep.Install:
                    if (!_state.InstallScheduler && !_state.InstallQueueEngine)
                    {
                        _state.ValidationMessage = "Pick at least one product";
                        return false;
                    }
                    return true;
                case WizardStep.Account:
                    if (!string.IsNullOrEmpty(_state.Password) &&
                        !string.Equals(_state.Password, _state.ConfirmPassword, StringComparison.Ordinal))
                    {
                        _state.ValidationMessage = "Passwords do not match";
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(_state.Email))
                    {
                        _state.ValidationMessage = "Email is required";
                        return false;
                    }
                    if (!Regex.IsMatch(_state.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        _state.ValidationMessage = "Enter a valid email";
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }

        private bool TryCompleteDatabaseStep()
        {
            if (!_state.UseSqlServer)
            {
                _odbcConfigured = false;
                return true;
            }

            if (_odbcConfigured && !string.IsNullOrEmpty(_state.SelectedOdbcDsn))
                return true;

            var dialog = new OdbcRetryDialog { Owner = this };
            dialog.ShowDialog();
            if (dialog.RetrySelected)
            {
                _state.SelectedOdbcDsn = "ArgentScheduler_DSN";
                _odbcConfigured = true;
                SetupLog.Current.LogQuoted("ODBC DSN selected", _state.SelectedOdbcDsn);
                return true;
            }

            SetupLog.Current.Log("Failed to open database — user opted to turn off SQL Server");
            _state.UseSqlServer = false;
            _odbcConfigured = false;
            _state.ValidationMessage =
                "SQL Server option turned off -- Codebase will be used (Codebase is only used for testing and evaluation)";
            RefreshUi();
            return false;
        }

        private void StartProgressSimulation()
        {
            _state.SimulatedProgress = 0;
            _state.ProgressStatus = "Installing bundled prerequisites…";
            _progressTimer?.Stop();
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            var fe = PageHost.Content as FrameworkElement;
            var refs = fe != null ? fe.Tag as PageFactory.ProgressRefs : null;
            if (refs == null) return;

            if (_state.SimulatedProgress < 100)
                _state.SimulatedProgress += _state.SimulatedProgress >= 90 ? 1 : 2;

            var prev = _state.ProgressStatus;
            if (_state.SimulatedProgress < 40)
                _state.ProgressStatus = "Installing bundled prerequisites…";
            else if (_state.SimulatedProgress < 70)
                _state.ProgressStatus = "Copying Argent program files…";
            else if (_state.SimulatedProgress < 90)
                _state.ProgressStatus = "Creating Queue Engine items…";
            else
                _state.ProgressStatus = "Finishing registry updates…";

            if (_state.ProgressStatus != prev)
                SetupLog.Current.Log(_state.ProgressStatus);

            refs.Bar.Value = _state.SimulatedProgress;
            refs.Pct.Text = _state.SimulatedProgress + "%";
            refs.Status.Text = _state.ProgressStatus;

            if (_state.SimulatedProgress >= 100)
            {
                _progressTimer.Stop();
                SetupLog.Current.Log("Install completed successfully");
                _step = WizardStep.Done;
                RefreshUi();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Quit Setup?", "Argent Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                SetupLog.Current.Log("User cancelled Setup");
                Close();
            }
        }

        private void HelpLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private sealed class StepItem
        {
            public string IndexDisplay { get; set; }
            public string Title { get; set; }
            public Brush CircleBrush { get; set; }
            public Brush IndexForeground { get; set; }
            public Brush TitleBrush { get; set; }
            public Brush RowBrush { get; set; }
        }
    }
}
