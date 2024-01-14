using System.ComponentModel;

namespace VPExtensionManager.Core.Models;

public enum VPVersion
{
    Unknown = 0,
    [Description("For VP13 and below")]
    Sony = 13,
    [Description("For VP14 and above")]
    Magix = 14
}
