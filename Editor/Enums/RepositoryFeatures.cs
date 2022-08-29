using System;

namespace VRC.FVPM.Enums
{
    [Flags]
    public enum RepositoryFeatures
    {
        Icon = 0b1,
        Banner = 0b10,
        FvpmAuthorizationOptional = 0b100,
        FvpmAuthorizationRequired = 0b1000,
        FvpmUpload = 0b10000,
    }
}