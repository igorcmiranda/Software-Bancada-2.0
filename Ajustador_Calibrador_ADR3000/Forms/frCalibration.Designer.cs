namespace Ajustador_Calibrador_ADR3000.Forms
{
    partial class frCalibration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frCalibration));
            this.pnCalibration = new System.Windows.Forms.Panel();
            this.ckbConsiderTolerance = new System.Windows.Forms.CheckBox();
            this.lbReport = new System.Windows.Forms.Label();
            this.tbReport = new System.Windows.Forms.TextBox();
            this.lbOSOPNum = new System.Windows.Forms.Label();
            this.tbOSOPNum = new System.Windows.Forms.TextBox();
            this.cbOSOP = new System.Windows.Forms.ComboBox();
            this.lbOSOP = new System.Windows.Forms.Label();
            this.cbMode = new System.Windows.Forms.ComboBox();
            this.lbMode = new System.Windows.Forms.Label();
            this.listBoxPoints = new System.Windows.Forms.ListBox();
            this.cbEssay = new System.Windows.Forms.ComboBox();
            this.lbPoints = new System.Windows.Forms.Label();
            this.lbEssay = new System.Windows.Forms.Label();
            this.gbProgress = new System.Windows.Forms.GroupBox();
            this.pbCalibration = new System.Windows.Forms.ProgressBar();
            this.btStopCalibration = new System.Windows.Forms.Button();
            this.btInitCalibration = new System.Windows.Forms.Button();
            this.lbInitCalibration = new System.Windows.Forms.Label();
            this.lbStopCalibration = new System.Windows.Forms.Label();
            this.cbDevice = new System.Windows.Forms.ComboBox();
            this.lbDevice = new System.Windows.Forms.Label();
            this.lbTimer = new System.Windows.Forms.Label();
            this.lbTimerMsg = new System.Windows.Forms.Label();
            this.gbErrors = new System.Windows.Forms.GroupBox();
            this.lbError = new System.Windows.Forms.Label();
            this.pnCalibration.SuspendLayout();
            this.gbProgress.SuspendLayout();
            this.gbErrors.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnCalibration
            // 
            this.pnCalibration.Controls.Add(this.ckbConsiderTolerance);
            this.pnCalibration.Controls.Add(this.lbReport);
            this.pnCalibration.Controls.Add(this.tbReport);
            this.pnCalibration.Controls.Add(this.lbOSOPNum);
            this.pnCalibration.Controls.Add(this.tbOSOPNum);
            this.pnCalibration.Controls.Add(this.cbOSOP);
            this.pnCalibration.Controls.Add(this.lbOSOP);
            this.pnCalibration.Controls.Add(this.cbMode);
            this.pnCalibration.Controls.Add(this.lbMode);
            this.pnCalibration.Controls.Add(this.listBoxPoints);
            this.pnCalibration.Controls.Add(this.cbEssay);
            this.pnCalibration.Controls.Add(this.lbPoints);
            this.pnCalibration.Controls.Add(this.lbEssay);
            this.pnCalibration.Controls.Add(this.gbProgress);
            this.pnCalibration.Controls.Add(this.btStopCalibration);
            this.pnCalibration.Controls.Add(this.btInitCalibration);
            this.pnCalibration.Controls.Add(this.lbInitCalibration);
            this.pnCalibration.Controls.Add(this.lbStopCalibration);
            this.pnCalibration.Controls.Add(this.cbDevice);
            this.pnCalibration.Controls.Add(this.lbDevice);
            this.pnCalibration.Controls.Add(this.lbTimer);
            this.pnCalibration.Controls.Add(this.lbTimerMsg);
            this.pnCalibration.Controls.Add(this.gbErrors);
            this.pnCalibration.Location = new System.Drawing.Point(12, 12);
            this.pnCalibration.Name = "pnCalibration";
            this.pnCalibration.Size = new System.Drawing.Size(874, 654);
            this.pnCalibration.TabIndex = 0;
            // 
            // ckbConsiderTolerance
            // 
            this.ckbConsiderTolerance.AutoSize = true;
            this.ckbConsiderTolerance.Checked = true;
            this.ckbConsiderTolerance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbConsiderTolerance.Enabled = false;
            this.ckbConsiderTolerance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ckbConsiderTolerance.Location = new System.Drawing.Point(611, 68);
            this.ckbConsiderTolerance.Name = "ckbConsiderTolerance";
            this.ckbConsiderTolerance.Size = new System.Drawing.Size(178, 24);
            this.ckbConsiderTolerance.TabIndex = 305;
            this.ckbConsiderTolerance.Text = "Considerar tolerância";
            this.ckbConsiderTolerance.UseVisualStyleBackColor = true;
            this.ckbConsiderTolerance.Visible = false;
            // 
            // lbReport
            // 
            this.lbReport.AutoSize = true;
            this.lbReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbReport.Location = new System.Drawing.Point(206, 327);
            this.lbReport.Name = "lbReport";
            this.lbReport.Size = new System.Drawing.Size(58, 20);
            this.lbReport.TabIndex = 304;
            this.lbReport.Text = "Laudo:";
            // 
            // tbReport
            // 
            this.tbReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbReport.Location = new System.Drawing.Point(270, 324);
            this.tbReport.Name = "tbReport";
            this.tbReport.Size = new System.Drawing.Size(326, 26);
            this.tbReport.TabIndex = 6;
            // 
            // lbOSOPNum
            // 
            this.lbOSOPNum.AutoSize = true;
            this.lbOSOPNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbOSOPNum.Location = new System.Drawing.Point(181, 295);
            this.lbOSOPNum.Name = "lbOSOPNum";
            this.lbOSOPNum.Size = new System.Drawing.Size(83, 20);
            this.lbOSOPNum.TabIndex = 302;
            this.lbOSOPNum.Text = "Nº OS/OP:";
            // 
            // tbOSOPNum
            // 
            this.tbOSOPNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbOSOPNum.Location = new System.Drawing.Point(270, 292);
            this.tbOSOPNum.Name = "tbOSOPNum";
            this.tbOSOPNum.Size = new System.Drawing.Size(326, 26);
            this.tbOSOPNum.TabIndex = 5;
            // 
            // cbOSOP
            // 
            this.cbOSOP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOSOP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbOSOP.FormattingEnabled = true;
            this.cbOSOP.Items.AddRange(new object[] {
            "OS",
            "OP"});
            this.cbOSOP.Location = new System.Drawing.Point(270, 258);
            this.cbOSOP.Name = "cbOSOP";
            this.cbOSOP.Size = new System.Drawing.Size(326, 28);
            this.cbOSOP.TabIndex = 4;
            // 
            // lbOSOP
            // 
            this.lbOSOP.AutoSize = true;
            this.lbOSOP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbOSOP.Location = new System.Drawing.Point(202, 261);
            this.lbOSOP.Name = "lbOSOP";
            this.lbOSOP.Size = new System.Drawing.Size(62, 20);
            this.lbOSOP.TabIndex = 300;
            this.lbOSOP.Text = "OS/OP:";
            // 
            // cbMode
            // 
            this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMode.FormattingEnabled = true;
            this.cbMode.Items.AddRange(new object[] {
            "Saída de pulsos",
            "Entrada de pulsos"});
            this.cbMode.Location = new System.Drawing.Point(270, 224);
            this.cbMode.Name = "cbMode";
            this.cbMode.Size = new System.Drawing.Size(326, 28);
            this.cbMode.TabIndex = 3;
            // 
            // lbMode
            // 
            this.lbMode.AutoSize = true;
            this.lbMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMode.Location = new System.Drawing.Point(211, 227);
            this.lbMode.Name = "lbMode";
            this.lbMode.Size = new System.Drawing.Size(53, 20);
            this.lbMode.TabIndex = 298;
            this.lbMode.Text = "Modo:";
            // 
            // listBoxPoints
            // 
            this.listBoxPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxPoints.FormattingEnabled = true;
            this.listBoxPoints.ItemHeight = 20;
            this.listBoxPoints.Location = new System.Drawing.Point(270, 134);
            this.listBoxPoints.Name = "listBoxPoints";
            this.listBoxPoints.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxPoints.Size = new System.Drawing.Size(326, 84);
            this.listBoxPoints.TabIndex = 2;
            // 
            // cbEssay
            // 
            this.cbEssay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEssay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEssay.FormattingEnabled = true;
            this.cbEssay.Location = new System.Drawing.Point(270, 100);
            this.cbEssay.Name = "cbEssay";
            this.cbEssay.Size = new System.Drawing.Size(326, 28);
            this.cbEssay.TabIndex = 1;
            this.cbEssay.SelectedIndexChanged += new System.EventHandler(this.CbEssay_SelectedIndexChanged);
            // 
            // lbPoints
            // 
            this.lbPoints.AutoSize = true;
            this.lbPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPoints.Location = new System.Drawing.Point(201, 134);
            this.lbPoints.Name = "lbPoints";
            this.lbPoints.Size = new System.Drawing.Size(63, 20);
            this.lbPoints.TabIndex = 296;
            this.lbPoints.Text = "Pontos:";
            // 
            // lbEssay
            // 
            this.lbEssay.AutoSize = true;
            this.lbEssay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbEssay.Location = new System.Drawing.Point(202, 103);
            this.lbEssay.Name = "lbEssay";
            this.lbEssay.Size = new System.Drawing.Size(62, 20);
            this.lbEssay.TabIndex = 294;
            this.lbEssay.Text = "Ensaio:";
            // 
            // gbProgress
            // 
            this.gbProgress.Controls.Add(this.pbCalibration);
            this.gbProgress.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbProgress.Location = new System.Drawing.Point(103, 516);
            this.gbProgress.Name = "gbProgress";
            this.gbProgress.Size = new System.Drawing.Size(493, 83);
            this.gbProgress.TabIndex = 275;
            this.gbProgress.TabStop = false;
            this.gbProgress.Text = "0 %";
            // 
            // pbCalibration
            // 
            this.pbCalibration.Location = new System.Drawing.Point(6, 25);
            this.pbCalibration.Name = "pbCalibration";
            this.pbCalibration.Size = new System.Drawing.Size(481, 43);
            this.pbCalibration.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbCalibration.TabIndex = 0;
            // 
            // btStopCalibration
            // 
            this.btStopCalibration.BackColor = System.Drawing.Color.White;
            this.btStopCalibration.Enabled = false;
            this.btStopCalibration.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btStopCalibration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btStopCalibration.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btStopCalibration.Image = ((System.Drawing.Image)(resources.GetObject("btStopCalibration.Image")));
            this.btStopCalibration.Location = new System.Drawing.Point(454, 406);
            this.btStopCalibration.Name = "btStopCalibration";
            this.btStopCalibration.Size = new System.Drawing.Size(142, 52);
            this.btStopCalibration.TabIndex = 8;
            this.btStopCalibration.UseVisualStyleBackColor = false;
            this.btStopCalibration.Click += new System.EventHandler(this.BtStopCalibration_Click);
            // 
            // btInitCalibration
            // 
            this.btInitCalibration.BackColor = System.Drawing.Color.White;
            this.btInitCalibration.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btInitCalibration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btInitCalibration.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btInitCalibration.Image = ((System.Drawing.Image)(resources.GetObject("btInitCalibration.Image")));
            this.btInitCalibration.Location = new System.Drawing.Point(235, 406);
            this.btInitCalibration.Name = "btInitCalibration";
            this.btInitCalibration.Size = new System.Drawing.Size(142, 52);
            this.btInitCalibration.TabIndex = 7;
            this.btInitCalibration.UseVisualStyleBackColor = false;
            this.btInitCalibration.Click += new System.EventHandler(this.BtInitCalibration_Click);
            // 
            // lbInitCalibration
            // 
            this.lbInitCalibration.AutoSize = true;
            this.lbInitCalibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInitCalibration.Location = new System.Drawing.Point(231, 381);
            this.lbInitCalibration.Name = "lbInitCalibration";
            this.lbInitCalibration.Size = new System.Drawing.Size(127, 20);
            this.lbInitCalibration.TabIndex = 273;
            this.lbInitCalibration.Text = "Iniciar calibração";
            // 
            // lbStopCalibration
            // 
            this.lbStopCalibration.AutoSize = true;
            this.lbStopCalibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStopCalibration.Location = new System.Drawing.Point(450, 383);
            this.lbStopCalibration.Name = "lbStopCalibration";
            this.lbStopCalibration.Size = new System.Drawing.Size(123, 20);
            this.lbStopCalibration.TabIndex = 274;
            this.lbStopCalibration.Text = "Parar calibração";
            // 
            // cbDevice
            // 
            this.cbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDevice.FormattingEnabled = true;
            this.cbDevice.Location = new System.Drawing.Point(270, 66);
            this.cbDevice.Name = "cbDevice";
            this.cbDevice.Size = new System.Drawing.Size(326, 28);
            this.cbDevice.TabIndex = 0;
            // 
            // lbDevice
            // 
            this.lbDevice.AutoSize = true;
            this.lbDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDevice.Location = new System.Drawing.Point(175, 69);
            this.lbDevice.Name = "lbDevice";
            this.lbDevice.Size = new System.Drawing.Size(89, 20);
            this.lbDevice.TabIndex = 270;
            this.lbDevice.Text = "Dispositivo:";
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTimer.Location = new System.Drawing.Point(115, 20);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(79, 20);
            this.lbTimer.TabIndex = 269;
            this.lbTimer.Text = "00:00:00";
            // 
            // lbTimerMsg
            // 
            this.lbTimerMsg.AutoSize = true;
            this.lbTimerMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTimerMsg.Location = new System.Drawing.Point(47, 20);
            this.lbTimerMsg.Name = "lbTimerMsg";
            this.lbTimerMsg.Size = new System.Drawing.Size(68, 20);
            this.lbTimerMsg.TabIndex = 268;
            this.lbTimerMsg.Text = "Tempo:";
            // 
            // gbErrors
            // 
            this.gbErrors.Controls.Add(this.lbError);
            this.gbErrors.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbErrors.Location = new System.Drawing.Point(675, 516);
            this.gbErrors.Name = "gbErrors";
            this.gbErrors.Size = new System.Drawing.Size(146, 83);
            this.gbErrors.TabIndex = 264;
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
            // frCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 678);
            this.Controls.Add(this.pnCalibration);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frCalibration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Calibração ADR 2000/ADR 3000";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrCalibration_FormClosing);
            this.Load += new System.EventHandler(this.FrCalibration_Load);
            this.SizeChanged += new System.EventHandler(this.FrCalibration_SizeChanged);
            this.pnCalibration.ResumeLayout(false);
            this.pnCalibration.PerformLayout();
            this.gbProgress.ResumeLayout(false);
            this.gbErrors.ResumeLayout(false);
            this.gbErrors.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnCalibration;
        private System.Windows.Forms.GroupBox gbErrors;
        private System.Windows.Forms.Label lbError;
        private System.Windows.Forms.ComboBox cbDevice;
        private System.Windows.Forms.Label lbDevice;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.Label lbTimerMsg;
        private System.Windows.Forms.Button btStopCalibration;
        private System.Windows.Forms.Button btInitCalibration;
        private System.Windows.Forms.Label lbInitCalibration;
        private System.Windows.Forms.Label lbStopCalibration;
        internal System.Windows.Forms.GroupBox gbProgress;
        internal System.Windows.Forms.ProgressBar pbCalibration;
        private System.Windows.Forms.Label lbOSOPNum;
        private System.Windows.Forms.TextBox tbOSOPNum;
        private System.Windows.Forms.ComboBox cbOSOP;
        private System.Windows.Forms.Label lbOSOP;
        private System.Windows.Forms.ComboBox cbMode;
        private System.Windows.Forms.Label lbMode;
        private System.Windows.Forms.ListBox listBoxPoints;
        private System.Windows.Forms.ComboBox cbEssay;
        private System.Windows.Forms.Label lbPoints;
        private System.Windows.Forms.Label lbEssay;
        private System.Windows.Forms.Label lbReport;
        private System.Windows.Forms.TextBox tbReport;
        private System.Windows.Forms.CheckBox ckbConsiderTolerance;
    }
}