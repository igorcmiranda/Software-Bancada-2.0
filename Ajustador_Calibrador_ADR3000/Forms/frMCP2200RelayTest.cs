using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ajustador_Calibrador_ADR3000.Devices;
using Ajustador_Calibrador_ADR3000.Helpers;
using ADODB;
using SimpleIO;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frMCP2200RelayTest : Form
    {
        private readonly string connectionString;

        public frMCP2200RelayTest(string _connectionString)
        {
            InitializeComponent();

            connectionString = _connectionString;
        }

        private void CenterControlInParent(Control ctrlToCenter)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;
            ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }

        private void frMCP2200RelayTest_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbVoltage.Text != "")
            {
                uint[] levels = new uint[] 
                {
                    Transformer5Out._TRANSFORMER_OFF_,
                    Transformer5Out._TRANSFORMER_100V_,
                    Transformer5Out._TRANSFORMER_120V_,
                    Transformer5Out._TRANSFORMER_180V_,
                    Transformer5Out._TRANSFORMER_220V_,
                    Transformer5Out._TRANSFORMER_240V_
                };

                
                Transformer5Out.EnableOutput(levels[cbVoltage.SelectedIndex]);
            }
        }

        private void frMCP2200RelayTest_Load(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();
            db.ConnectToDatabase();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            uint VendorID = Convert.ToUInt32(((dynamic)rs.Fields["VID"]).Value);
            uint ProductID = Convert.ToUInt32(((dynamic)rs.Fields["PID"]).Value);
            if (rs != null) rs.Close();
            db.CloseConnection();

            SimpleIOClass.InitMCP2200(VendorID, ProductID);
            if (!SimpleIOClass.IsConnected())
            {
                throw new ApplicationException("Falha ao conectar e configurar MCP2200 para trafo com 5 saídas.");
            }
            else
            {
                SimpleIOClass.ConfigureIoDefaultOutput(0xF0, (byte)Transformer5Out._TRANSFORMER_OFF_);
            }
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
