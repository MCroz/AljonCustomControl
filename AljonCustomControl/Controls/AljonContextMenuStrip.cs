using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using AljonCustomControl.Animations;

namespace AljonCustomControl.Controls
{
    public class AljonContextMenuStrip : ContextMenuStrip, IAljonCustomControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        internal AnimationManager animationManager;
        internal Point animationSource;

        public delegate void ItemClickStart(object sender, ToolStripItemClickedEventArgs e);
        public event ItemClickStart OnItemClickStart;

        public AljonContextMenuStrip()
        {
            Renderer = new MaterialToolStripRender();

            animationManager = new AnimationManager(false)
            {
                Increment = 0.07,
                AnimationType = AnimationType.Linear
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
            animationManager.OnAnimationFinished += sender => OnItemClicked(delayesArgs);

            //original
            //BackColor = SkinManager.GetApplicationBackgroundColor();
            BackColor = Color.FromArgb(255, 255, 255, 255);
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            base.OnMouseUp(mea);

            animationSource = mea.Location;
        }

        private ToolStripItemClickedEventArgs delayesArgs;
        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem != null && !(e.ClickedItem is ToolStripSeparator))
            {
                if (e == delayesArgs)
                {
                    //The event has been fired manualy because the args are the ones we saved for delay
                    base.OnItemClicked(e);
                }
                else
                {
                    //Interrupt the default on click, saving the args for the delay which is needed to display the animaton
                    delayesArgs = e;

                    //Fire custom event to trigger actions directly but keep cms open
                    if (OnItemClickStart != null) OnItemClickStart(this, e);

                    //Start animation
                    animationManager.StartNewAnimation(AnimationDirection.In);
                }
            }
        }
    }

    public class AljonToolStripMenuItem : ToolStripMenuItem
    {
        public AljonToolStripMenuItem()
        {
            AutoSize = false;
            Size = new Size(120, 30);
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            var baseDropDown = base.CreateDefaultDropDown();
            if (DesignMode) return baseDropDown;

            var defaultDropDown = new AljonContextMenuStrip();
            defaultDropDown.Items.AddRange(baseDropDown.Items);

            return defaultDropDown;
        }
    }

    internal class MaterialToolStripRender : ToolStripProfessionalRenderer, IAljonCustomControl
    {
        //Properties for managing the material design properties
        public int Depth { get; set; }
        public MouseState MouseState { get; set; }
        public AljonFontManager FontManager;

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            //Brush Override
            Color blak = Color.Black;
            Brush Blacck = new SolidBrush(blak);
            Color samp = Color.FromArgb(66, 0, 0, 0);
            Brush disabledBlacck = new SolidBrush(samp);

            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            var itemRect = GetItemRect(e.Item);
            var textRect = new Rectangle(24, itemRect.Y, itemRect.Width - (24 + 16), itemRect.Height);
            g.DrawString(
                e.Text,
                //SkinManager.ROBOTO_MEDIUM_10,
                //e.Item.Enabled ? SkinManager.GetPrimaryTextBrush() : SkinManager.GetDisabledOrHintBrush(),
                FontManager.ROBOTO_MEDIUM_10,
                e.Item.Enabled ? Blacck : disabledBlacck,
                textRect,
                new StringFormat { LineAlignment = StringAlignment.Center });
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color wayt = Color.White;

            Color CMS_BACKGROUND_LIGHT_HOVER = Color.FromArgb(255, 238, 238, 238);
            Brush CmsSelectedItemBrush = new SolidBrush(CMS_BACKGROUND_LIGHT_HOVER);

            var g = e.Graphics;
            g.Clear(wayt);

            //Draw background
            var itemRect = GetItemRect(e.Item);
            g.FillRectangle(e.Item.Selected && e.Item.Enabled ? CmsSelectedItemBrush : new SolidBrush(wayt), itemRect);

            //Ripple animation
            var toolStrip = e.ToolStrip as AljonContextMenuStrip;
            if (toolStrip != null)
            {
                var animationManager = toolStrip.animationManager;
                var animationSource = toolStrip.animationSource;
                if (toolStrip.animationManager.IsAnimating() && e.Item.Bounds.Contains(animationSource))
                {
                    for (int i = 0; i < animationManager.GetAnimationCount(); i++)
                    {
                        var animationValue = animationManager.GetProgress(i);
                        var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), Color.Black));
                        var rippleSize = (int)(animationValue * itemRect.Width * 2.5);
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, itemRect.Y - itemRect.Height, rippleSize, itemRect.Height * 3));
                    }
                }
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            //base.OnRenderImageMargin(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var g = e.Graphics;
            Color wayte = Color.White;
            Color DIVIDERS_BLACK = Color.FromArgb(31, 0, 0, 0);

            g.FillRectangle(new SolidBrush(wayte), e.Item.Bounds);
            g.DrawLine(
                new Pen(DIVIDERS_BLACK),
                new Point(e.Item.Bounds.Left, e.Item.Bounds.Height / 2),
                new Point(e.Item.Bounds.Right, e.Item.Bounds.Height / 2));
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            var g = e.Graphics;
            Color DIVIDERS_BLACK = Color.FromArgb(31, 0, 0, 0);

            g.DrawRectangle(
                new Pen(DIVIDERS_BLACK),
                new Rectangle(e.AffectedBounds.X, e.AffectedBounds.Y, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            //Brush Override
            Color blak = Color.Black;
            Brush Blacck = new SolidBrush(blak);
            Color samp = Color.FromArgb(66, 0, 0, 0);
            Brush disabledBlacck = new SolidBrush(samp);

            var g = e.Graphics;
            const int ARROW_SIZE = 4;

            var arrowMiddle = new Point(e.ArrowRectangle.X + e.ArrowRectangle.Width / 2, e.ArrowRectangle.Y + e.ArrowRectangle.Height / 2);
            var arrowBrush = e.Item.Enabled ? Blacck : disabledBlacck;
            using (var arrowPath = new GraphicsPath())
            {
                arrowPath.AddLines(
                    new[] { 
                        new Point(arrowMiddle.X - ARROW_SIZE, arrowMiddle.Y - ARROW_SIZE), 
                        new Point(arrowMiddle.X, arrowMiddle.Y), 
                        new Point(arrowMiddle.X - ARROW_SIZE, arrowMiddle.Y + ARROW_SIZE) });
                arrowPath.CloseFigure();

                g.FillPath(arrowBrush, arrowPath);
            }
        }

        private Rectangle GetItemRect(ToolStripItem item)
        {
            return new Rectangle(0, item.ContentRectangle.Y, item.ContentRectangle.Width + 4, item.ContentRectangle.Height);
        }
    }
}
