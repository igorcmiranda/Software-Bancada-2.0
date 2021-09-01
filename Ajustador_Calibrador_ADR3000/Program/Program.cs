using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ajustador_Calibrador_ADR3000.Forms;

namespace Ajustador_Calibrador_ADR3000
{
    static class Program
    {
        /**
         * 
         *  Revision History:
         * 
         *  1.0.0 ->    1 - First version.
         *  1.4.0 ->    1 - Added the voltage generation for ADR 3000 and changed the 
         *              multithreading access scheme to the use of Interlocked class.
         *              2 - Also fixed some bugs on prescaler and GF333B-M communications.
         *              3 - Improved communications in general.
         *              4 - Included 500 ms delay between queries to standard meter to get
         *                  its measures.
         *              5 - Included 3000 ms delay after the adjustment of each point, so
         *                  the user is able to see the approved error.
         *              6 - Changed the way that ADR 3000 is calibrated using GF333B and
         *                  GF333B-M standards, now the system uses the standard's pulse
         *                  input.
         *              7 - Changed the layout of the calibration report to specify that the
         *                  date of the pdf generation is a print date.
         *  1.5.0 ->    1 - Added support for adjustment and calibration of ADR 2000 devices.
         *              2 - Included the 5 outputs transformer as a power source for adjustment
         *                  and calibration processes.
         *                  
         *  1.6.0 ->    1 - Added support for the ADR 3000 LITE adjustment and calibration process.                
         * 
         * 
         */


        /// <summary>
        /// Main entry point for the app.
        /// 
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frMain());
        }
    }
}
