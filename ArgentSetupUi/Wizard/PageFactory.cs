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
                case WizardStep.Welcome: return Welcome(state);
                case WizardStep.License: return License(state);
                case WizardStep.NodeSummary: return NodeSummary(state);
                case WizardStep.InstallRemove: return InstallRemove(state);
                case WizardStep.LicenseDetails: return LicenseDetails(state);
                case WizardStep.InstallPaths: return InstallPaths(state);
                case WizardStep.ServiceAndDatabase: return ServiceAndDatabase(state);
                case WizardStep.CustomerInfo: return CustomerInfo(state);
                case WizardStep.Progress: return Progress(state);
                case WizardStep.Complete: return Complete(state);
                default: return new TextBlock { Text = "Unknown step" };
            }
        }

        private static StackPanel Shell(string title, string subtitle, params UIElement[] body)
        {
            var panel = new StackPanel { MaxWidth = 640 };
            panel.Children.Add(new TextBlock { Text = title, Style = (Style)Application.Current.FindResource("WizardTitle") });
            if (!string.IsNullOrEmpty(subtitle))
                panel.Children.Add(new TextBlock { Text = subtitle, Style = (Style)Application.Current.FindResource("WizardSubtitle") });
            foreach (var el in body)
                panel.Children.Add(el);
            return panel;
        }

        private static TextBlock Label(string text) =>
            new TextBlock { Text = text, Style = (Style)Application.Current.FindResource("FieldLabel") };

        private static TextBox Field(string text, bool readOnly = false)
        {
            var tb = new TextBox
            {
                Text = text,
                IsReadOnly = readOnly,
                Style = (Style)Application.Current.FindResource("WizardTextBox")
            };
            if (readOnly)
            {
                tb.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
            }
            return tb;
        }

        private static FrameworkElement Welcome(SetupState state)
        {
            var body = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                LineHeight = 22,
                Foreground = (Brush)Application.Current.FindResource("TextPrimaryBrush"),
                Text =
                    "Install Argent Job Scheduler and/or Queue Engine in a few clear steps.\n\n" +
                    "Everything is inside this Setup — no secondary downloads of .NET Framework " +
                    "(or version strings like “.Net 4.x.y.z”). Runtime pieces ship embedded so busy " +
                    "admins are not sent hunting for another installer.\n\n" +
                    "Close other apps if you can, then continue. You can go back any time before install starts.\n\n" +
                    "WARNING: This program product is protected by copyright law and international treaties. " +
                    "Unauthorized reproduction or distribution of this program, or any portion of it, may result " +
                    "in severe civil and criminal penalties, and will be prosecuted to the maximum extent possible under law."
            };
            return Shell("Install Argent in a few steps", null, body);
        }

        private static FrameworkElement License(SetupState state)
        {
            var license = new TextBox
            {
                Height = 280,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = true,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Padding = new Thickness(10),
                BorderBrush = (Brush)Application.Current.FindResource("BorderBrush"),
                Text =
                    "Software License and Usage Agreement -- Rev 001/Sep 2023\n\n" +
                    "IMPORTANT - READ CAREFULLY\n\n" +
                    "By exercising your rights to make and use copies of the Software (as may be provided for below), " +
                    "or keeping a copy or download of the Software for over 30 days, you agree to be bound by the terms " +
                    "of this Agreement. If you do not agree to the terms of this Agreement, do not use the Software.\n\n" +
                    "[Full license text would appear here in production — unchanged from the existing installer.]"
            };

            var accept = new RadioButton { Content = "I accept the Agreement", GroupName = "License", Margin = new Thickness(0, 8, 0, 4) };
            var decline = new RadioButton { Content = "I don't accept the Agreement", GroupName = "License" };
            accept.Checked += (_, __) => state.LicenseAccepted = true;
            decline.Checked += (_, __) => state.LicenseAccepted = false;
            if (state.LicenseAccepted) accept.IsChecked = true;

            return Shell(
                "License agreement",
                "Please read the following License Agreement. You must accept the Agreement to continue Setup.",
                license, accept, decline);
        }

        private static FrameworkElement NodeSummary(SetupState state)
        {
            var statusCard = new Border { Style = (Style)Application.Current.FindResource("GroupCard") };
            var statusPanel = new StackPanel();
            statusPanel.Children.Add(new TextBlock
            {
                Text = "Program status",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            statusPanel.Children.Add(new TextBlock { Text = $"Queue Engine — {state.QueueEngineStatus}", Margin = new Thickness(0, 0, 0, 4) });
            statusPanel.Children.Add(new TextBlock { Text = $"Argent Job Scheduler — {state.SchedulerStatus}" });
            statusCard.Child = statusPanel;

            var nodeBox = Field(state.InstallNode);
            nodeBox.TextChanged += (_, __) => state.InstallNode = nodeBox.Text;

            return Shell(
                $"Summary information for node {state.MachineName}",
                null,
                InfoRow("Current account", state.CurrentUser),
                InfoRow("Current domain", state.CurrentDomain),
                InfoRow("Current node", state.MachineName),
                Label("Install program on node"),
                nodeBox,
                statusCard);
        }

        private static UIElement InfoRow(string label, string value)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var l = new TextBlock { Text = label, Foreground = (Brush)Application.Current.FindResource("TextSecondaryBrush") };
            var v = new TextBlock { Text = value, FontWeight = FontWeights.SemiBold };
            Grid.SetColumn(v, 1);
            grid.Children.Add(l);
            grid.Children.Add(v);
            return grid;
        }

        private static FrameworkElement InstallRemove(SetupState state)
        {
            var ops = new[]
            {
                "Install Windows services including client programs",
                "Install client programs only",
                "Upgrade both Windows services and client programs",
                "Deinstall"
            };

            var opPanel = new StackPanel();
            for (var i = 0; i < ops.Length; i++)
            {
                var rb = new RadioButton
                {
                    Content = ops[i],
                    GroupName = "Operation",
                    Margin = new Thickness(0, 0, 0, 8),
                    IsChecked = state.InstallOperationIndex == i
                };
                var captured = i;
                rb.Checked += (_, __) => state.InstallOperationIndex = captured;
                opPanel.Children.Add(rb);
            }

            var opCard = new Border { Style = (Style)Application.Current.FindResource("GroupCard"), Child = opPanel };

            var sched = new CheckBox
            {
                Content = "Argent Job Scheduler",
                IsChecked = state.InstallScheduler,
                Margin = new Thickness(0, 0, 0, 8)
            };
            sched.Checked += (_, __) => state.InstallScheduler = true;
            sched.Unchecked += (_, __) => state.InstallScheduler = false;

            var queue = new CheckBox
            {
                Content = "Argent Queue Engine",
                IsChecked = state.InstallQueueEngine
            };
            queue.Checked += (_, __) => state.InstallQueueEngine = true;
            queue.Unchecked += (_, __) => state.InstallQueueEngine = false;

            return Shell(
                "Install / remove",
                $"Install or remove programs on node {state.InstallNode}.",
                Label("Install/remove program on node"),
                Field(state.InstallNode, readOnly: true),
                opCard,
                sched,
                queue);
        }

        private static FrameworkElement LicenseDetails(SetupState state)
        {
            var pathGrid = new Grid();
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var pathBox = Field(state.LicenseFilePath);
            pathBox.TextChanged += (_, __) => state.LicenseFilePath = pathBox.Text;
            var browse = new Button
            {
                Content = "Browse…",
                Style = (Style)Application.Current.FindResource("SecondaryButton"),
                Margin = new Thickness(8, 0, 0, 14),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(pathBox, 0);
            Grid.SetColumn(browse, 1);
            pathGrid.Children.Add(pathBox);
            pathGrid.Children.Add(browse);

            var standalone = new CheckBox
            {
                Content = "Standalone Queue Engine",
                IsChecked = state.StandaloneQueueEngine,
                Margin = new Thickness(0, 0, 0, 12)
            };
            standalone.Checked += (_, __) => state.StandaloneQueueEngine = true;
            standalone.Unchecked += (_, __) => state.StandaloneQueueEngine = false;

            var keyBox = Field(state.QueueEngineLicenseKey);
            keyBox.TextChanged += (_, __) => state.QueueEngineLicenseKey = keyBox.Text;

            var keyCard = new Border { Style = (Style)Application.Current.FindResource("GroupCard") };
            var keyPanel = new StackPanel();
            keyPanel.Children.Add(Label("Argent Queue Engine license key"));
            keyPanel.Children.Add(keyBox);
            keyCard.Child = keyPanel;

            return Shell(
                "License details",
                "Specify your Argent license file. If you do not have one, visit Argent.com — Products and Support.",
                Label("Install/remove program on node"),
                Field(state.InstallNode, readOnly: true),
                Label("Argent Job Scheduler license file"),
                pathGrid,
                standalone,
                keyCard);
        }

        private static FrameworkElement InstallPaths(SetupState state)
        {
            var schedBox = Field(state.SchedulerOutputPath);
            schedBox.TextChanged += (_, __) => state.SchedulerOutputPath = schedBox.Text;
            var queueBox = Field(state.QueueOutputPath);
            queueBox.TextChanged += (_, __) => state.QueueOutputPath = queueBox.Text;

            var paths = new Border { Style = (Style)Application.Current.FindResource("GroupCard") };
            var pathsPanel = new StackPanel();
            pathsPanel.Children.Add(Label("Argent Job Scheduler"));
            pathsPanel.Children.Add(schedBox);
            pathsPanel.Children.Add(Label("Argent Queue Engine"));
            pathsPanel.Children.Add(queueBox);
            paths.Child = pathsPanel;

            return Shell(
                "Installation paths",
                "Choose where Setup will copy program files. Defaults match the current installer.",
                Label("Install program on node"),
                Field(state.InstallNode, readOnly: true),
                Label("Input directory"),
                Field(state.InputDirectory, readOnly: true),
                paths);
        }

        private static FrameworkElement ServiceAndDatabase(SetupState state)
        {
            var gmsa = new CheckBox
            {
                Content = "Use Managed Service Account (gMSA)",
                IsChecked = state.UseManagedServiceAccount,
                Margin = new Thickness(0, 0, 0, 12)
            };
            gmsa.Checked += (_, __) => state.UseManagedServiceAccount = true;
            gmsa.Unchecked += (_, __) => state.UseManagedServiceAccount = false;

            var accountBox = Field(state.ServiceAccount);
            accountBox.TextChanged += (_, __) => state.ServiceAccount = accountBox.Text;

            var pwd = new PasswordBox { Style = (Style)Application.Current.FindResource("WizardPasswordBox") };
            var confirm = new PasswordBox { Style = (Style)Application.Current.FindResource("WizardPasswordBox") };
            pwd.PasswordChanged += (_, __) => state.Password = pwd.Password;
            confirm.PasswordChanged += (_, __) => state.ConfirmPassword = confirm.Password;

            var svcCard = new Border { Style = (Style)Application.Current.FindResource("GroupCard") };
            var svcPanel = new StackPanel();
            svcPanel.Children.Add(gmsa);
            svcPanel.Children.Add(Label("Account (Domain\\User)"));
            svcPanel.Children.Add(accountBox);
            svcPanel.Children.Add(Label("Password"));
            svcPanel.Children.Add(pwd);
            svcPanel.Children.Add(Label("Confirm password"));
            svcPanel.Children.Add(confirm);
            svcCard.Child = svcPanel;

            var sql = new CheckBox
            {
                Content = "Use SQL Server (7.0 or above) as Database Storage",
                IsChecked = state.UseSqlServer,
                Margin = new Thickness(0, 0, 0, 8)
            };
            sql.Checked += (_, __) => state.UseSqlServer = true;
            sql.Unchecked += (_, __) => state.UseSqlServer = false;

            var callout = new Border { Style = (Style)Application.Current.FindResource("InfoCallout") };
            callout.Child = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Text =
                    "Default database storage is SQL Server (not CodeBase). CodeBase is fine for small evaluations " +
                    "but should not be used in production. Contact help.Argent.com for assistance."
            };

            var dsnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var dsnLabel = new TextBlock
            {
                Text = string.IsNullOrEmpty(state.SelectedOdbcDsn)
                    ? "No ODBC DSN selected"
                    : $"ODBC DSN: {state.SelectedOdbcDsn}",
                Foreground = (Brush)Application.Current.FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            var advanced = new Button
            {
                Content = "Advanced…",
                Style = (Style)Application.Current.FindResource("SecondaryButton"),
                Margin = new Thickness(12, 0, 0, 0)
            };
            dsnRow.Children.Add(dsnLabel);
            dsnRow.Children.Add(advanced);

            return Shell(
                "Service account & database",
                "Configure the Windows service logon and optional SQL Server backend.",
                svcCard,
                callout,
                sql,
                dsnRow);
        }

        private static FrameworkElement CustomerInfo(SetupState state)
        {
            var fields = new[]
            {
                ("Email address", state.Email, true),
                ("Contact", "", false),
                ("Company", "", false),
                ("Address", "", false),
                ("Town/City", "", false),
                ("State/Province", "", false),
                ("ZIP/Postcode", "", false),
                ("Country", "", false),
                ("Phone", "", false),
                ("Sales rep", "", false)
            };

            var panel = new StackPanel();
            foreach (var (label, value, bindEmail) in fields)
            {
                panel.Children.Add(Label(label));
                var box = Field(value);
                if (bindEmail)
                {
                    box.Text = state.Email;
                    box.TextChanged += (_, __) => state.Email = box.Text;
                }
                panel.Children.Add(box);
            }

            return Shell(
                "Customer information",
                "Registration details are stored under Software\\Argent\\Customer (same as the legacy installer).",
                panel);
        }

        private static FrameworkElement Progress(SetupState state)
        {
            var status = new TextBlock
            {
                Name = "ProgressStatusText",
                Text = state.ProgressStatus,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 12)
            };
            var bar = new ProgressBar
            {
                Name = "ProgressBar",
                Height = 24,
                Minimum = 0,
                Maximum = 100,
                Value = state.SimulatedProgress
            };
            var pct = new TextBlock
            {
                Name = "ProgressPctText",
                Text = $"{state.SimulatedProgress}%",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0),
                Foreground = (Brush)Application.Current.FindResource("TextSecondaryBrush")
            };

            var panel = Shell(
                "Installing",
                "Setup is copying files, updating the registry, and configuring services.",
                status, bar, pct);
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

        private static FrameworkElement Complete(SetupState state)
        {
            var card = new Border { Style = (Style)Application.Current.FindResource("GroupCard") };
            card.Child = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                LineHeight = 22,
                Text =
                    $"Argent Job Scheduler and Argent Queue Engine were installed in {state.InstallDurationSeconds} seconds.\n\n" +
                    "Ready-to-run sample jobs were installed with Argent Job Scheduler. You can copy and edit these sample jobs.\n\n" +
                    "Sample cmd files and queues were created with Queue Engine. " +
                    $"Account `{state.DefaultQueueAccount}` is installed as the default account for Queue Engine.\n\n" +
                    "Please contact Support at Support@Argent.com or help.Argent.com."
            };

            return Shell("Setup complete", null, card);
        }
    }
}
