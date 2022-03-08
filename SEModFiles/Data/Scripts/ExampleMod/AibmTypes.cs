using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
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

    public enum CCStoreType
    {
        Ores = 1,
        Ingots,
        Components,
        Ammo,
        Items,
        Bottles,
        Misc,
    }

    public static class AibmCCUtils
    {
        public static Dictionary<string, byte> ItemTypeSortOrder = new Dictionary<string, byte>
        {
            { "MyObjectBuilder_OxygenContainerObject", 0 },
            { "MyObjectBuilder_GasContainerObject", 1 },
            { "MyObjectBuilder_ConsumableItem", 2 },
            { "MyObjectBuilder_PhysicalGunObject", 3 },
            { "MyObjectBuilder_AmmoMagazine", 4 },
            { "MyObjectBuilder_Datapad", 5 },
            { "MyObjectBuilder_Package", 6 },
            { "MyObjectBuilder_Component", 7 },
            { "MyObjectBuilder_Ingot", 8 },
            { "MyObjectBuilder_Ore", 9 },
            { "MyObjectBuilder_PhysicalObject", 10 },
        };

        public static Dictionary<MyItemType, MyPhysicalItemDefinition> ItemDefinitions = null;

        public static Dictionary<CCStoreType, string[]> CCStoreTypeToItemTypeIds = new Dictionary<CCStoreType, string[]>
        {
            { CCStoreType.Ores, new string[] { "MyObjectBuilder_Ore" } },
            { CCStoreType.Ingots, new string[] {"MyObjectBuilder_Ingot" } },
            { CCStoreType.Components, new string[] { "MyObjectBuilder_Component" } },
            { CCStoreType.Ammo, new string[] { "MyObjectBuilder_AmmoMagazine" } },
            { CCStoreType.Items, new string[] { "MyObjectBuilder_PhysicalGunObject" } },
            { CCStoreType.Bottles, new string[] { "MyObjectBuilder_OxygenContainerObject", "MyObjectBuilder_GasContainerObject" } },
            { CCStoreType.Misc, new string[] { "MyObjectBuilder_ConsumableItem", "MyObjectBuilder_Datapad", "MyObjectBuilder_Package", "MyObjectBuilder_PhysicalObject" } },
        };

        public static bool InitItemNames()
        {
            if (ItemDefinitions != null) return false;
            if (ItemDefinitions == null) ItemDefinitions = new Dictionary<MyItemType, MyPhysicalItemDefinition>();

            var validItems = MyDefinitionManager.Static.GetAllDefinitions().Where(x => x.Public && x is MyPhysicalItemDefinition);

            foreach (MyAssemblerDefinition s in MyDefinitionManager.Static.GetAllDefinitions().Where(x => x.Public && x is MyAssemblerDefinition))
            {
                //MyBlueprintDefinition.
                AeyosLogger.Log($"{s.DisplayNameText} - Speed:{s.AssemblySpeed}, Priority {s.AssemblySpeed} --- {s}");
                foreach(MyBlueprintClassDefinition bptab in s.BlueprintClasses.Where(x => x.AvailableInSurvival && x.Enabled && x.Public))
                {
                    AeyosLogger.Log($"-----TAB {bptab.DisplayNameText}");
                    foreach (var bp in bptab.Where(x => x.AvailableInSurvival && x.Enabled && x.Public))
                    {
                        AeyosLogger.Log($"--------------- {bp.DisplayNameText} ({bp.Id})");
                    }

                }
            }
            //foreach (var s in MyDefinitionManager.Static.GetAllDefinitions<MyBlueprintDefinition>().ToList().Select(x => $"{x.DisplayNameText} - Speed:{x.BaseProductionTimeInSeconds}, Priority {x.Priority}"))
            //{
            //    AeyosLogger.Log(s);
            //}

            foreach (MyPhysicalItemDefinition item in validItems)
            {
                ItemDefinitions.Add(new MyItemType(item.Id.TypeId, item.Id.SubtypeId), item);
            }
            return true;
        }

        public static bool IsItemStackable(MyItemType itemType)
        {
            return ItemDefinitions[itemType].MaxStackAmount > 1;
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
