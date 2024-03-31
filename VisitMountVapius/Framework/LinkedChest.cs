using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Tools;

namespace VisitMountVapius.Framework;
internal class LinkedChest
{
    internal static bool Action(GameLocation location, string[] args, Farmer farmer, Point point)
    {
        if (!ArgUtility.TryGet(args, 1, out var chest_name, out var error))
        {
            location.LogTileActionError(args, point.X, point.Y, error);
            return false;
        }

        if (Game1.activeClickableMenu is not null)
        {
            Game1.showRedMessage(I18n.NotNow());
            return false;
        }

        var mutex = farmer.team.GetOrCreateGlobalInventoryMutex(chest_name);

        mutex.RequestLock(() =>
        {
            LaunchMenu(farmer, chest_name);
        });

        return true;
    }

    private static void LaunchMenu(Farmer farmer, string chest_name)
    {
        var items = farmer.team.GetOrCreateGlobalInventory(chest_name);
        Game1.activeClickableMenu = new ItemGrabMenu(
            inventory: items,
            reverseGrab: false,
            showReceivingMenu: true,
            highlightFunction: highlightSansTools,
            behaviorOnItemSelectFunction: (item, farmer) => grabItemFromInventory(item, farmer, chest_name),
            message: null,
            behaviorOnItemGrab: (item, farmer) => grabItemFromChest(item, farmer, chest_name),
            snapToBottom: false,
            canBeExitedWithKey: true,
            playRightClickSound: true,
            allowRightClick: true,
            showOrganizeButton: true,
            source: ItemGrabMenu.source_chest,
            sourceItem: null);
    }

    private static bool highlightSansTools(Item i) => i is not Tool || i is MeleeWeapon;

    private static void grabItemFromInventory(Item item, Farmer who, string chest_name)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        var inventory = who.team.GetOrCreateGlobalInventory(chest_name);
        var tmp = AddToInventory(inventory, item);
        if (tmp is null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
        }
        inventory.RemoveEmptySlots();
        int oldID = Game1.activeClickableMenu.currentlySnappedComponent?.myID ?? -1;
        LaunchMenu(who, chest_name);
        (Game1.activeClickableMenu as ItemGrabMenu)!.heldItem = tmp;
        if (oldID != -1)
        {
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }

    }

    private static void grabItemFromChest(Item item, Farmer who, string chest_name)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            var items = who.team.GetOrCreateGlobalInventory(chest_name);
            items.Remove(item);
            items.RemoveEmptySlots();
            LaunchMenu(who, chest_name);
        }
    }

    private static Item? AddToInventory(IInventory inventory, Item item)
    {
        item.resetState();
        inventory.RemoveEmptySlots();
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null && inventory[i].canStackWith(item))
            {
                item.Stack = inventory[i].addToStack(item);
                if (item.Stack <= 0)
                {
                    return null;
                }
            }
        }
        if (inventory.Count < 36)
        {
            inventory.Add(item);
            return null;
        }
        return item;
    }
}
