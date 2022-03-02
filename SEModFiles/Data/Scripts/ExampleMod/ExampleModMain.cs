using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VRage.Game.ModAPI.Ingame;

namespace ExampleMod
{
    [XmlType("ExampleBlockSettings")]
    public class ExampleBlockData
    {
        public bool exampleToggle1 = false;
        public bool exampleToggle2 = false;
        public long entityId;
    }

    public static class ExampleModMain
    {
        public const string version =  "0.0.1";
        public const string MainBlockSubtypeId = "ExampleBlock";
        public static Dictionary<long, ExampleBlockData> exampleBlockSettings;

        static public void LoadData()
        {
            AeyosLogger.Log("ExampleModMain:LoadData");
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ExampleBlockSettings.xml", typeof(ExampleBlockData[]))) {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ExampleBlockSettings.xml", typeof(ExampleBlockData[]));
                    string readData = reader.ReadToEnd();
                    ExampleBlockData[] d = MyAPIGateway.Utilities.SerializeFromXML<ExampleBlockData[]>(readData);
                    AeyosLogger.Log("ExampleModMain:LoadData Done!");
                    exampleBlockSettings = new Dictionary<long, ExampleBlockData>();
                    foreach (ExampleBlockData data in d) { 
                        exampleBlockSettings[data.entityId] = data;
                    }
                }
            } catch (Exception e) {
                AeyosLogger.Error($"ExampleModMain:LoadData", e);
            }
            if (exampleBlockSettings == null)
            {
                exampleBlockSettings = new Dictionary<long, ExampleBlockData>();
            }
        }

        static public void SaveData() {
            AeyosLogger.Log("ExampleModMain:SaveData");
            try {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ExampleBlockSettings.xml", typeof(ExampleBlockData[]));
                var values = new ExampleBlockData[exampleBlockSettings.Count];
                exampleBlockSettings.Values.CopyTo(values, 0);
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(values));
                writer.Flush();
                writer.Close();
            } catch (Exception e) {
                AeyosLogger.Error($"ExampleModMain:SaveData", e);
            }
        }

        static public ExampleBlockData CreateOrLoadConfig(ExampleBlockLogic block)
        {
            AeyosLogger.Log($"ExampleModMain:CreateOrLoadConfig for {block.Entity.EntityId} from exampleBlockSettings ({exampleBlockSettings.Count} entries)");
            if (exampleBlockSettings.ContainsKey(block.Entity.EntityId) == false)
            {
                var blockData = new ExampleBlockData
                {
                    entityId = block.Entity.EntityId
                };
                exampleBlockSettings.Add(blockData.entityId, blockData);
            }
            return exampleBlockSettings[block.Entity.EntityId];
        }
    }
}