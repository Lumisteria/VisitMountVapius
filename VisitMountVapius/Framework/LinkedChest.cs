using HarmonyLib;

using Microsoft.Xna.Framework;

using Netcode;

using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

namespace VisitMountVapius.Framework;
internal class LinkedChest
{

    const string PREFIX = "VisitMountVapis.";

    private static readonly PerScreen<int> _lastMessage = new();

    internal static bool Action(GameLocation location, string[] args, Farmer farmer, Point point)
    {
        if (!ArgUtility.TryGet(args, 1, out string? chest_name, out string? error))
        {
            location.LogTileActionError(args, point.X, point.Y, error);
            return false;
        }

        if (Game1.activeClickableMenu is not null)
        {
            Game1.showRedMessage(I18n.NotNow());
            return false;
        }

        NetMutex mutex = farmer.team.GetOrCreateGlobalInventoryMutex(PREFIX + chest_name);

        mutex.RequestLock(() =>
        {
            LaunchMenu(farmer, chest_name);
        },
        () =>
        {
            long farmerID = new Traverse(mutex).Field<NetLong>("owner").Value.Value;
            ModEntry.ModMonitor.Log($"Huh, lock for linked chest held by {farmerID} - {Game1.getFarmer(farmerID)}");

            if (_lastMessage.Value + 300 > Game1.ticks)
            {
                Game1.showRedMessage(I18n.Occupied());
                _lastMessage.Value = Game1.ticks;
            }
        });

        return true;
    }

    private static void LaunchMenu(Farmer farmer, string chest_name)
    {
        Inventory items = farmer.team.GetOrCreateGlobalInventory(PREFIX + chest_name);
        ItemGrabMenu itemGrabMenu = new(
                    inventory: items,
                    reverseGrab: false,
                    showReceivingMenu: true,
                    highlightFunction: HighlightSansTools,
                    behaviorOnItemSelectFunction: (item, farmer) => GrabItemFromInventory(item, farmer, chest_name),
                    message: null,
                    behaviorOnItemGrab: (item, farmer) => GrabItemFromChest(item, farmer, chest_name),
                    snapToBottom: false,
                    canBeExitedWithKey: true,
                    playRightClickSound: true,
                    allowRightClick: true,
                    showOrganizeButton: true,
                    source: ItemGrabMenu.source_chest,
                    sourceItem: null)
        {
            behaviorBeforeCleanup = (menu) => farmer.team.GetOrCreateGlobalInventoryMutex(PREFIX + chest_name).ReleaseLock()
        };
        Game1.activeClickableMenu = itemGrabMenu;
    }

    private static bool HighlightSansTools(Item i) => (i is not Tool || i is MeleeWeapon) && (i is not SObject obj || !obj.questItem.Value);

    private static void GrabItemFromInventory(Item item, Farmer who, string chest_name)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        Inventory inventory = who.team.GetOrCreateGlobalInventory(PREFIX + chest_name);
        Item? tmp = AddToInventory(inventory, item);
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

    private static void GrabItemFromChest(Item item, Farmer who, string chest_name)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            Inventory items = who.team.GetOrCreateGlobalInventory(PREFIX + chest_name);
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
