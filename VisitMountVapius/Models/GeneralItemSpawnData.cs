using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley.GameData;
using StardewValley.Internal;

namespace VisitMountVapius.Models;
internal sealed class GeneralItemSpawnData : GenericSpawnItemDataWithCondition
{
    public ItemQuerySearchMode ItemQuerySearchMode { get; set; } = ItemQuerySearchMode.RandomOfTypeItem;
}
