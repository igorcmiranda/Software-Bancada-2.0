<<<<<<< HEAD
﻿namespace CalibradorMulti4000_Mesa.Forms
{
    partial class frConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frConfig));
            this.dgConfig = new System.Windows.Forms.DataGridView();
            this.lbGrid = new System.Windows.Forms.Label();
            this.lbMsg = new System.Windows.Forms.Label();
            this.btnRec = new System.Windows.Forms.Button();
            this.pnConfig = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgConfig)).BeginInit();
            this.pnConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgConfig
            // 
            this.dgConfig.AllowUserToAddRows = false;
            this.dgConfig.AllowUserToDeleteRows = false;
            this.dgConfig.AllowUserToResizeRows = false;
            this.dgConfig.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgConfig.Location = new System.Drawing.Point(3, 33);
            this.dgConfig.MultiSelect = false;
            this.dgConfig.Name = "dgConfig";
            this.dgConfig.Size = new System.Drawing.Size(920, 510);
            this.dgConfig.TabIndex = 0;
            // 
            // lbGrid
            // 
            this.lbGrid.AutoSize = true;
            this.lbGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbGrid.Location = new System.Drawing.Point(3, 12);
            this.lbGrid.Name = "lbGrid";
            this.lbGrid.Size = new System.Drawing.Size(133, 18);
            this.lbGrid.TabIndex = 1;
            this.lbGrid.Text = "Parâmetros atuais:";
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMsg.Location = new System.Drawing.Point(12, 19);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(647, 20);
            this.lbMsg.TabIndex = 2;
            this.lbMsg.Text = "Modifique o parâmetro na grid para atualizá-lo no banco de dados através do botão" +
    " gravar.";
            // 
            // btnRec
            // 
            this.btnRec.Location = new System.Drawing.Point(848, 562);
            this.btnRec.Name = "btnRec";
            this.btnRec.Size = new System.Drawing.Size(75, 23);
            this.btnRec.TabIndex = 3;
            this.btnRec.Text = "Gravar";
            this.btnRec.UseVisualStyleBackColor = true;
            this.btnRec.Click += new System.EventHandler(this.btnRec_Click);
            // 
            // pnConfig
            // 
            this.pnConfig.Controls.Add(this.lbGrid);
            this.pnConfig.Controls.Add(this.btnRec);
            this.pnConfig.Controls.Add(this.dgConfig);
            this.pnConfig.Location = new System.Drawing.Point(12, 42);
            this.pnConfig.Name = "pnConfig";
            this.pnConfig.Size = new System.Drawing.Size(932, 588);
            this.pnConfig.TabIndex = 4;
            // 
            // frConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1108, 629);
            this.Controls.Add(this.pnConfig);
            this.Controls.Add(this.lbMsg);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "frConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configurações";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frConfig_FormClosing);
            this.Load += new System.EventHandler(this.frConfig_Load);
            this.SizeChanged += new System.EventHandler(this.frConfig_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frConfig_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dgConfig)).EndInit();
            this.pnConfig.ResumeLayout(false);
            this.pnConfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgConfig;
        private System.Windows.Forms.Label lbGrid;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Button btnRec;
        private System.Windows.Forms.Panel pnConfig;
    }
=======
﻿namespace CalibradorMulti4000_Mesa.Forms
{
    partial class frConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frConfig));
            this.dgConfig = new System.Windows.Forms.DataGridView();
            this.lbGrid = new System.Windows.Forms.Label();
            this.lbMsg = new System.Windows.Forms.Label();
            this.btnRec = new System.Windows.Forms.Button();
            this.pnConfig = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgConfig)).BeginInit();
            this.pnConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgConfig
            // 
            this.dgConfig.AllowUserToAddRows = false;
            this.dgConfig.AllowUserToDeleteRows = false;
            this.dgConfig.AllowUserToResizeRows = false;
            this.dgConfig.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgConfig.Location = new System.Drawing.Point(3, 33);
            this.dgConfig.MultiSelect = false;
            this.dgConfig.Name = "dgConfig";
            this.dgConfig.Size = new System.Drawing.Size(920, 510);
            this.dgConfig.TabIndex = 0;
            // 
            // lbGrid
            // 
            this.lbGrid.AutoSize = true;
            this.lbGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbGrid.Location = new System.Drawing.Point(3, 12);
            this.lbGrid.Name = "lbGrid";
            this.lbGrid.Size = new System.Drawing.Size(133, 18);
            this.lbGrid.TabIndex = 1;
            this.lbGrid.Text = "Parâmetros atuais:";
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMsg.Location = new System.Drawing.Point(12, 19);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(647, 20);
            this.lbMsg.TabIndex = 2;
            this.lbMsg.Text = "Modifique o parâmetro na grid para atualizá-lo no banco de dados através do botão" +
    " gravar.";
            // 
            // btnRec
            // 
            this.btnRec.Location = new System.Drawing.Point(848, 562);
            this.btnRec.Name = "btnRec";
            this.btnRec.Size = new System.Drawing.Size(75, 23);
            this.btnRec.TabIndex = 3;
            this.btnRec.Text = "Gravar";
            this.btnRec.UseVisualStyleBackColor = true;
            this.btnRec.Click += new System.EventHandler(this.btnRec_Click);
            // 
            // pnConfig
            // 
            this.pnConfig.Controls.Add(this.lbGrid);
            this.pnConfig.Controls.Add(this.btnRec);
            this.pnConfig.Controls.Add(this.dgConfig);
            this.pnConfig.Location = new System.Drawing.Point(12, 42);
            this.pnConfig.Name = "pnConfig";
            this.pnConfig.Size = new System.Drawing.Size(932, 588);
            this.pnConfig.TabIndex = 4;
            // 
            // frConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1108, 629);
            this.Controls.Add(this.pnConfig);
            this.Controls.Add(this.lbMsg);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "frConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configurações";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frConfig_FormClosing);
            this.Load += new System.EventHandler(this.frConfig_Load);
            this.SizeChanged += new System.EventHandler(this.frConfig_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frConfig_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dgConfig)).EndInit();
            this.pnConfig.ResumeLayout(false);
            this.pnConfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgConfig;
        private System.Windows.Forms.Label lbGrid;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Button btnRec;
        private System.Windows.Forms.Panel pnConfig;
    }
>>>>>>> 1a7f22186b63985d8ecc7631fb58005891227614
}