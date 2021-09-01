
namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frMCP2200RelayTest
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
            this.cbVoltage = new System.Windows.Forms.ComboBox();
            this.lbVoltage = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.btExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbVoltage
            // 
            this.cbVoltage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVoltage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbVoltage.FormattingEnabled = true;
            this.cbVoltage.Items.AddRange(new object[] {
            "OFF",
            "100 V",
            "120 V",
            "180 V",
            "220 V",
            "240 V"});
            this.cbVoltage.Location = new System.Drawing.Point(163, 85);
            this.cbVoltage.Name = "cbVoltage";
            this.cbVoltage.Size = new System.Drawing.Size(205, 28);
            this.cbVoltage.TabIndex = 295;
            // 
            // lbVoltage
            // 
            this.lbVoltage.AutoSize = true;
            this.lbVoltage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbVoltage.Location = new System.Drawing.Point(95, 88);
            this.lbVoltage.Name = "lbVoltage";
            this.lbVoltage.Size = new System.Drawing.Size(66, 20);
            this.lbVoltage.TabIndex = 296;
            this.lbVoltage.Text = "Tensão:";
            // 
            // btOK
            // 
            this.btOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOK.Location = new System.Drawing.Point(384, 85);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(90, 28);
            this.btOK.TabIndex = 297;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btExit
            // 
            this.btExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btExit.Location = new System.Drawing.Point(225, 143);
            this.btExit.Name = "btExit";
            this.btExit.Size = new System.Drawing.Size(90, 28);
            this.btExit.TabIndex = 298;
            this.btExit.Text = "Sair";
            this.btExit.UseVisualStyleBackColor = true;
            this.btExit.Click += new System.EventHandler(this.btExit_Click);
            // 
            // frMCP2200RelayTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 196);
            this.Controls.Add(this.btExit);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.cbVoltage);
            this.Controls.Add(this.lbVoltage);
            this.Name = "frMCP2200RelayTest";
            this.Text = "frMCP2200RelayTest";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frMCP2200RelayTest_FormClosing);
            this.Load += new System.EventHandler(this.frMCP2200RelayTest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbVoltage;
        private System.Windows.Forms.Label lbVoltage;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btExit;
    }
}