using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SKYNET
{
    public class ClientSettings
    {
        public RegistryKey registry { get; set; }

        public ClientSettings()
        {
            registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] Battlerite client\", true);
            if (registry == null)
            {
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\[SKYNET] Battlerite client\");
                registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] Battlerite client\", true);
            }

        }
        public void CreateRegistryKey()
        {
            registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] Battlerite client\", true);
            if (registry == null)
            {
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\[SKYNET] Battlerite client\");
                registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] Battlerite client\", true);
            }
        }

        //internal bool GetBoolValue(string name)
        //{
        //    bool result = false;
        //    try { result = modCommon.StringToBool((string)registry.GetValue(name, RegistryValueKind.String)); } catch { }
        //    return result;
        //}
        //internal bool TryGetBoolValue(string name, out bool Value)
        //{
        //    try
        //    {
        //        Value = modCommon.StringToBool((string)registry.GetValue(name, RegistryValueKind.String));
        //        return true;
        //    }
        //    catch
        //    {
        //        Value = false;
        //        return false;
        //    }
        //}

        internal string GetStringValue(string name)
        {
            string result = "";
            try { result = (string)registry.GetValue(name, RegistryValueKind.String); } catch { }
            return result;
        }

        internal void SaveSettings()
        {
            if (registry == null)
            {
                CreateRegistryKey();
            }

            registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] Battlerite client\", true);

            try { registry.SetValue("Account Name", frmMain.AccountName); } catch { }
            try { registry.SetValue("Password", frmMain.Password); } catch { }
        }


        internal void SaveValue(string Key, object value)
        {
            if (registry == null)
            {
                CreateRegistryKey();
            }

            registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\[SKYNET] GC Client\", true);

            try { registry.SetValue(Key, value); } catch { }
        }
        public void SetWindowsStart(bool start)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
            try
            {
                if (start)
                {
                    registryKey.SetValue(Path.GetFileName(Process.GetCurrentProcess().ProcessName), Application.ExecutablePath.ToString());
                }
                else
                {
                    registryKey.DeleteValue(Path.GetFileName(Process.GetCurrentProcess().ProcessName));
                }
            }
            catch { }
        }
    }
}