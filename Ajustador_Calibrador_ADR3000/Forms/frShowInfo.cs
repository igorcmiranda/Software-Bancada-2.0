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
using Ajustador_Calibrador_ADR3000.Helpers;
using Ajustador_Calibrador_ADR3000.Delegates;
using System.Threading;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frShowInfo : Form
    {
        private readonly StandardMeter.StandardType standardType;
        private StandardMeter standardMeter;
        private int closingForm = 0;

        public frShowInfo(StandardMeter.StandardType _standardType)
        {
            InitializeComponent();

            standardType = _standardType;
        }

        private void frShowInfo_Load(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(() =>
            {
                int i;
                float[] measures;
                string text;
                standardMeter = new StandardMeter(standardType);
                standardMeter.ConnectToStandard(false);

                while (closingForm == 0)
                {
                    try
                    {
                        measures = standardMeter.GetMeasures();
                        switch (standardType)
                        {
                            default: 
                                {
                                    string[] labels = new string[]
                                    {
                                        "VA [V] = ",
                                        "VB [V] = ",
                                        "VC [V] = ",
                                        "VAB [V] = ",
                                        "VBC [V] = ",
                                        "VCA [V] = ",
                                        "IA [A] = ",
                                        "IB [A] = ",
                                        "IC [A] = ",
                                        "phiA [°] = ",
                                        "phiB [°] = ",
                                        "phiC [°] = ",
                                        "phi+ [°] = ",
                                        "phiVAVA [°] = ",
                                        "phiVAVB [°] = ",
                                        "phiVAVC [°] = ",
                                        "phiVAIA [°] = ",
                                        "phiVAIB [°] = ",
                                        "phiVAIC [°] = ",
                                        "PA [kW] = ",
                                        "PB [kW] = ",
                                        "PC [kW] = ",
                                        "P+ [kW] = ",
                                        "QA [kvar] = ",
                                        "QB [kvar] = ",
                                        "QC [kvar] = ",
                                        "Q+ [kvar] = ",
                                        "f [Hz] = ",                                        
                                        "US [] = ",
                                        "IS [] = ",
                                        "ES [] = ",
                                        "Ea [kWh] = ",
                                        "Er [kvarh] = ",
                                        "Es [kVAh] = ",
                                        "TM [ms] = ",
                                        "C [imp/kWh] = ",
                                        "NV [] = ",
                                        "ERROR [%] = ",
                                        "Cm [imp/kWh] = "
                                    };
                                    //
                                    //  GF333B and GF333B-M is default
                                    //
                                    text = "";
                                    for (i = 0; i < measures.Length; i++)
                                    {
                                        text += labels[i] + measures[i].ToString("0.000000") + Environment.NewLine;
                                    }
                                    
                                    break;
                                }
                        }

                        UpdateRichTextBox(text);

                        //
                        //  Waits 1 second to refresh standard measures
                        //
                        Thread.Sleep(3000);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        Close();
                        return;
                    }
                }
            }));
            th.Start();
        }

        private void UpdateRichTextBox(string text)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => 
            { 
                rtbMeasures.Text = text;
            }));
            else
            {
                rtbMeasures.Text = text;
            }
        }


        private void frShowInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            Interlocked.Exchange(ref closingForm, 1);

            if (standardMeter != null) standardMeter.Dispose();
            
            Dispose();
        }

        private void rtbMeasures_TextChanged(object sender, EventArgs e)
        {
            rtbMeasures.ScrollToCaret();
        }
    }
}
