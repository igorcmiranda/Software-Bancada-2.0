
namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frShowInfo
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
            this.lbInfo = new System.Windows.Forms.Label();
            this.rtbMeasures = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInfo.Location = new System.Drawing.Point(12, 42);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(81, 20);
            this.lbInfo.TabIndex = 0;
            this.lbInfo.Text = "Medições:";
            // 
            // rtbMeasures
            // 
            this.rtbMeasures.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMeasures.Location = new System.Drawing.Point(16, 65);
            this.rtbMeasures.Multiline = true;
            this.rtbMeasures.Name = "rtbMeasures";
            this.rtbMeasures.ReadOnly = true;
            this.rtbMeasures.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.rtbMeasures.Size = new System.Drawing.Size(476, 390);
            this.rtbMeasures.TabIndex = 2;
            // 
            // frShowInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 467);
            this.Controls.Add(this.rtbMeasures);
            this.Controls.Add(this.lbInfo);
            this.Name = "frShowInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Medições padrão";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frShowInfo_FormClosing);
            this.Load += new System.EventHandler(this.frShowInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.TextBox rtbMeasures;
    }
}