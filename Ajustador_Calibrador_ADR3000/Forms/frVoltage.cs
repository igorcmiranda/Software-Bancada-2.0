using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ajustador_Calibrador_ADR3000.Devices;
using Ajustador_Calibrador_ADR3000.Delegates;
using System.Threading;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frVoltage : Form
    {
        private readonly StandardMeter standardMeter;
        private readonly StandardMeter.StandardType standardType;
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private bool cancelSuccess = false;

        public float Voltage { get; set; } = 180.0f;

        public frVoltage(StandardMeter _standardMeter, StandardMeter.StandardType _standardType)
        {
            InitializeComponent();
            standardMeter = _standardMeter;
            standardType = _standardType;
        }

        private void UpdateVoltage(float voltage)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { lbVoltage.Text = voltage.ToString("0.000000"); }));
            else lbVoltage.Text = voltage.ToString("0.000000");
        }

        private void FrVoltage_Load(object sender, EventArgs e)
        {
            lbMessage.Text = "Ajuste a tensão para " + Voltage.ToString("0.0") + " Volts:";
            cancelSuccess = false;
            backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            float[] measures;

            while (!backgroundWorker.CancellationPending)
            {
                //
                // GetMeasures() blocks for 600 ms if standard is GF333B-M
                //
                measures = standardMeter.GetMeasures();
                UpdateVoltage(measures[0]);
                //
                // Waits necessary time to update voltage each 1 s
                //
                if (standardType == StandardMeter.StandardType.GF333B) Thread.Sleep(400);
                else Thread.Sleep(1000);
            }
            cancelSuccess = true;
        }

        private void FrVoltage_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundWorker.CancelAsync();
            while (!cancelSuccess) { }

            if (backgroundWorker != null) backgroundWorker.Dispose();
        }

        private void FrVoltage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) Close();
        }

        private void BtOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
