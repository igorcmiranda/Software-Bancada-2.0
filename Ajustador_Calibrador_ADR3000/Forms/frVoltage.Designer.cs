namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frVoltage
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
            this.lbMessage = new System.Windows.Forms.Label();
            this.lbVoltage = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMessage.Location = new System.Drawing.Point(55, 67);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(330, 25);
            this.lbMessage.TabIndex = 0;
            this.lbMessage.Text = "Ajuste a tensão para 180.0 Volts:";
            // 
            // lbVoltage
            // 
            this.lbVoltage.AutoSize = true;
            this.lbVoltage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbVoltage.ForeColor = System.Drawing.Color.Blue;
            this.lbVoltage.Location = new System.Drawing.Point(391, 67);
            this.lbVoltage.Name = "lbVoltage";
            this.lbVoltage.Size = new System.Drawing.Size(153, 25);
            this.lbVoltage.TabIndex = 1;
            this.lbVoltage.Text = "-180.000000 V";
            // 
            // btOK
            // 
            this.btOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOK.Location = new System.Drawing.Point(256, 155);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(111, 40);
            this.btOK.TabIndex = 0;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.BtOK_Click);
            // 
            // frVoltage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 265);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.lbVoltage);
            this.Controls.Add(this.lbMessage);
            this.Name = "frVoltage";
            this.Text = "Medição de tensão";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrVoltage_FormClosing);
            this.Load += new System.EventHandler(this.FrVoltage_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FrVoltage_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.Label lbVoltage;
        private System.Windows.Forms.Button btOK;
    }
}