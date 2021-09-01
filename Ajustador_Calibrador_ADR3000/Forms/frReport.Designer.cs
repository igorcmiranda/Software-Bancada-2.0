namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frReport));
            this.lbCustomer = new System.Windows.Forms.Label();
            this.cbCustomers = new System.Windows.Forms.ComboBox();
            this.cbStandards = new System.Windows.Forms.ComboBox();
            this.lbStandard = new System.Windows.Forms.Label();
            this.cbDevices = new System.Windows.Forms.ComboBox();
            this.lbDevice = new System.Windows.Forms.Label();
            this.cbLaudo = new System.Windows.Forms.ComboBox();
            this.lbLaudo = new System.Windows.Forms.Label();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.lbDate = new System.Windows.Forms.Label();
            this.tbDate = new System.Windows.Forms.TextBox();
            this.ckbAtiva = new System.Windows.Forms.CheckBox();
            this.ckbReativa = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // lbCustomer
            // 
            this.lbCustomer.AutoSize = true;
            this.lbCustomer.Location = new System.Drawing.Point(16, 37);
            this.lbCustomer.Name = "lbCustomer";
            this.lbCustomer.Size = new System.Drawing.Size(42, 13);
            this.lbCustomer.TabIndex = 0;
            this.lbCustomer.Text = "Cliente:";
            // 
            // cbCustomers
            // 
            this.cbCustomers.FormattingEnabled = true;
            this.cbCustomers.Location = new System.Drawing.Point(107, 34);
            this.cbCustomers.Name = "cbCustomers";
            this.cbCustomers.Size = new System.Drawing.Size(293, 21);
            this.cbCustomers.TabIndex = 0;
            // 
            // cbStandards
            // 
            this.cbStandards.FormattingEnabled = true;
            this.cbStandards.Location = new System.Drawing.Point(107, 61);
            this.cbStandards.Name = "cbStandards";
            this.cbStandards.Size = new System.Drawing.Size(293, 21);
            this.cbStandards.TabIndex = 1;
            // 
            // lbStandard
            // 
            this.lbStandard.AutoSize = true;
            this.lbStandard.Location = new System.Drawing.Point(16, 64);
            this.lbStandard.Name = "lbStandard";
            this.lbStandard.Size = new System.Drawing.Size(85, 13);
            this.lbStandard.TabIndex = 2;
            this.lbStandard.Text = "Padrão utilizado:";
            // 
            // cbDevices
            // 
            this.cbDevices.FormattingEnabled = true;
            this.cbDevices.Location = new System.Drawing.Point(107, 88);
            this.cbDevices.Name = "cbDevices";
            this.cbDevices.Size = new System.Drawing.Size(114, 21);
            this.cbDevices.TabIndex = 2;
            this.cbDevices.SelectedIndexChanged += new System.EventHandler(this.cbDevices_SelectedIndexChanged);
            // 
            // lbDevice
            // 
            this.lbDevice.AutoSize = true;
            this.lbDevice.Location = new System.Drawing.Point(16, 91);
            this.lbDevice.Name = "lbDevice";
            this.lbDevice.Size = new System.Drawing.Size(61, 13);
            this.lbDevice.TabIndex = 4;
            this.lbDevice.Text = "Dispositivo:";
            // 
            // cbLaudo
            // 
            this.cbLaudo.FormattingEnabled = true;
            this.cbLaudo.Location = new System.Drawing.Point(107, 115);
            this.cbLaudo.Name = "cbLaudo";
            this.cbLaudo.Size = new System.Drawing.Size(114, 21);
            this.cbLaudo.TabIndex = 3;
            this.cbLaudo.SelectedIndexChanged += new System.EventHandler(this.cbLaudo_SelectedIndexChanged);
            // 
            // lbLaudo
            // 
            this.lbLaudo.AutoSize = true;
            this.lbLaudo.Location = new System.Drawing.Point(16, 118);
            this.lbLaudo.Name = "lbLaudo";
            this.lbLaudo.Size = new System.Drawing.Size(40, 13);
            this.lbLaudo.TabIndex = 6;
            this.lbLaudo.Text = "Laudo:";
            // 
            // pbLogo
            // 
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Image = ((System.Drawing.Image)(resources.GetObject("pbLogo.Image")));
            this.pbLogo.Location = new System.Drawing.Point(443, 30);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(215, 82);
            this.pbLogo.TabIndex = 8;
            this.pbLogo.TabStop = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(555, 186);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(85, 28);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lbDate
            // 
            this.lbDate.AutoSize = true;
            this.lbDate.Location = new System.Drawing.Point(242, 118);
            this.lbDate.Name = "lbDate";
            this.lbDate.Size = new System.Drawing.Size(33, 13);
            this.lbDate.TabIndex = 10;
            this.lbDate.Text = "Data:";
            // 
            // tbDate
            // 
            this.tbDate.Enabled = false;
            this.tbDate.Location = new System.Drawing.Point(300, 115);
            this.tbDate.Name = "tbDate";
            this.tbDate.Size = new System.Drawing.Size(100, 20);
            this.tbDate.TabIndex = 50;
            this.tbDate.TabStop = false;
            // 
            // ckbAtiva
            // 
            this.ckbAtiva.AutoSize = true;
            this.ckbAtiva.Location = new System.Drawing.Point(107, 142);
            this.ckbAtiva.Name = "ckbAtiva";
            this.ckbAtiva.Size = new System.Drawing.Size(88, 17);
            this.ckbAtiva.TabIndex = 4;
            this.ckbAtiva.Text = "Energia ativa";
            this.ckbAtiva.UseVisualStyleBackColor = true;
            // 
            // ckbReativa
            // 
            this.ckbReativa.AutoSize = true;
            this.ckbReativa.Location = new System.Drawing.Point(107, 166);
            this.ckbReativa.Name = "ckbReativa";
            this.ckbReativa.Size = new System.Drawing.Size(97, 17);
            this.ckbReativa.TabIndex = 5;
            this.ckbReativa.Text = "Energia reativa";
            this.ckbReativa.UseVisualStyleBackColor = true;
            // 
            // frReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 244);
            this.Controls.Add(this.ckbAtiva);
            this.Controls.Add(this.ckbReativa);
            this.Controls.Add(this.tbDate);
            this.Controls.Add(this.lbDate);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.pbLogo);
            this.Controls.Add(this.cbLaudo);
            this.Controls.Add(this.lbLaudo);
            this.Controls.Add(this.cbDevices);
            this.Controls.Add(this.lbDevice);
            this.Controls.Add(this.cbStandards);
            this.Controls.Add(this.lbStandard);
            this.Controls.Add(this.cbCustomers);
            this.Controls.Add(this.lbCustomer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "frReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gerar relatório de calibração";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frReport_FormClosing);
            this.Load += new System.EventHandler(this.frReport_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frReport_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbCustomer;
        private System.Windows.Forms.ComboBox cbCustomers;
        private System.Windows.Forms.ComboBox cbStandards;
        private System.Windows.Forms.Label lbStandard;
        private System.Windows.Forms.ComboBox cbDevices;
        private System.Windows.Forms.Label lbDevice;
        private System.Windows.Forms.ComboBox cbLaudo;
        private System.Windows.Forms.Label lbLaudo;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lbDate;
        private System.Windows.Forms.TextBox tbDate;
        internal System.Windows.Forms.CheckBox ckbAtiva;
        internal System.Windows.Forms.CheckBox ckbReativa;
    }
}