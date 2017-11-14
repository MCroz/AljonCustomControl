using AljonCustomControl.Properties;
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
        private System.Windows.Forms.Timer animateOpen = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer animateClose = new System.Windows.Forms.Timer();
        private Color leftBoxColor;
        private Image logo;
        private string bannerText;

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
                    leftBoxColor = Color.FromArgb(232, 76, 61);
                    bannerText = "Error";
                    logo = Resources.Cross;
                    break;
                case NotificationType.info:
                    leftBoxColor = Color.FromArgb(53, 152, 219);
                    bannerText = "Information";
                    logo = Resources.Information;
                    break;
                case NotificationType.success:
                    leftBoxColor = Color.FromArgb(45, 204, 112);
                    bannerText = "Success";
                    logo = Resources.Check;
                    break;
                case NotificationType.warning:
                    leftBoxColor = Color.FromArgb(241, 196, 15);
                    bannerText = "Warning";
                    logo = Resources.Warning;
                    break;
            }
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Width = 400;
            Height = 150;
            //Opacity = .90;

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            timer.Interval = 5000;
            timer.Tick += new EventHandler(timer_Tick);
            animateOpen.Interval = 1;
            animateOpen.Tick += new EventHandler(animateOpen_Tick);
            animateClose.Interval = 1;
            animateClose.Tick += new EventHandler(animateClose_Tick);
        }

        protected override void OnLoad(EventArgs e)
        {
            //base.OnLoad(e);
            this.Top = 20;
            //this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width - 20;
            this.Left = Screen.PrimaryScreen.Bounds.Width + this.Width;
            timer.Start();
            animateOpen.Start();
        }

        /*
        protected override void OnShown(EventArgs e)
        {
            timer.Start();
            animateOpen.Start();
        }
        */

        /*
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
        */

        public enum NotificationType
        {
            success, info, warning, error
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            animateClose.Start();
        }


        private void animateOpen_Tick(object sender, EventArgs e)
        {
            if (this.Left > Screen.PrimaryScreen.Bounds.Width - this.Width - 20)
            {
                this.Left -= 20;

                Graphics g = this.CreateGraphics();
                Rectangle sideRect = new Rectangle(0,0,100,150);
                Brush selBrush = new SolidBrush(leftBoxColor);
                g.FillRectangle(selBrush, sideRect);

                g.DrawImage(logo, new Point(26, 51));

                Brush bannerTextColor = new SolidBrush(leftBoxColor);
                Rectangle bannerRect = new Rectangle(110, 0, 290, 50);
                g.DrawString(
                    bannerText,
                    AJFontManager.ROBOTO_MEDIUM_20,
                    bannerTextColor,
                    bannerRect,
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far });

                Brush textBrushColor = new SolidBrush(Color.FromArgb(124, 124, 124));
                //Rectangle textRect = ClientRectangle;
                Rectangle textRect = new Rectangle(112,50,290,100);
                g.DrawString(
                    messageToShow,
                    AJFontManager.ROBOTO_MEDIUM_13,
                    textBrushColor,
                    textRect,
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });
            }
            else
            {
                animateOpen.Stop();
            }
        }

        private void animateClose_Tick(object sender, EventArgs e)
        {
            if (this.Left < Screen.PrimaryScreen.Bounds.Width + this.Width)
            {
                this.Left += 20;

                Graphics g = this.CreateGraphics();

                Rectangle sideRect = new Rectangle(0, 0, 100, 150);
                Brush selBrush = new SolidBrush(leftBoxColor);
                g.FillRectangle(selBrush, sideRect);

                Brush bannerTextColor = new SolidBrush(leftBoxColor);
                Rectangle bannerRect = new Rectangle(110, 0, 290, 50);
                g.DrawString(
                    bannerText,
                    AJFontManager.ROBOTO_MEDIUM_20,
                    bannerTextColor,
                    bannerRect,
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far });

                Brush textBrushColor = new SolidBrush(Color.White);
                //Rectangle textRect = ClientRectangle;
                Rectangle textRect = new Rectangle(110, 0, 300, 150);
                g.DrawString(
                    messageToShow,
                    AJFontManager.ROBOTO_MEDIUM_13,
                    textBrushColor,
                    textRect,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            else
            {
                animateClose.Stop();
                this.Close();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            this.Close();
        }
    }
}
