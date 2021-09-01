namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frAdjust
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frAdjust));
            this.pnAdjust = new System.Windows.Forms.Panel();
            this.cbDevice = new System.Windows.Forms.ComboBox();
            this.lbDevice = new System.Windows.Forms.Label();
            this.lbTimer = new System.Windows.Forms.Label();
            this.lbTimerMsg = new System.Windows.Forms.Label();
            this.gbErrors = new System.Windows.Forms.GroupBox();
            this.lbError = new System.Windows.Forms.Label();
            this.lbStopAdjustment = new System.Windows.Forms.Label();
            this.lbInitAdjustment = new System.Windows.Forms.Label();
            this.btStopAdjustment = new System.Windows.Forms.Button();
            this.btInitAdjustment = new System.Windows.Forms.Button();
            this.gbProgress = new System.Windows.Forms.GroupBox();
            this.pbAdjust = new System.Windows.Forms.ProgressBar();
            this.pnAdjust.SuspendLayout();
            this.gbErrors.SuspendLayout();
            this.gbProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnAdjust
            // 
            this.pnAdjust.Controls.Add(this.cbDevice);
            this.pnAdjust.Controls.Add(this.lbDevice);
            this.pnAdjust.Controls.Add(this.lbTimer);
            this.pnAdjust.Controls.Add(this.lbTimerMsg);
            this.pnAdjust.Controls.Add(this.gbErrors);
            this.pnAdjust.Controls.Add(this.lbStopAdjustment);
            this.pnAdjust.Controls.Add(this.lbInitAdjustment);
            this.pnAdjust.Controls.Add(this.btStopAdjustment);
            this.pnAdjust.Controls.Add(this.btInitAdjustment);
            this.pnAdjust.Controls.Add(this.gbProgress);
            this.pnAdjust.Location = new System.Drawing.Point(12, 12);
            this.pnAdjust.Name = "pnAdjust";
            this.pnAdjust.Size = new System.Drawing.Size(816, 355);
            this.pnAdjust.TabIndex = 0;
            // 
            // cbDevice
            // 
            this.cbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDevice.FormattingEnabled = true;
            this.cbDevice.Location = new System.Drawing.Point(235, 57);
            this.cbDevice.Name = "cbDevice";
            this.cbDevice.Size = new System.Drawing.Size(326, 28);
            this.cbDevice.TabIndex = 0;
            // 
            // lbDevice
            // 
            this.lbDevice.AutoSize = true;
            this.lbDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDevice.Location = new System.Drawing.Point(140, 60);
            this.lbDevice.Name = "lbDevice";
            this.lbDevice.Size = new System.Drawing.Size(89, 20);
            this.lbDevice.TabIndex = 266;
            this.lbDevice.Text = "Dispositivo:";
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTimer.Location = new System.Drawing.Point(80, 11);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(79, 20);
            this.lbTimer.TabIndex = 265;
            this.lbTimer.Text = "00:00:00";
            // 
            // lbTimerMsg
            // 
            this.lbTimerMsg.AutoSize = true;
            this.lbTimerMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTimerMsg.Location = new System.Drawing.Point(12, 11);
            this.lbTimerMsg.Name = "lbTimerMsg";
            this.lbTimerMsg.Size = new System.Drawing.Size(68, 20);
            this.lbTimerMsg.TabIndex = 264;
            this.lbTimerMsg.Text = "Tempo:";
            // 
            // gbErrors
            // 
            this.gbErrors.Controls.Add(this.lbError);
            this.gbErrors.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbErrors.Location = new System.Drawing.Point(610, 247);
            this.gbErrors.Name = "gbErrors";
            this.gbErrors.Size = new System.Drawing.Size(146, 83);
            this.gbErrors.TabIndex = 263;
            this.gbErrors.TabStop = false;
            this.gbErrors.Text = "Erros ADR [%]";
            // 
            // lbError
            // 
            this.lbError.AutoSize = true;
            this.lbError.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbError.ForeColor = System.Drawing.Color.Red;
            this.lbError.Location = new System.Drawing.Point(38, 34);
            this.lbError.Name = "lbError";
            this.lbError.Size = new System.Drawing.Size(76, 29);
            this.lbError.TabIndex = 0;
            this.lbError.Text = "0.000";
            // 
            // lbStopAdjustment
            // 
            this.lbStopAdjustment.AutoSize = true;
            this.lbStopAdjustment.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStopAdjustment.Location = new System.Drawing.Point(415, 113);
            this.lbStopAdjustment.Name = "lbStopAdjustment";
            this.lbStopAdjustment.Size = new System.Drawing.Size(94, 20);
            this.lbStopAdjustment.TabIndex = 262;
            this.lbStopAdjustment.Text = "Parar ajuste";
            // 
            // lbInitAdjustment
            // 
            this.lbInitAdjustment.AutoSize = true;
            this.lbInitAdjustment.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInitAdjustment.Location = new System.Drawing.Point(210, 113);
            this.lbInitAdjustment.Name = "lbInitAdjustment";
            this.lbInitAdjustment.Size = new System.Drawing.Size(98, 20);
            this.lbInitAdjustment.TabIndex = 261;
            this.lbInitAdjustment.Text = "Iniciar ajuste";
            // 
            // btStopAdjustment
            // 
            this.btStopAdjustment.BackColor = System.Drawing.Color.White;
            this.btStopAdjustment.Enabled = false;
            this.btStopAdjustment.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btStopAdjustment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btStopAdjustment.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btStopAdjustment.Image = ((System.Drawing.Image)(resources.GetObject("btStopAdjustment.Image")));
            this.btStopAdjustment.Location = new System.Drawing.Point(419, 145);
            this.btStopAdjustment.Name = "btStopAdjustment";
            this.btStopAdjustment.Size = new System.Drawing.Size(142, 52);
            this.btStopAdjustment.TabIndex = 2;
            this.btStopAdjustment.UseVisualStyleBackColor = false;
            this.btStopAdjustment.Click += new System.EventHandler(this.BtStopAdjustment_Click);
            // 
            // btInitAdjustment
            // 
            this.btInitAdjustment.BackColor = System.Drawing.Color.White;
            this.btInitAdjustment.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btInitAdjustment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btInitAdjustment.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btInitAdjustment.Image = ((System.Drawing.Image)(resources.GetObject("btInitAdjustment.Image")));
            this.btInitAdjustment.Location = new System.Drawing.Point(214, 145);
            this.btInitAdjustment.Name = "btInitAdjustment";
            this.btInitAdjustment.Size = new System.Drawing.Size(142, 52);
            this.btInitAdjustment.TabIndex = 1;
            this.btInitAdjustment.UseVisualStyleBackColor = false;
            this.btInitAdjustment.Click += new System.EventHandler(this.BtInitAdjustment_Click);
            // 
            // gbProgress
            // 
            this.gbProgress.Controls.Add(this.pbAdjust);
            this.gbProgress.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbProgress.Location = new System.Drawing.Point(68, 247);
            this.gbProgress.Name = "gbProgress";
            this.gbProgress.Size = new System.Drawing.Size(493, 83);
            this.gbProgress.TabIndex = 244;
            this.gbProgress.TabStop = false;
            this.gbProgress.Text = "0 %";
            // 
            // pbAdjust
            // 
            this.pbAdjust.Location = new System.Drawing.Point(6, 25);
            this.pbAdjust.Name = "pbAdjust";
            this.pbAdjust.Size = new System.Drawing.Size(481, 43);
            this.pbAdjust.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbAdjust.TabIndex = 0;
            // 
            // frAdjust
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 379);
            this.Controls.Add(this.pnAdjust);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frAdjust";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ajuste ADR 2000/ADR 3000";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrAdjust_FormClosing);
            this.Load += new System.EventHandler(this.FrAdjust_Load);
            this.SizeChanged += new System.EventHandler(this.FrAdjust_SizeChanged);
            this.pnAdjust.ResumeLayout(false);
            this.pnAdjust.PerformLayout();
            this.gbErrors.ResumeLayout(false);
            this.gbErrors.PerformLayout();
            this.gbProgress.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnAdjust;
        private System.Windows.Forms.GroupBox gbErrors;
        private System.Windows.Forms.Label lbError;
        private System.Windows.Forms.Label lbStopAdjustment;
        private System.Windows.Forms.Label lbInitAdjustment;
        private System.Windows.Forms.Button btStopAdjustment;
        private System.Windows.Forms.Button btInitAdjustment;
        internal System.Windows.Forms.GroupBox gbProgress;
        internal System.Windows.Forms.ProgressBar pbAdjust;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.Label lbTimerMsg;
        private System.Windows.Forms.Label lbDevice;
        private System.Windows.Forms.ComboBox cbDevice;
    }
}