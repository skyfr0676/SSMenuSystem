// -----------------------------------------------------------------------
// <copyright file="AbilitiesExample.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Examples;

using System;
using System.Collections.Generic;

using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using Features;
using Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

/// <summary>
/// An example menu for demonstrating various abilities.
/// </summary>
internal class AbilitiesExample : Menu
{
    private const float HealAllyHp = 50f;
    private const float HealAllyRange = 3.5f;
    private const byte BoostIntensity = 60;
    private const float BoostHealthDrain = 5f;
    private static readonly HashSet<ReferenceHub> ActiveSpeedBoosts = new ();
    private List<ServerSpecificSettingBase> settings = null!;

    /// <inheritdoc/>
    public override string Name { get; set; } = "Abilities Extension";

    /// <inheritdoc/>
    public override int Id { get; set; } = -8;

    /// <inheritdoc/>
    public override Type? MenuRelated { get; set; } = typeof(MainExample);

    /// <inheritdoc/>
    public override ServerSpecificSettingBase[] Settings => this.GetSettings();

    /// <inheritdoc/>
    protected override void OnRegistered()
    {
        ReferenceHub.OnPlayerRemoved += OnDisconnect;
        PlayerRoleManager.OnRoleChanged += this.OnRoleChanged;
        StaticUnityMethods.OnUpdate += this.OnUpdate;
    }

    private static void OnDisconnect(ReferenceHub hub)
    {
        ActiveSpeedBoosts.Remove(hub);
    }

    private void TryHealAlly(ReferenceHub sender)
    {
        ItemIdentifier curItem = sender.inventory.CurItem;
        if (curItem.TypeId != ItemType.Medkit)
        {
            return;
        }

        Vector3 position = sender.PlayerCameraReference.position;
        Vector3 forward = sender.PlayerCameraReference.forward;

        while (Physics.Raycast(position, forward, out RaycastHit hitInfo, HealAllyRange) && hitInfo.collider.TryGetComponent(out HitboxIdentity component) && !HitboxIdentity.IsEnemy(component.TargetHub, sender))
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
        this.settings =
        [
            new SSGroupHeader("Abilities"), new Keybind(
                ExampleId.HealAlly,
                "Heal Ally",
                (hub, isPressed) =>
                {
                    if (isPressed)
                    {
                        this.TryHealAlly(hub);
                    }
                },
                suggestedKey: KeyCode.H,
                hint: $"Press this key while holding a medkit to instantly heal a stationary ally for {HealAllyHp} HP.",
                isGlobal: false),

            new Keybind(
                ExampleId.SpeedBoostKey,
                "Speed Boost (Human-only)",
                (hub, isPressed) =>
                {
                    if (hub.GetParameter<AbilitiesExample, SSTwoButtonsSetting>(ExampleId.SpeedBoostToggle) !.SyncIsB)
                    {
                        if (!isPressed)
                        {
                            return;
                        }

                        this.SetSpeedBoost(hub, !ActiveSpeedBoosts.Contains(hub));
                    }
                    else
                    {
                        this.SetSpeedBoost(hub, isPressed);
                    }
                },
                suggestedKey: KeyCode.Y,
                hint: "Increase your speed by draining your health.",
                isGlobal: false),

            new SSTwoButtonsSetting(
                ExampleId.SpeedBoostToggle,
                "Speed Boost - Activation Mode",
                "Hold",
                "Toggle"),

        ];

        return this.settings.ToArray();
    }

    private void OnRoleChanged(
        ReferenceHub userHub,
        PlayerRoleBase prevRole,
        PlayerRoleBase newRole)
    {
        this.SetSpeedBoost(userHub, false);
    }

    private void OnUpdate()
    {
        if (!StaticUnityMethods.IsPlaying)
        {
            return;
        }

        foreach (ReferenceHub activeSpeedBoost in ActiveSpeedBoosts)
        {
            if (!Mathf.Approximately(activeSpeedBoost.GetVelocity().SqrMagnitudeIgnoreY(), 0.0f))
            {
                activeSpeedBoost.playerStats.DealDamage(new UniversalDamageHandler(Time.deltaTime * BoostHealthDrain, DeathTranslations.Scp207));
            }
        }
    }

    private static class ExampleId
    {
        public static readonly int SpeedBoostKey = 5;
        public static readonly int SpeedBoostToggle = 7;
        public static readonly int HealAlly = 9;
    }
}