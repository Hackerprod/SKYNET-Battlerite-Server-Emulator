using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKYNET
{
    public partial class LoginBox : UserControl
    {
        private string _text;
        bool _IsPassword;


        public event EventHandler OnReturnPressed;
        public Color Control_BorderColor { get; set; }
        public Color Control_BackColor
        {
            get
            {
                return BackColor;
            }
            set
            {
                BackColor = value;
                ChangeColors(false);
            }
        }
        public string Empty_Text
        {
            get
            {
                return lblSearch.Text;
            }
            set
            {
                lblSearch.Text = value;
            }
        }
        public bool ShowLogo
        {
            get
            {
                return _ShowLogo;
            }
            set
            {
                _ShowLogo = value;
                logo_box.Visible = value;
            }
        }
        bool _ShowLogo = true;
        public Image Logo
        {
            get
            {
                return logo_box.Image;
            }
            set
            {
                logo_box.Image = value;
            }
        }
        public Color ActivatedBackColor { get; set; }
        public LoginBox()
        {
            InitializeComponent();
            textBox.Text = Text;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.KeyPress += TextBox_KeyPress;
            textBox.KeyUp += TextBox_KeyUp;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;

            if (Control_BackColor == null)
            {
                Control_BackColor = BackColor;
            }
            if (ActivatedBackColor == null)
            {
                ActivatedBackColor = BackColor;
            }
        }

        public bool IsPassword
        {
            get { return _IsPassword; }
            set
            {
                _IsPassword = value;
                if (value)
                {
                    textBox.UseSystemPasswordChar = true;
                }
                else
                {
                    textBox.UseSystemPasswordChar = false;
                }
            }
        }

        public new string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                textBox.Text = _text;
            }
        }
        private void TexBox_TextChanged(object sender, EventArgs e)
        {
            textBox.ForeColor = ForeColor;
            lblSearch.ForeColor = this.ForeColor;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                lblSearch.Visible = true;
            }
            else
            {
                lblSearch.Visible = false;
            }
            base.OnTextChanged(e);
        }
        private void TexBox_Enter(object sender, EventArgs e)
        {
            BackColor = Control_BorderColor;
            ChangeColors(true);
        }

        private void ChangeColors(bool ChangeColors)
        {
            if (ChangeColors)
            {
                textBox.BackColor = ActivatedBackColor;
                lblSearch.BackColor = ActivatedBackColor;
                panel1.BackColor = ActivatedBackColor;
                panel2.BackColor = ActivatedBackColor;
                panel3.BackColor = ActivatedBackColor;
                panel4.BackColor = ActivatedBackColor;
                panel5.BackColor = ActivatedBackColor;
                logo_box.BackColor = ActivatedBackColor;

            }
            else
            {
                textBox.BackColor = Control_BackColor;
                lblSearch.BackColor = Control_BackColor;
                panel1.BackColor = Control_BackColor;
                panel2.BackColor = Control_BackColor;
                panel3.BackColor = Control_BackColor;
                panel4.BackColor = Control_BackColor;
                panel5.BackColor = Control_BackColor;
                logo_box.BackColor = Control_BackColor;

            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            BackColor = Control_BackColor;
            ChangeColors(false);
        }

        private void LblSearch_Click(object sender, EventArgs e)
        {
            textBox.Focus();
        }



        private void SearchTextBox_Load(object sender, EventArgs e)
        {
            lblSearch.BackColor = this.Control_BackColor;
            textBox.BackColor = this.Control_BackColor;
            panel1.BackColor = this.Control_BackColor;
            panel2.BackColor = this.Control_BackColor;
            panel3.BackColor = this.Control_BackColor;
            panel4.BackColor = this.Control_BackColor;
            panel5.BackColor = this.Control_BackColor;
            logo_box.BackColor = this.Control_BackColor;
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Text = textBox.Text;
            base.OnKeyDown(e);
        }
        private void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            Text = textBox.Text;
            base.OnPreviewKeyDown(e);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Text = textBox.Text;
            base.OnKeyUp(e);
            if (e.KeyData == Keys.Return)
            {
                OnReturnPressed?.Invoke(this, e);
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            Text = textBox.Text;
            base.OnKeyPress(e);
        }

        private void LoginBox_ForeColorChanged(object sender, EventArgs e)
        {
            textBox.ForeColor = ForeColor;
        }

        private void LoginBox_FontChanged(object sender, EventArgs e)
        {
            textBox.Font = Font;
        }
    }
}
