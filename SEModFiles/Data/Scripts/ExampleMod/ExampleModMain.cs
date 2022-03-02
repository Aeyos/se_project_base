using Sandbox.ModAPI;
using System.Collections.Generic;
using System.IO;

namespace ExampleMod
{
    public static class ExampleModMain
    {
        public const string version =  "0.0.1";
        public const string MainBlockSubtypeId = "ExampleBlock";
        public static List<ExampleBlockLogic> blocks = new List<ExampleBlockLogic>();

        static public void SaveData() {
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ExampleBlockSettings.xml", typeof(ExampleBlockLogic));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(blocks));
            writer.Flush();
            writer.Close();
        }

        static public void AddBlock(ExampleBlockLogic block)
        {
            blocks.Add(block);
        }
    }
}