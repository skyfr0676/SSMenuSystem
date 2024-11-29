using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Interfaces
{
    public interface ISetting
    {
        ServerSpecificSettingBase Base { get; }
    }
}