using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace AIBM
{
    public enum AibmAlertMessageType
    {
        Enemy,
        LowOnItems,
    }

    public enum AibmCargoContainerType : byte
    {
        Components = 1,
        Ingots = 2,
        Ores = 4,
        Ammo = 8,
        Items = 16,
        Bottles = 32,
    }

    public class AibmAlertMessages
    {
        public AibmAlertMessageType type;
        public string message;
        public long targetId;
    }

    public class AibmCargoContainerData
    {
        public IMyTerminalBlock block = null;
        public IMyInventory inventory = null;

        public bool storeOres = false;
        public bool storeIngots  = false;
        public bool storeComponents  = false;
        public bool storeAmmo  = false;
        public bool storeItems  = false;
        public bool storeBottles  = false;
        public double FillRate { get { return (double)inventory.CurrentVolume / (double)inventory.MaxVolume; } }

        public bool CanStore(AibmCargoContainerType containerType)
        {
            if (containerType == AibmCargoContainerType.Ores) return storeOres;
            if (containerType == AibmCargoContainerType.Ingots) return storeIngots;
            if (containerType == AibmCargoContainerType.Components) return storeComponents;
            if (containerType == AibmCargoContainerType.Ammo) return storeAmmo;
            if (containerType == AibmCargoContainerType.Items) return storeItems;
            if (containerType == AibmCargoContainerType.Bottles) return storeBottles;
            return false;
        }

        public void SetStoreTypes(AibmCargoContainerType containerType, bool value)
        {
            if (containerType == AibmCargoContainerType.Ores) storeOres = value;
            if (containerType == AibmCargoContainerType.Ingots) storeIngots = value;
            if (containerType == AibmCargoContainerType.Components) storeComponents = value;
            if (containerType == AibmCargoContainerType.Ammo) storeAmmo = value;
            if (containerType == AibmCargoContainerType.Items) storeItems = value;
            if (containerType == AibmCargoContainerType.Bottles) storeBottles = value;
        }
        
        public string GetTitle()
        {
            List<string> title = new List<string>();
            if (storeOres) title.Add("Ores");
            if (storeIngots) title.Add("Ingots");
            if (storeComponents) title.Add("Components");
            if (storeAmmo) title.Add("Ammo");
            if (storeItems) title.Add("Items");
            if (storeBottles) title.Add("Bottles");
            return $"Cargo Container [{string.Join(", ", title)}] ({(FillRate * 100).ToString("0.#")}%)";
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine("AIBM");
            sb.AppendLine($"-storeOres: {storeOres}");
            sb.AppendLine($"-storeIngots: {storeIngots}");
            sb.AppendLine($"-storeComponents: {storeComponents}");
            sb.AppendLine($"-storeAmmo: {storeAmmo}");
            sb.AppendLine($"-storeItems: {storeItems}");
            sb.AppendLine($"-storeBottles: {storeBottles}");
            sb.AppendLine("/AIBM");
            return sb.ToString();
        }

        public static AibmCargoContainerData Deserialize(string data)
        {
            var cargoContainer = new AibmCargoContainerData();
            var dictionary = data.Split('\n')
                .Select(x => x.Split(':').Select(y => y.Trim()).ToArray())
                .Where(x => x.Length > 1)
                .ToDictionary(x => x[0].Substring(1, x[0].Length - 1), x => x[1]);
            foreach (KeyValuePair<string, string> d in dictionary)
            {
                if (d.Key == "storeOres") cargoContainer.storeOres = Boolean.Parse(d.Value);
                if (d.Key == "storeIngots") cargoContainer.storeIngots = Boolean.Parse(d.Value);
                if (d.Key == "storeComponents") cargoContainer.storeComponents = Boolean.Parse(d.Value);
                if (d.Key == "storeAmmo") cargoContainer.storeAmmo = Boolean.Parse(d.Value);
                if (d.Key == "storeItems") cargoContainer.storeItems = Boolean.Parse(d.Value);
                if (d.Key == "storeBottles") cargoContainer.storeBottles = Boolean.Parse(d.Value);
            }

            return cargoContainer;
        }
    }

    [XmlType("AIBMBlockSettings")]
    public class AibmBlockData
    {
        // Configuration Location (Title/Data)
        public long configurationLocationOptionId = 1;
        // Auto-sort containers (On/Off)
        public bool enableContainerSorting = false;
        // Auto-assign containers (On/Off)
        public bool enableAutoAssignContainers = false;
        // Sort items alphabetically (On/Off)
        public bool enableSortItemsByName = true;
        // Controlled assemblers (On/Off)
        public bool enableControlAssemblers = false;
        // Auto-adjust solar panels (On/Off)
        public bool enableSolarPanelControl = false;
        // Low item alerts (On/Off)
        public bool shouldAlertOnLowItems = true;
        // Low power alerts (On/Off)
        public bool shouldAlertOnLowPower = true;
        // Low storage alerts (On/Off)
        public bool shouldAlertOnLowStorage = true;
        // Enemy proximity alerts (On/Off)
        public bool shouldAlertOnEnemyClose = true;
        // Turn off Refineries on low Power (On/Off)
        public bool disableRefineriesOnLowPower = false;
        // Turn off Assemblers on low Power (On/Off)
        public bool disableAssemblersOnLowPower = false;
        // Entity Id for saving/loading options
        public long entityId;
        // Name of the ai
        public string aiName = "ABot";

        public static void CreateControlList(List<IMyTerminalControl> CustomControls)
        {
            // Add separator
            CustomControls.Add(AeyosUtils.CreateControl<IMyTerminalControlSeparator>("AibmBlockSeparator_1"));

            // Label
            CustomControls.Add(AeyosUtils.CreateControl("AibmBlockTitle_2", "AIBM Settings"));

            // Name
            var name = AeyosUtils.CreateControl<IMyTerminalControlTextbox>("AibmBlockText_2_1", "AI Name");
            name.Getter = AibmBlockData.GetAiName;
            name.Setter = AibmBlockData.SetAiName;
            CustomControls.Add(name);

            // Toggle 1
            // Configur1ation Location (Title/Data) // useTitleForTargeting //
            var cb1 = AeyosUtils.CreateControl<IMyTerminalControlCombobox>("AibmBlockText_3", "Block Prop Targeting");
            cb1.ComboBoxContent = AibmBlockData.ConfigurationLocationOptions;
            cb1.Getter = AibmBlockData.GetConfigurationLocation;
            cb1.Setter = AibmBlockData.SetConfigurationLocation;
            CustomControls.Add(cb1);

            // Toggle 2
            // Auto-sort containers (On/Off) // enableContainerSorting
            var t1 = AeyosUtils.CreateControl<IMyTerminalControlOnOffSwitch>("AibmBlockText_4", "Auto-sort containers", "Enable/Disable AIBM to sort items in your grid");
            t1.OnText = MyStringId.GetOrCompute("HudInfoOn");
            t1.OffText = MyStringId.GetOrCompute("HudInfoOff");
            t1.Getter = AibmBlockData.GetAutoSortOnOff;
            t1.Setter = AibmBlockData.SetAutoSortOnOff;
            CustomControls.Add(t1);

            // Button - clear cargo container custom data
            CustomControls.Add(AibmBlockLogic.CreateClearCustomDataButton());

            // Test - show blocks
            CustomControls.Add(AibmBlockLogic.CreateTestButton());
        }

        static public void SetAiName(IMyTerminalBlock tBlock, StringBuilder value)
        {
            var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
            ebl.blockData.aiName = value.ToString();
        }

        static public StringBuilder GetAiName(IMyTerminalBlock tBlock)
        {
            var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
            return new StringBuilder(ebl.blockData.aiName);
        }

        internal static void ConfigurationLocationOptions(List<MyTerminalControlComboBoxItem> comboBoxList)
        {
            comboBoxList.Clear();
            comboBoxList.Add(new MyTerminalControlComboBoxItem { Key = 1, Value = MyStringId.GetOrCompute("CustomData") });
            comboBoxList.Add(new MyTerminalControlComboBoxItem { Key = 2, Value = MyStringId.GetOrCompute("Title") });
        }

        internal static long GetConfigurationLocation(IMyTerminalBlock arg)
        {
            var ebl = arg.GameLogic.GetAs<AibmBlockLogic>();
            return ebl.blockData.configurationLocationOptionId;
        }

        internal static void SetConfigurationLocation(IMyTerminalBlock arg1, long arg2)
        {
            var ebl = arg1.GameLogic.GetAs<AibmBlockLogic>();
            ebl.blockData.configurationLocationOptionId = arg2;
        }

        internal static bool GetAutoSortOnOff(IMyTerminalBlock tBlock)
        {
            var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
            return ebl.blockData.enableContainerSorting;
        }

        internal static void SetAutoSortOnOff(IMyTerminalBlock tBlock, bool value)
        {
            var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
            ebl.blockData.enableContainerSorting = value;
        }
    }
}
