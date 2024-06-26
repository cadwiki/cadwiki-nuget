using System;

namespace cadwiki.NetUtils
{

    public static class VersionExtension
    {
        public static Version IncrementRevision(this Version version)
        {
            return version.AddVersion(0, 0, 0, 1);
        }
        public static Version IncrementBuild(this Version version)
        {
            return version.IncrementBuild(true);
        }
        public static Version IncrementBuild(this Version version, bool resetLowerNumbers)
        {
            return version.AddVersion(0, 0, 1, resetLowerNumbers ? -version.Revision : 0);
        }
        public static Version IncrementMinor(this Version version)
        {
            return version.IncrementMinor(true);
        }
        public static Version IncrementMinor(this Version version, bool resetLowerNumbers)
        {
            return version.AddVersion(0, 1, resetLowerNumbers ? -version.Build : 0, resetLowerNumbers ? -version.Revision : 0);
        }
        public static Version IncrementMajor(this Version version)
        {
            return version.IncrementMajor(true);
        }
        public static Version IncrementMajor(this Version version, bool resetLowerNumbers)
        {
            return version.AddVersion(1, resetLowerNumbers ? -version.Minor : 0, resetLowerNumbers ? -version.Build : 0, resetLowerNumbers ? -version.Revision : 0);
        }

        public static Version AddVersion(this Version version, string pAddVersion)
        {
            return version.AddVersion(new Version(pAddVersion));
        }
        public static Version AddVersion(this Version version, Version pAddVersion)
        {
            return version.AddVersion(pAddVersion.Major, pAddVersion.Minor, pAddVersion.Build, pAddVersion.Revision);
        }
        public static Version AddVersion(this Version version, int major, int minor, int build, int revision)
        {
            return version.SetVersion(version.Major + major, version.Minor + minor, version.Build + build, version.Revision + revision);
        }
        public static Version SetVersion(this Version version, int major, int minor, int build, int revision)
        {
            return new Version(major, minor, build, revision);
        }

    }
}