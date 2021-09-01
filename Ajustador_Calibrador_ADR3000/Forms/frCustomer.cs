using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ADODB;
using Ajustador_Calibrador_ADR3000.Helpers;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frCustomer : Form
    {
        //
        // Private variables, attributes, classes and structures
        //
        private long i = 0, MAX = 0;
        private string connectionString;

        /// <summary>
        /// Constructor for the frCustomer class.
        /// </summary>
        /// <param name="_connectionString">The connection string to be used with a database.</param>
        public frCustomer(string _connectionString)
        {
            InitializeComponent();
            connectionString = _connectionString;
        }

        /// <summary>
        /// Event triggered when the Add button is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            Add();
        }

        /// <summary>
        /// Event triggered when the Delete button is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void btnDel_Click(object sender, EventArgs e)
        {
            //Por enquanto não há deleção de clientes
        }

        /// <summary>
        /// Updates a customer record in the customers table.
        /// </summary>
        private void UpdateData()
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();
            string condition = "WHERE ID = " + i;

            try
            {
                db.ConnectToDatabase();
                db.GetRecords("Clientes", new List<string>() { "*" }, condition, ref rs);

                tbName.Text = ((dynamic)rs.Fields["Nome"]).Value;
                tbStreet.Text = ((dynamic)rs.Fields["Rua"]).Value;
                tbCity.Text = ((dynamic)rs.Fields["Cidade"]).Value;
                string aux = ((dynamic)rs.Fields["Fone"]).Value;
                tbPhone.Text = "(" + aux.Substring(0, 2) + ") " + aux.Substring(2, aux.Length - 2);
                tbNum.Text = ((dynamic)rs.Fields["Numero"]).Value.ToString();
                cbUF.Text = ((dynamic)rs.Fields["UF"]).Value;
                tbNbHood.Text = ((dynamic)rs.Fields["Bairro"]).Value;
                aux = ((dynamic)rs.Fields["CEP"]).Value.ToString();
                tbPost.Text = aux.Substring(0, 5) + "-" + aux.Substring(5, aux.Length - 5);
                tbEmail.Text = ((dynamic)rs.Fields["Email"]).Value;
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();
        }

        /// <summary>
        /// Event triggered when the instance of the frCustomer is active and there is a key pressed on the keyboard.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Left)
            {
                if (IsControlsEmpty())
                {
                    if (MessageBox.Show("Os dados preenchidos serão perdidos. Deseja" + 
                        " realmente prosseguir?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        != DialogResult.Yes)
                    {
                        return;
                    }
                }
                i--;
                if (i < 1) i = MAX;
                UpdateData();
                //Focus();
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (IsControlsEmpty())
                {
                    if (MessageBox.Show("Os dados preenchidos serão perdidos. Deseja" +
                        " realmente prosseguir?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        != DialogResult.Yes)
                    {
                        return;
                    }
                }
                i++;
                if (i > MAX) i = 1;
                UpdateData();
                //Focus();
            }
            else if (e.KeyCode == Keys.Escape) Close();
            else if (e.KeyCode == Keys.F2) Add();
        }

        /// <summary>
        /// Cleans all the fields in the instance of the frCustomer class.
        /// </summary>
        private void CleanTexts()
        {
            foreach (Control ct in Controls)
            {
                if (ct is TextBox) ((TextBox)ct).Text = "";
                else if (ct is ComboBox) ((ComboBox)ct).Text = "";
            }
        }

        /// <summary>
        /// Event triggered when the istance of the frCustomer class is closing.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Event triggered when the istance of the frCustomer class loads.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frCustomer_Load(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            try
            {
                db.ConnectToDatabase();
                db.MaxRecord("Clientes", "ID", "", ref rs);
                MAX = ((dynamic)rs.Fields[0]).Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();
        }

        private bool IsControlsEmpty()
        {
            foreach(Control ct in Controls)
            {
                if (ct is TextBox || ct is ComboBox)
                {
                    if (ct.Text == "") return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a new customer in the customers table.
        /// </summary>
        private void Add()
        {

            int n;
            long n1, n2;
            DialogResult dr = MessageBox.Show("Confirma novo cliente?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) return;
            else if ((!int.TryParse(tbNum.Text, out n)) || (!long.TryParse(tbPost.Text, out n1)) || (!long.TryParse(tbPhone.Text, out n2)))
            {
                MessageBox.Show("Campos número, CEP e telefone devem ser estritamente numéricos", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                foreach (Control tb in Controls)
                {
                    if (tb is TextBox)
                    {
                        if (((TextBox)tb).Text == "")
                        {
                            MessageBox.Show("Todos os campos são de preenchimento obrigatório", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else if (tb is ComboBox)
                    {
                        if (((ComboBox)tb).Text == "")
                        {
                            MessageBox.Show("Todos os campos são de preenchimento obrigatório", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                }
            }


            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            db.ConnectToDatabase();
            db.MaxRecord("Clientes", "ID", "", ref rs);

            long id;
            if ((rs.BOF) && (rs.EOF)) id = 1;
            else id = ((dynamic)rs.Fields[0]).Value + 1;

            if (rs != null) rs.Close();

            List<string> fields = new List<string>()
            {
                "ID",
                "Data_",
                "Nome",
                "Rua",
                "Numero",
                "Bairro",
                "Cidade",
                "UF",
                "CEP",
                "Fone",
                "Email"
            };
            List<string> values = new List<string>();


            values.Add("'" + id + "'");
            values.Add("'" + DateTime.Today.ToString("dd/MM/yyyy") + "'");
            values.Add("'" + tbName.Text + "'");
            values.Add("'" + tbStreet.Text + "'");
            values.Add("'" + tbNum.Text + "'");
            values.Add("'" + tbNbHood.Text + "'");
            values.Add("'" + tbCity.Text + "'");
            values.Add("'" + cbUF.Text + "'");
            values.Add("'" + tbPost.Text + "'");
            values.Add("'" + tbPhone.Text + "'");
            values.Add("'" + tbEmail.Text + "'");

            try
            {
                db.InsertData("Clientes", fields, values);
                MAX++;
                CleanTexts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            db.CloseConnection();
            db.Dispose();

        }
    }
}
