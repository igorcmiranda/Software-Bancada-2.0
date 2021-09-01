namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frCustomer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frCustomer));
            this.tbEmail = new System.Windows.Forms.TextBox();
            this.lbEmail = new System.Windows.Forms.Label();
            this.tbPhone = new System.Windows.Forms.TextBox();
            this.lbPhone = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tbPost = new System.Windows.Forms.TextBox();
            this.lbPost = new System.Windows.Forms.Label();
            this.cbUF = new System.Windows.Forms.ComboBox();
            this.lbUF = new System.Windows.Forms.Label();
            this.tbCity = new System.Windows.Forms.TextBox();
            this.lbCity = new System.Windows.Forms.Label();
            this.tbNbHood = new System.Windows.Forms.TextBox();
            this.lbNbHood = new System.Windows.Forms.Label();
            this.tbNum = new System.Windows.Forms.TextBox();
            this.lbNum = new System.Windows.Forms.Label();
            this.tbStreet = new System.Windows.Forms.TextBox();
            this.lbStreet = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lbName = new System.Windows.Forms.Label();
            this.btnDel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // tbEmail
            // 
            this.tbEmail.Location = new System.Drawing.Point(406, 221);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(278, 20);
            this.tbEmail.TabIndex = 8;
            // 
            // lbEmail
            // 
            this.lbEmail.AutoSize = true;
            this.lbEmail.Location = new System.Drawing.Point(362, 224);
            this.lbEmail.Name = "lbEmail";
            this.lbEmail.Size = new System.Drawing.Size(38, 13);
            this.lbEmail.TabIndex = 38;
            this.lbEmail.Text = "E-mail:";
            // 
            // tbPhone
            // 
            this.tbPhone.Location = new System.Drawing.Point(80, 218);
            this.tbPhone.Name = "tbPhone";
            this.tbPhone.Size = new System.Drawing.Size(273, 20);
            this.tbPhone.TabIndex = 7;
            // 
            // lbPhone
            // 
            this.lbPhone.AutoSize = true;
            this.lbPhone.Location = new System.Drawing.Point(40, 221);
            this.lbPhone.Name = "lbPhone";
            this.lbPhone.Size = new System.Drawing.Size(34, 13);
            this.lbPhone.TabIndex = 36;
            this.lbPhone.Text = "Fone:";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(459, 299);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(97, 30);
            this.btnAdd.TabIndex = 9;
            this.btnAdd.Text = "Adicionar";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // picLogo
            // 
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.InitialImage = null;
            this.picLogo.Location = new System.Drawing.Point(488, 30);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(225, 82);
            this.picLogo.TabIndex = 34;
            this.picLogo.TabStop = false;
            // 
            // tbPost
            // 
            this.tbPost.Location = new System.Drawing.Point(538, 181);
            this.tbPost.Name = "tbPost";
            this.tbPost.Size = new System.Drawing.Size(146, 20);
            this.tbPost.TabIndex = 6;
            // 
            // lbPost
            // 
            this.lbPost.AutoSize = true;
            this.lbPost.Location = new System.Drawing.Point(501, 184);
            this.lbPost.Name = "lbPost";
            this.lbPost.Size = new System.Drawing.Size(31, 13);
            this.lbPost.TabIndex = 32;
            this.lbPost.Text = "CEP:";
            // 
            // cbUF
            // 
            this.cbUF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUF.FormattingEnabled = true;
            this.cbUF.Items.AddRange(new object[] {
            "AC",
            "AL",
            "AP",
            "AM",
            "BA",
            "CE",
            "DF",
            "ES",
            "GO",
            "MA",
            "MT",
            "MS",
            "MG",
            "PA",
            "PB",
            "PR",
            "PE",
            "PI",
            "RJ",
            "RN",
            "RS",
            "RO",
            "RR",
            "SC",
            "SP",
            "SE",
            "TO"});
            this.cbUF.Location = new System.Drawing.Point(406, 177);
            this.cbUF.Name = "cbUF";
            this.cbUF.Size = new System.Drawing.Size(66, 21);
            this.cbUF.TabIndex = 5;
            // 
            // lbUF
            // 
            this.lbUF.AutoSize = true;
            this.lbUF.Location = new System.Drawing.Point(376, 181);
            this.lbUF.Name = "lbUF";
            this.lbUF.Size = new System.Drawing.Size(24, 13);
            this.lbUF.TabIndex = 30;
            this.lbUF.Text = "UF:";
            // 
            // tbCity
            // 
            this.tbCity.Location = new System.Drawing.Point(80, 178);
            this.tbCity.Name = "tbCity";
            this.tbCity.Size = new System.Drawing.Size(273, 20);
            this.tbCity.TabIndex = 4;
            // 
            // lbCity
            // 
            this.lbCity.AutoSize = true;
            this.lbCity.Location = new System.Drawing.Point(31, 181);
            this.lbCity.Name = "lbCity";
            this.lbCity.Size = new System.Drawing.Size(43, 13);
            this.lbCity.TabIndex = 28;
            this.lbCity.Text = "Cidade:";
            // 
            // tbNbHood
            // 
            this.tbNbHood.Location = new System.Drawing.Point(538, 137);
            this.tbNbHood.Name = "tbNbHood";
            this.tbNbHood.Size = new System.Drawing.Size(146, 20);
            this.tbNbHood.TabIndex = 3;
            // 
            // lbNbHood
            // 
            this.lbNbHood.AutoSize = true;
            this.lbNbHood.Location = new System.Drawing.Point(495, 140);
            this.lbNbHood.Name = "lbNbHood";
            this.lbNbHood.Size = new System.Drawing.Size(37, 13);
            this.lbNbHood.TabIndex = 26;
            this.lbNbHood.Text = "Bairro:";
            // 
            // tbNum
            // 
            this.tbNum.Location = new System.Drawing.Point(406, 137);
            this.tbNum.Name = "tbNum";
            this.tbNum.Size = new System.Drawing.Size(66, 20);
            this.tbNum.TabIndex = 2;
            // 
            // lbNum
            // 
            this.lbNum.AutoSize = true;
            this.lbNum.Location = new System.Drawing.Point(380, 140);
            this.lbNum.Name = "lbNum";
            this.lbNum.Size = new System.Drawing.Size(20, 13);
            this.lbNum.TabIndex = 24;
            this.lbNum.Text = "nº:";
            // 
            // tbStreet
            // 
            this.tbStreet.Location = new System.Drawing.Point(80, 137);
            this.tbStreet.Name = "tbStreet";
            this.tbStreet.Size = new System.Drawing.Size(273, 20);
            this.tbStreet.TabIndex = 1;
            // 
            // lbStreet
            // 
            this.lbStreet.AutoSize = true;
            this.lbStreet.Location = new System.Drawing.Point(44, 140);
            this.lbStreet.Name = "lbStreet";
            this.lbStreet.Size = new System.Drawing.Size(30, 13);
            this.lbStreet.TabIndex = 22;
            this.lbStreet.Text = "Rua:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(80, 96);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(273, 20);
            this.tbName.TabIndex = 0;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(36, 99);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(38, 13);
            this.lbName.TabIndex = 20;
            this.lbName.Text = "Nome:";
            // 
            // btnDel
            // 
            this.btnDel.Location = new System.Drawing.Point(587, 299);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(97, 30);
            this.btnDel.TabIndex = 10;
            this.btnDel.Text = "Deletar";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // frCustomer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 362);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.tbEmail);
            this.Controls.Add(this.lbEmail);
            this.Controls.Add(this.tbPhone);
            this.Controls.Add(this.lbPhone);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.tbPost);
            this.Controls.Add(this.lbPost);
            this.Controls.Add(this.cbUF);
            this.Controls.Add(this.lbUF);
            this.Controls.Add(this.tbCity);
            this.Controls.Add(this.lbCity);
            this.Controls.Add(this.tbNbHood);
            this.Controls.Add(this.lbNbHood);
            this.Controls.Add(this.tbNum);
            this.Controls.Add(this.lbNum);
            this.Controls.Add(this.tbStreet);
            this.Controls.Add(this.lbStreet);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lbName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "frCustomer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clientes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frCustomer_FormClosing);
            this.Load += new System.EventHandler(this.frCustomer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frCustomer_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.Label lbEmail;
        private System.Windows.Forms.TextBox tbPhone;
        private System.Windows.Forms.Label lbPhone;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.TextBox tbPost;
        private System.Windows.Forms.Label lbPost;
        private System.Windows.Forms.ComboBox cbUF;
        private System.Windows.Forms.Label lbUF;
        private System.Windows.Forms.TextBox tbCity;
        private System.Windows.Forms.Label lbCity;
        private System.Windows.Forms.TextBox tbNbHood;
        private System.Windows.Forms.Label lbNbHood;
        private System.Windows.Forms.TextBox tbNum;
        private System.Windows.Forms.Label lbNum;
        private System.Windows.Forms.TextBox tbStreet;
        private System.Windows.Forms.Label lbStreet;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Button btnDel;
    }
}