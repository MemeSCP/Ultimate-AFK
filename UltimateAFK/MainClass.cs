using System;
using InventorySystem.Items.Firearms;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Enums;
using PluginAPI.Events;
using UltimateAFK.player;

namespace UltimateAFK
{
    public class MainClass
    {
        private UAFKPlayerFactory _playerFactory;
        [PluginConfig] public Config pluginConfig;

        [PluginEntryPoint("Ultimate AFK", "2.0.1", "Anti AFK System", "MemeSCP Community")]
        void LoadUAFK()
        {
            _playerFactory = new UAFKPlayerFactory(this);
            Log.Info("Loading Ultimate AFK");
            
            EventManager.RegisterEvents(this);
            FactoryManager.RegisterPlayerFactory(this, _playerFactory);

            if (pluginConfig.EnableDebugLog)
            {
                Log.Debug("Debug mode enabled.");
            }
        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        void OnPlayerSpawn(Player player, RoleTypeId roleType) => player.ResetAfkCounter();

        [PluginEvent(ServerEventType.PlayerUseHotkey)]
        void OnPlayerHotkey(Player player, ActionName action) => player.ResetAfkCounter();

        [PluginEvent(ServerEventType.PlayerMakeNoise)]
        void OnPlayerNoise(Player player) => player.ResetAfkCounter();

        [PluginEvent(ServerEventType.PlayerAimWeapon)]
        void OnPlayerAim(Player player, Firearm gun, bool isAiming) => player.ResetAfkCounter();

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        void OnPlayerChangeItem(Player player, ushort oldItem, ushort newItem) => player.ResetAfkCounter();
    }
}