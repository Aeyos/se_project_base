using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace AIBM
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class AIBMModLogic : MySessionComponentBase {
        private bool _Init = false;
        List<IMyTerminalControl> CustomControls = new List<IMyTerminalControl>();

        public void Init()
        {
            AeyosLogger.Log("AIBMModLogic:Init Adding controls");
            CreateControlList();
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
        }

        private void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> ownControls)
        {
            if (block.BlockDefinition.SubtypeId == AibmModMain.MainBlockSubtypeId)
            {
                foreach (var item in this.CustomControls)
                {
                    // ownControls.Add(item);
                    ownControls.Insert(8 + CustomControls.IndexOf(item), item);
                }

            }
        }

        public sealed override void SaveData()
        {
            AeyosLogger.Log("ExampleModLogic:SaveData");
            AibmModMain.SaveData();
        }

        public sealed override void LoadData()
        {
            AeyosLogger.Log("ExampleModLogic:LoadData");
            AibmModMain.LoadData();
        }

        protected sealed override void UnloadData()
        {
            AeyosLogger.Log("ExampleModLogic:UnloadData Unloading world");
            AeyosLogger.FreeWriter();
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!_Init)
                {
                    _Init = true;
                    Init();
                }
            }
            catch (Exception e)
            {
                // Mod.Log.Error(e);
            }
        }

        public void CreateControlList() {
            // Add separator
            CustomControls.Add(MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("AibmBlockSeparator_1"));

            // Label
            var label = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("AibmBlockTitle_2");
            label.Label = MyStringId.GetOrCompute("AIBM Settings");
            CustomControls.Add(label);

            // Name
            var name = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>("AibmBlockText_2_1");
            name.Title = MyStringId.GetOrCompute("AIBM Name");
            name.Getter = (tBlock) => {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                return new StringBuilder(ebl.blockData.aiName);
            };
            name.Setter = (tBlock, value) =>
            {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                ebl.blockData.aiName = value.ToString();
            };
            CustomControls.Add(name);

            // Toggle 1
            // Configuration Location (Title/Data) // useTitleForTargeting //
            var t1 = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("AibmBlockToggle_3");
            t1.Title = MyStringId.GetOrCompute("Block Targeting");
            t1.Tooltip = MyStringId.GetOrCompute("Where should AIBM look for tags, either in the Title, or in the Custom Data, of containers, assemblers, refineries, etc.");
            t1.OnText = MyStringId.GetOrCompute("Title");
            t1.OffText = MyStringId.GetOrCompute("Custom Data");
            t1.Getter = (tBlock) => {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                return ebl.blockData.useTitleForTargeting;
            };
            t1.Setter = (tBlock, value) =>
            {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                ebl.blockData.useTitleForTargeting = value;
            };
            CustomControls.Add(t1);

            // Toggle 2
            // Auto-sort containers (On/Off) // enableContainerSorting
            var t2 = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("AibmBlockToggle_4");
            t2.Title = MyStringId.GetOrCompute("Auto-sort containers");
            t2.Tooltip = MyStringId.GetOrCompute("Enable/Disable AIBM to sort items in your grid");
            t2.OnText = MyStringId.GetOrCompute("HudInfoOn");
            t2.OffText = MyStringId.GetOrCompute("HudInfoOff");
            t2.Getter = (tBlock) => {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                return ebl.blockData.enableContainerSorting;
            };
            t2.Setter = (tBlock, value) =>
            {
                var ebl = tBlock.GameLogic.GetAs<AibmBlockLogic>();
                ebl.blockData.enableContainerSorting = value;
            };
            CustomControls.Add(t2);

            // Test - show blocks
            CustomControls.Add(AibmBlockLogic.CreateTestButton());
        }
    }
}