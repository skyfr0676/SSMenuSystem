using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;

namespace ServerSpecificSyncer.Examples
{
    public class AbilitiesExample : Menu
    {
        public const float HealAllyHp = 50f;
        public const float HealAllyRange = 3.5f;
        public const byte BoostIntensity = 60;
        public const float BoostHealthDrain = 5f;
        public override ServerSpecificSettingBase[] Settings => GetSettings();
        private List<ServerSpecificSettingBase> _settings;
        private static readonly HashSet<ReferenceHub> ActiveSpeedBoosts = new();

        public void TryHealAlly(ReferenceHub sender)
        {
            ItemIdentifier curItem = sender.inventory.CurItem;
            if (curItem.TypeId != ItemType.Medkit)
                return;
            Vector3 position = sender.PlayerCameraReference.position;
            Vector3 forward = sender.PlayerCameraReference.forward;

            while (Physics.Raycast(position, forward, out var hitInfo, 3.5f) && hitInfo.collider.TryGetComponent<HitboxIdentity>(out var component) && !HitboxIdentity.IsEnemy(component.TargetHub, sender))
            {
                if (component.TargetHub == sender)
                {
                    position += forward * 0.08f;
                }
                else
                {
                    component.TargetHub.playerStats.GetModule<HealthStat>().ServerHeal(50f);
                    sender.inventory.ServerRemoveItem(curItem.SerialNumber, null);
                    break;
                }
            }
        }

        public void SetSpeedBoost(ReferenceHub hub, bool state)
        {
            MovementBoost effect = hub.playerEffectsController.GetEffect<MovementBoost>();
            if (state && hub.IsHuman())
            {
                effect.ServerSetState(60);
                ActiveSpeedBoosts.Add(hub);
            }
            else
            {
                effect.ServerDisable();
                ActiveSpeedBoosts.Remove(hub);
            }
        }
        private ServerSpecificSettingBase[] GetSettings()
        {
            _settings = new()
            {
                new SSGroupHeader("Abilities"),
                new Keybind(ExampleId.HealAlly, "Heal Ally", (hub, isPressed) =>
                    {
                        if (isPressed)
                            TryHealAlly(hub);
                    }, KeyCode.H, hint:
                    $"Press this key while holding a medkit to instantly heal a stationary ally for {HealAllyHp} HP.", isGlobal:false),
                new Keybind(ExampleId.SpeedBoostKey, "Speed Boost (Human-only)", (hub, isPressed) =>
                {
                    if (hub.GetParameter<AbilitiesExample, SSTwoButtonsSetting>(ExampleId.SpeedBoostToggle).SyncIsB)
                    {
                        if (!isPressed)
                            return;
                        SetSpeedBoost(hub, !ActiveSpeedBoosts.Contains(hub));
                    }
                    SetSpeedBoost(hub, isPressed);
                }, KeyCode.Y, hint: "Increase your speed by draining your health.", isGlobal: false),
                new SSTwoButtonsSetting(ExampleId.SpeedBoostToggle, "Speed Boost - Activation Mode", "Hold", "Toggle")
            };

            return _settings.ToArray();
        }

        internal static void OnDisconnect(ReferenceHub hub)
        {
            ActiveSpeedBoosts.Remove(hub);
        }

        public void OnRoleChanged(
            ReferenceHub userHub,
            PlayerRoleBase prevRole,
            PlayerRoleBase newRole)
        {
            this.SetSpeedBoost(userHub, false);
        }

        public override void OnRegistered()
        {
            ReferenceHub.OnPlayerRemoved += OnDisconnect;
            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
            StaticUnityMethods.OnUpdate += OnUpdate;
        }
        
        public void OnUpdate()
        {
            if (!StaticUnityMethods.IsPlaying)
                return;
            foreach (ReferenceHub activeSpeedBoost in ActiveSpeedBoosts)
            {
                if (!Mathf.Approximately(activeSpeedBoost.GetVelocity().SqrMagnitudeIgnoreY(), 0.0f))
                    activeSpeedBoost.playerStats.DealDamage(new UniversalDamageHandler(Time.deltaTime * 5f, DeathTranslations.Scp207));
            }
        }

        public override string Name { get; set; } = "Abilities Extension";
        public override int Id { get; set; } = -8;
        internal static class ExampleId
        {
            public static readonly int SpeedBoostKey = 0;
            public static readonly int SpeedBoostToggle = 1;
            public static readonly int HealAlly = 2;
        }
        public override Type MenuRelated { get; set; } = typeof(MainExample);
    }
}