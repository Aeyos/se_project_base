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
    class BlockConfig
    {
        public bool exampleToggle1 = false;
        public bool exampleToggle2 = false;
    }


    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ExampleModLogic : MySessionComponentBase
    {
        Dictionary<IMyTerminalBlock, BlockConfig> Configurations = new Dictionary<IMyTerminalBlock, BlockConfig>();
        private bool _Init = false;

        public void Init()
        {
            CreateControls();
            MyAPIGateway.Utilities.ShowMessage("", "Custom Controls Added");
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


        public bool AlreadyHasControls(List<IMyTerminalControl> existingControls)
        {
            foreach (var control in existingControls)
            {
                if (control.Id == "ExampleToggleLabel")
                {
                    return true;
                }
            }
            return false;
        }

        public void AddControlsToType<T>(List<IMyTerminalControl> controls)
        {
            var existingControls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<T>(out existingControls);

            if (AlreadyHasControls(existingControls)) return;

            foreach (var control in controls)
            {
                MyAPIGateway.TerminalControls.AddControl<T>(control);
            }

        }

        public void CreateControls()
        {
            var newControls = CreateControlList();

            AddControlsToType<IMyTerminalBlock>(newControls);
        }

        public bool EnabledVisible(IMyTerminalBlock block)
        {
            return block.BlockDefinition.SubtypeId == "ExampleBlock";
        }

        public List<IMyTerminalControl> CreateControlList()
        {

            var controlList = new List<IMyTerminalControl>();

            //Separator
            // var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("Renamer_Separator");
            // separator.Enabled = (Block) => Block.BlockDefinition.SubtypeId == "ExampleBlock";
            // separator.Visible = (Block) => Block.BlockDefinition.SubtypeId == "ExampleBlock";
            // separator.SupportsMultipleBlocks = true;
            // controlList.Add(separator);


            // Label
            var label = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("ExampleToggleLabel");
            label.Enabled = EnabledVisible;
            label.Visible = EnabledVisible;
            label.SupportsMultipleBlocks = true;
            label.Label = MyStringId.GetOrCompute("Example Label");
            controlList.Add(label);

            // Toggle 1
            var toggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("ExampleToggleRadio1");
            toggle.Enabled = EnabledVisible;
            toggle.Visible = EnabledVisible;
            toggle.SupportsMultipleBlocks = false;
            toggle.OnText = MyStringId.GetOrCompute("HudInfoOn");
            toggle.OffText = MyStringId.GetOrCompute("HudInfoOff");
            toggle.Getter = (tBlock) => {
                if (Configurations.ContainsKey(tBlock))
                {
                    return Configurations[tBlock].exampleToggle1;
                }
                return false;
            };
            toggle.Setter = (tBlock, value) =>
            {
                if (Configurations.ContainsKey(tBlock) == false)
                {
                    var bc = new BlockConfig();
                    bc.exampleToggle1 = value;
                    Configurations.Add(tBlock, bc);
                }
                else
                {
                    Configurations[tBlock].exampleToggle1 = value;
                }
            };
            controlList.Add(toggle);

            // Toggle 2
            var toggle2 = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>("ExampleToggleRadio2");
            toggle2.Enabled = EnabledVisible;
            toggle2.Visible = EnabledVisible;
            toggle2.SupportsMultipleBlocks = false;
            toggle2.OnText = MyStringId.GetOrCompute("HudInfoOn");
            toggle2.OffText = MyStringId.GetOrCompute("HudInfoOff");
            toggle2.Getter = (tBlock) => {
                if (Configurations.ContainsKey(tBlock))
                {
                    return Configurations[tBlock].exampleToggle2;
                }
                return false;
            };
            toggle2.Setter = (tBlock, value) =>
            {
                if (Configurations.ContainsKey(tBlock) == false)
                {
                    var bc = new BlockConfig();
                    bc.exampleToggle2 = value;
                    Configurations.Add(tBlock, bc);
                }
                else
                {
                    Configurations[tBlock].exampleToggle2 = value;
                }
            };
            controlList.Add(toggle2);

            return controlList;

        }
    }
}