using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features.Interfaces
{
    internal interface ISetting
    {
        ServerSpecificSettingBase Base { get; }
    }
}