using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.ModAPI;
using VRage.Utils;

namespace AIBM
{
	public enum AibmAlertMessageType
    {
		Enemy,
		LowOnItems,
    }

	public class AibmAlertMessages
	{
		public AibmAlertMessageType type;
		public string message;
		public long targetId;
	}

	public class AibmCargoContainerData
    {
		public bool storeOre = false;
		public bool storeIngot = false;
		public bool storeComponent = false;
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
