using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
public class FlatButton : Control
{
    [Flags]
    public enum _Style
    {
        TextOnly = 0x0,
        ImageIconOnly = 0x1,
        ImageIconWithText = 0x2
    }
    private Color _BackColorMouseOver { get; set; }


    public Color BackColorMouseOver
    {
        get
        {
            return _BackColorMouseOver;
        }
        set
        {
            _BackColorMouseOver = value;
        }
    }
    private Color _ForeColorMouseOver { get; set; }

    public Color ForeColorMouseOver
    {
        get
        {
            return _ForeColorMouseOver;
        }
        set
        {
            _ForeColorMouseOver = value;
        }
    }
    [Flags]
    public enum _ImgAlign
    {
        Left = 0x0,
        Center = 0x1,
        Right = 0x2
    }

    private int W;

    private int H;

    private int _ImageX;

    private int _ImageY;

    private bool _Rounded;

    private _Style _ButtonStyle;

    private Image _ImageIcon;

    public _ImgAlign _ImageAlign;

    private MouseState State;

    [Category("Button Options")]
    public bool Rounded
    {
        get
        {
            return _Rounded;
        }
        set
        {
            _Rounded = value;
        }
    }

    [Category("Button Options")]
    public _Style Style
    {
        get
        {
            return _ButtonStyle;
        }
        set
        {
            _ButtonStyle = value;
        }
    }

    [Category("Button Options")]
    public Image ImageIcon
    {
        get
        {
            return _ImageIcon;
        }
        set
        {
            _ImageIcon = value;
        }
    }

