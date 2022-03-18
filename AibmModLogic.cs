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
            // TODO: Separate controls by block SubTypeId
            AibmBlockData.CreateControlList(CustomControls);
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
        }

        private void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> ownControls)
        {
            if (block.BlockDefinition.SubtypeId == AibmModMain.MainBlockSubtypeId)
            {
                foreach (var item in this.CustomControls)
                {
                    // ownControls.Add(item);
                    //ownControls.Insert(8 + CustomControls.IndexOf(item), item);
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
                AeyosLogger.Error("UpdateBeforeSimulation", e);
            }
        }
    }
}