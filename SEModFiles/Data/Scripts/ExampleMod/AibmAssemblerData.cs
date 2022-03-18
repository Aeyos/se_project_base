using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game;

namespace AIBM
{
    public class AibmAssemblerData
    {
        // PROPS
        public IMyAssembler block = null;
        public HashSet<MyDefinitionId> produceableBlueprints = new HashSet<MyDefinitionId>();

        public bool CanProduce(MyPhysicalItemDefinition item)
        {
            return item.Enabled && item.AvailableInSurvival && item.Public && block.CanUseBlueprint(item);
        }

        public bool DoAssemble(MyPhysicalItemDefinition item, int quantity)
        {
            block.Mode = Sandbox.ModAPI.Ingame.MyAssemblerMode.Assembly;
            if (CanProduce(item))
            {
                block.AddQueueItem(item, MyFixedPoint.Zero + quantity);
                return true;
            }
            return false;
        }

        public bool DoDisassemble(MyPhysicalItemDefinition item, int quantity)
        {
            block.Mode = Sandbox.ModAPI.Ingame.MyAssemblerMode.Disassembly;
            if (CanProduce(item))
            {
                block.AddQueueItem(item, MyFixedPoint.Zero + quantity);
                return true;
            }
            return false;
        }

        public void ClearQueue()
        {
            block.ClearQueue();
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine("AIBM");
            return sb.ToString();
        }

        public static AibmAssemblerData Deserialize(IMyAssembler assembler)
        {
            var data = assembler.CustomData;
            var assemblerData = new AibmAssemblerData
            {
                block = assembler,
            };
            var startDataBlock = data != null && data.Length > 0 ? data.IndexOf("AIBM") : -1;
            if (startDataBlock == -1) return null;

            //var metadata = data
            //    .Split('\n')
            //    .Select(x => x.Split(' ').Select(y => y.Trim()).ToArray())
            //    .Where(x => x.Length > 1)
            //    .Select(x => x.First().ToUpper() == "[X]" ? x.Last() : null)
            //    .Where(x => x != null);

            //foreach (var x in metadata)
            //{
            //    if (x == "Ores") assemblerContainer.AddStoreType(CCStoreType.Ores);
            //    if (x == "Ingots") assemblerContainer.AddStoreType(CCStoreType.Ingots);
            //    if (x == "Components") assemblerContainer.AddStoreType(CCStoreType.Components);
            //    if (x == "Ammo") assemblerContainer.AddStoreType(CCStoreType.Ammo);
            //    if (x == "Items") assemblerContainer.AddStoreType(CCStoreType.Items);
            //    if (x == "Bottles") assemblerContainer.AddStoreType(CCStoreType.Bottles);
            //    if (x == "Misc") assemblerContainer.AddStoreType(CCStoreType.Misc);
            //}

            return assemblerData;
        }
    }
}
