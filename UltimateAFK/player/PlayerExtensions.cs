using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using PluginAPI.Core;

namespace UltimateAFK.player;

public static class PlayerExtensions
{
    public static void ClearInventory(this Player player)
    {
        //TODO: Copied from strip command. Replace when Player has ClearInventory function
        var inv = player.ReferenceHub.inventory;
        while (inv.UserInventory.Items.Count > 0)
            inv.ServerRemoveItem(inv.UserInventory.Items.ElementAt<KeyValuePair<ushort, ItemBase>>(0).Key, null);
        inv.UserInventory.ReserveAmmo.Clear();
    }

    
    [Obsolete("Marking this obselete, so in the future then Server.Instance NRE is fixed, this is no longer needed.")]
    public static void PBroadcast(this Player player, string text, ushort duration, bool force = false)
    {
        var instance = Broadcast.Singleton;
        if (force) instance.TargetClearElements(player.ReferenceHub.connectionToClient);
        
        instance.TargetAddElement(player.ReferenceHub.connectionToClient, text, duration, Broadcast.BroadcastFlags.Normal);
    }
}