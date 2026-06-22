using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public class ToolStripEx : ToolStrip
    {
        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEACTIVATE = 0x21;

            try
            {
                if (m.Msg == WM_MOUSEACTIVATE && this.CanFocus && !this.Focused)
                    this.Focus();

                base.WndProc(ref m);
            }
            catch(Exception ex) { SaiTer.ATE.Log.Log.LogException(ex, "业务异常日志"); }
        }
    }
}
