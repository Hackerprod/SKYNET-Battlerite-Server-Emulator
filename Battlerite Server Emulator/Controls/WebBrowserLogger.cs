using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mshtml;
using Microsoft.VisualBasic.CompilerServices;

namespace SKYNET.Controls
{
    public partial class WebBrowserLogger : UserControl
    {
        [Category("SKYNET")]
        public event EventHandler<WebBrowserDocumentCompletedEventArgs> DocumentCompleted;
        [Category("SKYNET")]
        public event EventHandler<WebBrowserNavigatingEventArgs> Navigating;

        public WebBrowserLogger()
        {
            InitializeComponent();
            InitializeWebBrowser();
        }
        private void InitializeWebBrowser()
        {
            InternetExplorerBrowserEmulation.SetBrowserEmulationVersion();

            webChat.ScriptErrorsSuppressed = true;
            webChat.Navigate("about:blank");
            while (webChat.Document == null || webChat.Document.Body == null)
                Application.DoEvents();
            webChat.Document.OpenNew(true).Write($"<html><head>" + ScrollBar(_scroll) + GetJS() + $" <style>{GetStyles()}</style>  <title name = 'head'>SKYNET</title>" + $"</head><body class='body' bgcolor=White><table id='table'>");
            AssignStyleSheet();

            webChat.Navigating += new WebBrowserNavigatingEventHandler(webChat_Navigating);
            webChat.ContextMenuStrip = base.ContextMenuStrip;    //! Set our ContextMenuStrip
            webChat.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnDocumentCompleted);

        }
        [Category("SKYNET")]
        public Color LoggerBackColor
        {
            get
            {
                return _LoggerBackColor;
            }
            set
            {
                _LoggerBackColor = value;
            }
        }
        Color _LoggerBackColor;

        [Category("SKYNET")]
        public Color ScrollColors
        {
            get
            {
                return _scroll;
            }
            set
            {
                _scroll = value;
            }
        }
        Color _scroll;

        [Category("SKYNET")]
        public bool AutoScrollLines
        {
            get
            {
                return _autoscroll;
            }
            set
            {
                _autoscroll = value;
            }
        }

        public bool CancelNext { get; private set; }

        bool _autoscroll = true;


        public void WriteLine(string content, ILog.MessageType type)
        {
            if (string.IsNullOrEmpty(content))
                return;

            string htmlMessage = HtmlHelper.GetMessage(content, type);
            Write(htmlMessage);
        }

        public void Write(string html)
        {
            try
            {
                int bodyHeight = Convert.ToInt32(webChat.Document.InvokeScript("GetPageHeight"));
                bool scroll = bodyHeight - Height == webChat.Document.Body.ScrollTop;
                
                if (webChat.InvokeRequired)
                {
                    webChat.Invoke(new Action(() => 
                    {
                        webChat.Document.Write(html);
                        try
                        {
                            if (scroll)
                            {
                                webChat.Document.Window.ScrollTo(0, webChat.Document.Body.ScrollRectangle.Height);
                            }
                        }
                        catch { }
                    }));
                }
                else
                {
                    webChat.Document.Write(html);
                    try
                    {
                        if (scroll)
                        {
                            webChat.Document.Window.ScrollTo(0, webChat.Document.Body.ScrollRectangle.Height);
                        }
                    }
                    catch { }

                }
            }
            catch { }
        }






        public void AssignStyleSheet()
        {
            string name = webChat.Name;
            IHTMLStyleSheet2 instance = (IHTMLStyleSheet2)((IHTMLDocument2)webChat.Document.DomDocument).createStyleSheet("", 0);

            NewLateBinding.LateSet(instance, null, "cssText", new object[1]
            {
                HtmlHelper.GetStyles()
            }, null, null);

            HtmlElement htmlElement = webChat.Document.GetElementsByTagName("head")[0];
            HtmlElement htmlElement2 = webChat.Document.CreateElement("script");
            IHTMLScriptElement iHTMLScriptElement = (IHTMLScriptElement)htmlElement2.DomElement;

            iHTMLScriptElement.text = HtmlHelper.GetJavascript();
            htmlElement.AppendChild(htmlElement2);
        }
        public static string ScrollBar(Color scroll)
        {
            return @"<style type='text/css'> body { " +
            $"scrollbar-face-color:#52a1f2; " + //barra
            $"scrollbar-highligh-color:{scroll}; " +
            $"scrollbar-3dligh-color:{scroll}; " +
            $"scrollbar-darkshadow-color:{scroll}; " +
            $"scrollbar-shadow-color:#73b5f8; " + //Borde afuera
            $"scrollbar-track-color:{scroll}; " + //Fondo de la barra
             "scrollbar-arrow-color:#52a1f2;" + //Arrow
            "} </style>" +
            "";
        }

