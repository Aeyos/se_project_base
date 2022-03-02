using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;

namespace ExampleMod
{
    static internal class AeyosLogger
    {
        const string LogFileName = "AeyosUtilsLog.txt";
        static TextWriter writer;

        static public void Log(string data)
        {
            WriteToFile("Log", data);
        }

        static public void Warn(string data)
        {
            WriteToFile("Warn", data);
        }

        static public void Error(string data, Exception e)
        {
            if (e == null) {
                WriteToFile("Error", $"{data} (Exception e is \"null\")");
            } else {
                WriteToFile("Error", $"{data} {e.Message}\n{e.StackTrace}");
            }
        }

        static private void WriteToFile(string type, string data)
        {
            if (writer == null)
            {
                try
                {
                    writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(LogFileName, typeof(ExampleModMain));
                }
                catch {
                    return;
                }
            }
            writer.WriteLine($"[{DateTime.Now.ToString("u")}][{type}] - {data}");
            writer.Flush();
        }

        static public void FreeWriter()
        {
            if (writer != null)
            {
                try
                {
                    writer.Flush();
                    writer.Close();
                    writer = null;
                }
                catch { }
            }
        }
    }
}
