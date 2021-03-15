using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using SKYNET.Properties;
using mshtml;
using StunGUI;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Gameplay.DataIO;
using BloodGUI_Binding;

namespace SKYNET
{
    [ComVisibleAttribute(true)]
    public partial class frmMain : Form
    {
        bool start = true;
        private bool mouseDown;     //Mover ventana
        private Point lastLocation; //Mover ventana
        private readonly Dictionary<string, string> UsersAndIds = new Dictionary<string, string>();
        public static frmMain frm;
        public StringBuilder HtmlString;
        public bool Searching;
        private System.Timers.Timer AutoSave;
        public static BattleriteServer BattleriteServer { get; set; }
        DateTime StartTime;
        private ILog ilog;

        public frmMain()
        {
            InitializeComponent();
            frm = this;
            CheckForIllegalCrossThreadCalls = false;
            
            ilog = new ILog();
            ilog.OnNewMessage += Ilog_OnNewMessage;

            StartTime = DateTime.Now;

            ilog.Info("Starting [SKYNET] Battlerite Server Emulator");

            BattleriteServer = new BattleriteServer(ilog);
            BattleriteServer.Initialize();
        }

        private static void Ilog_OnNewMessage(object sender, ILog.MessageLog e)
        {
            Write(e);
        }

        private static void Write(ILog.MessageLog e)
        {
            if (frm.Logger.InvokeRequired)
            {
                frm.Logger.Invoke(new Action(() =>
                {
                    frm.Logger.WriteLine(e.Message, e.Type);
                }));
            }
            else
                frm.Logger.WriteLine(e.Message, e.Type);
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            AutoSave = new System.Timers.Timer();
            AutoSave.AutoReset = false;
            AutoSave.Elapsed += this.AutoSave_Tick;

            modCommon.OnMessage += ModCommon_OnMessage; //; modCommon_OnMessage;
        }

        private void ModCommon_OnMessage(object sender, modCommon.LogMessage e)
        {
            //modCommon.DialogResult = new frmMessage(e.Message, e.YesNo ? frmMessage.TypeMessage.YesNo : frmMessage.TypeMessage.Normal).ShowDialog();
        }


        //private void Settings_AutoSaveDB_Changed(object sender, int hours)
        //{
        //    int Auto_Save_Time = GameCoordinator.Settings.Auto_Save_Time;
        //    int interval = Auto_Save_Time == 0 ? 0 : ((1000 * 60) * 60) * Auto_Save_Time;
        //    if (interval == 0)
        //    {
        //        AutoSave.Stop();
        //        return;
        //    }
        //    AutoSave.Interval = interval;
        //    AutoSave.Start();
        //}

        private void frmMain_Load(object sender, EventArgs e)
        {

            //MessageBox.Show(eDOTAGCMsg.ToString());
            // modCommon.Show(eDOTAGCMsg);
        }


        private void closeBox_Click(object sender, EventArgs e)
        {
            ilog.Info("Closing GC server, please wait");
            bool result;
            var task = Task.Factory.StartNew(() => result = BattleriteServer.Stop());

            task.ContinueWith( t =>
                {

                    //GameCoordinator.Settings.Save();
                    Process.GetCurrentProcess().Kill();
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            
                       
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Control control = (Control)sender;
                if (control is PictureBox)
                {
                    switch (control.Name)
                    {
                        case "ClosePic": CloseBox.BackColor = Color.FromArgb(56, 151, 240); break;
                        case "MinPic": MinBox.BackColor = Color.FromArgb(56, 151, 240); break;
                    }
                }
                if (control is Panel)
                {
                    switch (control.Name)
                    {
                        case "CloseBox": CloseBox.BackColor = Color.FromArgb(56, 151, 240); break;
                        case "MinBox": MinBox.BackColor = Color.FromArgb(56, 151, 240); break;
                    }
                }
            }   catch { }
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            MinBox.BackColor = Color.FromArgb(46, 141, 230);
            CloseBox.BackColor = Color.FromArgb(46, 141, 230);
        }

        private void Event_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Location = new Point((Location.X - lastLocation.X) + e.X, (Location.Y - lastLocation.Y) + e.Y);
                Update();
                Opacity = 0.93;
            }
        }

