namespace Argent.SetupUi.Wizard
{
    public enum WizardStep
    {
        Welcome = 0,
        License = 1,
        NodeSummary = 2,
        InstallRemove = 3,
        LicenseDetails = 4,
        InstallPaths = 5,
        ServiceAndDatabase = 6,
        CustomerInfo = 7,
        Progress = 8,
        Complete = 9
    }

    public static class WizardStepNames
    {
        public static readonly string[] SidebarLabels =
        {
            "Welcome",
            "License",
            "Node summary",
            "Install / remove",
            "License details",
            "Install paths",
            "Service & database",
            "Customer info",
            "Installing",
            "Complete"
        };
    }
}
