using System.ComponentModel;

namespace VPExtensionManager.Models;

public enum VPVersion
{
    [Description("Unknown version")]
    Unknown = 0,
    [Description("For VP13 and earlier")]
    Sony = 13,
    [Description("For VP14 and later")]
    Magix = 14
}
