using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace HeroItems
{
    [DefaultEvent("SelectedIndexChanged")]
    class HeroComboBox : ComboBox
    {

        private int X;
        public HeroComboBox() : base()
        {
            TextChanged += GhostCombo_TextChanged;
            DropDownClosed += GhostComboBox_DropDownClosed;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 18;
            BackColor = Color.FromArgb(43, 47, 48);
            DropDownStyle = ComboBoxStyle.DropDownList;

        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            X = e.X;
            Invalidate();
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            X = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!(DropDownStyle == ComboBoxStyle.DropDownList))
                DropDownStyle = ComboBoxStyle.DropDownList;
            Bitmap B = new Bitmap(Width, Height);
            Graphics G = Graphics.FromImage(B);

            G.Clear(Color.FromArgb(43, 47, 48));
            LinearGradientBrush GradientBrush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height / 5 * 2), Color.FromArgb(63, 67, 68), Color.FromArgb(43, 47, 48), 90f);
            G.FillRectangle(GradientBrush, new Rectangle(0, 0, Width, Height / 5 * 2));
            HatchBrush hatch = default(HatchBrush);

            int S1 = (int)G.MeasureString("OrainsComboBox", Font).Height;
            if (SelectedIndex != -1)
            {
                G.DrawString(Items[SelectedIndex].ToString(), Font, new SolidBrush(ForeColor), 4, Height / 2 - S1 / 2);
            }
            else
            {
                if ((Items != null) & Items.Count > 0)
                {
                    G.DrawString(Items[0].ToString(), Font, new SolidBrush(ForeColor), 4, Height / 2 - S1 / 2);
                }
                else
                {
                    G.DrawString("OrainsComboBox", Font, new SolidBrush(ForeColor), 4, Height / 2 - S1 / 2);
                }
            }

            if (MouseButtons == MouseButtons.None & X > Width - 25)
            {
                //G.FillRectangle(new SolidBrush(Color.FromArgb(7, Color.White)), Width - 25, 1, Width - 25, Height - 3);
                  //ElseIf MouseButtons = Windows.Forms.MouseButtons.None and X < Width - 25 And X >= 0 Then
                G.FillRectangle(new SolidBrush(Color.FromArgb(7, Color.Red)), 2, 1, Width - 5, Height - 3);
            }

            G.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
            G.DrawRectangle(new Pen(Color.FromArgb(40, 40, 40)), 1, 1, Width - 3, Height - 3);
            //G.DrawLine(New Pen(Color.FromArgb(40, 40, 40)), Width - 25, 1, Width - 25, Height - 3)
            //G.DrawLine(Pens.Black, Width - 24, 0, Width - 24, Height)
            // G.DrawLine(New Pen(Color.FromArgb(40, 40, 40)), Width - 23, 1, Width - 23, Height - 3)

            G.FillPolygon(Brushes.Black, Triangle(new Point(Width - 14, Height / 2), new Size(5, 3)));
            G.FillPolygon(Brushes.White, Triangle(new Point(Width - 15, Height / 2 - 1), new Size(5, 3)));

            e.Graphics.DrawImage((Bitmap)B.Clone(), 0, 0);
            G.Dispose();
            B.Dispose();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            Rectangle rect = new Rectangle();
            rect.X = e.Bounds.X;
            rect.Y = e.Bounds.Y;
            rect.Width = e.Bounds.Width - 1;
            rect.Height = e.Bounds.Height - 1;

            e.DrawBackground();
            if ((int)e.State == 785 | (int)e.State == 17)
            {
                e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);
                Rectangle x2 = new Rectangle(e.Bounds.Location, new Size(e.Bounds.Width + 2, e.Bounds.Height));
                Rectangle x3 = new Rectangle(x2.Location, new Size(x2.Width, (x2.Height / 2) - 1));
                LinearGradientBrush G1 = new LinearGradientBrush(new Point(x2.X, x2.Y), new Point(x2.X, x2.Y + x2.Height), Color.FromArgb(63, 67, 68), Color.FromArgb(23, 27, 28));//Color mausemove
                HatchBrush H = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Color.FromArgb(63, 67, 68), Color.Transparent); 
                e.Graphics.FillRectangle(G1, x2);
                G1.Dispose();
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(43, 47, 48)), x3);
                e.Graphics.FillRectangle(H, x2);
                G1.Dispose();
                e.Graphics.DrawString(" " + Items[e.Index].ToString(), Font, Brushes.White, e.Bounds.X, e.Bounds.Y + 1); //ForeColor mousemove
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);
                e.Graphics.DrawString(" " + Items[e.Index].ToString(), Font, Brushes.Gray, e.Bounds.X, e.Bounds.Y + 1);

            }

            base.OnDrawItem(e);
        }

        public Point[] Triangle(Point Location, Size Size)
        {
            Point[] ReturnPoints = new Point[4];
            ReturnPoints[0] = Location;
            ReturnPoints[1] = new Point(Location.X + Size.Width, Location.Y);
            ReturnPoints[2] = new Point(Location.X + Size.Width / 2, Location.Y + Size.Height);
            ReturnPoints[3] = Location;

            return ReturnPoints;
        }

        private void GhostComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            DropDownStyle = ComboBoxStyle.Simple;
            Application.DoEvents();
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void GhostCombo_TextChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // HeroComboBox
            // 
            this.SelectedIndexChanged += new System.EventHandler(this.HeroComboBox_SelectedIndexChanged);
            this.ResumeLayout(false);

        }
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Invalidate();
        }
        private void HeroComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            base.SelectedIndexChanged += HeroComboBox_SelectedIndexChanged;
            Invalidate();
        }
    }

}
