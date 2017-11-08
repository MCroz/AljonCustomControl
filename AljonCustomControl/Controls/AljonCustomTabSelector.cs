using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using AljonCustomControl.Animations;

namespace AljonCustomControl.Controls
{
    public class AljonCustomTabSelector : Control, IAljonCustomControl
    {
        #region Variable Initiation
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private AljonCustomTabControl baseTabControl;

        private int previousSelectedTabIndex;
        private Point animationSource;
        private readonly AnimationManager animationManager;

        private List<Rectangle> tabRects;
        private const int TAB_HEADER_PADDING = 24;
        private const int TAB_INDICATOR_HEIGHT = 2;

        private Font AljonFont;
        private Color underlineColor;
        private Color foreColor;
        #endregion

        #region BaseTabControl Initiation
        public AljonCustomTabControl BaseTabControl
        {
            get { return baseTabControl; }
            set
            {
                baseTabControl = value;
                if (baseTabControl == null) return;
                previousSelectedTabIndex = baseTabControl.SelectedIndex;
                baseTabControl.Deselected += (sender, args) =>
                {
                    previousSelectedTabIndex = baseTabControl.SelectedIndex;
                };
                baseTabControl.SelectedIndexChanged += (sender, args) =>
                {
                    animationManager.SetProgress(0);
                    animationManager.StartNewAnimation(AnimationDirection.In);
                };
                baseTabControl.ControlAdded += delegate
                {
                    Invalidate();
                };
                baseTabControl.ControlRemoved += delegate
                {
                    Invalidate();
                };
            }
        }
        #endregion

        #region Initialization of the Control
        public AljonCustomTabSelector()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 48;

            animationManager = new AnimationManager
            {
                AnimationType = AnimationType.EaseOut,
                Increment = 0.04
            };
            animationManager.OnAnimationProgress += sender => Invalidate();

            AljonFont = Font;
            underlineColor = Color.Gray;
            foreColor = Color.Black;
        }
        #endregion

        #region OnPaint Override
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            //Para to sa pag config ng BackColor
            g.Clear(Parent.BackColor);
            //g.Clear(SkinManager.ColorScheme.PrimaryColor);

            if (baseTabControl == null) return;

            if (!animationManager.IsAnimating() || tabRects == null || tabRects.Count != baseTabControl.TabCount)
                UpdateTabRects();

            double animationProgress = animationManager.GetProgress();

