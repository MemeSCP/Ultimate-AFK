using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using UltimateAFK.player;

namespace UltimateAFK
{
    public class MainClass
    {
        private UAFKPlayerFactory _playerFactory;

        [PluginEntryPoint("Ultimate AFK", "2.0.0", "Anti AFK System", "Sqbika")]
        void LoadUAFK()
        {
            _playerFactory = new UAFKPlayerFactory(this);
            Log.Info("Loading uAFK");
            FactoryManager.RegisterPlayerFactory(this, _playerFactory);
            
            EventManager.RegisterEvents(this);
        }

        [PluginConfig] public Config pluginConfig;
    }
}