    [Category("Button Options")]
    public _ImgAlign ImageAlignment
    {
        get
        {
            return _ImageAlign;
        }
        set
        {
            _ImageAlign = value;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    public FlatButton()
    {
        _Rounded = false;
        _ButtonStyle = _Style.TextOnly;
        _ImageAlign = _ImgAlign.Left;
        State = MouseState.None;
        SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
        DoubleBuffered = true;
        base.Size = new Size(100, 32);
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 9f);
        Cursor = Cursors.Hand;
        _ButtonStyle = _Style.TextOnly;
        _ImageAlign = _ImgAlign.Left;
        _ImageX = 0;
        _ImageY = 0;
        _ImageIcon = null;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap B = new Bitmap(base.Width, base.Height);
        Graphics G = Graphics.FromImage(B);
        checked
        {
            W = base.Width - 1;
            H = base.Height - 1;
            if (_ButtonStyle == _Style.ImageIconOnly)
            {
                if (_ImageAlign == _ImgAlign.Left)
                {
                    _ImageX = 3;
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
                else if (_ImageAlign == _ImgAlign.Center)
                {
                    _ImageX = (int)Math.Round(unchecked(Math.Round((double)base.Width / 2.0) - Math.Round((double)_ImageIcon.Width / 2.0)));
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
                else if (_ImageAlign == _ImgAlign.Right)
                {
                    _ImageX = base.Width - _ImageIcon.Width - 3;
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
            }
            else if (_ButtonStyle == _Style.ImageIconWithText)
            {
                if (_ImageAlign == _ImgAlign.Left)
                {
                    _ImageX = 10;
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
                else if (_ImageAlign == _ImgAlign.Center)
                {
                    _ImageX = (int)Math.Round(unchecked(Math.Round((double)base.Width / 2.0) - Math.Round((double)_ImageIcon.Width / 2.0)));
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
                else if (_ImageAlign == _ImgAlign.Right)
                {
                    _ImageX = base.Width - _ImageIcon.Width - 3;
                    _ImageY = (int)Math.Round(unchecked(Math.Round((double)base.Height / 2.0) - Math.Round((double)_ImageIcon.Height / 2.0)));
                }
            }
            else if (_ButtonStyle == _Style.TextOnly)
            {
                _ImageX = 0;
                _ImageY = 0;
            }
            GraphicsPath graphicsPath = new GraphicsPath();
            Rectangle rectangle = new Rectangle(0, 0, W, H);
            new Point((int)Math.Round(Math.Round(unchecked((double)W / 2.0))), (int)Math.Round(Math.Round(unchecked((double)H / 2.0))));
            Graphics g = G;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);
            switch (State)
            {
                case MouseState.None:
                    if (Rounded)
                    {
                        graphicsPath = RoundRec(rectangle, 6);
                        g.FillPath(new SolidBrush(BackColor), graphicsPath);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(BackColor), rectangle);
                    }
                    break;
                case MouseState.Over:
                    if (Rounded)
                    {
                        graphicsPath = RoundRec(rectangle, 6);
                        g.FillPath(new SolidBrush(BackColor), graphicsPath);
                        g.FillPath(new SolidBrush(Color.FromArgb(20, Color.White)), graphicsPath);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(BackColor), rectangle);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.White)), rectangle);
                    }
                    break;
                case MouseState.Down:
                    if (Rounded)
                    {
                        graphicsPath = RoundRec(rectangle, 6);
                        g.FillPath(new SolidBrush(BackColor), graphicsPath);
                        g.FillPath(new SolidBrush(Color.FromArgb(20, Color.Black)), graphicsPath);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(BackColor), rectangle);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.Black)), rectangle);
                    }
                    break;
            }
            if (_ButtonStyle == _Style.TextOnly)
            {
                g.DrawString(Text, Font, new SolidBrush(ForeColor), rectangle, CenterSF);
            }
            else if (_ButtonStyle == _Style.ImageIconOnly)
            {
                g.DrawImage(_ImageIcon, _ImageX, _ImageY, _ImageIcon.Width, _ImageIcon.Height);
            }
            else if (_ButtonStyle == _Style.ImageIconWithText)
            {
                g.DrawImage(_ImageIcon, _ImageX, _ImageY, _ImageIcon.Width, _ImageIcon.Height);
                g.DrawString(Text, Font, new SolidBrush(ForeColor), rectangle, CenterSF);
            }
            g = null;
            base.OnPaint(e);
            G.Dispose();
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImageUnscaled(B, 0, 0);
            B.Dispose();
        }


    }
    internal static StringFormat CenterSF = new StringFormat
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };
    public static GraphicsPath RoundRec(Rectangle Rectangle, int Curve)
    {
        GraphicsPath graphicsPath = new GraphicsPath();
        checked
        {
            int num = Curve * 2;
            graphicsPath.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, num, num), -180f, 90f);
            graphicsPath.AddArc(new Rectangle(Rectangle.Width - num + Rectangle.X, Rectangle.Y, num, num), -90f, 90f);
            graphicsPath.AddArc(new Rectangle(Rectangle.Width - num + Rectangle.X, Rectangle.Height - num + Rectangle.Y, num, num), 0f, 90f);
            graphicsPath.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - num + Rectangle.Y, num, num), 90f, 90f);
            graphicsPath.AddLine(new Point(Rectangle.X, Rectangle.Height - num + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
            return graphicsPath;
        }
    }

    public static GraphicsPath RoundRect(float x, float y, float w, float h, float r = 0.3f, bool TL = true, bool TR = true, bool BR = true, bool BL = true)
    {
        float num = Math.Min(w, h) * r;
        float num2 = x + w;
        float num3 = y + h;
        GraphicsPath graphicsPath = new GraphicsPath();
        GraphicsPath graphicsPath2 = graphicsPath;
        if (TL)
        {
            graphicsPath2.AddArc(x, y, num, num, 180f, 90f);
        }
        else
        {
            graphicsPath2.AddLine(x, y, x, y);
        }
        if (TR)
        {
            graphicsPath2.AddArc(num2 - num, y, num, num, 270f, 90f);
        }
        else
        {
            graphicsPath2.AddLine(num2, y, num2, y);
        }
        if (BR)
        {
            graphicsPath2.AddArc(num2 - num, num3 - num, num, num, 0f, 90f);
        }
        else
        {
            graphicsPath2.AddLine(num2, num3, num2, num3);
        }
        if (BL)
        {
            graphicsPath2.AddArc(x, num3 - num, num, num, 90f, 90f);
        }
        else
        {
            graphicsPath2.AddLine(x, num3, x, num3);
        }
        graphicsPath2.CloseFigure();
        graphicsPath2 = null;
        return graphicsPath;
    }

}