        private void Event_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;

        }

        private void Event_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            Opacity = 100;
        }

        private void Minimize_click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        private void TittleLbl_Click(object sender, EventArgs e)
        {


        }

        private void Check_Tick(object sender, EventArgs e)
        {
            try
            {
                TimeOnline.Text = GetTotalTime(StartTime);

                //if (GameCoordinator.Events != null)
                //{
                //    ConnectedClients.Text = GameCoordinator.Events.Connections.ToString(); 
                //    GC_Version.Text = GameCoordinator.Version.ToString();
                //    Users_playing.Text = GameCoordinator.Events.UsersPlaying.ToString(); 
                //    Accounts_Created.Text = GameCoordinator.Events.AccountsCreated.ToString();
                //}
                //Accounts_Created
            }
            catch { }
        }
        private string GetTotalTime(DateTime startTime)
        {
            TimeSpan duration = DateTime.Now - startTime;
            StringBuilder stringBuilder = new StringBuilder();
            if (duration.Days > 0)
            {
                stringBuilder.Append(duration.Days);
                stringBuilder.Append((duration.Days > 1) ? " days " : " day ");
            }
            stringBuilder.AppendFormat("{0:d2} : {1:d2} : {2:d2}", duration.Hours, duration.Minutes, duration.Seconds);
            return stringBuilder.ToString();
        }
        private void Settings_Click(object sender, EventArgs e)
        {

        }


        private void CM_UsersManager_Click(object sender, EventArgs e)
        {

        }

        private void CM_ReloadItemsDB_Click(object sender, EventArgs e)
        {

        }

        private void RegenItem_OnDone(object sender, string e)
        {
        }

        private void RegenItem_OnStart(object sender, string e)
        {

        }


        private void ClearScreen_MouseClick(object sender, MouseEventArgs e)
        {
            Logger.ClearScreen();
        }

        private void CM_ExportImport_Click(object sender, EventArgs e)
        {
        }

        private void Logo_Click(object sender, EventArgs e)
        {
            User userx = new User("Dayron", "12345", "Dayron");
            BattleriteServer.DbManager.Users.Create(userx);

            User userxx = new User("Hackerprod", "12345", "Hackerprod");
            BattleriteServer.DbManager.Users.Create(userxx);

            //List<Picture> Pictures = JsonConvert.DeserializeObject<List<Picture>>(File.ReadAllText(@"E:\Battlerite\Battlerite\AccountVanity\AccountVanity.sjson"));

            //foreach (Picture item in Pictures)
            //{
            //    try
            //    {
            //        string filename = item.File + ".png";
            //        string sourcefile = Path.Combine(@"E:\Battlerite\Battlerite\AccountVanity\cache", item.Hash);
            //        File.Copy(sourcefile, "C:/GC/" + filename);
            //    }
            //    catch (Exception)
            //    {}
            //}


            //return;
            List<NewsItem> entries = new List<NewsItem>()
            {
                new NewsItem()
                {
                     SizeType = NewsItem.SizeTypes.UltraWide,
                      Style = NewsItem.Styles.Default,
                       Tag = "tag",
                        Text = "sdsdsdsd",
                         Title = "Title", 
                          URL = "http://10.31.0.2:25000/gg"
                },

            };
            string json = new JavaScriptSerializer().Serialize(entries);
            File.WriteAllText(@"D:\Instaladores\Programación\Projects\[SKYNET] Battlerite Server Emulator\Battlerite Server Emulator\bin\Debug\Data\Files\JSON\entries.json", json);
            return;
            User user = new User("Hackerprod", "12345", "Hackerprod");
            User user2 = new User("Yohel", "12345", "Yohel.com");
            User user3 = new User("Dairon", "12345", "Dairon");
            User user4 = new User("Alejandro", "12345", "Alejandro");
            BattleriteServer.DbManager.Users.Create(user);
            BattleriteServer.DbManager.Users.Create(user2);
            BattleriteServer.DbManager.Users.Create(user3);
            BattleriteServer.DbManager.Users.Create(user4);

        }
        
        private void AutoSave_Tick(object sender, EventArgs e)
        {
            //ilog.Debug("Exporting mongodb database.");
            //GameCoordinator.DbManager.ExportDatabase();

            //int Auto_Save_Time = GameCoordinator.Settings.Auto_Save_Time;
            //int interval = Auto_Save_Time == 0 ? 0 : ((1000 * 60) * 60) * Auto_Save_Time;
            //AutoSave.Interval = interval;
            //AutoSave.Start();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            int attrValue = 2;
            DwmApi.DwmSetWindowAttribute(base.Handle, 2, ref attrValue, 4);
            DwmApi.MARGINS mARGINS = default(DwmApi.MARGINS);
            mARGINS.cyBottomHeight = 1;
            mARGINS.cxLeftWidth = 0;
            mARGINS.cxRightWidth = 0;
            mARGINS.cyTopHeight = 0;
            DwmApi.MARGINS marInset = mARGINS;
            DwmApi.DwmExtendFrameIntoClientArea(base.Handle, ref marInset);

            modCommon.ShowShadow = false;
            shadow.Dock = DockStyle.None;
        }
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnActivated(e);

            if (modCommon.ShowShadow)
            {
                shadow.Dock = DockStyle.Fill;
            }
        }

        private void WebBrowserpnl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void GlobalNotificationMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}
