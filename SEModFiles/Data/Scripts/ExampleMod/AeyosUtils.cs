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
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;
using Sandbox.Game.Gui;
using Sandbox.Game;

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
        static public void Error(string data, string errorDescription)
        {
            WriteToFile("Error", errorDescription);
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

        static public void MessageAll(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("AeyosLog", message);
        }

        static public void MessageMe(string message, string author = null)
        {
            MessageMe(message, author: author, color: Color.White);
        }

        static public void MessageMe(string message, Color color, string author = null)
        {
            if (author == null) {
                author = message;
                message = "";
            }
            MyVisualScriptLogicProvider.SendChatMessageColored(message, color, author: author);
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

        //public static object Deserialize<T>(string data) where T : class
        //{
        //    var containerData = Activator.CreateInstance(typeof(T));
        //    var dataDictionary = data.Split('\n')
        //        .Select(x => x.Split(':').Select(y => y.Trim()).ToArray())
        //        .Where(x => x.Length > 1)
        //        .ToDictionary(x => x[0].Substring(1, x[0].Length - 1), x => x[1]);

        //    foreach (System.Reflection.FieldInfo f in typeof(AibmCargoContainerData).GetFields())
        //    {
        //        TypeConverter typeConverter = TypeDescriptor.GetConverter(f.FieldType);
        //        object propValue = typeConverter.ConvertFromString(dataDictionary[f.Name]);
        //        f.SetValue(containerData, propValue);
        //    }
        //    return containerData;
        //}

        //public static string Serialize(object o)
        //{
        //    string ownProps = "";
        //    foreach (System.Reflection.FieldInfo f in typeof(AibmCargoContainerData).GetFields())
        //    {
        //        ownProps += $"-{f.Name}: {f.GetValue(o).ToString()}\n";
        //    }
        //    return $"AIBM\n{ownProps}/AIBM";
        //}

        public static List<T> getBlocksFromGrid<T>(IMyCubeGrid grid) where T : class, IMyCubeBlock
        {
            return grid.GetFatBlocks<T>().ToList<T>();
        }

        public static T CreateControl<T>(string name)
        {
            return MyAPIGateway.TerminalControls.CreateControl<T, IMyTerminalBlock>(name);
        }

        public static T CreateControl<T>(string name, string title, string tooltip = null) where T : IMyTerminalControlTitleTooltip
        {
            var control = MyAPIGateway.TerminalControls.CreateControl<T, IMyTerminalBlock>(name);
            control.Title = MyStringId.GetOrCompute(title);
            if (tooltip != null)
            {
                control.Tooltip = MyStringId.GetOrCompute(tooltip);
            }
            return control;
        }

        public static IMyTerminalControlLabel CreateControl(string name, string label)
        {
            var control = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>(name);
            control.Label = MyStringId.GetOrCompute(label);
            return control;
        }

        //public static IMyTerminalControlTextbox CreateControl(string name, string label, Func<IMyTerminalBlock, StringBuilder> fget, Action<IMyTerminalBlock, StringBuilder> fset)
        //{
        //    IMyTerminalControlTextbox control = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>(name);
        //    control.Title = MyStringId.GetOrCompute(label);
        //    control.Getter = fget;
        //    control.Setter = fset;
        //    return control;
        //}
    }
}
