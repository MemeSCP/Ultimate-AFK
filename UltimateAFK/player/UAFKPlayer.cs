using PlayableScps;
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

        private Vector3 _lastAngle;
        private Vector3 _lastPos;

        public UAFKPlayer(IGameComponent player, MainClass plugin) : base(player)
        {
            _player = player;
            _plugin = plugin;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _timer += Time.deltaTime;
            if (_timer < _periodicity) return;

            _timer = 0;
            if (IsExcludedFromCheck()) return;

            switch (Role)
            {
                case RoleType.Scp079:
                    if (!Camera.position.Equals(_lastPos) || !Camera.eulerAngles.Equals(_lastAngle))
                        _afkTime++;
                    else
                        _afkTime = 0;
                    break;
                case RoleType.Scp096:
                    var controller96 = ReferenceHub.scpsController.CurrentScp as Scp096;
                    
                    if (controller96 != null && controller96.PlayerState == Scp096PlayerState.TryNotToCry)
                        _afkTime = 0;
                    else
                    {
                        if (!Position.Equals(_lastPos) || !Rotation.Equals(_lastAngle))
                            _afkTime++;
                        else
                            _afkTime = 0;
                    }

                    break;
                default:
                    if (!Position.Equals(_lastPos) || !Rotation.Equals(_lastAngle))
                        _afkTime++;
                    else
                        _afkTime = 0;
                    break;
            }

            if (_afkTime == 0 || _afkTime < _plugin.pluginConfig.AfkTime) return;

            var secUntilReplacing = _plugin.pluginConfig.AfkTime + _plugin.pluginConfig.GraceTime - _afkTime;
            if (secUntilReplacing > 0)
            {
                ClearBroadcasts();
                SendBroadcast($"{_plugin.pluginConfig.MsgPrefix} ${_plugin.pluginConfig.MsgReplace.Replace("%timeleft%", secUntilReplacing.ToString())}", 1);
                return;
            }

            _afkTime = 0;

            if (Role == RoleType.Spectator) return;

            HandleReplacement();
        }

        private void HandleReplacement()
        {
            if (ShouldReplace())
            {
                //TODO: Get inventory
                //TODO: 079 Transfer data
            }
            
            //TODO: Well fuck. This is a 12.0 code feature, doesn't exist in current Assembly. Fuck me.
            //SetRole(PlayerRole);
            ClearBroadcasts();
            SendBroadcast($"{_plugin.pluginConfig.MsgPrefix} {_plugin.pluginConfig.MsgReplace}", 10);

            _afkCount++;
            if (_afkCount >= _plugin.pluginConfig.NumBeforeKick)
            {
                Disconnect(_plugin.pluginConfig.MsgKick);
            }
        }

        private bool IsExcludedFromCheck()
        {
            return
                Role == RoleType.Spectator ||
                GetPlayers<Player>().Count < _plugin.pluginConfig.MinPlayers ||
                _plugin.pluginConfig.IgnoreTut && Role == RoleType.Tutorial;
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