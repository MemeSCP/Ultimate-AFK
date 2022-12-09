using System.Linq;
using InventorySystem;
using PlayableScps;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using UnityEngine;

namespace UltimateAFK.player
{
    public class UAFKPlayer : Player
    {
        private readonly IGameComponent _player;
        private readonly MainClass _plugin;
        
        private int _afkTime = 0;
        private int _afkCount = 0;

        private float _periodicity = 1.0f;
        private float _timer = 0.0f;

        private Vector3 _lastAngle = Vector3.zero;
        private Vector3 _lastPos = Vector3.zero;

        public UAFKPlayer(IGameComponent player, MainClass plugin) : base(player)
        {
            _player = player;
            _plugin = plugin;
            
            Log.Debug("UAFK Player Created.");
        }

        public void ResetAfkCounter()
        {
            _afkTime = 0;
            _lastAngle = Rotation;
            _lastPos = Position;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (IsServer) return;

            _timer += Time.deltaTime;
            if (_timer < _periodicity) return;

            _timer = 0;
            
            Log.Debug($"OnUpdate - {Nickname} - {_afkTime} - {(IsExcludedFromCheck() ? "yes" : "no")}");

            if (IsExcludedFromCheck()) return;

            switch (Role)
            {
                case RoleTypeId.Scp079:
                    if (Camera.position.Equals(_lastPos) && Camera.eulerAngles.Equals(_lastAngle))
                        _afkTime++;
                    else
                        ResetAfkCounter();
                    break;
                case RoleTypeId.Scp096:
                    var controller96 = ReferenceHub.scpsController.CurrentScp as Scp096;
                    
                    if (controller96 != null && controller96.PlayerState == Scp096PlayerState.TryNotToCry)
                        ResetAfkCounter();
                    else
                    {
                        if (Position.Equals(_lastPos) && Rotation.Equals(_lastAngle))
                            _afkTime++;
                        else
                            ResetAfkCounter();
                    }

                    break;
                default:
                    Log.Debug($"{Position} / {_lastPos} / {Position.Equals(_lastPos)}");
                    Log.Debug($"{Rotation} / {_lastAngle} / {Rotation.Equals(_lastAngle)}");
                    if (Position.Equals(_lastPos) && Rotation.Equals(_lastAngle))
                        _afkTime++;
                    else
                        ResetAfkCounter();
                    break;
            }

            if (_afkTime == 0 || _afkTime < _plugin.pluginConfig.AfkTime) return;

            var secUntilReplacing = _plugin.pluginConfig.AfkTime + _plugin.pluginConfig.GraceTime - _afkTime;
            if (secUntilReplacing > 0)
            {
                this.PBroadcast($"{_plugin.pluginConfig.MsgPrefix} {_plugin.pluginConfig.MsgGrace.Replace("%timeleft%", secUntilReplacing.ToString())}", 1, true);
                return;
            }

            ResetAfkCounter();
            
            if (Role == RoleTypeId.Spectator) return;

            HandleReplacement();
        }

        private void HandleReplacement()
        {
            if (ShouldReplace()) FindAndSpawnReplacement();
            
            this.ClearInventory();
            SetRole(RoleTypeId.Spectator);
            this.PBroadcast($"{_plugin.pluginConfig.MsgPrefix} {_plugin.pluginConfig.MsgFspec}", 10, true);

            _afkCount++;
            ResetAfkCounter();
            
            if (_afkCount >= _plugin.pluginConfig.NumBeforeKick)
            {
                Disconnect(_plugin.pluginConfig.MsgKick);
            }
        }

        private void FindAndSpawnReplacement()
        {
            var toReplaceWith = GetPlayers<UAFKPlayer>().RandomItem();

            var trying = 0;

            while (
                toReplaceWith.Role != RoleTypeId.Spectator ||
                toReplaceWith.IsOverwatchEnabled ||
                toReplaceWith == this
            )
            {
                //If we didn't find a good condidate 5 times, just skip it.
                if (trying > 4) return;
                
                trying++;
                toReplaceWith = GetPlayers<UAFKPlayer>().RandomItem();
            }

            var items = ReferenceHub.inventory.UserInventory.Items.ToDictionary(pair => pair.Key, pair => pair.Value);
            var ammo = ReferenceHub.inventory.UserInventory.ReserveAmmo.ToDictionary(pair => pair.Key, pair => pair.Value);
            
            toReplaceWith.SetRole(Role);
            toReplaceWith.ClearInventory();
            toReplaceWith.Position = Position;
            toReplaceWith.Rotation = Rotation;
            toReplaceWith.PBroadcast($"{_plugin.pluginConfig.MsgPrefix} {_plugin.pluginConfig.MsgReplace}", 10, true);

            var replacedInventory = toReplaceWith.ReferenceHub.inventory;

            foreach (var ammoPair in ammo)
            {
                replacedInventory.ServerAddAmmo(ammoPair.Key, ammoPair.Value);
            }

            foreach (var itemPair in items)
            {
                replacedInventory.ServerAddItem(itemPair.Value.ItemTypeId, itemPair.Key, null);
            }
        }

        private bool IsExcludedFromCheck()
        {
            return
                Role is RoleTypeId.None or RoleTypeId.Spectator ||
                GetPlayers<UAFKPlayer>().Count < _plugin.pluginConfig.MinPlayers ||
                _plugin.pluginConfig.IgnoreTut && Role == RoleTypeId.Tutorial;
        }

        private bool ShouldReplace()
        {
            return
                _plugin.pluginConfig.TryReplace && (
                    _plugin.pluginConfig.MaxReplaceTime == -1 ||
                    Statistics.Round.Duration.TotalSeconds <= _plugin.pluginConfig.MaxReplaceTime
                );
        }
    }
}