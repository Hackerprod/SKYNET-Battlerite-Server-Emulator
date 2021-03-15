using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
using SKYNET.Models;
using System.Web;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

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
        Media media;
        ClientSettings settings;
        public static string Password { get; internal set; }
        public static string AccountName { get; internal set; }

        public frmMain()
        {
            InitializeComponent();
            frm = this;
            CheckForIllegalCrossThreadCalls = false;

            media = new Media();
            settings = new ClientSettings();
        }
        GameEnvironment gameEnvironment;
        private void FrmClient_Load(object sender, EventArgs e)
        {
            AccountName = settings.GetStringValue("Account Name");
            Password = settings.GetStringValue("Password");

            _AccountName.Text = AccountName;
            _Password.Text = Password;

            if (File.Exists("loop"))
            {
                HideLoop();
            }
            else
            {
                File.WriteAllBytes("loop", Resources.loop);
                HideLoop();
            }
            
            //media.Open("movie481 00_01_12-00_01_16~1.mp4", pictureBox1);
            media.Open("loop", pictureBox1);
            media.Repeat = true;
            media.Play();

            gameEnvironment = new GameEnvironment();
            gameEnvironment.ReadFromFile(@"E:\Battlerite\ip.sjson");
            gameEnvironment.ReadFromFile("ip.sjson");

            if (!string.IsNullOrEmpty(gameEnvironment.DefaultURL[0]) && Uri.TryCreate(gameEnvironment.DefaultURL[0], UriKind.RelativeOrAbsolute, out Uri result))
            {
                _ServerIp.Text = result.Host;
                _Port.Text = result.Port.ToString();
            }
        }

        private void HideLoop()
        {
            FileInfo info = new FileInfo("loop");
            info.Attributes = FileAttributes.Hidden;
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {

        }

        private void closeBox_Click(object sender, EventArgs e)
        {
            settings.SaveSettings();
            Process.GetCurrentProcess().Kill();
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
                        case "ClosePic": CloseBox.BackColor = Color.FromArgb(39, 49, 61); break;
                        case "MinPic": MinBox.BackColor = Color.FromArgb(39, 49, 61); break;
                    }
                }
                if (control is Panel)
                {
                    switch (control.Name)
                    {
                        case "CloseBox": CloseBox.BackColor = Color.FromArgb(39, 49, 61); break;
                        case "MinBox": MinBox.BackColor = Color.FromArgb(39, 49, 61); break;
                    }
                }
            }   catch { }
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            MinBox.BackColor = Color.FromArgb(29, 39, 51);
            CloseBox.BackColor = Color.FromArgb(29, 39, 51);
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

        private void CM_UsersManager_Click(object sender, EventArgs e)
        {

        }

        private void CM_ReloadItemsDB_Click(object sender, EventArgs e)
        {

        }

        private void CM_ExportImport_Click(object sender, EventArgs e)
        {
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
        }

        private void B_Login_Click(object sender, EventArgs e)
        {
            Auth auth = new Auth()
            {
                AccountName = AccountName,
                Password = Password
            };
            System.Net.WebClient request = new WebClient();
            NameValueCollection ValueCollection = new NameValueCollection();
            ValueCollection.Set("AccountName", AccountName);
            ValueCollection.Set("Password", Password);
            request.QueryString = ValueCollection;

            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    string response = request.UploadString($"http://{_ServerIp.Text}:{_Port.Text}/auth/client", "login");
                    LoginResponse loginResponse = Deserialize<LoginResponse>(response);
                    if (loginResponse.Result)
                    {
                        modCommon.Show("Login Done.");
                    }
                    else
                        modCommon.Show(loginResponse.ErrorMessage);
                }
                catch (Exception)
                {
                    modCommon.Show("Error connecting to server");
                }

            });
            thread.Start();

        }

        public T Deserialize<T>(string json)
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<T>(json);
            }
            catch
            {
                return (T)default;
            }
        }
        private void B_CreateAccount_Click(object sender, EventArgs e)
        {

        }

        private void _AccountName_KeyUp(object sender, KeyEventArgs e)
        {
            AccountName = _AccountName.Text;
        }

        private void _Password_KeyUp(object sender, KeyEventArgs e)
        {
            Password = _Password.Text;
        }

        private void _ServerIp_KeyUp(object sender, KeyEventArgs e)
        {
            //gameEnvironment.
        }
    }
}
