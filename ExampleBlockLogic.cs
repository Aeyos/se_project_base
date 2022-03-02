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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LCDPanelsBlock),false, "ExampleBlock")]
    public class ExampleBlockLogic: MyGameLogicComponent
    {
        public bool exampleToggle1 = false;
        public bool exampleToggle2 = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyAPIGateway.Utilities.ShowMessage("help", "Mod initialized");
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (exampleToggle1)
            {
                MyAPIGateway.Utilities.ShowMessage("help", $"ExampleToggle1 from {this.Entity.EntityId}");
            }
            if (exampleToggle2)
            {
                MyAPIGateway.Utilities.ShowMessage("help", $"ExampleToggle2 from {this.Entity.EntityId}");
            }
        }
    }
}