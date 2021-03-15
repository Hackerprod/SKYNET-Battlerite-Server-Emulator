using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

public class modCommon
{
    public class LogMessage
    {
        public string Message { get; set; }
        public bool YesNo { get; set; }
    }

    private static Process currentProcess;
    private static string str2 = "QWERTYUIOPASDFGHJKLÑZXCVBNM1234567890";
    public static int AccountId
    {
        get { _AccountId++;  return _AccountId; }
    }

    public static bool ShowShadow { get; internal set; }

    public static event EventHandler<LogMessage> OnMessage;

    static int _AccountId = 1000;
    public static string GetUniqueAlphaNumericID()
    {
        string str1 = "";
        try
        {
            short num1 = checked((short)str2.Length);
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder();
            int num2 = 1;
            do
            {
                int startIndex = random.Next(0, (int)num1);
                stringBuilder.Append(str2.Substring(startIndex, 1));
                checked { ++num2; }
            }
            while (num2 <= 6);
            stringBuilder.Append(DateAndTime.Now.ToString("HHmmss"));
            str1 = stringBuilder.ToString();
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            ProjectData.ClearProjectError();
        }
        return str1;
    }
    public static string GetPatch()
    {
        try
        {
            currentProcess = Process.GetCurrentProcess();
            return new FileInfo(currentProcess.MainModule.FileName).Directory?.FullName;
        }
        finally
        {
            currentProcess = null;
        }
    }
    public static int RandomID()
    {
        Random r = new Random();
        return r.Next(500000000, 999999999);
    }

    internal static void Show(object v)
    {
        MessageBox.Show(v.ToString());
    }
}