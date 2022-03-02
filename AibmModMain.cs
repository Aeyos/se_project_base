using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VRage.Game.ModAPI.Ingame;

namespace AIBM
{
    [XmlType("AIBMBlockSettings")]
    public class AibmBlockData
    {
        // Configuration Location (Title/Data)
        public bool useTitleForTargeting = false;
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
    }

    public static class AibmModMain
    {
        public const string version =  "0.0.1";
        public const string MainBlockSubtypeId = "AIBMBlock";
        public const string aibmBlockSettingsFile = "AIBMBlockSettings.xml";
        public static Dictionary<long, AibmBlockData> aibmBlockSettings;

        static public void LoadData()
        {
            AeyosLogger.Log("AibmModMain:LoadData");
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(aibmBlockSettingsFile, typeof(AibmBlockData[]))) {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(aibmBlockSettingsFile, typeof(AibmBlockData[]));
                    string readData = reader.ReadToEnd();
                    AibmBlockData[] d = MyAPIGateway.Utilities.SerializeFromXML<AibmBlockData[]>(readData);
                    AeyosLogger.Log("AibmModMain:LoadData Done!");
                    aibmBlockSettings = new Dictionary<long, AibmBlockData>();
                    foreach (AibmBlockData data in d) { 
                        aibmBlockSettings[data.entityId] = data;
                    }
                }
            } catch (Exception e) {
                AeyosLogger.Error($"AibmModMain:LoadData", e);
            }
            if (aibmBlockSettings == null)
            {
                aibmBlockSettings = new Dictionary<long, AibmBlockData>();
            }
        }

        static public void SaveData() {
            AeyosLogger.Log("AibmModMain:SaveData");
            try {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(aibmBlockSettingsFile, typeof(AibmBlockData[]));
                var values = new AibmBlockData[aibmBlockSettings.Count];
                aibmBlockSettings.Values.CopyTo(values, 0);
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(values));
                writer.Flush();
                writer.Close();
            } catch (Exception e) {
                AeyosLogger.Error($"AibmModMain:SaveData", e);
            }
        }

        static public AibmBlockData CreateOrLoadConfig(AibmBlockLogic block)
        {
            AeyosLogger.Log($"AibmModMain:CreateOrLoadConfig for {block.Entity.EntityId} from AIBMBlockSettings ({aibmBlockSettings.Count} entries)");
            if (aibmBlockSettings.ContainsKey(block.Entity.EntityId) == false)
            {
                var blockData = new AibmBlockData
                {
                    entityId = block.Entity.EntityId
                };
                aibmBlockSettings.Add(blockData.entityId, blockData);
            } else
            {
                AeyosLogger.Log($"AibmModMain:CreateOrLoadConfig, found config for entity, loading...");
            }
            return aibmBlockSettings[block.Entity.EntityId];
        }
    }
}