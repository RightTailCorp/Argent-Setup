using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Argent.SetupUi.Wizard
{
    /// <summary>UI-only state for the wizard prototype (no install logic).</summary>
    public sealed class SetupState : INotifyPropertyChanged
    {
        private bool _licenseAccepted;
        private bool _useSqlServer = true;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _validationMessage = string.Empty;

        public string ProductLine1 { get; } = "Argent Job Scheduler 10.0-2401-64W-A";
        public string ProductLine2 { get; } = "Argent Queue Engine 10.0-2401-64W-A";

        public string MachineName { get; } = System.Environment.MachineName;
        public string CurrentUser { get; } = System.Environment.UserName;
        public string CurrentDomain { get; } = System.Environment.UserDomainName;

        public string InputDirectory { get; set; } =
            @"D:\ARGENT_JOB_SCHEDULER_10_0A_2401_A\_ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A";

        public string SchedulerOutputPath { get; set; } = @"C:\ARGENT\SchedulingEngine";
        public string QueueOutputPath { get; set; } = @"C:\ARGENT\QueueEngine";

        public string InstallNode { get; set; } = System.Environment.MachineName;

        public string SchedulerStatus { get; set; } = "Not available";
        public string QueueEngineStatus { get; set; } = "Not Available";

        public bool InstallScheduler { get; set; } = true;
        public bool InstallQueueEngine { get; set; } = true;

        public int InstallOperationIndex { get; set; } = 0;

        public string LicenseFilePath { get; set; } =
            @"D:\ARGENT_JOB_SCHEDULER_10_0A_2401_A\ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A";

        public bool StandaloneQueueEngine { get; set; } = true;
        public string QueueEngineLicenseKey { get; set; } = "NC02-CI61-HE28-OL51-2DDF";

        public bool UseManagedServiceAccount { get; set; }
        public string ServiceAccount { get; set; }

        public bool UseSqlServer
        {
            get => _useSqlServer;
            set
            {
                if (_useSqlServer == value) return;
                _useSqlServer = value;
                OnPropertyChanged();
            }
        }

        public string SelectedOdbcDsn { get; set; } = string.Empty;

        public bool LicenseAccepted
        {
            get => _licenseAccepted;
            set
            {
                if (_licenseAccepted == value) return;
                _licenseAccepted = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email == value) return;
                _email = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword == value) return;
                _confirmPassword = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                if (_validationMessage == value) return;
                _validationMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public int SimulatedProgress { get; set; }
        public string ProgressStatus { get; set; } = "Preparing installation…";
        public int InstallDurationSeconds { get; set; } = 266;
        public string DefaultQueueAccount { get; set; }

        public SetupState()
        {
            ServiceAccount = $@"{MachineName}\{CurrentUser}";
            DefaultQueueAccount = $@"{MachineName}\{CurrentUser}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
