using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI.Ingame;
using VRage.Utils;

namespace AIBM
{
    public class AibmCargoContainerCollection
    {
        /// <summary>
        /// List of managed cargo containers data
        /// </summary>
        private readonly List<AibmCCData> aibmCargoContainersData = new List<AibmCCData>();
        /// <summary>
        /// Managed cargo containers, used to check if block is managed or not
        /// </summary>
        private readonly HashSet<IMyCargoContainer> managedCargoContainers = new HashSet<IMyCargoContainer>();
        /// <summary>
        /// Get cargo fill rate of all managed containers
        /// </summary>
        public double FillRate
        {
            get
            {
                if (aibmCargoContainersData.Count == 0) return 0;
                return aibmCargoContainersData.Select(x => x.FillRate).Sum() / aibmCargoContainersData.Count();
            }
        }
        
        /// <summary>
        /// Gets how much the grid can store of a certain item type
        /// </summary>
        /// <param name="ccType"></param>
        /// <returns></returns>
        public double GetFillRate(CCStoreType ccType)
        {
            var containers = aibmCargoContainersData.Where(x => x.CanStore(ccType));
            if (containers.Count() == 0) return 1d;
            return containers.Select(x => x.FillRate).Sum() / containers.Count();
        }

        /// <summary>
        /// Start managing a new cargo container block
        /// </summary>
        /// <param name="cargo"></param>
        /// <param name="storeType"></param>
        public void Add(IMyCargoContainer cargo, CCStoreType storeType = 0)
        {
            if (managedCargoContainers.Contains(cargo)) return;
            AibmCCData ccData;
            // GET DATA FROM BLOCK CUSTOM DATA
            if (storeType == 0)
            {
                ccData = AibmCCData.Deserialize(cargo);
                // ccData may be null
                if (ccData != null)
                {
                    ccData.block = cargo;
                    ccData.inventory = cargo.GetInventory();
                    // Overwrite data in case of inconsistencies
                    cargo.CustomData = ccData.Serialize();
                }
            }
            // ELSE CREATE NEW DATA
            else
            {
                ccData = new AibmCCData
                {
                    block = cargo,
                    inventory = cargo.GetInventory(),
                };

                ccData.AddStoreType(storeType);

                cargo.CustomData = ccData.Serialize();

            }

            aibmCargoContainersData.Add(ccData);
            managedCargoContainers.Add(cargo);
            cargo.CustomName = ccData.GetTitle(cargo.CustomName);
        }

        /// <summary>
        /// Reads/writes metadata from block's CustomData
        /// </summary>
        public void UpdateMetadata()
        {
            // For all my containers
            foreach (var container in aibmCargoContainersData)
            {
                // Update this container metadata
                container.UpdateMetadata();
                // If marked for deletion
                if (container.markedForDeletion)
                {
                    // Remove block from managed
                    managedCargoContainers.Remove(container.block);
                }
            }
            // Remove all maked for deletion
            aibmCargoContainersData.RemoveAll(x => x.markedForDeletion == true);
        }

        /// <summary>
        /// Updates container title to add types and fill rate
        /// </summary>
        public void UpdateTitle()
        {
            // For all my containers
            foreach (var ccData in aibmCargoContainersData)
            {
                ccData.block.CustomName = ccData.GetTitle(ccData.block.CustomName);
            }
        }

        /// <summary>
        /// Sort items alphabetically between managed containers
        /// </summary>
        internal void SortAlphabetically()
        {
            foreach (var ccData in aibmCargoContainersData)
            {
                ccData.SortInventory();
            }
        }

        /// <summary>
        /// Gets containers that can contain the item, either fully or partially
        /// </summary>
        /// <param name="item">Item the containers should be able to handle</param>
        /// <param name="divisible">Transfer item partially or fully</param>
        /// <returns></returns>
        internal AibmCCData[] GetCargoContainersFor(MyInventoryItem item, bool divisible = true)
        {
            
            return aibmCargoContainersData.Where(x => x.CanStore(item.Type.TypeId) && (divisible == true || x.inventory.CanItemsBeAdded(item.Amount, item.Type))).ToArray();
        }

        /// <summary>
        /// Transfers items between managed containers
        /// </summary>
        internal void TransferItems()
        {
            foreach (var ccData in aibmCargoContainersData)
            {
                var items = ccData.GetRogueItems();
                foreach (var item in items)
                {
                    if (AibmCCUtils.IsItemStackable(item.Type))
                    {
                        var availableContainers = GetCargoContainersFor(item);
                        for (var i = 0; i < availableContainers.Length && item.Amount > 0; i++)
                        {
                            availableContainers[i].inventory.TransferItemFrom(ccData.inventory, item);
                        }
                    }
                    else
                    {
                        var availableContainers = GetCargoContainersFor(item, divisible: false);
                        if (availableContainers.Length > 0)
                        {
                            ccData.inventory.TransferItemTo(availableContainers[0].inventory, item);
                        }
                    }
                }
            }
        }
    }
}
