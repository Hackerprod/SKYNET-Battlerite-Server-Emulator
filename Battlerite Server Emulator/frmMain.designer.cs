

using SKYNET.Properties;

namespace SKYNET
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.MinBox = new System.Windows.Forms.Panel();
            this.MinPic = new System.Windows.Forms.PictureBox();
            this.CloseBox = new System.Windows.Forms.Panel();
            this.ClosePic = new System.Windows.Forms.PictureBox();
            this.tittleLbl = new System.Windows.Forms.Label();
            this.acceptBtn = new System.Windows.Forms.Button();
            this.Browser = new System.Windows.Forms.WebBrowser();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.WebBrowserpnl = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.Check = new System.Windows.Forms.Timer(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.Users_playing = new System.Windows.Forms.Label();
            this.Accounts_Created = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.GC_Version = new System.Windows.Forms.Label();
            this.ConnectedClients = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TimeOnline = new System.Windows.Forms.Label();
            this.Logo = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bodyContainer = new System.Windows.Forms.Panel();
            this.CM_UsersManager = new System.Windows.Forms.ToolStripMenuItem();
            this.GlobalNotificationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_ReloadItemsImage = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_ReloadItemsDB = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_Export = new System.Windows.Forms.ToolStripMenuItem();
            this.shadow = new SKYNET.ShadowBox();
            this.Logger = new SKYNET.Controls.WebBrowserLogger();
            this.panel1.SuspendLayout();
            this.MinBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinPic)).BeginInit();
            this.CloseBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClosePic)).BeginInit();
            this.WebBrowserpnl.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            this.panel2.SuspendLayout();
            this.bodyContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(141)))), ((int)(((byte)(230)))));
            this.panel1.Controls.Add(this.MinBox);
            this.panel1.Controls.Add(this.CloseBox);
            this.panel1.Controls.Add(this.tittleLbl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(700, 26);
            this.panel1.TabIndex = 5;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Event_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Event_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Event_MouseUp);
            // 
            // MinBox
            // 
            this.MinBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(141)))), ((int)(((byte)(230)))));
            this.MinBox.Controls.Add(this.MinPic);
            this.MinBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.MinBox.Location = new System.Drawing.Point(632, 0);
            this.MinBox.Name = "MinBox";
            this.MinBox.Size = new System.Drawing.Size(34, 26);
            this.MinBox.TabIndex = 12;
            this.MinBox.Click += new System.EventHandler(this.Minimize_click);
            this.MinBox.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
            this.MinBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            // 
            // MinPic
            // 
            this.MinPic.Image = global::SKYNET.Properties.Resources.min_new;
            this.MinPic.Location = new System.Drawing.Point(11, 12);
            this.MinPic.Name = "MinPic";
            this.MinPic.Size = new System.Drawing.Size(13, 12);
            this.MinPic.TabIndex = 4;
            this.MinPic.TabStop = false;
            this.MinPic.Click += new System.EventHandler(this.Minimize_click);
            this.MinPic.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
            this.MinPic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            // 
            // CloseBox
            // 
            this.CloseBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(141)))), ((int)(((byte)(230)))));
            this.CloseBox.Controls.Add(this.ClosePic);
            this.CloseBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.CloseBox.Location = new System.Drawing.Point(666, 0);
            this.CloseBox.Name = "CloseBox";
            this.CloseBox.Size = new System.Drawing.Size(34, 26);
            this.CloseBox.TabIndex = 11;
            this.CloseBox.Click += new System.EventHandler(this.closeBox_Click);
            this.CloseBox.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
            this.CloseBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            // 
            // ClosePic
            // 
            this.ClosePic.Image = global::SKYNET.Properties.Resources.close_new;
            this.ClosePic.Location = new System.Drawing.Point(11, 7);
            this.ClosePic.Name = "ClosePic";
            this.ClosePic.Size = new System.Drawing.Size(13, 12);
            this.ClosePic.TabIndex = 4;
            this.ClosePic.TabStop = false;
            this.ClosePic.Click += new System.EventHandler(this.closeBox_Click);
            this.ClosePic.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
            this.ClosePic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            // 
            // tittleLbl
            // 
            this.tittleLbl.AutoSize = true;
            this.tittleLbl.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tittleLbl.Location = new System.Drawing.Point(8, 4);
            this.tittleLbl.Name = "tittleLbl";
            this.tittleLbl.Size = new System.Drawing.Size(200, 16);
            this.tittleLbl.TabIndex = 7;
            this.tittleLbl.Text = "Dota2 GameCoordinator Server";
            this.tittleLbl.Click += new System.EventHandler(this.TittleLbl_Click);
            this.tittleLbl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Event_MouseDown);
            this.tittleLbl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Event_MouseMove);
            this.tittleLbl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Event_MouseUp);
            // 
            // acceptBtn
            // 
            this.acceptBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.acceptBtn.Location = new System.Drawing.Point(819, 372);
            this.acceptBtn.Name = "acceptBtn";
            this.acceptBtn.Size = new System.Drawing.Size(75, 23);
            this.acceptBtn.TabIndex = 16;
            this.acceptBtn.Text = "button1";
            this.acceptBtn.UseVisualStyleBackColor = true;
            // 
            // Browser
            // 
            this.Browser.Location = new System.Drawing.Point(-21, -2);
            this.Browser.Name = "Browser";
            this.Browser.Size = new System.Drawing.Size(16, 20);
            this.Browser.TabIndex = 18;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "P0.jpg");
            this.imageList1.Images.SetKeyName(1, "P1.jpg");
            this.imageList1.Images.SetKeyName(2, "P2.jpg");
            this.imageList1.Images.SetKeyName(3, "P3.jpg");
            this.imageList1.Images.SetKeyName(4, "P4.jpg");
            // 
            // WebBrowserpnl
            // 
            this.WebBrowserpnl.BackColor = System.Drawing.Color.White;
            this.WebBrowserpnl.Controls.Add(this.Logger);
            this.WebBrowserpnl.Dock = System.Windows.Forms.DockStyle.Top;
            this.WebBrowserpnl.Location = new System.Drawing.Point(0, 64);
            this.WebBrowserpnl.Name = "WebBrowserpnl";
            this.WebBrowserpnl.Padding = new System.Windows.Forms.Padding(8);
            this.WebBrowserpnl.Size = new System.Drawing.Size(700, 400);
            this.WebBrowserpnl.TabIndex = 34;
            this.WebBrowserpnl.Paint += new System.Windows.Forms.PaintEventHandler(this.WebBrowserpnl_Paint);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(161)))), ((int)(((byte)(242)))));
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel3.Location = new System.Drawing.Point(633, 38);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(1);
            this.panel3.Size = new System.Drawing.Size(61, 20);
            this.panel3.TabIndex = 39;
            this.panel3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ClearScreen_MouseClick);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(141)))), ((int)(((byte)(230)))));
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(1, 1);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(59, 18);
            this.panel4.TabIndex = 28;
            this.panel4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ClearScreen_MouseClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(141)))), ((int)(((byte)(230)))));
            this.label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label2.Font = new System.Drawing.Font("Segoe UI Emoji", 7F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(1, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 14);
            this.label2.TabIndex = 27;
            this.label2.Text = "Clear screen";
            this.label2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ClearScreen_MouseClick);
            // 
            // Check
            // 
            this.Check.Enabled = true;
            this.Check.Interval = 1000;
            this.Check.Tick += new System.EventHandler(this.Check_Tick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(83, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 15);
            this.label5.TabIndex = 42;
            this.label5.Text = "Users playing";
            // 
            // Users_playing
            // 
            this.Users_playing.AutoSize = true;
            this.Users_playing.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Users_playing.Location = new System.Drawing.Point(197, 41);
            this.Users_playing.Name = "Users_playing";
            this.Users_playing.Size = new System.Drawing.Size(13, 15);
            this.Users_playing.TabIndex = 43;
            this.Users_playing.Text = "0";
            // 
            // Accounts_Created
            // 
            this.Accounts_Created.AutoSize = true;
            this.Accounts_Created.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Accounts_Created.Location = new System.Drawing.Point(197, 5);
            this.Accounts_Created.Name = "Accounts_Created";
            this.Accounts_Created.Size = new System.Drawing.Size(13, 15);
            this.Accounts_Created.TabIndex = 40;
            this.Accounts_Created.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(281, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(104, 15);
            this.label11.TabIndex = 44;
            this.label11.Text = "Dota2 GC version";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(83, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 15);
            this.label3.TabIndex = 39;
            this.label3.Text = "Accounts created";
            // 
            // GC_Version
            // 
            this.GC_Version.AutoSize = true;
            this.GC_Version.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.GC_Version.Location = new System.Drawing.Point(395, 5);
            this.GC_Version.Name = "GC_Version";
            this.GC_Version.Size = new System.Drawing.Size(66, 15);
            this.GC_Version.TabIndex = 45;
            this.GC_Version.Text = "GC_Version";
            // 
            // ConnectedClients
            // 
            this.ConnectedClients.AutoSize = true;
            this.ConnectedClients.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ConnectedClients.Location = new System.Drawing.Point(197, 23);
            this.ConnectedClients.Name = "ConnectedClients";
            this.ConnectedClients.Size = new System.Drawing.Size(13, 15);
            this.ConnectedClients.TabIndex = 37;
            this.ConnectedClients.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(281, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 15);
            this.label9.TabIndex = 46;
            this.label9.Text = "Server Online time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(83, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 15);
            this.label1.TabIndex = 36;
            this.label1.Text = "Connected Clients";
            // 
            // TimeOnline
            // 
            this.TimeOnline.AutoSize = true;
            this.TimeOnline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.TimeOnline.Location = new System.Drawing.Point(395, 23);
            this.TimeOnline.Name = "TimeOnline";
            this.TimeOnline.Size = new System.Drawing.Size(61, 15);
            this.TimeOnline.TabIndex = 47;
            this.TimeOnline.Text = "00 : 00 : 00";
            // 
            // Logo
            // 
            this.Logo.Image = global::SKYNET.Properties.Resources._9;
            this.Logo.Location = new System.Drawing.Point(8, 6);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(57, 52);
            this.Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Logo.TabIndex = 1;
            this.Logo.TabStop = false;
            this.Logo.Click += new System.EventHandler(this.Logo_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(161)))), ((int)(((byte)(242)))));
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.Logo);
            this.panel2.Controls.Add(this.TimeOnline);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.ConnectedClients);
            this.panel2.Controls.Add(this.GC_Version);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.Accounts_Created);
            this.panel2.Controls.Add(this.Users_playing);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.ForeColor = System.Drawing.Color.White;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(700, 64);
            this.panel2.TabIndex = 33;
            // 
            // bodyContainer
            // 
            this.bodyContainer.BackColor = System.Drawing.Color.White;
            this.bodyContainer.Controls.Add(this.WebBrowserpnl);
            this.bodyContainer.Controls.Add(this.panel2);
            this.bodyContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyContainer.Location = new System.Drawing.Point(0, 26);
            this.bodyContainer.Name = "bodyContainer";
            this.bodyContainer.Size = new System.Drawing.Size(700, 507);
            this.bodyContainer.TabIndex = 41;
            // 
            // CM_UsersManager
            // 
            this.CM_UsersManager.Name = "CM_UsersManager";
            this.CM_UsersManager.Size = new System.Drawing.Size(174, 22);
            this.CM_UsersManager.Text = "Users manager";
            this.CM_UsersManager.Click += new System.EventHandler(this.CM_UsersManager_Click);
            // 
            // GlobalNotificationMenuItem
            // 
            this.GlobalNotificationMenuItem.Name = "GlobalNotificationMenuItem";
            this.GlobalNotificationMenuItem.Size = new System.Drawing.Size(174, 22);
            this.GlobalNotificationMenuItem.Text = "Send global notification";
            this.GlobalNotificationMenuItem.Click += new System.EventHandler(this.GlobalNotificationMenuItem_Click);
            // 
            // CM_ReloadItemsImage
            // 
            this.CM_ReloadItemsImage.Name = "CM_ReloadItemsImage";
            this.CM_ReloadItemsImage.Size = new System.Drawing.Size(174, 22);
            this.CM_ReloadItemsImage.Text = "Reload Items images";
            // 
            // CM_ReloadItemsDB
            // 
            this.CM_ReloadItemsDB.Name = "CM_ReloadItemsDB";
            this.CM_ReloadItemsDB.Size = new System.Drawing.Size(174, 22);
            this.CM_ReloadItemsDB.Text = "Reload Items in DB";
            this.CM_ReloadItemsDB.Click += new System.EventHandler(this.CM_ReloadItemsDB_Click);
            // 
            // CM_Export
            // 
            this.CM_Export.Name = "CM_Export";
            this.CM_Export.Size = new System.Drawing.Size(174, 22);
            this.CM_Export.Text = "Import/Export DB";
            this.CM_Export.Click += new System.EventHandler(this.CM_ExportImport_Click);
            // 
            // shadow
            // 
            this.shadow.BackColor = System.Drawing.Color.Transparent;
            this.shadow.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(26)))), ((int)(((byte)(37)))));
            this.shadow.Location = new System.Drawing.Point(0, 0);
            this.shadow.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.shadow.Name = "shadow";
            this.shadow.Opacity = 70;
            this.shadow.Size = new System.Drawing.Size(0, 0);
            this.shadow.TabIndex = 1;
            // 
            // Logger
            // 
            this.Logger.AutoScrollLines = true;
            this.Logger.BackColor = System.Drawing.Color.Yellow;
            this.Logger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Logger.Location = new System.Drawing.Point(8, 8);
            this.Logger.LoggerBackColor = System.Drawing.Color.White;
            this.Logger.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Logger.Name = "Logger";
            this.Logger.ScrollColors = System.Drawing.Color.Red;
            this.Logger.Size = new System.Drawing.Size(684, 384);
            this.Logger.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(700, 533);
            this.Controls.Add(this.shadow);
            this.Controls.Add(this.bodyContainer);
            this.Controls.Add(this.Browser);
            this.Controls.Add(this.acceptBtn);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "[SKYNET] Dota2 GCS";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.MinBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MinPic)).EndInit();
            this.CloseBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ClosePic)).EndInit();
            this.WebBrowserpnl.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.bodyContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button acceptBtn;
        private System.Windows.Forms.WebBrowser Browser;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem CM_UsersManager;
        private System.Windows.Forms.ToolStripMenuItem CM_ReloadItemsDB;
        private System.Windows.Forms.ToolStripMenuItem CM_ReloadItemsImage;
        private System.Windows.Forms.Panel CloseBox;
        private System.Windows.Forms.PictureBox ClosePic;
        private System.Windows.Forms.Panel MinBox;
        private System.Windows.Forms.PictureBox MinPic;
        public System.Windows.Forms.Label tittleLbl;
        private System.Windows.Forms.Panel WebBrowserpnl;
        private System.Windows.Forms.Timer Check;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label Users_playing;
        public System.Windows.Forms.Label Accounts_Created;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label GC_Version;
        public System.Windows.Forms.Label ConnectedClients;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label TimeOnline;
        private System.Windows.Forms.PictureBox Logo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem CM_Export;
        public Controls.WebBrowserLogger Logger;
        private ShadowBox shadow;
        private System.Windows.Forms.Panel bodyContainer;
        private System.Windows.Forms.ToolStripMenuItem GlobalNotificationMenuItem;
    }
}