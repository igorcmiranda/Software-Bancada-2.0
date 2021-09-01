using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using Ajustador_Calibrador_ADR3000.Devices;
using Ajustador_Calibrador_ADR3000.Delegates;
using Ajustador_Calibrador_ADR3000.Helpers;
using ADODB;
using System.IO.Ports;
using System.IO;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frSourceControl : Form
    {
        private const int _SUPPLIER = 1;
        private const int _ACP300 = 2;

        private TcpClient tcpClient;
        private PowerSupply powerSupply;
        private ACP300 acpPowerSource;
        private readonly string connectionString;
        private readonly int sourceType;
        public frSourceControl(string _connectionString, int _sourceType)
        {
            InitializeComponent();
            connectionString = _connectionString;
            sourceType = _sourceType;
        }

        private void ConfigureToolTip(Control ct, string caption, Point p)
        {
            ttHint.SetToolTip(ct, caption);
            ttHint.Show(caption, this, p);
        }

        private void AttachToolTip(Control ct, string caption, Point p)
        {
            if (InvokeRequired) Invoke(new AttachToolTipHandler(ConfigureToolTip), new object[] { ct, caption, p });
            else ConfigureToolTip(ct, caption, p);
        }

        private void HideToolTip()
        {
            if (InvokeRequired) Invoke(new HideToolTipHandler(() => { ttHint.Hide(this); }));
            else ttHint.Hide(this);
        }

        private void FrSourceControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (powerSupply != null) powerSupply.Dispose();
            if (acpPowerSource != null) acpPowerSource.Dispose();
            if (tcpClient != null) tcpClient.Dispose();
            Dispose();
        }

        private void TbSetPoint_MouseHover(object sender, EventArgs e)
        {
            Point p = tbSetPoint.FindForm().PointToClient(tbSetPoint.Parent.PointToScreen(tbSetPoint.Location));
            AttachToolTip(tbSetPoint, "Set point para fonte", p);
        }

        private void TbSetPoint_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtMagnitudeAll_MouseHover(object sender, EventArgs e)
        {
            Point p = btMagnitudeAll.FindForm().PointToClient(btMagnitudeAll.Parent.PointToScreen(btMagnitudeAll.Location));
            AttachToolTip(btMagnitudeAll, "Envia set point para amplitude trifásica", p);
        }

        private void BtMagnitudeAll_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtMagnitudeA_MouseHover(object sender, EventArgs e)
        {
            Point p = btMagnitudeA.FindForm().PointToClient(btMagnitudeA.Parent.PointToScreen(btMagnitudeA.Location));
            AttachToolTip(btMagnitudeA, "Envia set point para amplitude da fase A", p);
        }

        private void BtMagnitudeA_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtMagnitudeB_MouseHover(object sender, EventArgs e)
        {
            Point p = btMagnitudeB.FindForm().PointToClient(btMagnitudeB.Parent.PointToScreen(btMagnitudeB.Location));
            AttachToolTip(btMagnitudeB, "Envia set point para amplitude da fase B", p);
        }

        private void BtMagnitudeB_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtMagnitudeC_MouseHover(object sender, EventArgs e)
        {
            Point p = btMagnitudeC.FindForm().PointToClient(btMagnitudeC.Parent.PointToScreen(btMagnitudeC.Location));
            AttachToolTip(btMagnitudeC, "Envia set point para amplitude da fase C", p);
        }

        private void BtMagnitudeC_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtPhaseShiftAll_MouseHover(object sender, EventArgs e)
        {
            Point p = btPhaseShiftAll.FindForm().PointToClient(btPhaseShiftAll.Parent.PointToScreen(btPhaseShiftAll.Location));
            AttachToolTip(btPhaseShiftAll, "Envia set point para defasagem trifásica", p);
        }

        private void BtPhaseShiftAll_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtPhaseShiftA_MouseHover(object sender, EventArgs e)
        {
            Point p = btPhaseShiftA.FindForm().PointToClient(btPhaseShiftA.Parent.PointToScreen(btPhaseShiftA.Location));
            AttachToolTip(btPhaseShiftA, "Envia set point para defasagem da fase A", p);
        }

        private void BtPhaseShiftA_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtPhaseShiftB_MouseHover(object sender, EventArgs e)
        {
            Point p = btPhaseShiftB.FindForm().PointToClient(btPhaseShiftB.Parent.PointToScreen(btPhaseShiftB.Location));
            AttachToolTip(btPhaseShiftB, "Envia set point para defasagem da fase B", p);
        }

        private void BtPhaseShiftB_MouseLeave(object sender, EventArgs e)
        {
            HideToolTip();
        }

        private void BtPhaseShiftC_MouseHover(object sender, EventArgs e)
        {
            Point p = btPhaseShiftC.FindForm().PointToClient(btPhaseShiftC.Parent.PointToScreen(btPhaseShiftC.Location));
            AttachToolTip(btPhaseShiftC, "Envia set point para defasagem da fase C", p);
        }

        private void FonteVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            try
            {
                if (sourceType == _SUPPLIER)
                {
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    string sourceIP = ((dynamic)rs.Fields["VAddr"]).Value.ToString();
                    if (rs != null) rs.Close();
                    db.CloseConnection();
                    db.Dispose();

                    tcpClient = new TcpClient();
                    tcpClient.Connect(sourceIP, 502);

                    powerSupply = new PowerSupply(tcpClient.GetStream());
                }
                else
                {
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);

                    int baudRate = Convert.ToInt32(rs.Fields["BAUD_PowerSupply"].Value);
                    int stopBits = Convert.ToInt32(rs.Fields["StopBits_PowerSupply"].Value);
                    int parity = Convert.ToInt32(rs.Fields["Parity_PowerSupply"].Value);
                    int dataSize = Convert.ToInt32(rs.Fields["DataSize_PowerSupply"].Value);
                    string port = rs.Fields["COM_PowerSupply"].Value.ToString();

                    if (rs != null) rs.Close();
                    db.CloseConnection();
                    db.Dispose();

                    acpPowerSource = new ACP300(port, baudRate, dataSize,
                        (Parity)parity, (StopBits)stopBits, false, false);

                    if (!acpPowerSource.InitSerial())
                    {
                        throw new IOException("Falha ao conectar com fonte de tensão ACP300");
                    }

                    Answer answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                    answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                    if (!answer.Status)
                    {
                        throw new IOException("Falha ao conectar com fonte de tensão ACP300");
                    }

                    answer = acpPowerSource.SendCommand(ACP300.SET_FREQ_RANGE, ACP300.F_RANG_HZ);
                    if (!answer.Status)
                    {
                        throw new IOException("Falha ao conectar com fonte de tensão ACP300");
                    }
                }

                lbStatusState.Text = "Conectado";
                lbStatusState.ForeColor = Color.Green;

                UpdateControlsStatus(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (rs != null) rs.Close();
                if (db != null) db.Dispose();
                
                lbStatusState.Text = "Desconectado";
                lbStatusState.ForeColor = Color.Red;

                UpdateControlsStatus(false);
            }

            
        }

        private void DesconectarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (powerSupply != null) powerSupply.Dispose();
            if (acpPowerSource != null) acpPowerSource.Dispose();
            if (tcpClient != null) tcpClient.Dispose();
            lbStatusState.Text = "Desconectado";
            lbStatusState.ForeColor = Color.Red;

            UpdateControlsStatus(false);
        }

        private void UpdateControlsStatus(bool status)
        {
            btMagnitudeAll.Enabled = status;
            btFrequency.Enabled = status;
            btON.Enabled = status;
            btOFF.Enabled = status;
        }

        private void BtMagnitudeAll_Click(object sender, EventArgs e)
        {
            if (float.TryParse(tbSetPoint.Text, out float setPoint))
            {
                try
                {
                    if (sourceType == _SUPPLIER)
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, setPoint, PowerSupply.Command.PutSetPoint);
                    }
                    else
                    {
                        Answer answer = acpPowerSource.SendCommand(ACP300.SET_VOLTAGE, tbSetPoint.Text);
                        if (!answer.Status)
                        {
                            MessageBox.Show("Problemas na comunicação com a fonte de tensão.",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("Valor inválido para set point.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtFrequency_Click(object sender, EventArgs e)
        {
            if (float.TryParse(tbSetPoint.Text, out float setPoint))
            {
                try
                {
                    if (sourceType == _SUPPLIER)
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, setPoint, PowerSupply.Command.SetFrequency);
                    }
                    else
                    {
                        Answer answer = acpPowerSource.SendCommand(ACP300.SET_FREQUENCY, tbSetPoint.Text);
                        if (!answer.Status)
                        {
                            MessageBox.Show("Problemas na comunicação com a fonte de tensão.",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("Valor inválido para set point.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Compares two arrays element by element.
        /// </summary>
        /// <param name="a1">The first array.</param>
        /// <param name="a2">The second array.</param>
        /// <returns>True if the arrays are identic, false otherwise.</returns>
        private bool CheckArrays(Array a1, Array a2)
        {
            //
            // Checks if the arrays are the same reference
            //
            if (a1 == a2) return (true);

            //
            // Checks if one of them is a null reference
            //
            if ((a1 == null) || (a2 == null)) return (false);

            //
            // Checks if they have the same length
            //
            if (a1.Length != a2.Length) return (false);

            //
            // Verifies each element of the arrays and if the arrays are not identical to each other
            // returns false
            //
            int i;
            for (i = 0; i < a1.Length; i++)
            {
                if (a1.GetValue(i) != a2.GetValue(i)) return (false);
            }

            //
            // If there is no problem returns true
            //
            return (true);
        }

        private void BtON_Click(object sender, EventArgs e)
        {
            try
            {
                if (sourceType == _SUPPLIER)
                {
                    powerSupply.TurnOn();
                }
                else
                {
                    Answer answer = acpPowerSource.SendCommand(ACP300.SET_OUTPUT, ACP300._ON);
                    if (!answer.Status)
                    {
                        throw new IOException("Problemas na comunicação com a fonte de tensão.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtOFF_Click(object sender, EventArgs e)
        {
            try
            {
                if (sourceType == _SUPPLIER)
                {
                    powerSupply.TurnOff();
                }
                else
                {
                    Answer answer = acpPowerSource.SendCommand(ACP300.SET_OUTPUT, ACP300._OFF);
                    if (!answer.Status)
                    {
                        throw new IOException("Problemas na comunicação com a fonte de tensão.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
