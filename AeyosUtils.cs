using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using VRageMath;
using System.ComponentModel;

namespace AIBM
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
                    writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(LogFileName, typeof(AeyosLogger));
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

    static class AeyosUtils
    {
        static private List<Color> colors = new List<Color> { Color.Red, Color.Blue, Color.Purple };
        static private Random random = new Random();
        static public Color RandomColor
        {
            get { return colors[random.Next(colors.Count)]; }
        }

        public static object Deserialize<T>(string data) where T : class
        {
            var containerData = Activator.CreateInstance(typeof(T));
            var dataDictionary = data.Split('\n')
                .Select(x => x.Split(':').Select(y => y.Trim()).ToArray())
                .Where(x => x.Length > 1)
                .ToDictionary(x => x[0].Substring(1, x[0].Length - 1), x => x[1]);

            foreach (System.Reflection.FieldInfo f in typeof(AibmCargoContainerData).GetFields())
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(f.FieldType);
                object propValue = typeConverter.ConvertFromString(dataDictionary[f.Name]);
                f.SetValue(containerData, propValue);
            }
            return containerData;
        }

        public static string Serialize(object o)
        {
            string ownProps = "";
            foreach (System.Reflection.FieldInfo f in typeof(AibmCargoContainerData).GetFields())
            {
                ownProps += $"-{f.Name}: {f.GetValue(o).ToString()}\n";
            }
            return $"AIBM\n{ownProps}/AIBM";
        }
    }

    // Aeyos Grid Utils
    static class AGU
    {
        public static List<T> getBlocksFromGrid<T>(IMyCubeGrid grid) where T : class, IMyCubeBlock {
            return grid.GetFatBlocks<T>().ToList<T>();
        }
    }
}
