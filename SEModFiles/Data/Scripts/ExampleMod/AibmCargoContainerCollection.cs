using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace AIBM
{
    public class AibmCargoContainerCollection
    {
        private readonly List<AibmCargoContainerData> aibmCargoContainersData = new List<AibmCargoContainerData>();
        private readonly HashSet<IMyCargoContainer> managedCargoContainers = new HashSet<IMyCargoContainer>();
        public double FillRate
        {
            get
            {
                if (aibmCargoContainersData.Count == 0) return 0;
                return aibmCargoContainersData.Select(x => x.FillRate).Sum() / aibmCargoContainersData.Count();
            }
        }
        
        public double GetFillRate(AibmCargoContainerType ccType)
        {
            var containers = aibmCargoContainersData.Where(x => x.CanStore(ccType));
            if (containers.Count() == 0) return 1d;
            return containers.Select(x => x.FillRate).Sum() / containers.Count();
        }

        public void Add(IMyCargoContainer cargo, AibmCargoContainerType containerTypes = 0)
        {
            if (managedCargoContainers.Contains(cargo)) return;
            AibmCargoContainerData ccData;
            // GET DATA FROM BLOCK CUSTOM DATA
            if (containerTypes == 0)
            {
                ccData = AibmCargoContainerData.Deserialize(cargo.CustomData);
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
                ccData = new AibmCargoContainerData
                {
                    block = cargo,
                    inventory = cargo.GetInventory(),
                };
                ccData.SetStoreTypes(containerTypes, true);
                cargo.CustomData = ccData.Serialize();
            }
            aibmCargoContainersData.Add(ccData);
            managedCargoContainers.Add(cargo);
            cargo.CustomName = ccData.GetTitle(cargo.CustomName);
        }

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

        public void UpdateTitle()
        {
            // For all my containers
            foreach (var ccData in aibmCargoContainersData)
            {
                ccData.block.CustomName = ccData.GetTitle(ccData.block.CustomName);
            }
        }

        internal void SortAlphabetically()
        {
            foreach (var ccData in aibmCargoContainersData)
            {
                ccData.SortInventory();
            }
        }
    }
}
