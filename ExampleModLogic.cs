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

namespace ExampleMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ExampleModLogic : MySessionComponentBase {
        private bool _Init = false;
        List<IMyTerminalControl> CustomControls = new List<IMyTerminalControl>();

        public void Init()
        {
            AeyosLogger.Log("ExampleModLogic:Init Adding controls");
            CreateControlList();
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
        }

        private void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> ownControls)
        {
            if (block.BlockDefinition.SubtypeId == ExampleModMain.MainBlockSubtypeId)
            {
                MyAPIGateway.Utilities.ShowMessage("", "Getter called");

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
            ExampleModMain.SaveData();
        }

        public sealed override void LoadData()
        {
            AeyosLogger.Log("ExampleModLogic:LoadData");
            ExampleModMain.LoadData();
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
            //Separator
            // var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("Renamer_Separator");
            // separator.Enabled = (Block) => Block.BlockDefinition.SubtypeId == "ExampleBlock";
            // separator.Visible = (Block) => Block.BlockDefinition.SubtypeId == "ExampleBlock";
            // separator.SupportsMultipleBlocks = true;
            // controlList.Add(separator);

            CustomControls.Add(MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("ExampleToggleSeparator"));

            // Label
            var label = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("ExampleToggleLabel");
            //label.Enabled = EnabledVisible;
            //label.Visible = EnabledVisible;
            label.SupportsMultipleBlocks = true;
            label.Label = MyStringId.GetOrCompute("Example Label");
            CustomControls.Add(label);

            // Toggle 1
            var toggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("ExampleToggleRadio1");
            //toggle.Enabled = EnabledVisible;
            //toggle.Visible = EnabledVisible;
            toggle.SupportsMultipleBlocks = false;
            toggle.OnText = MyStringId.GetOrCompute("HudInfoOn");
            toggle.OffText = MyStringId.GetOrCompute("HudInfoOff");
            toggle.Title = MyStringId.GetOrCompute("Send messages from this block");
            toggle.Getter = (tBlock) => {
                var ebl = tBlock.GameLogic.GetAs<ExampleBlockLogic>();
                return ebl.blockData.exampleToggle1 || false;
            };
            toggle.Setter = (tBlock, value) =>
            {
                var ebl = tBlock.GameLogic.GetAs<ExampleBlockLogic>();
                ebl.blockData.exampleToggle1 = value;
            };
            CustomControls.Add(toggle);

            // Toggle 2
            var toggle2 = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("ExampleToggleRadio2");
            //toggle2.Enabled = EnabledVisible;
            //toggle2.Visible = EnabledVisible;
            toggle2.SupportsMultipleBlocks = false;
            toggle2.OnText = MyStringId.GetOrCompute("HudInfoOn");
            toggle2.OffText = MyStringId.GetOrCompute("HudInfoOff");
            toggle2.Title = MyStringId.GetOrCompute("Send messages from this block");
            toggle2.Getter = (tBlock) => {
                var ebl = tBlock.GameLogic.GetAs<ExampleBlockLogic>();
                return ebl.blockData.exampleToggle2 || false;
            };
            toggle2.Setter = (tBlock, value) =>
            {
                var ebl = tBlock.GameLogic.GetAs<ExampleBlockLogic>();
                ebl.blockData.exampleToggle2 = value;
            };
            CustomControls.Add(toggle2);

            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("ExampleButton");
            button.Title = MyStringId.GetOrCompute("Save data");
            button.Action = (block) =>
            {
                // var l = new List<ExampleBlockLogic>();
                // l.Add(block.GameLogic.GetAs<ExampleBlockLogic>());
                ExampleModMain.SaveData();
            };
            CustomControls.Add(button);
        }
    }
}