using System;
using System.IO;
using System.Text;
using Lidgren.Network;
using StunShared;

public class FileWatch
{
    public static void Save(string folder, string filename, string content)
    {
        try
        {
            if (File.Exists("save.txt"))
            {
                string text = Path.Combine("Captures", folder);
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                string path = Path.Combine(text, filename);
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                using (FileStream fileStream = File.Open(path, FileMode.Create))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Info("Error saving dump in disk: " + ex.Message + " " + ex.StackTrace, Responsible.khct, LogFilter.GameTool, LogIgnoreMask.All, null, false);
        }
    }

    public static void Save(string folder, string filename, byte[] content)
    {
        try
        {
            if (File.Exists("save.txt"))
            {
                string text = Path.Combine("Captures", folder);
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                using (FileStream fileStream = File.Open(Path.Combine(text, filename), FileMode.Create))
                {
                    fileStream.Write(content, 0, content.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Info("Error saving dump in disk: " + ex.Message + " " + ex.StackTrace, Responsible.khct, LogFilter.GameTool, LogIgnoreMask.All, null, false);
        }
    }

    public static void Save(NetIncomingMessageType type, byte[] content)
    {
        string folder = "GameData";
        string text;
        if (type != NetIncomingMessageType.StatusChanged)
        {
            if (type != NetIncomingMessageType.Data)
            {
                text = "Default_" + FileWatch.Default_Consecutive;
                FileWatch.Default_Consecutive++;
            }
            else
            {
                text = "Data" + FileWatch.Data_Consecutive;
                FileWatch.Data_Consecutive++;
            }
        }
        else
        {
            text = "StatusChanged_" + FileWatch.StatusChanged_Consecutive;
            FileWatch.StatusChanged_Consecutive++;
        }
        text += ".bin";
        try
        {
            if (File.Exists("save.txt"))
            {
                string text2 = Path.Combine("Captures", folder);
                if (!Directory.Exists(text2))
                {
                    Directory.CreateDirectory(text2);
                }
                using (FileStream fileStream = File.Open(Path.Combine(text2, text), FileMode.Create))
                {
                    fileStream.Write(content, 0, content.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Info("Error saving dump in disk: " + ex.Message + " " + ex.StackTrace, Responsible.khct, LogFilter.GameTool, LogIgnoreMask.All, null, false);
        }
    }

    public static void Save(string folder, byte[] content)
    {
    }

    private static int StatusChanged_Consecutive;

    private static int Data_Consecutive;

    private static int Default_Consecutive;
}

