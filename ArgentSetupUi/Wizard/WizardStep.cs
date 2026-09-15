namespace Argent.SetupUi.Wizard
{
    public enum WizardStep
    {
        Start = 0,
        SystemCheck = 1,
        Install = 2,
        PathsLicense = 3,
        Account = 4,
        Installing = 5,
        Done = 6
    }

    public static class WizardStepNames
    {
        public static readonly string[] SidebarLabels =
        {
            "Start",
            "System check",
            "Install",
            "Paths & license",
            "Account",
            "Installing",
            "Done"
        };
    }
}
