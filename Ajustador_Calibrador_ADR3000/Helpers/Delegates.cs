using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Ajustador_Calibrador_ADR3000.Delegates
{
    public delegate void AttachToolTipHandler(Control ct, string caption, Point p);
    public delegate void HideToolTipHandler();
    public delegate void UpdateTextHandler();
    public delegate void UpdateProgressHandler();
    public delegate void MyHandler();
}