            //Click feedback
            if (animationManager.IsAnimating())
            {
                var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationProgress * 50)), Color.White));
                var rippleSize = (int)(animationProgress * tabRects[baseTabControl.SelectedIndex].Width * 1.75);

                g.SetClip(tabRects[baseTabControl.SelectedIndex]);
                g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                g.ResetClip();
                rippleBrush.Dispose();
            }

            //Draw tab headers
            foreach (TabPage tabPage in baseTabControl.TabPages)
            {
                int currentTabIndex = baseTabControl.TabPages.IndexOf(tabPage);
                Brush textBrush = new SolidBrush(Color.FromArgb(CalculateTextAlpha(currentTabIndex, animationProgress), foreColor));

                g.DrawString(
                    tabPage.Text.ToUpper(),
                    //SkinManager.ROBOTO_MEDIUM_10,
                    AljonFont,
                    textBrush,
                    tabRects[currentTabIndex],
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                textBrush.Dispose();
            }

            //Animate tab indicator
            int previousSelectedTabIndexIfHasOne = previousSelectedTabIndex == -1 ? baseTabControl.SelectedIndex : previousSelectedTabIndex;
            Rectangle previousActiveTabRect = tabRects[previousSelectedTabIndexIfHasOne];
            Rectangle activeTabPageRect = tabRects[baseTabControl.SelectedIndex];

            int y = activeTabPageRect.Bottom - 2;
            int x = previousActiveTabRect.X + (int)((activeTabPageRect.X - previousActiveTabRect.X) * animationProgress);
            int width = previousActiveTabRect.Width + (int)((activeTabPageRect.Width - previousActiveTabRect.Width) * animationProgress);

            //original
            //g.FillRectangle(SkinManager.ColorScheme.AccentBrush, x, y, width, TAB_INDICATOR_HEIGHT);
            Brush AccentBrush = new SolidBrush(underlineColor);
            g.FillRectangle(AccentBrush, x, y, width, TAB_INDICATOR_HEIGHT);
        }
        #endregion

        #region Calculate Text Alpha
        private int CalculateTextAlpha(int tabIndex, double animationProgress)
        {
            //original
            //int primaryA = SkinManager.ACTION_BAR_TEXT.A;
            //int secondaryA = SkinManager.ACTION_BAR_TEXT_SECONDARY.A;
            int primaryA = 255;
            int secondaryA = 153;

            if (tabIndex == baseTabControl.SelectedIndex && !animationManager.IsAnimating())
            {
                return primaryA;
            }
            if (tabIndex != previousSelectedTabIndex && tabIndex != baseTabControl.SelectedIndex)
            {
                return secondaryA;
            }
            if (tabIndex == previousSelectedTabIndex)
            {
                return primaryA - (int)((primaryA - secondaryA) * animationProgress);
            }
            return secondaryA + (int)((primaryA - secondaryA) * animationProgress);
        }
        #endregion

        #region OnMouseUp
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (tabRects == null) UpdateTabRects();
            for (int i = 0; i < tabRects.Count; i++)
            {
                if (tabRects[i].Contains(e.Location))
                {
                    baseTabControl.SelectedIndex = i;
                }
            }

            animationSource = e.Location;
        }
        #endregion

        #region UpdateRectangle
        private void UpdateTabRects()
        {
            tabRects = new List<Rectangle>();

            //If there isn't a base tab control, the rects shouldn't be calculated
            //If there aren't tab pages in the base tab control, the list should just be empty which has been set already; exit the void
            if (baseTabControl == null || baseTabControl.TabCount == 0) return;

            //Calculate the bounds of each tab header specified in the base tab control
            using (var b = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(b))
                {
                    //original
                    //tabRects.Add(new Rectangle(SkinManager.FORM_PADDING, 0, TAB_HEADER_PADDING * 2 + (int)g.MeasureString(baseTabControl.TabPages[0].Text, SkinManager.ROBOTO_MEDIUM_10).Width, Height));
                    tabRects.Add(new Rectangle(14, 0, TAB_HEADER_PADDING * 2 + (int)g.MeasureString(baseTabControl.TabPages[0].Text, AljonFont).Width, Height));
                    for (int i = 1; i < baseTabControl.TabPages.Count; i++)
                    {
                        //tabRects.Add(new Rectangle(tabRects[i - 1].Right, 0, TAB_HEADER_PADDING * 2 + (int)g.MeasureString(baseTabControl.TabPages[i].Text, SkinManager.ROBOTO_MEDIUM_10).Width, Height));
                        tabRects.Add(new Rectangle(tabRects[i - 1].Right, 0, TAB_HEADER_PADDING * 2 + (int)g.MeasureString(baseTabControl.TabPages[i].Text, AljonFont).Width, Height));
                    }
                }
            }
        }
        #endregion

        #region Properties ng Control
        [Category("Underline Color")]
        [Description("The Color To Be Used for the Underline Rectangle")]
        public Color UnderLineColor
        {
            get
            {
                return this.underlineColor;
            }

            set
            {
                this.underlineColor = value;
                this.Invalidate();
            }
        }

        [Category("Aljon TabSelector Font")]
        [Description("The Font To Be Used By The Tab")]
        public Font TabSelectorFont
        {
            get
            {
                return this.AljonFont;
            }

            set
            {
                this.AljonFont = value;
                this.Invalidate();
            }
        }

        [Category("Aljon TabSelector Font Color")]
        [Description("The Color To Be Used Of The Font Inside The Tab Selector")]
        public Color TabSelectorFontColor
        {
            get
            {
                return this.foreColor;
            }

            set
            {
                this.foreColor = value;
                this.Invalidate();
            }
        }
        #endregion
    }
}
