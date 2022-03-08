using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIBM
{

    public class AibmCCData
    {
        // PROPS
        public IMyCargoContainer block = null;
        public VRage.Game.ModAPI.IMyInventory inventory = null;
        public bool markedForDeletion = false;
        public HashSet<string> storeItemTypeId = new HashSet<string>();
        // GET/SET
        public double FillRate { get { return (double)inventory.CurrentVolume / (double)inventory.MaxVolume; } }
        public long EmptyVolume { get { return (long)inventory.MaxVolume - (long)inventory.CurrentVolume; } }
        public bool StoreOres { get { return storeItemTypeId.Contains("MyObjectBuilder_Ore"); } }
        public bool StoreIngots { get { return storeItemTypeId.Contains("MyObjectBuilder_Ingot"); } }
        public bool StoreComponents { get { return storeItemTypeId.Contains("MyObjectBuilder_Component"); } }
        public bool StoreAmmo { get { return storeItemTypeId.Contains("MyObjectBuilder_AmmoMagazine"); } }
        public bool StoreItems { get { return storeItemTypeId.Contains("MyObjectBuilder_PhysicalGunObject"); } }
        public bool StoreBottles { get { return storeItemTypeId.Contains("MyObjectBuilder_OxygenContainerObject"); } }
        public bool StoreMisc { get { return storeItemTypeId.Contains("MyObjectBuilder_ConsumableItem"); } }

        public bool CanStore(string itemTypeId)
        {
            return storeItemTypeId.Contains(itemTypeId);
        }

        public bool CanStore(CCStoreType storeType)
        {
            return storeItemTypeId.Contains(AibmCCUtils.CCStoreTypeToItemTypeIds[storeType][0]);
        }

        public void AddStoreType(CCStoreType storeType)
        {
            foreach (var s in AibmCCUtils.CCStoreTypeToItemTypeIds[storeType])
            {
                storeItemTypeId.Add(s);
            }
        }

        internal List<VRage.Game.ModAPI.Ingame.MyInventoryItem> GetRogueItems()
        {
            List<VRage.Game.ModAPI.Ingame.MyInventoryItem> items = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
            block.GetInventory().GetItems(items);
            return items.Where(x => storeItemTypeId.Contains(x.Type.TypeId) == false).ToList();
        }

        internal void UpdateMetadata()
        {
            var tempData = AibmCCData.Deserialize(block);
            if (tempData == null)
            {
                markedForDeletion = true;
                block.CustomName = block.DefinitionDisplayNameText;
            }
        }

        public string GetTitle(string originalName)
        {
            List<string> title = new List<string>();
            if (StoreOres) title.Add("Ores");
            if (StoreIngots) title.Add("Ingots");
            if (StoreComponents) title.Add("Components");
            if (StoreAmmo) title.Add("Ammo");
            if (StoreItems) title.Add("Items");
            if (StoreBottles) title.Add("Bottles");
            if (StoreMisc) title.Add("Misc");
            return Regex.Replace(originalName, @"(\[.*\]|\(.*\))", "").Trim() + $" [{string.Join(", ", title)}] ({FillRate * 100:0.#}%)";
        }

        internal void SortInventory()
        {
            // No items to sort
            if (inventory.ItemCount <= 0) return;

            // Create stopwatch to benchmark times
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Get list of items
            var sourceList = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
            var azList = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
            inventory.GetItems(sourceList);
            inventory.GetItems(azList);
            // ---
            AeyosLogger.Log($"Generating lists: {stopWatch.ElapsedTicks}");
            stopWatch.Restart();

            // Init translated names, expensive operation, begin sorting next time only
            if (AibmCCUtils.InitItemNames())
            {
                // If names initiaded this frame, skip sorting inventories
                AeyosLogger.Log($"InitNames: {stopWatch.ElapsedTicks}");
                stopWatch.Restart();
                return;
            }

            // Sort from A-Z based on TypeId and then by DisplayName
            azList.Sort((a, b) =>
            {
                if (a.Type.TypeId != b.Type.TypeId)
                {
                    return AibmCCUtils.ItemTypeSortOrder[a.Type.TypeId].CompareTo(AibmCCUtils.ItemTypeSortOrder[b.Type.TypeId]);
                }
                return AibmCCUtils.ItemDefinitions[a.Type].DisplayNameText.CompareTo(AibmCCUtils.ItemDefinitions[b.Type].DisplayNameText);
            });
            // ---
            AeyosLogger.Log($"AzSort: {stopWatch.ElapsedTicks}");
            stopWatch.Restart();

            // Check if there any duplicate stacks
            bool dupedStacks = azList.Select(x => $"{x.Type}{AibmCCUtils.IsItemStackable(x.Type)}").GroupBy(x => x).Any(g => g.Count() > 1);

            // If items already sorted and has no duplicate stacks
            if (azList.Select(x => x.Type.SubtypeId).SequenceEqual(sourceList.Select(x => x.Type.SubtypeId)) && dupedStacks == false)
            {
                // Skip sorting
                AeyosLogger.Log($"Skipping sort");
                return;
            }

            // Get current item count
            int itemCount = azList.Count;
            // Repeat while there are items to sort
            for (; azList.Count > 0;)
            {
                // Get item being sorted
                var item = azList[0];
                // Get item to sort index on inventory
                var sourceItemIndex = sourceList.IndexOf(item);
                // Remove items from lists (updates sourceIndex)
                azList.RemoveAt(0);
                sourceList.RemoveAt(sourceItemIndex);

                // If type of next item to sort is the same as the last item to sort AND it is stackable
                if (inventory.GetItemAt(inventory.ItemCount - 1).Value.Type == item.Type && AibmCCUtils.IsItemStackable(item.Type))
                {
                    // Move sorting item to stack
                    inventory.TransferItemTo(inventory, sourceItemIndex: sourceItemIndex, targetItemIndex: inventory.ItemCount - 1, stackIfPossible: true);
                }
                else
                {
                    // Move item to last slot
                    inventory.TransferItemTo(inventory, sourceItemIndex: sourceItemIndex, targetItemIndex: inventory.ItemCount + 1, stackIfPossible: true);
                }
            }
            // ---
            AeyosLogger.Log($"Cargo rearranging: {stopWatch.ElapsedTicks}");
            stopWatch.Stop();
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine("AIBM");
            sb.AppendLine($"[{(StoreOres == true ? "X" : " ")}] Ores");
            sb.AppendLine($"[{(StoreIngots == true ? "X" : " ")}] Ingots");
            sb.AppendLine($"[{(StoreComponents == true ? "X" : " ")}] Components");
            sb.AppendLine($"[{(StoreAmmo == true ? "X" : " ")}] Ammo");
            sb.AppendLine($"[{(StoreItems == true ? "X" : " ")}] Items");
            sb.AppendLine($"[{(StoreBottles == true ? "X" : " ")}] Bottles");
            sb.AppendLine($"[{(StoreMisc == true ? "X" : " ")}] Misc");
            sb.AppendLine("/AIBM");
            return sb.ToString();
        }

        public static AibmCCData Deserialize(IMyCargoContainer cargo)
        {
            var data = cargo.CustomData;
            var cargoContainer = new AibmCCData
            {
                block = cargo,
                inventory = cargo.GetInventory(),
            };
            var startDataBlock = data != null ? data.IndexOf("AIBM") : -1;

            if (startDataBlock == -1) return null;

            var metadata = data
                .Split('\n')
                .Select(x => x.Split(' ').Select(y => y.Trim()).ToArray())
                .Where(x => x.Length > 1)
                .Select(x => x.First().ToUpper() == "[X]" ? x.Last() : null)
                .Where(x => x != null);

            foreach (var x in metadata)
            {
                if (x == "Ores") cargoContainer.AddStoreType(CCStoreType.Ores);
                if (x == "Ingots") cargoContainer.AddStoreType(CCStoreType.Ingots);
                if (x == "Components") cargoContainer.AddStoreType(CCStoreType.Components);
                if (x == "Ammo") cargoContainer.AddStoreType(CCStoreType.Ammo);
                if (x == "Items") cargoContainer.AddStoreType(CCStoreType.Items);
                if (x == "Bottles") cargoContainer.AddStoreType(CCStoreType.Bottles);
                if (x == "Misc") cargoContainer.AddStoreType(CCStoreType.Misc);
            }

            return cargoContainer;
        }
    }
}
