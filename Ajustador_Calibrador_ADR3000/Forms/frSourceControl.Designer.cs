namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frSourceControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frSourceControl));
            this.pnSourceControl = new System.Windows.Forms.Panel();
            this.btOFF = new System.Windows.Forms.Button();
            this.btON = new System.Windows.Forms.Button();
            this.lbFrequency = new System.Windows.Forms.Label();
            this.btFrequency = new System.Windows.Forms.Button();
            this.lbStatusState = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.lbPhaseShift = new System.Windows.Forms.Label();
            this.lbMagnitude = new System.Windows.Forms.Label();
            this.btPhaseShiftC = new System.Windows.Forms.Button();
            this.btPhaseShiftB = new System.Windows.Forms.Button();
            this.btPhaseShiftA = new System.Windows.Forms.Button();
            this.btPhaseShiftAll = new System.Windows.Forms.Button();
            this.btMagnitudeC = new System.Windows.Forms.Button();
            this.btMagnitudeB = new System.Windows.Forms.Button();
            this.btMagnitudeA = new System.Windows.Forms.Button();
            this.btMagnitudeAll = new System.Windows.Forms.Button();
            this.lbTbSetPoint = new System.Windows.Forms.Label();
            this.tbSetPoint = new System.Windows.Forms.TextBox();
            this.ttHint = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.conectarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fonteVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fonteIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.desconectarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnSourceControl.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnSourceControl
            // 
            this.pnSourceControl.Controls.Add(this.btOFF);
            this.pnSourceControl.Controls.Add(this.btON);
            this.pnSourceControl.Controls.Add(this.lbFrequency);
            this.pnSourceControl.Controls.Add(this.btFrequency);
            this.pnSourceControl.Controls.Add(this.lbStatusState);
            this.pnSourceControl.Controls.Add(this.lbStatus);
            this.pnSourceControl.Controls.Add(this.lbPhaseShift);
            this.pnSourceControl.Controls.Add(this.lbMagnitude);
            this.pnSourceControl.Controls.Add(this.btPhaseShiftC);
            this.pnSourceControl.Controls.Add(this.btPhaseShiftB);
            this.pnSourceControl.Controls.Add(this.btPhaseShiftA);
            this.pnSourceControl.Controls.Add(this.btPhaseShiftAll);
            this.pnSourceControl.Controls.Add(this.btMagnitudeC);
            this.pnSourceControl.Controls.Add(this.btMagnitudeB);
            this.pnSourceControl.Controls.Add(this.btMagnitudeA);
            this.pnSourceControl.Controls.Add(this.btMagnitudeAll);
            this.pnSourceControl.Controls.Add(this.lbTbSetPoint);
            this.pnSourceControl.Controls.Add(this.tbSetPoint);
            this.pnSourceControl.Location = new System.Drawing.Point(12, 31);
            this.pnSourceControl.Name = "pnSourceControl";
            this.pnSourceControl.Size = new System.Drawing.Size(608, 305);
            this.pnSourceControl.TabIndex = 0;
            // 
            // btOFF
            // 
            this.btOFF.Enabled = false;
            this.btOFF.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOFF.Location = new System.Drawing.Point(383, 243);
            this.btOFF.Name = "btOFF";
            this.btOFF.Size = new System.Drawing.Size(95, 39);
            this.btOFF.TabIndex = 17;
            this.btOFF.Text = "OFF";
            this.btOFF.UseVisualStyleBackColor = true;
            this.btOFF.Click += new System.EventHandler(this.BtOFF_Click);
            // 
            // btON
            // 
            this.btON.Enabled = false;
            this.btON.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btON.Location = new System.Drawing.Point(247, 243);
            this.btON.Name = "btON";
            this.btON.Size = new System.Drawing.Size(95, 39);
            this.btON.TabIndex = 16;
            this.btON.Text = "ON";
            this.btON.UseVisualStyleBackColor = true;
            this.btON.Click += new System.EventHandler(this.BtON_Click);
            // 
            // lbFrequency
            // 
            this.lbFrequency.AutoSize = true;
            this.lbFrequency.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbFrequency.Location = new System.Drawing.Point(129, 183);
            this.lbFrequency.Name = "lbFrequency";
            this.lbFrequency.Size = new System.Drawing.Size(112, 24);
            this.lbFrequency.TabIndex = 15;
            this.lbFrequency.Text = "Frequência:";
            // 
            // btFrequency
            // 
            this.btFrequency.Enabled = false;
            this.btFrequency.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btFrequency.Location = new System.Drawing.Point(247, 182);
            this.btFrequency.Name = "btFrequency";
            this.btFrequency.Size = new System.Drawing.Size(67, 29);
            this.btFrequency.TabIndex = 14;
            this.btFrequency.Text = "f";
            this.btFrequency.UseVisualStyleBackColor = true;
            this.btFrequency.Click += new System.EventHandler(this.BtFrequency_Click);
            // 
            // lbStatusState
            // 
            this.lbStatusState.AutoSize = true;
            this.lbStatusState.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStatusState.ForeColor = System.Drawing.Color.Red;
            this.lbStatusState.Location = new System.Drawing.Point(86, 14);
            this.lbStatusState.Name = "lbStatusState";
            this.lbStatusState.Size = new System.Drawing.Size(132, 24);
            this.lbStatusState.TabIndex = 13;
            this.lbStatusState.Text = "Desconectado";
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStatus.Location = new System.Drawing.Point(15, 14);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(65, 24);
            this.lbStatus.TabIndex = 12;
            this.lbStatus.Text = "Status:";
            // 
            // lbPhaseShift
            // 
            this.lbPhaseShift.AutoSize = true;
            this.lbPhaseShift.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPhaseShift.Location = new System.Drawing.Point(39, 148);
            this.lbPhaseShift.Name = "lbPhaseShift";
            this.lbPhaseShift.Size = new System.Drawing.Size(202, 24);
            this.lbPhaseShift.TabIndex = 11;
            this.lbPhaseShift.Text = "Deslocamento de fase:";
            // 
            // lbMagnitude
            // 
            this.lbMagnitude.AutoSize = true;
            this.lbMagnitude.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMagnitude.Location = new System.Drawing.Point(141, 113);
            this.lbMagnitude.Name = "lbMagnitude";
            this.lbMagnitude.Size = new System.Drawing.Size(100, 24);
            this.lbMagnitude.TabIndex = 10;
            this.lbMagnitude.Text = "Amplitude:";
            // 
            // btPhaseShiftC
            // 
            this.btPhaseShiftC.Enabled = false;
            this.btPhaseShiftC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btPhaseShiftC.Location = new System.Drawing.Point(466, 147);
            this.btPhaseShiftC.Name = "btPhaseShiftC";
            this.btPhaseShiftC.Size = new System.Drawing.Size(67, 29);
            this.btPhaseShiftC.TabIndex = 9;
            this.btPhaseShiftC.Text = "C";
            this.btPhaseShiftC.UseVisualStyleBackColor = true;
            this.btPhaseShiftC.MouseHover += new System.EventHandler(this.BtPhaseShiftC_MouseHover);
            // 
            // btPhaseShiftB
            // 
            this.btPhaseShiftB.Enabled = false;
            this.btPhaseShiftB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btPhaseShiftB.Location = new System.Drawing.Point(393, 147);
            this.btPhaseShiftB.Name = "btPhaseShiftB";
            this.btPhaseShiftB.Size = new System.Drawing.Size(67, 29);
            this.btPhaseShiftB.TabIndex = 8;
            this.btPhaseShiftB.Text = "B";
            this.btPhaseShiftB.UseVisualStyleBackColor = true;
            this.btPhaseShiftB.MouseLeave += new System.EventHandler(this.BtPhaseShiftB_MouseLeave);
            this.btPhaseShiftB.MouseHover += new System.EventHandler(this.BtPhaseShiftB_MouseHover);
            // 
            // btPhaseShiftA
            // 
            this.btPhaseShiftA.Enabled = false;
            this.btPhaseShiftA.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btPhaseShiftA.Location = new System.Drawing.Point(320, 147);
            this.btPhaseShiftA.Name = "btPhaseShiftA";
            this.btPhaseShiftA.Size = new System.Drawing.Size(67, 29);
            this.btPhaseShiftA.TabIndex = 7;
            this.btPhaseShiftA.Text = "A";
            this.btPhaseShiftA.UseVisualStyleBackColor = true;
            this.btPhaseShiftA.MouseLeave += new System.EventHandler(this.BtPhaseShiftA_MouseLeave);
            this.btPhaseShiftA.MouseHover += new System.EventHandler(this.BtPhaseShiftA_MouseHover);
            // 
            // btPhaseShiftAll
            // 
            this.btPhaseShiftAll.Enabled = false;
            this.btPhaseShiftAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btPhaseShiftAll.Location = new System.Drawing.Point(247, 147);
            this.btPhaseShiftAll.Name = "btPhaseShiftAll";
            this.btPhaseShiftAll.Size = new System.Drawing.Size(67, 29);
            this.btPhaseShiftAll.TabIndex = 6;
            this.btPhaseShiftAll.Text = "3φ";
            this.btPhaseShiftAll.UseVisualStyleBackColor = true;
            this.btPhaseShiftAll.MouseLeave += new System.EventHandler(this.BtPhaseShiftAll_MouseLeave);
            this.btPhaseShiftAll.MouseHover += new System.EventHandler(this.BtPhaseShiftAll_MouseHover);
            // 
            // btMagnitudeC
            // 
            this.btMagnitudeC.Enabled = false;
            this.btMagnitudeC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btMagnitudeC.Location = new System.Drawing.Point(466, 112);
            this.btMagnitudeC.Name = "btMagnitudeC";
            this.btMagnitudeC.Size = new System.Drawing.Size(67, 29);
            this.btMagnitudeC.TabIndex = 5;
            this.btMagnitudeC.Text = "C";
            this.btMagnitudeC.UseVisualStyleBackColor = true;
            this.btMagnitudeC.MouseLeave += new System.EventHandler(this.BtMagnitudeC_MouseLeave);
            this.btMagnitudeC.MouseHover += new System.EventHandler(this.BtMagnitudeC_MouseHover);
            // 
            // btMagnitudeB
            // 
            this.btMagnitudeB.Enabled = false;
            this.btMagnitudeB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btMagnitudeB.Location = new System.Drawing.Point(393, 112);
            this.btMagnitudeB.Name = "btMagnitudeB";
            this.btMagnitudeB.Size = new System.Drawing.Size(67, 29);
            this.btMagnitudeB.TabIndex = 4;
            this.btMagnitudeB.Text = "B";
            this.btMagnitudeB.UseVisualStyleBackColor = true;
            this.btMagnitudeB.MouseLeave += new System.EventHandler(this.BtMagnitudeB_MouseLeave);
            this.btMagnitudeB.MouseHover += new System.EventHandler(this.BtMagnitudeB_MouseHover);
            // 
            // btMagnitudeA
            // 
            this.btMagnitudeA.Enabled = false;
            this.btMagnitudeA.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btMagnitudeA.Location = new System.Drawing.Point(320, 112);
            this.btMagnitudeA.Name = "btMagnitudeA";
            this.btMagnitudeA.Size = new System.Drawing.Size(67, 29);
            this.btMagnitudeA.TabIndex = 3;
            this.btMagnitudeA.Text = "A";
            this.btMagnitudeA.UseVisualStyleBackColor = true;
            this.btMagnitudeA.MouseLeave += new System.EventHandler(this.BtMagnitudeA_MouseLeave);
            this.btMagnitudeA.MouseHover += new System.EventHandler(this.BtMagnitudeA_MouseHover);
            // 
            // btMagnitudeAll
            // 
            this.btMagnitudeAll.Enabled = false;
            this.btMagnitudeAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btMagnitudeAll.Location = new System.Drawing.Point(247, 112);
            this.btMagnitudeAll.Name = "btMagnitudeAll";
            this.btMagnitudeAll.Size = new System.Drawing.Size(67, 29);
            this.btMagnitudeAll.TabIndex = 2;
            this.btMagnitudeAll.Text = "3φ";
            this.btMagnitudeAll.UseVisualStyleBackColor = true;
            this.btMagnitudeAll.Click += new System.EventHandler(this.BtMagnitudeAll_Click);
            this.btMagnitudeAll.MouseLeave += new System.EventHandler(this.BtMagnitudeAll_MouseLeave);
            this.btMagnitudeAll.MouseHover += new System.EventHandler(this.BtMagnitudeAll_MouseHover);
            // 
            // lbTbSetPoint
            // 
            this.lbTbSetPoint.AutoSize = true;
            this.lbTbSetPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTbSetPoint.Location = new System.Drawing.Point(152, 80);
            this.lbTbSetPoint.Name = "lbTbSetPoint";
            this.lbTbSetPoint.Size = new System.Drawing.Size(89, 24);
            this.lbTbSetPoint.TabIndex = 1;
            this.lbTbSetPoint.Text = "Set Point:";
            // 
            // tbSetPoint
            // 
            this.tbSetPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbSetPoint.Location = new System.Drawing.Point(247, 77);
            this.tbSetPoint.Name = "tbSetPoint";
            this.tbSetPoint.Size = new System.Drawing.Size(286, 29);
            this.tbSetPoint.TabIndex = 0;
            this.tbSetPoint.MouseLeave += new System.EventHandler(this.TbSetPoint_MouseLeave);
            this.tbSetPoint.MouseHover += new System.EventHandler(this.TbSetPoint_MouseHover);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(632, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.conectarToolStripMenuItem,
            this.desconectarToolStripMenuItem});
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.menuToolStripMenuItem.Text = "Menu";
            // 
            // conectarToolStripMenuItem
            // 
            this.conectarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fonteVToolStripMenuItem,
            this.fonteIToolStripMenuItem});
            this.conectarToolStripMenuItem.Name = "conectarToolStripMenuItem";
            this.conectarToolStripMenuItem.Size = new System.Drawing.Size(160, 24);
            this.conectarToolStripMenuItem.Text = "Conectar";
            // 
            // fonteVToolStripMenuItem
            // 
            this.fonteVToolStripMenuItem.Name = "fonteVToolStripMenuItem";
            this.fonteVToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.fonteVToolStripMenuItem.Text = "Fonte V";
            this.fonteVToolStripMenuItem.Click += new System.EventHandler(this.FonteVToolStripMenuItem_Click);
            // 
            // fonteIToolStripMenuItem
            // 
            this.fonteIToolStripMenuItem.Enabled = false;
            this.fonteIToolStripMenuItem.Name = "fonteIToolStripMenuItem";
            this.fonteIToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.fonteIToolStripMenuItem.Text = "Fonte I";
            // 
            // desconectarToolStripMenuItem
            // 
            this.desconectarToolStripMenuItem.Name = "desconectarToolStripMenuItem";
            this.desconectarToolStripMenuItem.Size = new System.Drawing.Size(160, 24);
            this.desconectarToolStripMenuItem.Text = "Desconectar";
            this.desconectarToolStripMenuItem.Click += new System.EventHandler(this.DesconectarToolStripMenuItem_Click);
            // 
            // frSourceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 348);
            this.Controls.Add(this.pnSourceControl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frSourceControl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configuração da fonte";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrSourceControl_FormClosing);
            this.pnSourceControl.ResumeLayout(false);
            this.pnSourceControl.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnSourceControl;
        private System.Windows.Forms.Label lbTbSetPoint;
        private System.Windows.Forms.TextBox tbSetPoint;
        private System.Windows.Forms.Label lbPhaseShift;
        private System.Windows.Forms.Label lbMagnitude;
        private System.Windows.Forms.Button btPhaseShiftC;
        private System.Windows.Forms.Button btPhaseShiftB;
        private System.Windows.Forms.Button btPhaseShiftA;
        private System.Windows.Forms.Button btPhaseShiftAll;
        private System.Windows.Forms.Button btMagnitudeC;
        private System.Windows.Forms.Button btMagnitudeB;
        private System.Windows.Forms.Button btMagnitudeA;
        private System.Windows.Forms.Button btMagnitudeAll;
        private System.Windows.Forms.ToolTip ttHint;
        private System.Windows.Forms.Label lbStatusState;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem conectarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fonteVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fonteIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem desconectarToolStripMenuItem;
        private System.Windows.Forms.Label lbFrequency;
        private System.Windows.Forms.Button btFrequency;
        private System.Windows.Forms.Button btOFF;
        private System.Windows.Forms.Button btON;
    }
}