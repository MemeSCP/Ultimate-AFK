using System;
using PluginAPI.Core;
using PluginAPI.Core.Factories;
using PluginAPI.Core.Interfaces;

namespace UltimateAFK.player
{
    public class UAFKPlayerFactory : PlayerFactory
    {
        private readonly MainClass _plugin;

        public UAFKPlayerFactory(MainClass plugin)
        {
            _plugin = plugin;
        }
        
        public override Type BaseType { get; } = typeof(UAFKPlayer);

        public override IPlayer Create(IGameComponent component) => new UAFKPlayer(component, _plugin);
    }
}