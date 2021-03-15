

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.MinBox = new System.Windows.Forms.Panel();
            this.MinPic = new System.Windows.Forms.PictureBox();
            this.CloseBox = new System.Windows.Forms.Panel();
            this.ClosePic = new System.Windows.Forms.PictureBox();
            this.acceptBtn = new System.Windows.Forms.Button();
            this.Browser = new System.Windows.Forms.WebBrowser();
            this.CM_UsersManager = new System.Windows.Forms.ToolStripMenuItem();
            this.GlobalNotificationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_ReloadItemsImage = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_ReloadItemsDB = new System.Windows.Forms.ToolStripMenuItem();
            this.CM_Export = new System.Windows.Forms.ToolStripMenuItem();
            this.password = new System.Windows.Forms.Label();
            this._account = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this._Port = new SKYNET.LoginBox();
            this.b_CreateAccount = new FlatButton();
            this.b_Login = new FlatButton();
            this._ServerIp = new SKYNET.LoginBox();
            this._Password = new SKYNET.LoginBox();
            this._AccountName = new SKYNET.LoginBox();
            this.panel1.SuspendLayout();
            this.MinBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinPic)).BeginInit();
            this.CloseBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClosePic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.panel1.Controls.Add(this.MinBox);
            this.panel1.Controls.Add(this.CloseBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 233);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(416, 26);
            this.panel1.TabIndex = 5;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Event_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Event_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Event_MouseUp);
            // 
            // MinBox
            // 
            this.MinBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.MinBox.Controls.Add(this.MinPic);
            this.MinBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.MinBox.Location = new System.Drawing.Point(348, 0);
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
            this.CloseBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.CloseBox.Controls.Add(this.ClosePic);
            this.CloseBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.CloseBox.Location = new System.Drawing.Point(382, 0);
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
            // password
            // 
            this.password.AutoSize = true;
            this.password.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this.password.ForeColor = System.Drawing.Color.White;
            this.password.Location = new System.Drawing.Point(81, 349);
            this.password.Name = "password";
            this.password.Size = new System.Drawing.Size(66, 17);
            this.password.TabIndex = 51;
            this.password.Text = "Password";
            // 
            // _account
            // 
            this._account.AutoSize = true;
            this._account.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this._account.ForeColor = System.Drawing.Color.White;
            this._account.Location = new System.Drawing.Point(81, 275);
            this._account.Name = "_account";
            this._account.Size = new System.Drawing.Size(58, 17);
            this._account.TabIndex = 50;
            this._account.Text = "Account";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(81, 424);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 55;
            this.label1.Text = "Server";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(416, 233);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(247, 424);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 17);
            this.label2.TabIndex = 59;
            this.label2.Text = "Port";
            // 
            // _Port
            // 
            this._Port.ActivatedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Port.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Port.Control_BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Port.Control_BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Port.Empty_Text = "Fill all data";
            this._Port.ForeColor = System.Drawing.Color.White;
            this._Port.IsPassword = false;
            this._Port.Location = new System.Drawing.Point(250, 444);
            this._Port.Logo = global::SKYNET.Properties.Resources.network_document_48px;
            this._Port.Name = "_Port";
            this._Port.Padding = new System.Windows.Forms.Padding(2);
            this._Port.ShowLogo = true;
            this._Port.Size = new System.Drawing.Size(82, 37);
            this._Port.TabIndex = 58;
            this._Port.KeyUp += new System.Windows.Forms.KeyEventHandler(this._ServerIp_KeyUp);
            // 
            // b_CreateAccount
            // 
            this.b_CreateAccount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this.b_CreateAccount.BackColorMouseOver = System.Drawing.Color.Empty;
            this.b_CreateAccount.Cursor = System.Windows.Forms.Cursors.Hand;
            this.b_CreateAccount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.b_CreateAccount.ForeColor = System.Drawing.Color.White;
            this.b_CreateAccount.ForeColorMouseOver = System.Drawing.Color.Empty;
            this.b_CreateAccount.ImageAlignment = FlatButton._ImgAlign.Left;
            this.b_CreateAccount.ImageIcon = null;
            this.b_CreateAccount.Location = new System.Drawing.Point(213, 500);
            this.b_CreateAccount.Name = "b_CreateAccount";
            this.b_CreateAccount.Rounded = false;
            this.b_CreateAccount.Size = new System.Drawing.Size(119, 32);
            this.b_CreateAccount.Style = FlatButton._Style.TextOnly;
            this.b_CreateAccount.TabIndex = 57;
            this.b_CreateAccount.Text = "Create account";
            this.b_CreateAccount.Click += new System.EventHandler(this.B_CreateAccount_Click);
            // 
            // b_Login
            // 
            this.b_Login.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this.b_Login.BackColorMouseOver = System.Drawing.Color.Empty;
            this.b_Login.Cursor = System.Windows.Forms.Cursors.Hand;
            this.b_Login.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.b_Login.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.b_Login.ForeColorMouseOver = System.Drawing.Color.Empty;
            this.b_Login.ImageAlignment = FlatButton._ImgAlign.Left;
            this.b_Login.ImageIcon = null;
            this.b_Login.Location = new System.Drawing.Point(84, 500);
            this.b_Login.Name = "b_Login";
            this.b_Login.Rounded = false;
            this.b_Login.Size = new System.Drawing.Size(119, 32);
            this.b_Login.Style = FlatButton._Style.TextOnly;
            this.b_Login.TabIndex = 56;
            this.b_Login.Text = "Login";
            this.b_Login.Click += new System.EventHandler(this.B_Login_Click);
            // 
            // _ServerIp
            // 
            this._ServerIp.ActivatedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._ServerIp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._ServerIp.Control_BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._ServerIp.Control_BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._ServerIp.Empty_Text = "Fill all data";
            this._ServerIp.ForeColor = System.Drawing.Color.White;
            this._ServerIp.IsPassword = false;
            this._ServerIp.Location = new System.Drawing.Point(84, 444);
            this._ServerIp.Logo = global::SKYNET.Properties.Resources.network_document_48px;
            this._ServerIp.Name = "_ServerIp";
            this._ServerIp.Padding = new System.Windows.Forms.Padding(2);
            this._ServerIp.ShowLogo = true;
            this._ServerIp.Size = new System.Drawing.Size(160, 37);
            this._ServerIp.TabIndex = 54;
            this._ServerIp.KeyUp += new System.Windows.Forms.KeyEventHandler(this._ServerIp_KeyUp);
            // 
            // _Password
            // 
            this._Password.ActivatedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Password.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Password.Control_BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Password.Control_BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._Password.Empty_Text = "Fill all data";
            this._Password.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this._Password.ForeColor = System.Drawing.Color.White;
            this._Password.IsPassword = true;
            this._Password.Location = new System.Drawing.Point(84, 369);
            this._Password.Logo = global::SKYNET.Properties.Resources.key_2_60px;
            this._Password.Name = "_Password";
            this._Password.Padding = new System.Windows.Forms.Padding(2);
            this._Password.ShowLogo = true;
            this._Password.Size = new System.Drawing.Size(248, 37);
            this._Password.TabIndex = 53;
            this._Password.KeyUp += new System.Windows.Forms.KeyEventHandler(this._Password_KeyUp);
            // 
            // _AccountName
            // 
            this._AccountName.ActivatedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._AccountName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._AccountName.Control_BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._AccountName.Control_BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(54)))), ((int)(((byte)(68)))));
            this._AccountName.Empty_Text = "Fill all data";
            this._AccountName.Font = new System.Drawing.Font("Malgun Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this._AccountName.ForeColor = System.Drawing.Color.White;
            this._AccountName.IsPassword = false;
            this._AccountName.Location = new System.Drawing.Point(84, 295);
            this._AccountName.Logo = global::SKYNET.Properties.Resources.menu_contacts;
            this._AccountName.Name = "_AccountName";
            this._AccountName.Padding = new System.Windows.Forms.Padding(2);
            this._AccountName.ShowLogo = true;
            this._AccountName.Size = new System.Drawing.Size(248, 37);
            this._AccountName.TabIndex = 52;
            this._AccountName.KeyUp += new System.Windows.Forms.KeyEventHandler(this._AccountName_KeyUp);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(416, 567);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._Port);
            this.Controls.Add(this.b_CreateAccount);
            this.Controls.Add(this.b_Login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._ServerIp);
            this.Controls.Add(this._Password);
            this.Controls.Add(this._AccountName);
            this.Controls.Add(this.password);
            this.Controls.Add(this._account);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Browser);
            this.Controls.Add(this.acceptBtn);
            this.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "[SKYNET] Battlerite Client";
            this.Load += new System.EventHandler(this.FrmClient_Load);
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Event_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Event_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Event_MouseUp);
            this.panel1.ResumeLayout(false);
            this.MinBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MinPic)).EndInit();
            this.CloseBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ClosePic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button acceptBtn;
        private System.Windows.Forms.WebBrowser Browser;
        private System.Windows.Forms.ToolStripMenuItem CM_UsersManager;
        private System.Windows.Forms.ToolStripMenuItem CM_ReloadItemsDB;
        private System.Windows.Forms.ToolStripMenuItem CM_ReloadItemsImage;
        private System.Windows.Forms.Panel CloseBox;
        private System.Windows.Forms.PictureBox ClosePic;
        private System.Windows.Forms.Panel MinBox;
        private System.Windows.Forms.PictureBox MinPic;
        private System.Windows.Forms.ToolStripMenuItem CM_Export;
        private System.Windows.Forms.ToolStripMenuItem GlobalNotificationMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label password;
        private System.Windows.Forms.Label _account;
        private LoginBox _AccountName;
        private LoginBox _Password;
        private LoginBox _ServerIp;
        private System.Windows.Forms.Label label1;
        private FlatButton b_Login;
        private FlatButton b_CreateAccount;
        private LoginBox _Port;
        private System.Windows.Forms.Label label2;
    }
}