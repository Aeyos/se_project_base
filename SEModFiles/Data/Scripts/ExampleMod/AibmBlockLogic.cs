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
        private readonly Dictionary<IMyCargoContainer, bool> d_managedContainers = new Dictionary<IMyCargoContainer,bool>();
        private AibmCargoContainerCollection d_containers = new AibmCargoContainerCollection();
        private List<IMyCargoContainer> _tempCargoContainerList;
        private List<IMyCargoContainer> _tempMetaCargoContainerList;

        internal static AibmBlockLogic ToLogic(IMyTerminalBlock block)
        {
            return block.GameLogic.GetAs<AibmBlockLogic>();
        }

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
            UpdateAutoSort();
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

            if (newColor != emissiveColor)
            {
                myBlock.SetEmissiveParts("Emissive", newColor, 1f);
                emissiveColor = newColor;
            }
        }

        private void UpdateAutoSort()
        {
            // DON'T auto-sort containers, return
            if (blockData.enableContainerSorting == false) return;

            // Refresh cargo containers
            RefreshCargoContainerLists();

            // CHECK ALL UNMANAGED
            // For all unmanaged cargos that have customData
            foreach (IMyCargoContainer cargo in _tempMetaCargoContainerList)
            {
                // Manage them
                d_containers.Add(cargo);
            }

            // CHECK FILL RATES FOR MANAGED CONTAINERS
            // For all types of containers
            foreach (AibmCargoContainerType cargoContainerType in Enum.GetValues(typeof(AibmCargoContainerType)))
            {
                var enumName = Enum.GetName(typeof(AibmCargoContainerType), cargoContainerType).ToString();

                // There are no containers for TYPE or FILL RATE >= 99%
                if (d_containers.GetFillRate(cargoContainerType) >= 0.99)
                {
                    // Get free container
                    var cargo = GetUnassignedContainer();

                    // Assign container if it exists
                    if (cargo != null)
                    {
                        d_containers.Add(cargo, cargoContainerType);
                    }
                    else
                    {
                        // TODO: alert user for lack of avaialable containers
                        // Enum.GetName(typeof(AibmCargoContainerType), containerType).ToString();
                        // AeyosLogger.MessageAll($"I need a container for {enumName}");
                    }
                }

                //if (d_containers.ContainsKey(containerType) == true)
                //{
                //    foreach (var container in d_containers[containerType].cargoContainers)
                //    {
                //        container.CustomName = $"Container for: {enumName}";
                //        container.
                //    }
                //}
            }

            // DISCOVER UNCONTROLLED CARGOS WITH METADATA
            // SORT MANAGED CONTAINERS
            // TODO: Sort UN-MANGED containers (? - add option)
        }

        IMyCargoContainer GetUnassignedContainer()
        {
            // GET FIRST CONTAINER IF THEY ARE NOT TAKEN
            if (_tempCargoContainerList.Count > 0)
            {
                var cargo = _tempCargoContainerList.ElementAt(0);
                _tempCargoContainerList.Remove(cargo);
                return cargo;
            }

            // NO CONTAINERS FOUND
            return null;
        }

        void RefreshCargoContainerLists()
        {
            // CREATE LISTS // OVERRIDE LIST FROM LAST UPDATE //
            _tempCargoContainerList = new List<IMyCargoContainer>();
            _tempMetaCargoContainerList = new List<IMyCargoContainer>();

            // GET ALL UNMANAGED CONTAINERS
            // SEPARATE CARGOS WITH META
            // Get all cargo containers from this cubeGrid
            var allCargoContainers = AeyosUtils.getBlocksFromGrid<IMyCargoContainer>(myBlock.CubeGrid);
            foreach (var cargoContainer in allCargoContainers)
            {
                // If container has custom data, add to meta cargo container list
                if (cargoContainer.CustomData != null && cargoContainer.CustomData.Contains("AIBM"))
                {
                    _tempMetaCargoContainerList.Add(cargoContainer);
                }
                // If container is NOT managed, add it to the unmanaged list
                else if (d_managedContainers.ContainsKey(cargoContainer) == false)
                {
                    _tempCargoContainerList.Add(cargoContainer);
                }
            }

            // SORT BY CAPACITY
            _tempCargoContainerList.Sort((a, b) => b.GetInventory().MaxVolume.RawValue.CompareTo(a.GetInventory().MaxVolume.RawValue));
        }

        public static IMyTerminalControl CreateClearCustomDataButton()
        {
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("AibmBlockLogic_ClearCustomDataButton");
            button.Title = MyStringId.GetOrCompute("clear cccdata");
            button.Action = (cblock) => {
                var bLogic = AibmBlockLogic.ToLogic(cblock);
                var cargoContainers = AeyosUtils.getBlocksFromGrid<IMyCargoContainer>(cblock.CubeGrid);
                foreach (var cargo in cargoContainers)
                {
                    cargo.CustomData = "";
                }
            };
            return button;
        }

        internal static IMyTerminalControl CreateTestButton()
        {
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("AibmBlockLogic_TestButton");
            button.Title = MyStringId.GetOrCompute("TEST!");
            button.Action = (cblock) => {
                // RANOMIZE EMISSIVE PARTS // PLAY SOUND FOR USER ONLY // GET PLAYER COMPONENTS //
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
                var cargoContainers = AeyosUtils.getBlocksFromGrid<IMyCargoContainer>(cblock.CubeGrid);
                cargoContainers.Sort((a,b) => ((double)b.GetInventory().MaxVolume).CompareTo((double) a.GetInventory().MaxVolume));
                foreach (var cargo in cargoContainers)
                {
                    var cargoInv = cargo.GetInventory();
                    
                }
            };
            return button;
        }
    }
}