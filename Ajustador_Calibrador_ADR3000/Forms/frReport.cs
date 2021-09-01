using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADODB;
using Ajustador_Calibrador_ADR3000.Helpers;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frReport : Form
    {
        //
        // Private classes, attributes and structures
        //
        private string connectionString;

        /// <summary>
        /// The constructor for the frReport class.
        /// </summary>
        /// <param name="_connectionString">The connection string to connect with a database.</param>
        public frReport(string _connectionString)
        {
            InitializeComponent();
            connectionString = _connectionString;
        }
        
        /// <summary>
        /// Checks if the fields are correctly filled and generates a report
        /// </summary>
        private void action()
        {
            foreach(Control ct in Controls)
            {
                if((ct is ComboBox)||(ct is TextBox))
                {
                    if(ct.Text == "")
                    {
                        MessageBox.Show("Não são permitidos campos em branco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            if ((!ckbAtiva.Checked) && (!ckbReativa.Checked))
            {
                MessageBox.Show("Selecione ao menos um tipo de energia.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ushort ene = 0;
            if ((ckbAtiva.Checked)&&(!ckbReativa.Checked)) ene = 0;
            else if ((!ckbAtiva.Checked) && (ckbReativa.Checked)) ene = 1;
            else if ((ckbAtiva.Checked)&&(ckbReativa.Checked)) ene = 2;

            
            ReportGenerator reportGen = new ReportGenerator();
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset(), rsCustomer = new Recordset(), 
            rsStandard = new Recordset(), Montrel = new Recordset(), 
            dados = new Recordset(), sig = new Recordset(),
            userName = new Recordset();

            bool reportStatus = false;
            string modelo;
            string executorName;
            int start = cbStandards.Text.IndexOf('-') + 2;
            db.ConnectToDatabase();

            //
            //  Dummy report to get the number of pages
            //
            db.GetRecords("Clientes", new List<string>() { "*" }, "WHERE Nome = '" + cbCustomers.Text + "'", ref rsCustomer);
            modelo = cbStandards.Text.Substring(start, cbStandards.Text.Length - start);
            db.GetRecords("Padroes", new List<string>() { "*" }, "WHERE Modelo = '" + modelo + "'", ref rsStandard);
            db.GetRecords("Montrel", new List<string>() { "*" }, "", ref Montrel);
            db.GetRecords("Calibracao", new List<string>() { "*" }, "WHERE Laudo = '" + cbLaudo.Text + "' AND Nome = '" + cbDevices.Text + "' ORDER BY Energia, Clamp, Tensao, Corrente, Código", ref dados);
            db.GetRecords("Acessos", new List<string>() { "*" }, "WHERE Usuario = '" + ((dynamic)dados.Fields["Usuario"]).Value.ToString() + "'", ref userName);
            db.GetRecords("Assinaturas", new List<string>() { "*" }, "WHERE Chk = '1'", ref sig);
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);

            executorName = ((dynamic)userName.Fields["Nome_Usuario"]).Value.ToString();
            reportStatus = reportGen.CalibrationReport(rs, rsCustomer, rsStandard, Montrel, dados, sig, ene, executorName);

            if (reportGen.HasMoreThanNinePages)
            {
                //
                //  Real report
                //
                rs = new Recordset();
                rsCustomer = new Recordset();
                rsStandard = new Recordset();
                Montrel = new Recordset();
                dados = new Recordset();
                sig = new Recordset();
                userName = new Recordset();

                db.GetRecords("Clientes", new List<string>() { "*" }, "WHERE Nome = '" + cbCustomers.Text + "'", ref rsCustomer);
                modelo = cbStandards.Text.Substring(start, cbStandards.Text.Length - start);
                db.GetRecords("Padroes", new List<string>() { "*" }, "WHERE Modelo = '" + modelo + "'", ref rsStandard);
                db.GetRecords("Montrel", new List<string>() { "*" }, "", ref Montrel);
                db.GetRecords("Calibracao", new List<string>() { "*" }, "WHERE Laudo = '" + cbLaudo.Text + "' AND Nome = '" + cbDevices.Text + "' ORDER BY Energia, Clamp, Tensao, Corrente, Código", ref dados);
                db.GetRecords("Acessos", new List<string>() { "*" }, "WHERE Usuario = '" + ((dynamic)dados.Fields["Usuario"]).Value.ToString() + "'", ref userName);
                db.GetRecords("Assinaturas", new List<string>() { "*" }, "WHERE Chk = '1'", ref sig);
                db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);

                executorName = ((dynamic)userName.Fields["Nome_Usuario"]).Value.ToString();
                reportStatus = reportGen.CalibrationReport(rs, rsCustomer, rsStandard, Montrel, dados, sig, ene, executorName);
            }

            if (reportStatus)
                MessageBox.Show("Relatório gerado com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Falha na geração de relatório", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (rsCustomer != null) rsCustomer.Close();
            if (rsStandard != null) rsStandard.Close();
            if (Montrel != null) Montrel.Close();
            if (dados != null) dados.Close();
            if (sig != null) sig.Close();

            db.CloseConnection();
            db.Dispose();
        }

        /// <summary>
        /// Event triggered when the OK button is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            action();
        }

        /// <summary>
        /// Event triggered when the instance of frReport is active and a key is pressed.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) action();
            else if (e.KeyCode == Keys.Escape) Close();
        }

        /// <summary>
        /// Event triggered when the form is closing.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Event triggered when the index of the content of the combo box named cbDevices changes.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argumento to the event.</param>
        private void cbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbDate.Clear(); //Limpa textBox da data
            cbLaudo.Items.Clear(); //Limpa comboBox de laudos

            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            string condition = "WHERE Nome = '" + cbDevices.Text + "'";
            string laudo = "";
            db.ConnectToDatabase();

            db.GetRecords("Calibracao", new List<string>() { "Laudo" }, condition, ref rs);

            while (!rs.EOF)
            {
                if ((laudo == "") || (laudo != ((dynamic)rs.Fields[0]).Value))
                {
                    cbLaudo.Items.Add(rs.Fields[0].Value);
                    laudo = ((dynamic)rs.Fields[0]).Value;
                }
                rs.MoveNext();
            }

            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();
        }

        /// <summary>
        /// Event triggered when the index of the content of the combo box named cbLaudo changes.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argumento to the event.</param>
        private void cbLaudo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbDate.Clear(); //Limpa textBox da data

            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();
            string condition = "WHERE (Nome = '" + cbDevices.Text + "' AND Laudo = '" + cbLaudo.Text + "')";

            db.ConnectToDatabase();

            db.GetRecords("Calibracao", new List<string>() { "Data_" }, condition, ref rs);

            tbDate.Text = ((dynamic)rs.Fields[0]).Value.ToString("dd/MM/yyyy").Substring(0, 10);

            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();
        }

        /// <summary>
        /// Event triggered when the instance of frReport class loads.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frReport_Load(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rsCustomers = new Recordset(), rsStardards = new Recordset(), rsDevices = new Recordset();

            try
            {
                db.ConnectToDatabase(); //conecta com banco de dados
                /*coleta informações para preenchimento do Form*/
                db.GetRecords("Clientes", new List<string>() { "Nome" }, "", ref rsCustomers);
                db.GetRecords("Padroes", new List<string>() { "Tipo", "Modelo" }, "", ref rsStardards);
                db.GetDistinctData("Calibracao", "Nome", ref rsDevices);

                while (!rsCustomers.EOF)
                {
                    string text = rsCustomers.Fields[0].Value.ToString();
                    cbCustomers.Items.Add(text);
                    if (text.Length * cbCustomers.Font.Size > cbCustomers.DropDownWidth) cbCustomers.DropDownWidth = Convert.ToInt32(text.Length * cbCustomers.Font.Size);
                    rsCustomers.MoveNext();
                }
                while (!rsStardards.EOF)
                {
                    string text = rsStardards.Fields[0].Value.ToString() + " - " + rsStardards.Fields[1].Value.ToString();
                    cbStandards.Items.Add(text);
                    if (text.Length * cbStandards.Font.Size > cbStandards.DropDownWidth) cbStandards.DropDownWidth = Convert.ToInt32(text.Length * cbStandards.Font.Size);
                    rsStardards.MoveNext();
                }
                while (!rsDevices.EOF)
                {
                    string text = rsDevices.Fields[0].Value.ToString();
                    cbDevices.Items.Add(text);
                    if (text.Length * cbDevices.Font.Size > cbDevices.DropDownWidth) cbDevices.DropDownWidth = Convert.ToInt32(text.Length * cbDevices.Font.Size);
                    rsDevices.MoveNext();
                }

                if (rsCustomers != null) rsCustomers.Close();
                if (rsStardards != null) rsStardards.Close();
                if (rsDevices != null) rsDevices.Close();
                db.CloseConnection();
                db.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (rsCustomers != null) rsCustomers.Close();
                if (rsStardards != null) rsStardards.Close();
                if (rsDevices != null) rsDevices.Close();
                db.Dispose();
            }
        }
    }
}
