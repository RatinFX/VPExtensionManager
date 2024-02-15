using System.ComponentModel;

namespace VPExtensionManager.Models;

public enum VPVersion
{
    [Description("EnumDescriptionVPVersionUnknown")]
    Unknown = 0,
    [Description("EnumDescriptionVPVersionSony")]
    Sony = 13,
    [Description("EnumDescriptionVPVersionMagix")]
    Magix = 14
}