        internal static string GetJS()
        {
            string  intHeigth = @"
                <script>
                    function GetPageHeight()
                    {
	                    var body = document.body;
 	                    var html = document.documentElement;
 	                    var height = Math.max(body.scrollHeight ,body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
 	                    return height;
                     }
                    function getSelectionText() 
                    { 
                        var text = '';                        
                        if (window.getSelection) 
                        {                               
                            text = window.getSelection().toString();
                        } 
                        else if (document.selection && document.selection.type != 'Control') 
                        {
                            text = document.selection.createRange().text;                        
                        }                        
                        return text;                    
                    }    
                </script>
                    ";

            return intHeigth;
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElementCollection body = webChat.Document.GetElementsByTagName("BODY");
            for (int i = 0; i < body.Count; i++)
            {
                HtmlElement el = body[i];
                el.AttachEventHandler("onclick", (Sender, args) => OnElementSelect(el, EventArgs.Empty));
            }
            DocumentCompleted?.Invoke(sender, e);
        }
        protected void OnElementSelect(object sender, EventArgs args)
        {
            //Hecho por mi
            string selection = webChat.Document.InvokeScript("getSelectionText").ToString();
            if (!string.IsNullOrEmpty(selection))
            {
                Clipboard.Clear();
                Clipboard.SetText(selection, TextDataFormat.UnicodeText);
            }
        }

        private void webChat_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Navigating?.Invoke(sender, e);
        }

        public void ClearScreen()
        {
            webChat.Document.OpenNew(true);
            InitializeWebBrowser();
        }
        private void InjectAlertBlocker(WebBrowser browser)
        {
            try
            {
                HtmlElement head = browser.Document?.GetElementsByTagName("head")[0];
                HtmlElement scriptEl = browser.Document?.CreateElement("script");
                IHTMLScriptElement element = (IHTMLScriptElement)scriptEl?.DomElement;
                string alertBlocker = "window.alert = function () { }";
                if (element != null) element.text = alertBlocker;
                if (scriptEl != null) head?.AppendChild(scriptEl);
            }
            catch { }
        }
        public static string GetStyles()
        {
            string clase = @"
            .message-data-time 
            {
                color: #a8aab1;
                font-size: 9pt;
                FONT-FAMILY: Segoe UI;
            }
            ";
            return clase;
        }
        private void WebChat_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }

    #region HtmlHelper
    public class HtmlHelper
    {
        public static string GetMessage(string content, ILog.MessageType type)
        {
            DateTime t = DateTime.Now;
            string time = Date(t.Day) + "/" + Date(t.Month) + "/" + Date(t.Year) + "  " + Hour(t.Hour) + ":" + Date(t.Minute);

            //
            StringBuilder code = new StringBuilder();
            code.AppendLine("<tr>");
            code.AppendLine("<td width=\"120\">");
            code.AppendLine("<h5 Class='message-data-time'>");
            code.AppendLine(time);
            code.AppendLine("</h5>");
            code.AppendLine("</td>");
            code.AppendLine("<td width=\"60\">");
            code.AppendLine(GetMessageTypeCode(type));
            code.AppendLine("</td>");
            code.AppendLine("<td width=\"500\">");
            code.AppendLine("<h5 Class='message-data-time'>");
            code.AppendLine(content);
            code.AppendLine("</h5>");
            code.AppendLine("</td>");
            code.AppendLine("</tr>");
            return code.ToString();
        }
        private static string Date(int time)
        {
            if (time < 10)
                return "0" + time;
            else
                return time.ToString();
        }
        private static string Time(int time)
        {
            if (time < 10)
                return "0" + time;
            else
                return Time(time - 12);
        }
        private static string Hour(int time)
        {
            if (time < 10)
                return "0" + time;
            else if (time > 9 && time < 13)
                return time.ToString();
            else
                return Hour(time - 12);
        }
        private static string GetMessageTypeCode(ILog.MessageType type)
        {
            string str1 = "";
            switch (type)
            {
                case ILog.MessageType.INFO:
                    str1 = "<h5 style='color:#a8aab1' Class='message-type'>" + type + "</h5>";
                    break;
                case ILog.MessageType.WARN:
                    str1 = "<h5 style='color:#f58207' Class='message-type'>" + type + "</h5>";
                    break;
                case ILog.MessageType.ERROR:
                    str1 = "<h5 style='color:#f50729' Class='message-type'>" + type + "</h5>";
                    break;
                case ILog.MessageType.DEBUG:
                    str1 = "<h5 style='color:#07a4f5' Class='message-type'>" + type + "</h5>";
                    break;
                default:
                    str1 = "<h5 style='color:#a8aab1' Class='message-type'>" + type + "</h5>";
                    break;
            }
            return str1;

        }

        public static string GetStyles()
        {
            string clase = @"

            .message-data-time {
            color: #a8aab1;
            font-size: 9pt;
            FONT-FAMILY: Segoe UI;
            }
            .message-type {
            color: #a8aab1;
            font-size: 9pt;
            FONT-FAMILY: Segoe UI;
            }

            ";
            return clase;
        }

        public static string GetJavascript()
        {
            return "";
        }


        public static string ScrollBar()
        {
            return @"<style type='text/css'> body { " +
            $"scrollbar-face-color:#3d4145; " + //barra
            $"scrollbar-highligh-color:#3d4145; " +
            $"scrollbar-3dligh-color:#00FF00; " +
            $"scrollbar-darkshadow-color:#00FFFF; " +
            $"scrollbar-shadow-color:#4d5156; " + //Borde afuera
            $"scrollbar-track-color:#1c1d20; " +
             "scrollbar-arrow-color:#fff;" +
            "} </style>" +
            "";
        }
    }
    #endregion

}
