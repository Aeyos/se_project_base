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
        public readonly List<AibmCargoContainerData> aibmCargoContainersData = new List<AibmCargoContainerData>();
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

        public AibmCargoContainerData Add(IMyCargoContainer cargo, AibmCargoContainerType containerTypes = 0)
        {
            AibmCargoContainerData ccData;
            // GET DATA FROM BLOCK CUSTOM DATA
            if (containerTypes == 0)
            {
                ccData = AibmCargoContainerData.Deserialize(cargo.CustomData);
                ccData.block = cargo;
                ccData.inventory = cargo.GetInventory();
                // Overwrite data in case of inconsistencies
                cargo.CustomData = ccData.Serialize();
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
            cargo.CustomName = ccData.GetTitle();
            return ccData;
        }
    }
}
