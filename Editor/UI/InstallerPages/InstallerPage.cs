using VRC.FVPM.ScriptableObjects;

namespace VRC.FVPM.UI.InstallerPages
{
    public abstract class InstallerPage
    {
        public readonly PackageInstallationPopup Window;
        public readonly FvpmPackage Package;

        public InstallerPage(PackageInstallationPopup window, FvpmPackage package)
        {
            Window = window;
            Package = package;
        }

        public abstract void Draw();
    }
}