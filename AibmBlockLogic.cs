using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

namespace AIBM
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LCDPanelsBlock), false, AibmModMain.MainBlockSubtypeId)]
    public class AibmBlockLogic : MyGameLogicComponent {
        public AibmBlockData blockData;
        public AibmAlertMessages[] alertMessages;
        public Color emissiveColor;
        public IMyTerminalBlock myBlock;
        private bool _init = false;
    
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            AeyosLogger.Log($"AIBMBlockLogic:Init {this.Entity.EntityId}");
            base.Init(objectBuilder);
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
            blockData = AibmModMain.CreateOrLoadConfig(this);
            myBlock = Entity as IMyTerminalBlock;
            _init = true;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (_init == false) return;
            UpdateEmissiveColor();
        }

        private void UpdateEmissiveColor()
        {
            Color newColor = emissiveColor;
            // Broken
            if (myBlock.IsFunctional == false) newColor = Color.Black;
            // Off/No Power
            else if (myBlock.IsWorking == false) newColor = Color.Red;
            // Working normally
            else if (emissiveColor != Color.Aquamarine) newColor = Color.Green;

            if (newColor != emissiveColor) {
                myBlock.SetEmissiveParts("Emissive", newColor, 1f);
                emissiveColor = newColor;
            }
        }

        internal static AibmBlockLogic ToLogic(IMyTerminalBlock block)
        {
            return block.GameLogic.GetAs<AibmBlockLogic>();
        }

        internal static IMyTerminalControl CreateTestButton()
        {
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("AibmBlockLogic_TestButton");
            button.Title = MyStringId.GetOrCompute("TEST!");
            button.Action = (cblock) => {
                //var blocks = cblock.CubeGrid.GetFatBlocks<IMyTerminalBlock>();
                //foreach (IMyTerminalBlock b in blocks)
                //{
                //    b.SetEmissiveParts("Emissive", AeyosUtils.RandomColor, 1);
                //    b.CustomData = $"AIBM:\n-storeIngots: true\n-storeOre: false\n-storeComponents: true\n/AIBM";
                //    MySoundPair mySoundPair = new MySoundPair("ArcPoofExplosionCat1");
                //    MyAPIGateway.Session.Player.Character.GameLogic.Container.Get<MyCharacterSoundComponent>().PlayActionSound(mySoundPair);
                //    foreach (Type t in MyAPIGateway.Session.Player.Character.GameLogic.Container.GetComponentTypes().ToList())
                //    {
                //        MyAPIGateway.Utilities.ShowMessage("", t.FullName);
                //    }
                //}
                var bLogic = AibmBlockLogic.ToLogic(cblock);
                var cargoContainers = AGU.getBlocksFromGrid<IMyCargoContainer>(cblock.CubeGrid);
                foreach (var cargo in cargoContainers)
                {
                    cargo.CustomName = $">{bLogic.blockData.aiName}< was here";
                }
            };
            return button;
        }
    }
}