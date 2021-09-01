using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ADODB;
using Ajustador_Calibrador_ADR3000.Helpers;
using Ajustador_Calibrador_ADR3000.Devices;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frMain : Form
    {
        //
        // Private classes, variables, structures to be used
        //
        private bool closingFromCode = false;
        private string connectionString;
        private string user;
        private const int _VARIVOLT = 0;
        private const int _SUPPLIER = 1;
        private const int _ACP300 = 2;
        private const int _5_OUTPUTS_TRANSFORMER = 3;
        private int sourceType;
        private StandardMeter.StandardType standardType;

        /// <summary>
        /// Constructor for the frMain class.
        /// </summary>
        public frMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Centralizes a control in the screen.
        /// </summary>
        /// <param name="ctrlToCenter">The control to centralize.</param>
        private void CenterControlInParent(Control ctrlToCenter)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;
            ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }

        /// <summary>
        /// Event triggered when frMain loads.
        /// </summary>
        /// <param name="sender">Event caller.</param>
        /// <param name="e">Argument for the event</param>
        private void FrMain_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("Ganho de tensão: " + ADR2000.EE_CHV1 + Environment.NewLine + 
            //    "Ganho de corrente: " + ADR2000.EE_CHC1_0 + Environment.NewLine + 
            //    "Cte tensão: " + ADR2000.EE_VOLTAGE_CONST + Environment.NewLine + 
            //   "Cte corrente: " + ADR2000.EE_CURRENT_CONST);

            StreamReader stream = null;
            //
            // Tries to open the initialization file. If not successful, closes the program with a message
            //
            try
            {
                stream = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "param.ini"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arquivo de inicialização 'param.ini' faltando. Mensagem da exceção: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                closingFromCode = true;
                Close();
            }

            //
            // If successful, gets the path to the database used by the program
            //
            connectionString = stream.ReadLine();
            int start = connectionString.IndexOf("=") + 2;
            int length = connectionString.Length - start;

            connectionString = connectionString.Substring(start, length);

            string[] value = new frLogin(connectionString).getProfile();

            //
            // Checks if all went well with the authentication process
            //
            if (value[1] != "exit")
            {
                lbUser2.Text = value[0];
                lbHour2.Text = DateTime.Now.ToString("hh:mm:ss");
                tmTimerHour.Enabled = true;
                user = value[0];

                MSAccess db = new MSAccess(connectionString);
                Recordset rs = new Recordset();
                int source = 0;
                int stdType = 0;

                try
                {
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    source = ((dynamic)rs.Fields["Source"]).Value;
                    stdType = ((dynamic)rs.Fields["StandardMeter"]).Value;
                    rs.Close();
                    db.CloseConnection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                db.Dispose();

                if (source > 2) transformer5OutputsToolStripMenuItem.Checked = true;
                else if (source > 1) aCP300ToolStripMenuItem.Checked = true;
                else if (source > 0) supplierToolStripMenuItem.Checked = true;
                else varivoltToolStripMenuItem.Checked = true;
                sourceType = source;

                if (stdType > 2) gF333BMToolStripMenuItem.Checked = true;
                else if (stdType > 1) rMM3006ToolStripMenuItem.Checked = true;
                else if (stdType > 0) rD20ToolStripMenuItem.Checked = true;
                else gF333BToolStripMenuItem.Checked = true;

                standardType = (StandardMeter.StandardType)stdType;
            }
            else
            {
                closingFromCode = true;
                Close();
            }
        }

        /// <summary>
        /// Event triggered when the size of frMain changes.
        /// </summary>
        /// <param name="sender">Event caller.</param>
        /// <param name="e">Argument for the event</param>
        private void FrMain_SizeChanged(object sender, EventArgs e)
        {
            CenterControlInParent(pnMain);
        }

        /// <summary>
        /// Event triggered when the frMain is closing.
        /// </summary>
        /// <param name="sender">Event caller.</param>
        /// <param name="e">Arguments to the event.</param>
        private void FrMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closingFromCode)
            {
                DialogResult dialog = MessageBox.Show("Deseja realmente sair?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialog != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                else
                {
                    HandleUserInfoOnClosingForm();
                    Dispose();
                }
            }
            else
            {
                HandleUserInfoOnClosingForm();
                Dispose();
            }
        }

        /// <summary>
        /// Updates the log in status of the user in the database to offline.
        /// </summary>
        private void HandleUserInfoOnClosingForm()
        {
            MSAccess db = new MSAccess(connectionString);
            string condition = "WHERE Usuario = '" + user + "'";

            db.ConnectToDatabase();
            db.UpdateData("Acessos", new List<string>() { "Online" }, new List<string>() { "'Off'" }, condition);
            db.CloseConnection();
            db.Dispose();
        }

        /// <summary>
        /// Event triggered when the period configured in msTimer_Hour elapses.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument for the event.</param>
        private void TmTimerHour_Tick(object sender, EventArgs e)
        {
            lbHour2.Text = DateTime.Now.ToString("hh:mm:ss");
        }

        /// <summary>
        /// Event triggered when the item named Relatórios of the menu strip is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument for the event.</param>
        private void ReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frReport>().Count() == 0) new frReport(connectionString).Show();
            else Application.OpenForms.OfType<frReport>().FirstOrDefault().Activate();
        }

        /// <summary>
        /// Event triggered when the item named Clientes of the menu strip is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument for the event.</param>
        private void CustomersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frCustomer>().Count() == 0) new frCustomer(connectionString).Show();
            else Application.OpenForms.OfType<frCustomer>().FirstOrDefault().Activate();
        }

        private void VarivoltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "Source" }, new List<string>() { "0" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            varivoltToolStripMenuItem.Checked = true;
            supplierToolStripMenuItem.Checked = false;
            aCP300ToolStripMenuItem.Checked = false;
            transformer5OutputsToolStripMenuItem.Checked = false;
            sourceType = _VARIVOLT;
        }

        private void SupplierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "Source" }, new List<string>() { "1" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            varivoltToolStripMenuItem.Checked = false;
            supplierToolStripMenuItem.Checked = true;
            aCP300ToolStripMenuItem.Checked = false;
            transformer5OutputsToolStripMenuItem.Checked = false;
            sourceType = _SUPPLIER;
        }

        private void AjusteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frCalibration>().Count() > 0)
            {
                MessageBox.Show("Não pode haver uma instância do formulário de calibração junto com um formulário de ajuste.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Application.OpenForms.OfType<frAdjust>().Count() > 0) Application.OpenForms.OfType<frAdjust>().FirstOrDefault().Activate();
                else new frAdjust(sourceType, standardType, connectionString).Show();
            }
        }

        private void GF333BMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "StandardMeter" }, new List<string>() { "3" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            gF333BMToolStripMenuItem.Checked = true;
            rD20ToolStripMenuItem.Checked = false;
            rMM3006ToolStripMenuItem.Checked = false;
            gF333BToolStripMenuItem.Checked = false;

            standardType = StandardMeter.StandardType.GF333BM;
        }

        private void RD20ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "StandardMeter" }, new List<string>() { "1" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            gF333BMToolStripMenuItem.Checked = false;
            rD20ToolStripMenuItem.Checked = true;
            rMM3006ToolStripMenuItem.Checked = false;
            gF333BToolStripMenuItem.Checked = false;

            standardType = StandardMeter.StandardType.RD20;
        }

        private void CalibracaoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void FrMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                if (e.KeyCode == Keys.R)
                {
                    if (Application.OpenForms.OfType<frReport>().Count() == 0) new frReport(connectionString).Show();
                    else Application.OpenForms.OfType<frReport>().FirstOrDefault().Activate();
                }
                else if (e.KeyCode == Keys.C)
                {
                    if (Application.OpenForms.OfType<frCustomer>().Count() == 0) new frCustomer(connectionString).Show();
                    else Application.OpenForms.OfType<frCustomer>().FirstOrDefault().Activate();
                }
                else if (e.KeyCode == Keys.A)
                {
                    if (Application.OpenForms.OfType<frCalibration>().Count() > 0)
                    {
                        MessageBox.Show("Não pode haver uma instância do formulário de calibração junto com um formulário de ajuste.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (Application.OpenForms.OfType<frAdjust>().Count() > 0) Application.OpenForms.OfType<frAdjust>().FirstOrDefault().Activate();
                        else new frAdjust(sourceType, standardType, connectionString).Show();
                    }
                }
                else if (e.KeyCode == Keys.Q)
                {
                    if (Application.OpenForms.OfType<frAdjust>().Count() > 0)
                    {
                        MessageBox.Show("Não pode haver uma instância do formulário de ajuste junto com um formulário de calibração.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (Application.OpenForms.OfType<frCalibration>().Count() > 0) Application.OpenForms.OfType<frCalibration>().FirstOrDefault().Activate();
                        else new frCalibration(sourceType, connectionString, standardType, user).Show();
                    }
                }
            }
        }

        private void ConfigurarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((Application.OpenForms.OfType<frAdjust>().Count() > 0)||
                (Application.OpenForms.OfType<frCalibration>().Count() > 0))
            {
                MessageBox.Show("Não é permitido configurar a fonte com formulários de ajuste ou calibração abertos", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Application.OpenForms.OfType<frSourceControl>().Count() > 0) Application.OpenForms.OfType<frSourceControl>().FirstOrDefault().Activate();
                else new frSourceControl(connectionString, sourceType).Show();
            }
        }

        private void RMM3006ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "StandardMeter" }, new List<string>() { "2" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            gF333BMToolStripMenuItem.Checked = false;
            rD20ToolStripMenuItem.Checked = false;
            rMM3006ToolStripMenuItem.Checked = true;
            gF333BToolStripMenuItem.Checked = false;

            standardType = StandardMeter.StandardType.RMM3006;
        }

        private void ACP300ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "Source" }, new List<string>() { "2" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            varivoltToolStripMenuItem.Checked = false;
            supplierToolStripMenuItem.Checked = false;
            aCP300ToolStripMenuItem.Checked = true;
            transformer5OutputsToolStripMenuItem.Checked = false;
            sourceType = _ACP300;
        }

        private void GF333BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "StandardMeter" }, new List<string>() { "0" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            gF333BMToolStripMenuItem.Checked = false;
            rD20ToolStripMenuItem.Checked = false;
            rMM3006ToolStripMenuItem.Checked = false;
            gF333BToolStripMenuItem.Checked = true;

            standardType = StandardMeter.StandardType.GF333B;
        }

        private void TestStdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frShowInfo>().Count() > 0)
                Application.OpenForms.OfType<frShowInfo>().FirstOrDefault().Activate();
            else
                new frShowInfo(standardType).Show();   
        }

        private void Transformer5OutputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);

            try
            {
                db.ConnectToDatabase();
                db.UpdateData("Parametros", new List<string>() { "Source" }, new List<string>() { "3" }, "WHERE id = 1");
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                db.Dispose();
                return;
            }

            varivoltToolStripMenuItem.Checked = false;
            supplierToolStripMenuItem.Checked = false;
            aCP300ToolStripMenuItem.Checked = false;
            transformer5OutputsToolStripMenuItem.Checked = true;
            sourceType = _5_OUTPUTS_TRANSFORMER;
        }

        private void TestTransformer5OutputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frMCP2200RelayTest testForm = new frMCP2200RelayTest(connectionString);
            testForm.ShowDialog();
            testForm.Dispose();
        }

        private void ADR30002000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frCalibration>().Count() > 0)
            {
                MessageBox.Show("Não pode haver uma instância do formulário de calibração junto com um formulário de ajuste.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Application.OpenForms.OfType<frAdjust>().Count() > 0) Application.OpenForms.OfType<frAdjust>().FirstOrDefault().Activate();
                else new frAdjust(sourceType, standardType, connectionString).Show();
            }
        }

        private void ADR30002000ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frAdjust>().Count() > 0)
            {
                MessageBox.Show("Não pode haver uma instância do formulário de ajuste junto com um formulário de calibração.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Application.OpenForms.OfType<frCalibration>().Count() > 0) Application.OpenForms.OfType<frCalibration>().FirstOrDefault().Activate();
                else new frCalibration(sourceType, connectionString, standardType, user).Show();
            }
        }

        /// <summary>
        /// Event triggered when the item named Relatórios of the menu strip is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument for the event.</param>
        private void RelatóriosToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frReport>().Count() == 0) new frReport(connectionString).Show();
            else Application.OpenForms.OfType<frReport>().FirstOrDefault().Activate();
        }

        /// <summary>
        /// Event triggered when the item named Clientes of the menu strip is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument for the event.</param>
        private void ClientesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<frCustomer>().Count() == 0) new frCustomer(connectionString).Show();
            else Application.OpenForms.OfType<frCustomer>().FirstOrDefault().Activate();
        }
    }
}
