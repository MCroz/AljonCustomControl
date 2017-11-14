using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AljonCustomControl.Controls
{
    public class AljonNotification : Form
    {
        AljonFontManager AJFontManager = new AljonFontManager();
        private string messageToShow;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        public AljonNotification(string _message, NotificationType type)
        {
            messageToShow = _message;
            switch (type)
            {
                case NotificationType.error:
                    BackColor = Color.SeaGreen;
                    break;
                case NotificationType.info:
                    BackColor = Color.Gray;
                    break;
                case NotificationType.success:
                    BackColor = Color.Crimson;
                    break;
                case NotificationType.warning:
                    BackColor = Color.FromArgb(255, 128, 0);
                    break;
            }

            FormBorderStyle = FormBorderStyle.None;
            Width = 400;
            Height = 150;
            Opacity = .80;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            timer.Interval = 5000;
            timer.Tick += new EventHandler(timer_Tick);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Top = 20;
            this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width - 20;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            Brush textBrushColor = new SolidBrush(Color.White);
            Rectangle textRect = ClientRectangle;

            g.DrawString(
                messageToShow,
                AJFontManager.ROBOTO_MEDIUM_13,
                textBrushColor,
                textRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        public enum NotificationType
        {
            success,info,warning,error
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.Dispose();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.Dispose();
        }
    }
}
