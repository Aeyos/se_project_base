using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using VRage.Game.ModAPI.Ingame;

namespace AIBM
{
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