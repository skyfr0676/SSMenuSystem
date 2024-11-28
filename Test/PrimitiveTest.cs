using System;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Test
{
    public class PrimitiveTest : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new SSButton(2, "test", "test"),
            new SSTwoButtonsSetting(3, "test", "optA", "optB", true, "jnique ta soeur"),
        };
        public override string Name { get; set; } = "Primitve Example";
        public override int Id { get; set; } = -2;

        public override string Description { get; set; } = "Ceci est un test pour voir si tout foncitonne.";

        public override void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
        {
            if (setting is SSButton && setting.SettingId == 2)
                throw new ArgumentException("this is a test exception");
        }
    }
}