using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AljonCustomControl.Controls
{
    public class AljonCustomTabControl : TabControl, IAljonCustomControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode) m.Result = (IntPtr)1;
            else base.WndProc(ref m);
        }
    }
}
