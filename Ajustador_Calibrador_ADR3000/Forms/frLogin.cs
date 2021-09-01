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
    public partial class frLogin : Form
    {
        //
        // Private variables, classes, structures and attributes
        //
        
        private string connectionString;
        private string Profile = "exit";
        private string User = "none";
        private bool closingFromSource = false;
        private bool authenticationOk = false;

        /// <summary>
        /// Constructor for the frLogin class.
        /// </summary>
        /// <param name="_connectionString">The connection string to be used with a database.</param>
        public frLogin(string _connectionString)
        {
            InitializeComponent();

            connectionString = _connectionString;
        }

        /// <summary>
        /// Gets the data necessary for the authentication.
        /// </summary>
        /// <returns>Returns information about the user being authenticated.</returns>
        public string[] getProfile()
        {
            ShowDialog();
            return(new string[] { User, Profile });
        }

        /// <summary>
        /// Event triggered when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Sair();
        }

        /// <summary>
        /// Event triggered when the OK button is clicked.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            Autenticar();
        }

        /// <summary>
        /// Event triggered when the instance of frLogin is active and a key is pressed on the keyboard.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) Autenticar();
            else if (e.KeyCode == Keys.Escape) Sair();
        }

        /// <summary>
        /// Event triggered when the instance of the frLogin is closing.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closingFromSource)
            {
                DialogResult dr = MessageBox.Show("Deseja realmente sair?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) e.Cancel = true;
                else Dispose();
            }
            else Dispose();
        }

        /// <summary>
        /// Authenticates the user of the program.
        /// </summary>
        private void Autenticar()
        {
            User = txtName.Text;
            string pass = txtPassword.Text;
            string condition = "WHERE Usuario = '" + User + "'";
            List<string> campos = new List<string>();
            List<string> valores = new List<string>();
            campos.Add("*");
            Recordset rs = new Recordset();
            MSAccess db = new MSAccess(connectionString);

            db.ConnectToDatabase();
            db.GetRecords("Acessos", campos, condition, ref rs);
            if ((rs.BOF) && (rs.EOF)) MessageBox.Show("Usuário inexistente", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                if (((dynamic)rs.Fields["Senha"]).Value != pass)
                    MessageBox.Show("Senha incorreta", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    Profile = ((dynamic)rs.Fields["Perfil"]).Value;
                    campos.Clear();
                    campos.Add("Online");
                    valores.Add("'On'");
                    condition = "WHERE Usuario = '" + User + "'";
                    db.UpdateData("Acessos", campos, valores, condition);
                    authenticationOk = true;
                }
            }

            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();
            if (authenticationOk)
            {
                closingFromSource = true;
                Close();
            }
        }

        /// <summary>
        /// Asks confirmation to the user and if positive, closes the program.
        /// </summary>
        private void Sair()
        {
            DialogResult drRes;
            drRes = MessageBox.Show("Deseja mesmo sair?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (drRes == DialogResult.Yes)
            {
                closingFromSource = true;
                Close();
            }        
        }

        /// <summary>
        /// Event triggered when the instance of the frLogin class is loaded.
        /// </summary>
        /// <param name="sender">The event caller.</param>
        /// <param name="e">Argument to the event.</param>
        private void frLogin_Load(object sender, EventArgs e)
        {
            txtName.Focus();
        }
    }
}
