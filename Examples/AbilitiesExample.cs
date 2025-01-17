using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Examples
{
    internal class AbilitiesExample : Menu
    {
        private const float HealAllyHp = 50f;
        private const float HealAllyRange = 3.5f;
        private const byte BoostIntensity = 60;
        private const float BoostHealthDrain = 5f;
        private List<ServerSpecificSettingBase> _settings;
        private static readonly HashSet<ReferenceHub> ActiveSpeedBoosts = new();

        public override ServerSpecificSettingBase[] Settings => GetSettings();

        private void TryHealAlly(ReferenceHub sender)
        {
            ItemIdentifier curItem = sender.inventory.CurItem;
            if (curItem.TypeId != ItemType.Medkit)
                return;
            Vector3 position = sender.PlayerCameraReference.position;
            Vector3 forward = sender.PlayerCameraReference.forward;

            while (Physics.Raycast(position, forward, out RaycastHit hitInfo, HealAllyRange) && hitInfo.collider.TryGetComponent<HitboxIdentity>(out HitboxIdentity component) && !HitboxIdentity.IsEnemy(component.TargetHub, sender))
            {
                if (component.TargetHub == sender)
                {
                    position += forward * 0.08f;
                }
                else
                {
                    component.TargetHub.playerStats.GetModule<HealthStat>().ServerHeal(HealAllyHp);
                    sender.inventory.ServerRemoveItem(curItem.SerialNumber, null);
                    break;
                }
            }
        }

        private void SetSpeedBoost(ReferenceHub hub, bool state)
        {
            MovementBoost effect = hub.playerEffectsController.GetEffect<MovementBoost>();
            if (state && hub.IsHuman())
            {
                effect.ServerSetState(BoostIntensity);
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
            _settings = new List<ServerSpecificSettingBase>
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
                    else
                        SetSpeedBoost(hub, isPressed);
                }, KeyCode.Y, hint: "Increase your speed by draining your health.", isGlobal: false),
                new SSTwoButtonsSetting(ExampleId.SpeedBoostToggle, "Speed Boost - Activation Mode", "Hold", "Toggle")
            };

            return _settings.ToArray();
        }

        private static void OnDisconnect(ReferenceHub hub)
        {
            ActiveSpeedBoosts.Remove(hub);
        }

        private void OnRoleChanged(
            ReferenceHub userHub,
            PlayerRoleBase prevRole,
            PlayerRoleBase newRole)
        {
            this.SetSpeedBoost(userHub, false);
        }

        protected override void OnRegistered()
        {
            ReferenceHub.OnPlayerRemoved += OnDisconnect;
            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
            StaticUnityMethods.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            if (!StaticUnityMethods.IsPlaying)
                return;
            foreach (ReferenceHub activeSpeedBoost in ActiveSpeedBoosts)
            {
                if (!Mathf.Approximately(activeSpeedBoost.GetVelocity().SqrMagnitudeIgnoreY(), 0.0f))
                    activeSpeedBoost.playerStats.DealDamage(new UniversalDamageHandler(Time.deltaTime * BoostHealthDrain, DeathTranslations.Scp207));
            }
        }

        public override string Name { get; set; } = "Abilities Extension";
        public override int Id { get; set; } = -8;
        private static class ExampleId
        {
            public static readonly int SpeedBoostKey = 5;
            public static readonly int SpeedBoostToggle = 7;
            public static readonly int HealAlly = 9;
        }
        public override Type MenuRelated { get; set; } = typeof(MainExample);
    }
}