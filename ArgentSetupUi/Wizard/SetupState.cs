using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Argent.SetupUi.Wizard
{
    public enum PrereqStatus
    {
        Pending,
        Scanning,
        Found,
        WillInstall
    }

    public sealed class PrerequisiteItem : INotifyPropertyChanged
    {
        private PrereqStatus _status = PrereqStatus.Pending;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }

        public PrereqStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusLabel));
            }
        }

        public string StatusLabel
        {
            get
            {
                switch (_status)
                {
                    case PrereqStatus.Pending: return "Waiting";
                    case PrereqStatus.Scanning: return "Checking…";
                    case PrereqStatus.Found: return "Already on this computer";
                    case PrereqStatus.WillInstall: return "Missing — Setup will install";
                    default: return "";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public sealed class SetupState : INotifyPropertyChanged
    {
        private bool _licenseAccepted;
        private bool _useSqlServer = true;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _validationMessage = string.Empty;
        private bool _scanDone;
        private bool _showMoreContact;

        public string ProductLine1 { get; } = "Job Scheduler 10.0-2401-64W-A";
        public string ProductLine2 { get; } = "Queue Engine 10.0-2401-64W-A";

        public string MachineName { get; } = System.Environment.MachineName;
        public string CurrentUser { get; } = System.Environment.UserName;
        public string CurrentDomain { get; } = System.Environment.UserDomainName;

        public string InputDirectory { get; set; } =
            @"D:\ARGENT_JOB_SCHEDULER_10_0A_2401_A\_ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A";

        public string SchedulerOutputPath { get; set; } = @"C:\ARGENT\SchedulingEngine";
        public string QueueOutputPath { get; set; } = @"C:\ARGENT\QueueEngine";
        public string InstallNode { get; set; }

        public bool InstallScheduler { get; set; } = true;
        public bool InstallQueueEngine { get; set; } = true;
        public int InstallOperationIndex { get; set; }

        public string LicenseFilePath { get; set; } =
            @"D:\ARGENT_JOB_SCHEDULER_10_0A_2401_A\ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A";

        public bool StandaloneQueueEngine { get; set; } = true;
        public string QueueEngineLicenseKey { get; set; } = "NC02-CI61-HE28-OL51-2DDF";

        public bool UseManagedServiceAccount { get; set; }
        public string ServiceAccount { get; set; }

        public bool UseSqlServer
        {
            get => _useSqlServer;
            set { if (_useSqlServer == value) return; _useSqlServer = value; OnPropertyChanged(); }
        }

        public string SelectedOdbcDsn { get; set; } = string.Empty;

        public bool LicenseAccepted
        {
            get => _licenseAccepted;
            set { if (_licenseAccepted == value) return; _licenseAccepted = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { if (_email == value) return; _email = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Contact { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StateProv { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string SalesRep { get; set; } = string.Empty;

        public string Password
        {
            get => _password;
            set { if (_password == value) return; _password = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { if (_confirmPassword == value) return; _confirmPassword = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set { if (_validationMessage == value) return; _validationMessage = value ?? string.Empty; OnPropertyChanged(); }
        }

        public bool ScanDone
        {
            get => _scanDone;
            set { if (_scanDone == value) return; _scanDone = value; OnPropertyChanged(); }
        }

        public bool ShowMoreContact
        {
            get => _showMoreContact;
            set { if (_showMoreContact == value) return; _showMoreContact = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PrerequisiteItem> Prerequisites { get; } =
            new ObservableCollection<PrerequisiteItem>();

        public int SimulatedProgress { get; set; }
        public string ProgressStatus { get; set; } = "Preparing…";
        public int InstallDurationSeconds { get; set; } = 266;
        public string DefaultQueueAccount { get; set; }

        public SetupState()
        {
            InstallNode = MachineName;
            ServiceAccount = MachineName + "\\" + CurrentUser;
            DefaultQueueAccount = ServiceAccount;

            Prerequisites.Add(new PrerequisiteItem
            {
                Id = "dotnet",
                Name = ".NET Framework 4.8",
                Detail = "Required runtime for Setup and client tools"
            });
            Prerequisites.Add(new PrerequisiteItem
            {
                Id = "vcredist",
                Name = "Visual C++ Redistributable",
                Detail = "Native libraries used by Queue Engine / services"
            });
            Prerequisites.Add(new PrerequisiteItem
            {
                Id = "odbc",
                Name = "SQL Server ODBC Driver",
                Detail = "Needed when SQL Server is selected as storage"
            });
            Prerequisites.Add(new PrerequisiteItem
            {
                Id = "admin",
                Name = "Administrator rights",
                Detail = "Required to install Windows services"
            });
            Prerequisites.Add(new PrerequisiteItem
            {
                Id = "disk",
                Name = "Disk space (≈ 500 MB free)",
                Detail = "On the drive you choose for install folders"
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
