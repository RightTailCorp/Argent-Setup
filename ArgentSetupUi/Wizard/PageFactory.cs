using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Argent.SetupUi.Wizard
{
    internal static class PageFactory
    {
        public static FrameworkElement Create(WizardStep step, SetupState state)
        {
            switch (step)
            {
                case WizardStep.Start: return Start(state);
                case WizardStep.SystemCheck: return SystemCheck(state);
                case WizardStep.Install: return Install(state);
                case WizardStep.PathsLicense: return PathsLicense(state);
                case WizardStep.Account: return Account(state);
                case WizardStep.Installing: return Installing(state);
                case WizardStep.Done: return Done(state);
                default: return new TextBlock { Text = "Unknown step" };
            }
        }

        private static StackPanel Shell(string title, string subtitle, params UIElement[] body)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = Brush("#0B1F3A"),
                Margin = new Thickness(0, 0, 0, 8)
            });
            if (!string.IsNullOrEmpty(subtitle))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = subtitle,
                    FontSize = 14,
                    Foreground = Brush("#64748B"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 18)
                });
            }
            foreach (var el in body)
                panel.Children.Add(el);
            return panel;
        }

        private static SolidColorBrush Brush(string hex)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
        }

        private static TextBlock Label(string text) => new TextBlock
        {
            Text = text,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush("#475569"),
            Margin = new Thickness(0, 0, 0, 6)
        };

        private static TextBlock RequiredLabel(string text)
        {
            var tb = new TextBlock { FontSize = 12, Margin = new Thickness(0, 0, 0, 6) };
            tb.Inlines.Add(new System.Windows.Documents.Run(text + " ")
            {
                FontWeight = FontWeights.SemiBold,
                Foreground = Brush("#475569")
            });
            tb.Inlines.Add(new System.Windows.Documents.Run("required")
            {
                FontWeight = FontWeights.Bold,
                Foreground = Brush("#DC2626")
            });
            return tb;
        }

        private static TextBox Field(string text, bool readOnly = false)
        {
            var tb = new TextBox
            {
                Text = text,
                IsReadOnly = readOnly,
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                BorderBrush = Brush("#E2E8F0"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            if (readOnly) tb.Background = Brush("#F8FAFC");
            return tb;
        }

        private static Border Promise(string bold, string rest)
        {
            var sp = new StackPanel();
            var tb = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 13, Foreground = Brush("#1E3A5F") };
            tb.Inlines.Add(new System.Windows.Documents.Run(bold) { FontWeight = FontWeights.SemiBold, Foreground = Brush("#0066CC") });
            tb.Inlines.Add(new System.Windows.Documents.Run(" " + rest));
            sp.Children.Add(tb);
            return new Border
            {
                Background = Brush("#EFF6FF"),
                BorderBrush = Brush("#DBEAFE"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 16),
                Child = sp
            };
        }

        private static FrameworkElement Start(SetupState state)
        {
            var license = new TextBox
            {
                Height = 160,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = true,
                FontSize = 12,
                Padding = new Thickness(10),
                BorderBrush = Brush("#E2E8F0"),
                Background = Brush("#F8FAFC"),
                Margin = new Thickness(0, 0, 0, 12),
                Text =
                    "Software License and Usage Agreement -- Rev 001/Sep 2023\n\n" +
                    "IMPORTANT - READ CAREFULLY\n\n" +
                    "By exercising your rights to make and use copies of the Software, or keeping a copy " +
                    "or download for over 30 days, you agree to this Agreement.\n\n" +
                    "[Full license text unchanged from production installer.]"
            };

            var accept = new RadioButton { Content = "I accept the Agreement", GroupName = "License", Margin = new Thickness(0, 4, 0, 6) };
            var decline = new RadioButton { Content = "I don't accept", GroupName = "License" };
            accept.Checked += (_, __) => state.LicenseAccepted = true;
            decline.Checked += (_, __) => state.LicenseAccepted = false;
            if (state.LicenseAccepted) accept.IsChecked = true;
            else decline.IsChecked = true;

            return Shell(
                "Argent Job Scheduler Setup",
                null,
                Promise("Argent Job Scheduler Setup", ""),
                new TextBlock
                {
                    Text = "Close other apps if you can, then accept the license.",
                    FontSize = 14,
                    Foreground = Brush("#475569"),
                    Margin = new Thickness(0, 0, 0, 10)
                },
                license,
                accept,
                decline);
        }

        private static FrameworkElement SystemCheck(SetupState state)
        {
            var list = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            foreach (var p in state.Prerequisites)
            {
                var row = new Border
                {
                    BorderBrush = Brush("#E8EEF5"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8),
                    Background = Brushes.White
                };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var icon = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(8),
                    Background = Brush("#F1F5F9"),
                    Child = new TextBlock
                    {
                        Text = StatusIcon(p.Status),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.Bold
                    }
                };
                ApplyPrereqColors(row, icon, p.Status);

                var body = new StackPanel { Margin = new Thickness(10, 0, 10, 0) };
                body.Children.Add(new TextBlock { Text = p.Name, FontWeight = FontWeights.SemiBold, FontSize = 13 });
                body.Children.Add(new TextBlock { Text = p.Detail, FontSize = 12, Foreground = Brush("#64748B"), TextWrapping = TextWrapping.Wrap });

                var badge = new TextBlock
                {
                    Text = p.StatusLabel,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brush("#64748B"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(icon, 0);
                Grid.SetColumn(body, 1);
                Grid.SetColumn(badge, 2);
                grid.Children.Add(icon);
                grid.Children.Add(body);
                grid.Children.Add(badge);
                row.Child = grid;
                list.Children.Add(row);

                // refresh when status changes
                p.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(PrerequisiteItem.Status) || e.PropertyName == nameof(PrerequisiteItem.StatusLabel))
                    {
                        ((TextBlock)icon.Child).Text = StatusIcon(p.Status);
                        badge.Text = p.StatusLabel;
                        ApplyPrereqColors(row, icon, p.Status);
                    }
                };
            }

            UIElement summary = state.ScanDone
                ? Promise("Check complete", "Continue when you're ready")
                : (UIElement)new TextBlock
                {
                    Text = "Checking your system… this takes a few seconds.",
                    Foreground = Brush("#64748B"),
                    FontSize = 13
                };

            return Shell(
                "System check",
                "Scanning this computer for what Setup needs.",
                list,
                summary);
        }

        private static string StatusIcon(PrereqStatus s)
        {
            switch (s)
            {
                case PrereqStatus.Found: return "✓";
                case PrereqStatus.WillInstall: return "+";
                case PrereqStatus.Scanning: return "…";
                default: return "○";
            }
        }

        private static void ApplyPrereqColors(Border row, Border icon, PrereqStatus status)
        {
            if (status == PrereqStatus.Found)
            {
                row.Background = Brush("#F0FDF4");
                row.BorderBrush = Brush("#BBF7D0");
                icon.Background = Brush("#16A34A");
                ((TextBlock)icon.Child).Foreground = Brushes.White;
            }
            else if (status == PrereqStatus.WillInstall)
            {
                row.Background = Brush("#F8FBFF");
                row.BorderBrush = Brush("#BFDBFE");
                icon.Background = Brush("#0066CC");
                ((TextBlock)icon.Child).Foreground = Brushes.White;
            }
            else if (status == PrereqStatus.Scanning)
            {
                icon.Background = Brush("#EFF6FF");
                ((TextBlock)icon.Child).Foreground = Brush("#0066CC");
            }
        }

        private static FrameworkElement Install(SetupState state)
        {
            var ops = new[]
            {
                "Install Windows services and Argent Job Scheduler programs",
                "Install Argent Job Scheduler only",
                "Upgrade services and clients",
                "Deinstall"
            };
            var opPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            for (var i = 0; i < ops.Length; i++)
            {
                var rb = new RadioButton
                {
                    Content = ops[i],
                    GroupName = "Op",
                    Margin = new Thickness(0, 0, 0, 8),
                    IsChecked = state.InstallOperationIndex == i
                };
                var captured = i;
                rb.Checked += (_, __) => state.InstallOperationIndex = captured;
                opPanel.Children.Add(rb);
            }

            var node = Field(state.InstallNode);
            node.TextChanged += (_, __) => state.InstallNode = node.Text;

            var sched = new CheckBox { Content = "Argent Job Scheduler", IsChecked = state.InstallScheduler, Margin = new Thickness(0, 0, 0, 8) };
            sched.Checked += (_, __) => state.InstallScheduler = true;
            sched.Unchecked += (_, __) => state.InstallScheduler = false;
            var queue = new CheckBox { Content = "Argent Queue Engine", IsChecked = state.InstallQueueEngine };
            queue.Checked += (_, __) => state.InstallQueueEngine = true;
            queue.Unchecked += (_, __) => state.InstallQueueEngine = false;

            return Shell(
                "What to install",
                "Node, operation, and products",
                Info("Account", state.CurrentUser),
                Info("Domain / node", state.MachineName),
                Label("Install on node"),
                node,
                new TextBlock { Text = "OPERATION", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brush("#0066CC"), Margin = new Thickness(0, 4, 0, 8) },
                opPanel,
                new TextBlock { Text = "PRODUCTS", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brush("#0066CC"), Margin = new Thickness(0, 4, 0, 8) },
                sched,
                queue);
        }

        private static UIElement Info(string label, string value)
        {
            var g = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var l = new TextBlock { Text = label, Foreground = Brush("#64748B"), FontSize = 13 };
            var v = new TextBlock { Text = value, FontWeight = FontWeights.SemiBold, FontSize = 13 };
            Grid.SetColumn(v, 1);
            g.Children.Add(l);
            g.Children.Add(v);
            return g;
        }

        private static FrameworkElement PathsLicense(SetupState state)
        {
            var path = Field(state.LicenseFilePath);
            path.TextChanged += (_, __) => state.LicenseFilePath = path.Text;
            var key = Field(state.QueueEngineLicenseKey);
            key.TextChanged += (_, __) => state.QueueEngineLicenseKey = key.Text;
            var sched = Field(state.SchedulerOutputPath);
            sched.TextChanged += (_, __) => state.SchedulerOutputPath = sched.Text;
            var queue = Field(state.QueueOutputPath);
            queue.TextChanged += (_, __) => state.QueueOutputPath = queue.Text;
            var standalone = new CheckBox
            {
                Content = "Standalone Queue Engine",
                IsChecked = state.StandaloneQueueEngine,
                Margin = new Thickness(0, 0, 0, 12)
            };
            standalone.Checked += (_, __) => state.StandaloneQueueEngine = true;
            standalone.Unchecked += (_, __) => state.StandaloneQueueEngine = false;

            return Shell(
                "Paths & license",
                "License files and install folders — defaults match the current installer.",
                Label("Job Scheduler license file"),
                path,
                standalone,
                Label("Queue Engine license key"),
                key,
                new TextBlock { Text = "INSTALL FOLDERS", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brush("#0066CC"), Margin = new Thickness(0, 8, 0, 8) },
                Label("Source (input)"),
                Field(state.InputDirectory, true),
                Label("Job Scheduler"),
                sched,
                Label("Queue Engine"),
                queue);
        }

        private static FrameworkElement Account(SetupState state)
        {
            var gmsa = new CheckBox
            {
                Content = "Use Managed Service Account (gMSA)",
                IsChecked = state.UseManagedServiceAccount,
                Margin = new Thickness(0, 0, 0, 10)
            };
            gmsa.Checked += (_, __) => state.UseManagedServiceAccount = true;
            gmsa.Unchecked += (_, __) => state.UseManagedServiceAccount = false;

            var account = Field(state.ServiceAccount);
            account.TextChanged += (_, __) => state.ServiceAccount = account.Text;
            var pwd = new PasswordBox { Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 12) };
            var confirm = new PasswordBox { Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 12) };
            pwd.PasswordChanged += (_, __) => state.Password = pwd.Password;
            confirm.PasswordChanged += (_, __) => state.ConfirmPassword = confirm.Password;

            var sql = new CheckBox
            {
                Content = "Use SQL Server (7.0+) as database storage",
                IsChecked = state.UseSqlServer,
                Margin = new Thickness(0, 0, 0, 10)
            };
            sql.Checked += (_, __) => state.UseSqlServer = true;
            sql.Unchecked += (_, __) => state.UseSqlServer = false;

            var email = Field(state.Email);
            email.TextChanged += (_, __) => state.Email = email.Text;
            var contact = Field(state.Contact);
            contact.TextChanged += (_, __) => state.Contact = contact.Text;
            var company = Field(state.Company);
            company.TextChanged += (_, __) => state.Company = company.Text;
            var address = Field(state.Address);
            address.TextChanged += (_, __) => state.Address = address.Text;
            var city = Field(state.City);
            city.TextChanged += (_, __) => state.City = city.Text;
            var stateProv = Field(state.StateProv);
            stateProv.TextChanged += (_, __) => state.StateProv = stateProv.Text;
            var zip = Field(state.Zip);
            zip.TextChanged += (_, __) => state.Zip = zip.Text;
            var country = Field(state.Country);
            country.TextChanged += (_, __) => state.Country = country.Text;
            var phone = Field(state.Phone);
            phone.TextChanged += (_, __) => state.Phone = phone.Text;
            var salesRep = Field(state.SalesRep);
            salesRep.TextChanged += (_, __) => state.SalesRep = salesRep.Text;

            return Shell(
                "Account & contact",
                "Service logon, SQL, and registration — then Install runs.",
                gmsa,
                Label("Account (Domain\\User)"),
                account,
                Label("Password"),
                pwd,
                Label("Confirm"),
                confirm,
                Promise("SQL Server is the default -- CodeBase is not for production", ""),
                sql,
                new TextBlock
                {
                    Text = string.IsNullOrEmpty(state.SelectedOdbcDsn) ? "No ODBC DSN yet" : "DSN: " + state.SelectedOdbcDsn,
                    Foreground = Brush("#64748B"),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 12)
                },
                new TextBlock { Text = "REGISTRATION", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brush("#0066CC"), Margin = new Thickness(0, 4, 0, 8) },
                RequiredLabel("Email"),
                email,
                Label("Contact"),
                contact,
                Label("Company"),
                company,
                Label("Address"),
                address,
                Label("Town / City"),
                city,
                Label("State / Province"),
                stateProv,
                Label("ZIP / Postcode"),
                zip,
                Label("Country"),
                country,
                Label("Phone"),
                phone,
                Label("Account Manager"),
                salesRep);
        }

        private static FrameworkElement Installing(SetupState state)
        {
            var status = new TextBlock { Text = state.ProgressStatus, FontSize = 14, Margin = new Thickness(0, 0, 0, 12) };
            var bar = new ProgressBar { Height = 12, Minimum = 0, Maximum = 100, Value = state.SimulatedProgress };
            var pct = new TextBlock
            {
                Text = state.SimulatedProgress + "%",
                Foreground = Brush("#64748B"),
                Margin = new Thickness(0, 8, 0, 0)
            };
            var panel = Shell(
                "Installing",
                null,
                status,
                bar,
                pct,
                new TextBlock
                {
                    Text = "Missing prerequisites being installed",
                    FontSize = 14,
                    Foreground = Brush("#475569"),
                    Margin = new Thickness(0, 16, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                });
            panel.Tag = new ProgressRefs(status, bar, pct);
            return panel;
        }

        internal sealed class ProgressRefs
        {
            public ProgressRefs(TextBlock status, ProgressBar bar, TextBlock pct)
            {
                Status = status;
                Bar = bar;
                Pct = pct;
            }
            public TextBlock Status { get; }
            public ProgressBar Bar { get; }
            public TextBlock Pct { get; }
        }

        private static FrameworkElement Done(SetupState state)
        {
            return Shell(
                "You're set",
                "Installed in about " + state.InstallDurationSeconds + " seconds.",
                new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    LineHeight = 22,
                    Text =
                        "• Prerequisites checked — existing ones skipped; missing ones installed from Setup.\n\n" +
                        "• Sample jobs are ready in Job Scheduler.\n\n" +
                        "• Default Queue Engine account: " + state.DefaultQueueAccount + "\n\n" +
                        "• Support: Support@Argent.com or help.Argent.com"
                });
        }
    }
}
