using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using Ajustador_Calibrador_ADR3000.Delegates;
using Ajustador_Calibrador_ADR3000.Devices;
using Ajustador_Calibrador_ADR3000.Helpers;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Ports;
using ADODB;
using SimpleIO;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frAdjust : Form
    {
        private const int _VARIVOLT = 0;
        private const int _SUPPLIER = 1;
        private const int _ACP300 = 2;
        private const int _5_OUTPUTS_TRANSFORMER = 3;

        private const int ACP300_POS = 0;

        private readonly int sourceType;
        private readonly string connectionString;
        private readonly StandardMeter.StandardType standardType;
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private readonly System.Timers.Timer timer = new System.Timers.Timer(1000);
        private string time = "00:00:00";

        public frAdjust(int _sourceType, StandardMeter.StandardType _standardType, string _connectionString)
        {
            InitializeComponent();

            sourceType = _sourceType;
            standardType = _standardType;
            connectionString = _connectionString;            
        }

        /// <summary>
        /// Centralizes a control in the screen.
        /// </summary>
        /// <param name="ctrlToCenter">The control to centralize.</param>
        private void CenterControlInParent(Control ctrlToCenter)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;
            ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }

        private void FrAdjust_SizeChanged(object sender, EventArgs e)
        {
            CenterControlInParent(pnAdjust);
        }

        private void UpdateControlsStatus(bool status)
        {
            if (InvokeRequired) Invoke(new MyHandler(() => { btInitAdjustment.Enabled = status; btStopAdjustment.Enabled = !status; cbDevice.Enabled = status; }));
            else { btInitAdjustment.Enabled = status; btStopAdjustment.Enabled = !status; cbDevice.Enabled = status; }
        }

        private void UpdateText(string text)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { gbProgress.Text = text; }));
            else gbProgress.Text = text;
        }

        private void UpdateTimer(string time)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { lbTimer.Text = time; }));
            else lbTimer.Text = time;
        }

        private void UpdateProgress(int progress)
        {
            if (InvokeRequired) Invoke(new UpdateProgressHandler(() => { pbAdjust.Value = progress; }));
            else pbAdjust.Value = progress;
        }

        private void UpdateError(float error, Color color)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { lbError.Text = error.ToString("0.000"); lbError.ForeColor = color; }));
            else
            {
                lbError.Text = error.ToString("0.000");
                lbError.ForeColor = color;
            }
        }

        private bool ADR3000AdjustParameter(bool isGain, int parameter, int factorSize, int infLimit, int supLimit, 
            int sampleSize, float tolerance, float offset, int increment, StandardMeter standardMeter, ADR3000 adr3000)
        {
            int x0 = 0, x1 = 0, x = 0, it = 0, itMax = 30;
            float e0 = 0, e1 = 0, eavg, eavgAux;
            bool passed = false;
            float[] error = new float[sampleSize];

            eavg = 0.0f;
            eavgAux = 0.0f;
            x0 = adr3000.GetFactor(parameter, factorSize);
            
            if ((x0 < infLimit) || (x0 > supLimit))
            {
                x0 = Convert.ToInt32(0.5f * (infLimit + supLimit));
                adr3000.WriteFactor(parameter, x0, factorSize);
                Thread.Sleep(2000);
            }

            int i;
            if (isGain)
            {
                for (i = 0; i < sampleSize; i++)
                {
                    if (backgroundWorker.CancellationPending) return (false);
                    float[] stdMeasures = standardMeter.GetMeasures();
                    float[] adrMeasures = adr3000.GetMeasures();

                    //
                    //  500 ms delay to assure measurements update
                    //
                    Thread.Sleep(500);

                    error[i] = (adrMeasures[0] - stdMeasures[0]) * 100 / stdMeasures[0];
                    eavg += error[i] + offset;
                    eavgAux += error[i];
                }
                eavg /= sampleSize;
                eavgAux /= sampleSize;
            }
            else
            {
                adr3000.StartAccuracyTest();
                try
                {
                    while (true)
                    {
                        adr3000.GetMeasures();
                        if (backgroundWorker.CancellationPending) return (false);
                    }
                }
                catch (ApplicationException)
                {
                    eavgAux = adr3000.MyAccuracyTestResult.Error;
                    eavg = eavgAux + offset;
                }
            }

            Color color;
            if (Math.Abs(eavg) <= tolerance)
            {
                color = Color.Green;
                passed = true;
            }
            else color = Color.Red;
            UpdateError(eavgAux, color);

            e0 = eavg;


            if (!passed)
            {
                if (isGain)
                {
                    if (e0 > 0) x1 = x0 - increment;
                    else x1 = x0 + increment;
                }
                else
                {
                    if (e0 > 0)
                    {
                        if ((parameter == ADR3000.PHC2H) || (parameter == ADR3000.PHC2L) ||
                            (parameter == ADR3000.PHC1H) || (parameter == ADR3000.PHC1L) ||
                            (parameter == ADR3000.PHV1H) || (parameter == ADR3000.PHV1L) ||
                            (parameter == ADR3000.PHV2H) || (parameter == ADR3000.PHV2L))
                        {
                            x1 = x0 - increment;
                        }
                        else
                        {
                            x1 = x0 + increment;
                        }
                    }
                    else
                    {
                        if ((parameter == ADR3000.PHC2H) || (parameter == ADR3000.PHC2L) ||
                            (parameter == ADR3000.PHC1H) || (parameter == ADR3000.PHC1L) ||
                            (parameter == ADR3000.PHV1H) || (parameter == ADR3000.PHV1L) ||
                            (parameter == ADR3000.PHV2H) || (parameter == ADR3000.PHV2L))
                        {
                            x1 = x0 + increment;
                        }
                        else
                        {
                            x1 = x0 - increment;
                        }
                    }
                }

                adr3000.WriteFactor(parameter, x1, factorSize);
                Thread.Sleep(2000);

                eavg = 0.0f;
                eavgAux = 0.0f;
                if (isGain)
                {
                    for (i = 0; i < sampleSize; i++)
                    {
                        if (backgroundWorker.CancellationPending) return (false);
                        float[] stdMeasures = standardMeter.GetMeasures();
                        float[] adrMeasures = adr3000.GetMeasures();

                        //
                        //  500 ms delay to assure measurements update
                        //
                        Thread.Sleep(500);

                        error[i] = (adrMeasures[0] - stdMeasures[0]) * 100 / stdMeasures[0];
                        eavg += error[i] + offset;
                        eavgAux += error[i];
                    }
                    eavg /= sampleSize;
                    eavgAux /= sampleSize;
                }
                else
                {
                    adr3000.StartAccuracyTest();
                    try
                    {
                        while (true)
                        {
                            adr3000.GetMeasures();
                            if (backgroundWorker.CancellationPending) return (false);
                        }
                    }
                    catch (ApplicationException)
                    {
                        eavgAux = adr3000.MyAccuracyTestResult.Error;
                        eavg = eavgAux + offset;
                    }
                }


                if (Math.Abs(eavg) <= tolerance)
                {
                    color = Color.Green;
                    passed = true;
                }
                else color = Color.Red;
                UpdateError(eavgAux, color);

                e1 = eavg;
            }

            if (!passed)
            {
                do
                {
                    eavg = 0.0f;
                    eavgAux = 0.0f;

                    try
                    {
                        x = Convert.ToInt32((Convert.ToSingle(x0) * e1 - Convert.ToSingle(x1) * e0) / (e1 - e0));
                    }
                    catch (Exception)
                    {
                        if (x > supLimit) x = supLimit - increment;
                        else if (x < infLimit) x = infLimit + increment;
                    }
                    adr3000.WriteFactor(parameter, x, factorSize);
                    Thread.Sleep(2000);

                    if (isGain)
                    {
                        for (i = 0; i < sampleSize; i++)
                        {
                            if (backgroundWorker.CancellationPending) return (false);
                            float[] stdMeasures = standardMeter.GetMeasures();
                            float[] adrMeasures = adr3000.GetMeasures();

                            //
                            //  500 ms delay to assure measurements update
                            //
                            Thread.Sleep(500);

                            error[i] = (adrMeasures[0] - stdMeasures[0]) * 100 / stdMeasures[0];
                            eavg += error[i] + offset;
                            eavgAux += error[i];
                        }
                        eavg /= sampleSize;
                        eavgAux /= sampleSize;
                    }
                    else
                    {
                        adr3000.StartAccuracyTest();
                        try
                        {
                            while (true)
                            {
                                adr3000.GetMeasures();
                                if (backgroundWorker.CancellationPending) return (false);
                            }
                        }
                        catch (ApplicationException)
                        {
                            eavgAux = adr3000.MyAccuracyTestResult.Error;
                            eavg = eavgAux + offset;
                        }
                    }

                    if (Math.Abs(eavg) <= tolerance)
                    {
                        color = Color.Green;
                        passed = true;
                    }
                    else color = Color.Red;
                    UpdateError(eavgAux, color);

                    x0 = x1;
                    x1 = x;
                    e0 = e1;
                    e1 = eavg;

                    it++;
                } while ((it <= itMax) && (!passed));
            }

            return (true);
        }

        private bool ADR3000LITEAdjustParameter(int stdIndex, int adrIndex, int parameter, int factorSize, int infLimit, int supLimit,
            int sampleSize, float tolerance, float offset, int increment, StandardMeter standardMeter, ADR3000 adr3000)
        {
            int x0, x1 = 0, x = 0, it = 0, itMax = 30;
            float e0, e1 = 0, eavg, eavgAux;
            bool passed = false;
            float[] error = new float[sampleSize];
            int i;

            eavg = 0.0f;
            eavgAux = 0.0f;
            x0 = adr3000.GetFactor(parameter, factorSize);

            if ((x0 < infLimit) || (x0 > supLimit))
            {
                x0 = Convert.ToInt32(0.5f * (infLimit + supLimit));
                adr3000.WriteFactor(parameter, x0, factorSize);
                Thread.Sleep(2000);
            }

            

            for (i = 0; i < sampleSize; i++) adr3000.GetMeasures();
            for (i = 0; i < sampleSize; i++)
            {
                if (backgroundWorker.CancellationPending) return false;
                float[] stdMeasures = standardMeter.GetMeasures();
                float[] adrMeasures = adr3000.GetMeasures();

                //
                //  500 ms delay to assure measurements update
                //
                Thread.Sleep(500);

                error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                eavg += error[i] + offset;
                eavgAux += error[i];
            }
            eavg /= sampleSize;
            eavgAux /= sampleSize;

            Color color;
            if (Math.Abs(eavg) <= tolerance)
            {
                color = Color.Green;
                passed = true;
            }
            else color = Color.Red;
            UpdateError(eavgAux, color);

            e0 = eavg;


            if (!passed)
            {
                if (e0 > 0) x1 = x0 - increment;
                else x1 = x0 + increment;

                adr3000.WriteFactor(parameter, x1, factorSize);
                Thread.Sleep(2000);

                eavg = 0.0f;
                eavgAux = 0.0f;
                for (i = 0; i < sampleSize; i++) adr3000.GetMeasures();
                for (i = 0; i < sampleSize; i++)
                {
                    if (backgroundWorker.CancellationPending) return (false);
                    float[] stdMeasures = standardMeter.GetMeasures();
                    float[] adrMeasures = adr3000.GetMeasures();

                    //
                    //  500 ms delay to assure measurements update
                    //
                    Thread.Sleep(500);

                    error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                    eavg += error[i] + offset;
                    eavgAux += error[i];
                }
                eavg /= sampleSize;
                eavgAux /= sampleSize;

                if (Math.Abs(eavg) <= tolerance)
                {
                    color = Color.Green;
                    passed = true;
                }
                else color = Color.Red;
                UpdateError(eavgAux, color);

                e1 = eavg;
            }

            if (!passed)
            {
                do
                {
                    eavg = 0.0f;
                    eavgAux = 0.0f;

                    try
                    {
                        x = Convert.ToInt32((Convert.ToSingle(x0) * e1 - Convert.ToSingle(x1) * e0) / (e1 - e0));
                    }
                    catch (Exception)
                    {
                        if (x > supLimit) x = supLimit - increment;
                        else if (x < infLimit) x = infLimit + increment;
                    }

                    adr3000.WriteFactor(parameter, x, factorSize);
                    Thread.Sleep(2000);

                    for (i = 0; i < sampleSize; i++) adr3000.GetMeasures();
                    for (i = 0; i < sampleSize; i++)
                    {
                        if (backgroundWorker.CancellationPending) return false;
                        float[] stdMeasures = standardMeter.GetMeasures();
                        float[] adrMeasures = adr3000.GetMeasures();

                        //
                        //  500 ms delay to assure measurements update
                        //
                        Thread.Sleep(500);

                        error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                        eavg += error[i] + offset;
                        eavgAux += error[i];
                    }
                    eavg /= sampleSize;
                    eavgAux /= sampleSize;

                    if (Math.Abs(eavg) <= tolerance)
                    {
                        color = Color.Green;
                        passed = true;
                    }
                    else color = Color.Red;
                    UpdateError(eavgAux, color);

                    x0 = x1;
                    x1 = x;
                    e0 = e1;
                    e1 = eavg;

                    it++;
                } while ((it <= itMax) && (!passed));
            }

            return true;

        }

        private bool ADR2000AdjustParameter(bool isGain, int parameter, int infLimit, int supLimit, int stdIndex, int adrIndex,
            int sampleSize, float tolerance, float offset, int increment, StandardMeter standardMeter, ADR2000 adr2000)
        {
            int x0, x1 = 0, x = 0, it = 0, itMax = 30;
            float e0, e1 = 0, eavg, eavgAux;
            bool passed = false;
            float[] error = new float[sampleSize];

            eavg = 0.0f;
            eavgAux = 0.0f;
            x0 = adr2000.ReadMemoryShort(parameter);

            if ((x0 < infLimit) || (x0 > supLimit))
            {
                x0 = Convert.ToInt32(0.5f * (infLimit + supLimit));
                adr2000.WriteMemoryShort(parameter, x0);
                Thread.Sleep(2000);
            }

            int i;
            if (isGain)
            {
                for (i = 0; i < sampleSize; i++)
                {
                    if (backgroundWorker.CancellationPending) return (false);
                    float[] stdMeasures = standardMeter.GetMeasures();
                    float[] adrMeasures = new float[ADR2000.NUM_OF_MEASURES];
                    adr2000.GetMeasures(adrMeasures);
                    //
                    //  500 ms delay to assure measurements update
                    //
                    Thread.Sleep(500);

                    error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                    eavg += error[i] + offset;
                    eavgAux += error[i];
                }
                eavg /= sampleSize;
                eavgAux /= sampleSize;
            }
            else
            {
                throw new ApplicationException("Ajuste não suportado no momento.");
            }

            Color color;
            if (Math.Abs(eavg) <= tolerance)
            {
                color = Color.Green;
                passed = true;
            }
            else color = Color.Red;
            UpdateError(eavgAux, color);

            e0 = eavg;

            if (!passed)
            {
                if (isGain)
                {
                    if (e0 > 0) x1 = x0 - increment;
                    else x1 = x0 + increment;
                }
                else
                {
                    throw new ApplicationException("Ajuste não suportado no momento.");
                }

                adr2000.WriteMemoryShort(parameter, x1);
                Thread.Sleep(2000);

                eavg = 0.0f;
                eavgAux = 0.0f;
                if (isGain)
                {
                    for (i = 0; i < sampleSize; i++)
                    {
                        if (backgroundWorker.CancellationPending) return (false);
                        float[] stdMeasures = standardMeter.GetMeasures();
                        float[] adrMeasures = new float[ADR2000.NUM_OF_MEASURES];
                        adr2000.GetMeasures(adrMeasures);
                        //
                        //  500 ms delay to assure measurements update
                        //
                        Thread.Sleep(500);

                        error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                        eavg += error[i] + offset;
                        eavgAux += error[i];
                    }
                    eavg /= sampleSize;
                    eavgAux /= sampleSize;
                }
                else
                {
                    throw new ApplicationException("Ajuste não suportado no momento.");
                }


                if (Math.Abs(eavg) <= tolerance)
                {
                    color = Color.Green;
                    passed = true;
                }
                else color = Color.Red;
                UpdateError(eavgAux, color);

                e1 = eavg;
            }

            if (!passed)
            {
                do
                {
                    eavg = 0.0f;
                    eavgAux = 0.0f;

                    try
                    {
                        x = Convert.ToInt32((Convert.ToSingle(x0) * e1 - Convert.ToSingle(x1) * e0) / (e1 - e0));
                    }
                    catch (Exception)
                    {
                        if (x > supLimit) x = supLimit - increment;
                        else if (x < infLimit) x = infLimit + increment;
                    }
                    adr2000.WriteMemoryShort(parameter, x);
                    Thread.Sleep(2000);

                    if (isGain)
                    {
                        for (i = 0; i < sampleSize; i++)
                        {
                            if (backgroundWorker.CancellationPending) return (false);
                            float[] stdMeasures = standardMeter.GetMeasures();
                            float[] adrMeasures = new float[ADR2000.NUM_OF_MEASURES];
                            adr2000.GetMeasures(adrMeasures);

                            //
                            //  500 ms delay to assure measurements update
                            //
                            Thread.Sleep(500);

                            error[i] = (adrMeasures[adrIndex] - stdMeasures[stdIndex]) * 100 / stdMeasures[stdIndex];
                            eavg += error[i] + offset;
                            eavgAux += error[i];
                        }
                        eavg /= sampleSize;
                        eavgAux /= sampleSize;
                    }
                    else
                    {
                        throw new ApplicationException("Ajuste não suportado no momento.");
                    }

                    if (Math.Abs(eavg) <= tolerance)
                    {
                        color = Color.Green;
                        passed = true;
                    }
                    else color = Color.Red;
                    UpdateError(eavgAux, color);

                    x0 = x1;
                    x1 = x;
                    e0 = e1;
                    e1 = eavg;

                    it++;
                } while ((it <= itMax) && (!passed));
            }

            return (true);
        }

        private bool AdjustADR3000(int serialNumber, StandardMeter standardMeter, ADR3000 adr3000,
            PowerSupply powerSupply, frVoltage formVoltage, MSAccess db, object args)
        {
            bool isHighScale = true;
            int totalSteps = 6, progress;
            int[] parameter = new int[] { ADR3000.CHV2, ADR3000.CHV1, ADR3000.CHC2H, ADR3000.PHC2H, ADR3000.CHC2L, ADR3000.PHC2L };

            string[] paramMsg = new string[] { "tensão V2", "tensão V1", "corrente escala alta", "defasagem escala alta", "corrente escala baixa", "defasagem escala baixa" };
            float[] iSp = new float[] { 25.0f, 25.0f, 25.0f, 25.0f, 1.25f, 1.25f };
            float[] phiSp = new float[] { 0.0f, 0.0f, 0.0f, 60.0f, 0.0f, 60.0f };
            float[] kd = new float[] { 3.0f, 3.0f, 3.0f, 3.0f, 0.25f, 0.25f };
            int[] kh = new int[] { 72, 72, 72, 72, 720, 720 };
            int[] pulses = new int[] { 1, 1, 6, 6, 6, 6 };

            int[] factorSize = new int[] { ADR3000._16b, ADR3000._16b, ADR3000._16b, ADR3000._16b, ADR3000._16b, ADR3000._16b };

            Recordset rs = new Recordset();
            db.ConnectToDatabase();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            int nE = ((dynamic)rs.Fields["nE"]).Value;
            int nV = ((dynamic)rs.Fields["nV"]).Value;
            int nI = ((dynamic)rs.Fields["nI"]).Value;
            float vTolerance = ((dynamic)rs.Fields["vTolerance"]).Value;
            float iTolerance = ((dynamic)rs.Fields["iTolerance"]).Value;
            float eTolerance = ((dynamic)rs.Fields["eTolerance"]).Value;
            float offsetVoltage1 = ((dynamic)rs.Fields["offsetVoltage1"]).Value;
            float offsetVoltage2 = ((dynamic)rs.Fields["offsetVoltage2"]).Value;
            float offsetCurrentHigh_High = ((dynamic)rs.Fields["offsetCurrentHigh_High"]).Value;
            float offsetCurrentHigh_Low = ((dynamic)rs.Fields["offsetCurrentHigh_Low"]).Value;
            float offsetCurrentHigh_High_Ind = ((dynamic)rs.Fields["offsetCurrentHigh_High_Ind"]).Value;
            float offsetCurrentHigh_Low_Ind = ((dynamic)rs.Fields["offsetCurrentHigh_Low_Ind"]).Value;
            float offsetCurrentLow = ((dynamic)rs.Fields["offsetCurrentLow"]).Value;
            float offsetCurrentLow_Ind = ((dynamic)rs.Fields["offsetCurrentLow_Ind"]).Value;
            int incGO = ((dynamic)rs.Fields["incGO"]).Value;
            int incPHI = ((dynamic)rs.Fields["incPHI"]).Value;
            int infLimitV2 = ((dynamic)rs.Fields["infLimitV2"]).Value;
            int infLimitV1 = ((dynamic)rs.Fields["infLimitV1"]).Value;
            int infLimitI2High = ((dynamic)rs.Fields["infLimitI2High"]).Value;
            int infLimitPhiHigh = ((dynamic)rs.Fields["infLimitPhiHigh"]).Value;
            int infLimitI2Low = ((dynamic)rs.Fields["infLimitI2Low"]).Value;
            int infLimitPhiLow = ((dynamic)rs.Fields["infLimitPhiLow"]).Value;
            int supLimitV2 = ((dynamic)rs.Fields["supLimitV2"]).Value;
            int supLimitV1 = ((dynamic)rs.Fields["supLimitV1"]).Value;
            int supLimitI2High = ((dynamic)rs.Fields["supLimitI2High"]).Value;
            int supLimitPhiHigh = ((dynamic)rs.Fields["supLimitPhiHigh"]).Value;
            int supLimitI2Low = ((dynamic)rs.Fields["supLimitI2Low"]).Value;
            int supLimitPhiLow = ((dynamic)rs.Fields["supLimitPhiLow"]).Value;
            rs.Close();
            db.CloseConnection();

            float[] tolerance = new float[] { vTolerance, vTolerance, iTolerance, eTolerance, iTolerance, eTolerance };
            float[] offset = new float[] { offsetVoltage2, offsetVoltage1, offsetCurrentHigh_Low, offsetCurrentHigh_Low_Ind, offsetCurrentLow, offsetCurrentLow_Ind };
            int[] increment = new int[] { incGO, incGO, incGO, incPHI, incGO, incPHI };
            int[] sampleSize = new int[] { nV, nV, nI, nE, nI, nE };
            int[] infLimit = new int[] { infLimitV2, infLimitV1, infLimitI2High, infLimitPhiHigh, infLimitI2Low, infLimitPhiLow };
            int[] supLimit = new int[] { supLimitV2, supLimitV1, supLimitI2High, supLimitPhiHigh, supLimitI2Low, supLimitPhiLow };
            bool[] isGain = new bool[] { true, true, false, false, false, false };

            int[] bestVRng = new int[] { 1, 1 };
            int[] bestIRng = new int[] { 1, 1 };

            object[] obj = new object[1];
            if (args != null)
            {
                obj = (object[])args;
            }

            bool adrHasVoltageGeneration = (bool)obj[1];

            //
            //  Ensures that the voltage range of the standard is the best one
            //
            if (!adrHasVoltageGeneration)
            {
                if (sourceType == _SUPPLIER)
                {
                    float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                    }
                    else
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                    }

                    Thread.Sleep(5000);
                }
                else if (sourceType == _ACP300)
                {
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                    }
                    else
                    {
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                    }

                    Thread.Sleep(5000);
                }
                else
                {
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            formVoltage.Voltage = 180.0f;
                            formVoltage.ShowDialog();
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            formVoltage.Voltage = 180.0f;
                            formVoltage.ShowDialog();
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            formVoltage.Voltage = 180.0f;
                            formVoltage.ShowDialog();
                        }
                    }
                    else
                    {
                        formVoltage.Voltage = 180.0f;
                        formVoltage.ShowDialog();
                    }
                }
            }
            else
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        adr3000.SetVoltageSetPoint(180.0f);
                        adr3000.TurnOnV();
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        adr3000.SetVoltageSetPoint(180.0f);
                        adr3000.TurnOnV();
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        adr3000.SetVoltageSetPoint(180.0f);
                        adr3000.TurnOnV();
                    }
                }
                else
                {
                    adr3000.SetVoltageSetPoint(180.0f);
                    adr3000.TurnOnV();
                }

                Thread.Sleep(5000);
            }

            adr3000.CancelTest();
            adr3000.SetEnergyType(ADR3000.ACTIVE_ENERGY);
            adr3000.SetCountingType(ADR3000.PULSES_LAPS);
            adr3000.SetNumberOfElements(ADR3000.SINGLE_PHASE);

            adr3000.SetSerialNumber(serialNumber);

            if (standardType == StandardMeter.StandardType.RMM3006)
            {
                int currentRng = standardMeter.GetBestCurrentRange(iSp[0])[0];
                int zeraIRng = standardMeter.GetRangeStatus()[1];

                if (currentRng != zeraIRng)
                {
                    standardMeter.SetActualCurrentRange(currentRng);
                    Thread.Sleep(2000);
                }
            }

            adr3000.SetCurrentSetPoint(iSp[0]);
            //
            //  Configures the K1 and K2 keys (necessary with RMM3006)
            //  
            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            standardMeter.SetK1K2(false, true);
            Thread.Sleep(4000);

            adr3000.TurnOnI();


            adr3000.SetEEPROMProtectionState(ADR3000.UNBLOCKED);

            adr3000.WriteFactor(ADR3000.PHV2L, 1, ADR3000._8b);
            adr3000.WriteFactor(ADR3000.PHC2L, 512, ADR3000._16b);
            adr3000.SetPulseOutputDivider(2);

            standardMeter.GetMeasures();

            int i;
            for (i = 0; i < totalSteps; i++)
            {
                if (backgroundWorker.CancellationPending) return (false);

                UpdateError(0.0f, Color.Blue);
                Thread.Sleep(1000);

                progress = (i + 1) * 100 / totalSteps;
                UpdateText(progress + " % - Ajustando " + paramMsg[i]);
                UpdateProgress(progress);


                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestIRng = standardMeter.GetBestCurrentRange(iSp[i]);
                    int currentRng = bestIRng[0];
                    int zeraIRng = standardMeter.GetRangeStatus()[1];
                    bool isCurrentOff = false;

                    if (iSp[i] <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
                    {
                        if (isHighScale)
                        {
                            adr3000.TurnOffI();
                            isCurrentOff = true;
                            standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                            standardMeter.SetK1K2(true, false); Thread.Sleep(4000);
                            isHighScale = false;
                        }
                    }

                    if (currentRng != zeraIRng)
                    {
                        adr3000.TurnOffI();
                        standardMeter.SetActualCurrentRange(currentRng);
                        Thread.Sleep(1000);
                        adr3000.SetCurrentSetPoint(iSp[i]);
                        adr3000.TurnOnI();

                        Thread.Sleep(5000);
                    }
                    else
                    {
                        adr3000.SetCurrentSetPoint(iSp[i]);
                        if (isCurrentOff) adr3000.TurnOnI();
                    }
                    kh[i] = 750 * bestVRng[1] * bestIRng[1];
                }
                else adr3000.SetCurrentSetPoint(iSp[i]);

                adr3000.SetPhiSetPoint(phiSp[i]);

                Thread.Sleep(5000);

                standardMeter.SetPulseEnergyType(StandardMeter.EnergyType.ACTIVE);
                standardMeter.SetKd(StandardMeter.EnergyType.ACTIVE, kd[i], kh[i]);

                adr3000.SetNumberOfTurns(pulses[i]);
                adr3000.SetKd(kd[i]);

                Thread.Sleep(2000);

                if (!ADR3000AdjustParameter(isGain[i], parameter[i], factorSize[i], infLimit[i], supLimit[i], sampleSize[i],
                    tolerance[i], offset[i], increment[i], standardMeter, adr3000))
                {
                    return (false);
                }

                //
                //  This delay assures that the user is able to see the approved error.
                //
                Thread.Sleep(3000);
            }

            return (true);
        }

        private bool AdjustADR3000LITE(int serialNumber, StandardMeter standardMeter, ADR3000 adr3000, MSAccess db)
        {
            
            const int totalSteps = 10;
            const int numOfChannelType = 2;
            int progress;
            int[] parameter = new int[] 
            { 
                ADR3000.EE_CHV1_0_LITE,
                ADR3000.EE_CHC10_0_LITE,
                ADR3000.EE_CHC10_1_LITE,
                ADR3000.EE_CHC10_2_LITE,
                ADR3000.EE_CHC10_3_LITE,
                ADR3000.EE_CHV1_1_LITE,
                ADR3000.EE_CHC11_0_LITE,
                ADR3000.EE_CHC11_1_LITE,
                ADR3000.EE_CHC11_2_LITE,
                ADR3000.EE_CHC11_3_LITE
            };

            string[] paramMsg = new string[] 
            { 
                "tensão V1 - 120 V", 
                "corrente I1 - 120 V/1.5 A",
                "corrente I1 - 120 V/3.0 A",
                "corrente I1 - 120 V/15.0f A",
                "corrente I1 - 120 V/45.0f A",
                "tensão V1 - 220 V",
                "corrente I1 - 220 V/1.5 A",
                "corrente I1 - 220 V/3.0 A",
                "corrente I1 - 220 V/15.0f A",
                "corrente I1 - 220 V/45.0f A",
                "tensão V2 - 120 V",
                "corrente I2 - 120 V/1.5 A",
                "corrente I2 - 120 V/3.0 A",
                "corrente I2 - 120 V/15.0f A",
                "corrente I2 - 120 V/45.0f A",
                "tensão V2 - 220 V",
                "corrente I2 - 220 V/1.5 A",
                "corrente I2 - 220 V/3.0 A",
                "corrente I2 - 220 V/15.0f A",
                "corrente I2 - 220 V/45.0f A",

            };
            float[] iSp = new float[] { 0.0f, 1.5f, 3.0f, 15.0f, 45.0f, 0.0f, 1.5f, 3.0f, 15.0f, 45.0f };
            float[] vSp = new float[] { 120.0f, 120.0f, 120.0f, 120.0f, 120.0f, 220.0f, 220.0f, 220.0f, 220.0f, 220.0f };
            float currentVoltage = 0.0f;

            Recordset rs = new Recordset();
            db.ConnectToDatabase();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            
            int nV = ((dynamic)rs.Fields["nV_ADR3000LITE"]).Value;
            int nI = ((dynamic)rs.Fields["nI_ADR3000LITE"]).Value;
            float vTolerance = ((dynamic)rs.Fields["vTolerance_ADR3000LITE"]).Value;
            float iTolerance = ((dynamic)rs.Fields["iTolerance_ADR3000LITE"]).Value;
            int incGO = ((dynamic)rs.Fields["incGO"]).Value;
            
            int infLimitV = ((dynamic)rs.Fields["infLimV_ADR3000LITE"]).Value;
            int infLimitI = ((dynamic)rs.Fields["infLimI_ADR3000LITE"]).Value;            
            int supLimitV = ((dynamic)rs.Fields["supLimV_ADR3000LITE"]).Value;
            int supLimitI = ((dynamic)rs.Fields["supLimI_ADR3000LITE"]).Value;
            rs.Close();
            db.CloseConnection();

            float[] tolerance = new float[] { vTolerance, iTolerance, iTolerance, iTolerance, iTolerance, vTolerance, iTolerance, iTolerance, iTolerance, iTolerance };
            
            int[] sampleSize = new int[] { nV, nI, nI, nI, nI, nV, nI, nI, nI, nI };
            int[] infLimit = new int[] { infLimitV, infLimitI, infLimitI, infLimitI, infLimitI, infLimitV, infLimitI, infLimitI, infLimitI, infLimitI };
            int[] supLimit = new int[] { supLimitV, supLimitI, supLimitI, supLimitI, supLimitI, supLimitV, supLimitI, supLimitI, supLimitI, supLimitI };            

            int[] bestVRng;
            int[] bestIRng;
          

            adr3000.CancelTest();
            adr3000.SetEnergyType(ADR3000.ACTIVE_ENERGY);
            adr3000.SetCountingType(ADR3000.PULSES_LAPS);
            adr3000.SetNumberOfElements(ADR3000.SINGLE_PHASE);

            adr3000.SetSerialNumber(serialNumber);
           
            //
            //  Configures the K1 and K2 keys (necessary with RMM3006)
            //  
            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            standardMeter.SetK1K2(true, false);
            Thread.Sleep(4000);
            bool isHighScale = false;

            adr3000.SetEEPROMProtectionState(ADR3000.UNBLOCKED);

            standardMeter.GetMeasures();

            const int adrIndexOffset = 2;
            int[] adrIndex = new int[] { 0, 1, 1, 1, 1, 0, 1, 1, 1, 1 };
            int[] stdIndex;
            switch (standardType)
            {
                case StandardMeter.StandardType.GF333B:
                case StandardMeter.StandardType.GF333BM:
                    stdIndex = new int[] { 0, 6, 6, 6, 6, 0, 6, 6, 6, 6 };
                    break;
                case StandardMeter.StandardType.RD20:
                    stdIndex = new int[] { 0, 1, 1, 1, 1, 0, 1, 1, 1, 1 };
                    break;
                default:
                    stdIndex = new int[] { 0, 3, 3, 3, 3, 0, 3, 3, 3, 3 };
                    break;
            }

            int i, j;
            for (i = 0; i < totalSteps; i++)
            {
                if (backgroundWorker.CancellationPending) return false;

                adr3000.TurnOffI();
                Thread.Sleep(3000);
                if (currentVoltage != vSp[i])
                {
                    currentVoltage = vSp[i];
                    adr3000.TurnOffV();
                    Thread.Sleep(3000);
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(vSp[i]);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            adr3000.SetVoltageSetPoint(vSp[i]);                            
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            adr3000.SetVoltageSetPoint(vSp[i]);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                        }
                        else
                        {
                            adr3000.SetVoltageSetPoint(vSp[i]);
                        }
                    }
                    else
                    {
                        adr3000.SetVoltageSetPoint(vSp[i]);
                    }

                    adr3000.TurnOnV();
                    Thread.Sleep(5000);
                }

                if (iSp[i] > 0.0f)
                {
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestIRng = standardMeter.GetBestCurrentRange(iSp[i]);
                        int currentRng = bestIRng[0];
                        int zeraIRng = standardMeter.GetRangeStatus()[1];

                        if (iSp[i] <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
                        {
                            if (isHighScale)
                            {
                                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                                standardMeter.SetK1K2(true, false); Thread.Sleep(4000);
                                isHighScale = false;
                            }
                        }
                        else
                        {
                            if (!isHighScale)
                            {
                                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                                standardMeter.SetK1K2(false, true); Thread.Sleep(4000);
                                isHighScale = true;
                            }
                        }

                        if (currentRng != zeraIRng)
                        {
                            standardMeter.SetActualCurrentRange(currentRng);
                            Thread.Sleep(1000);

                        }
                    }

                    adr3000.SetCurrentSetPoint(iSp[i]);
                    adr3000.TurnOnI();

                    Thread.Sleep(5000);
                }

                for (j = 0; j < numOfChannelType; j++)
                {
                    if (backgroundWorker.CancellationPending) return false;

                    UpdateError(0.0f, Color.Blue);
                    Thread.Sleep(1000);
                    progress = (2*i + j + 1) * 100 / (numOfChannelType * totalSteps);
                    UpdateText(progress + " % - Ajustando " + paramMsg[i + j * totalSteps]);
                    UpdateProgress(progress);

                    if (!ADR3000LITEAdjustParameter(stdIndex[i], adrIndex[i] + j * adrIndexOffset, parameter[i] + j * ADR3000.EE_CHANNEL_OFFSET_LITE, 
                        ADR3000._16b, infLimit[i], supLimit[i], sampleSize[i], tolerance[i], 0, incGO, standardMeter, adr3000))
                    {
                        return false;
                    }
                    //
                    //  This delay assures that the user is able to see the approved error.
                    //
                    Thread.Sleep(3000);
                }
            }

            return true;
        }

        private bool AdjustADR2000(int serialNumber, StandardMeter standardMeter, ADR2000 adr2000,
            PowerSupply powerSupply, frVoltage formVoltage, MSAccess db, object args)
        {
            bool isHighScale = false;
            int totalSteps = 2, progress;
            int[] parameter = new int[] { ADR2000.EE_CHV1, ADR2000.EE_CHC1_0 };

            string[] paramMsg = new string[] { "tensão", "corrente" };

            float[] iSp = new float[] { 3.0f, 3.0f };
            float[] kd = new float[] { 0.1f, 7.2f };
            bool[] turbo = new bool[] { false, true };
            bool[] useCurrent = new bool[] { false, true };
            
            int[] kh = new int[] { 720, 720 };
            int[] pulses = new int[] { 50, 50 };


            Recordset rs = new Recordset();
            db.ConnectToDatabase();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            int nE = ((dynamic)rs.Fields["nE"]).Value;
            int nV = ((dynamic)rs.Fields["nV_ADR2000"]).Value;
            int nI = ((dynamic)rs.Fields["nI_ADR2000"]).Value;
            float vTolerance = ((dynamic)rs.Fields["vTolerance_ADR2000"]).Value;
            float iTolerance = ((dynamic)rs.Fields["iTolerance_ADR2000"]).Value;
            float eTolerance = ((dynamic)rs.Fields["eTolerance"]).Value;
            float offsetVoltage1 = ((dynamic)rs.Fields["offsetVoltage1"]).Value;
            float offsetVoltage2 = ((dynamic)rs.Fields["offsetVoltage2"]).Value;
            float offsetCurrentHigh_High = ((dynamic)rs.Fields["offsetCurrentHigh_High"]).Value;
            float offsetCurrentHigh_Low = ((dynamic)rs.Fields["offsetCurrentHigh_Low"]).Value;
            float offsetCurrentHigh_High_Ind = ((dynamic)rs.Fields["offsetCurrentHigh_High_Ind"]).Value;
            float offsetCurrentHigh_Low_Ind = ((dynamic)rs.Fields["offsetCurrentHigh_Low_Ind"]).Value;
            float offsetCurrentLow = ((dynamic)rs.Fields["offsetCurrentLow"]).Value;
            float offsetCurrentLow_Ind = ((dynamic)rs.Fields["offsetCurrentLow_Ind"]).Value;
            int incGO = ((dynamic)rs.Fields["incGO"]).Value;
            int incPHI = ((dynamic)rs.Fields["incPHI"]).Value;
            int infLimitV2 = ((dynamic)rs.Fields["infLimitV2"]).Value;
            int infLimitV1 = ((dynamic)rs.Fields["infLimitV1"]).Value;
            int infLimitI2High = ((dynamic)rs.Fields["infLimitI2High"]).Value;
            int infLimitPhiHigh = ((dynamic)rs.Fields["infLimitPhiHigh"]).Value;
            int infLimitI2Low = ((dynamic)rs.Fields["infLimitI2Low"]).Value;
            int infLimitPhiLow = ((dynamic)rs.Fields["infLimitPhiLow"]).Value;
            int supLimitV2 = ((dynamic)rs.Fields["supLimitV2"]).Value;
            int supLimitV1 = ((dynamic)rs.Fields["supLimitV1"]).Value;
            int supLimitI2High = ((dynamic)rs.Fields["supLimitI2High"]).Value;
            int supLimitPhiHigh = ((dynamic)rs.Fields["supLimitPhiHigh"]).Value;
            int supLimitI2Low = ((dynamic)rs.Fields["supLimitI2Low"]).Value;
            int supLimitPhiLow = ((dynamic)rs.Fields["supLimitPhiLow"]).Value;
            int stdIndex_V = ((dynamic)rs.Fields["stdIndex_V"]).Value;
            int stdIndex_I = ((dynamic)rs.Fields["stdIndex_I"]).Value;
            int adrIndex_V = ((dynamic)rs.Fields["adrIndex_V"]).Value;
            int adrIndex_I = ((dynamic)rs.Fields["adrIndex_I"]).Value;
            rs.Close();
            db.CloseConnection();

            int[] adrIndex = new int[] { adrIndex_V, adrIndex_I };
            int[] stdIndex = new int[] { stdIndex_V, stdIndex_I };
            float[] tolerance = new float[] { vTolerance, iTolerance };
            float[] offset = new float[] { offsetVoltage1, offsetCurrentLow };
            int[] increment = new int[] { incGO, incGO };
            int[] sampleSize = new int[] { nV, nI };
            int[] infLimit = new int[] { infLimitV1, infLimitI2Low };
            int[] supLimit = new int[] { supLimitV1, supLimitI2Low };
            bool[] isGain = new bool[] { true, true };

            int[] bestVRng = new int[] { 1, 1 };
            int[] bestIRng = new int[] { 1, 1 };

            object[] obj = new object[1];
            if (args != null)
            {
                obj = (object[])args;
            }
            //
            //  Ensures that the voltage range of the standard is the best one
            //
            if (sourceType == _SUPPLIER)
            {
                float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                }
                else
                {
                    powerSupply.SendCommand(PowerSupply.Phase.All, 180.0f / sqrt3, PowerSupply.Command.PutSetPoint);
                }

                Thread.Sleep(5000);
            }
            else if (sourceType == _ACP300)
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                }
                else
                {
                    ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, "180.000");
                }

                Thread.Sleep(5000);
            }
            else if(sourceType == _VARIVOLT)
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        formVoltage.Voltage = 180.0f;
                        formVoltage.ShowDialog();
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        formVoltage.Voltage = 180.0f;
                        formVoltage.ShowDialog();
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        formVoltage.Voltage = 180.0f;
                        formVoltage.ShowDialog();
                    }
                }
                else
                {
                    formVoltage.Voltage = 180.0f;
                    formVoltage.ShowDialog();
                }
            }

            adr2000.StopAll();                            
           
            if (standardType == StandardMeter.StandardType.RMM3006)
            {
                int currentRng = standardMeter.GetBestCurrentRange(iSp[0])[0];
                int zeraIRng = standardMeter.GetRangeStatus()[1];

                if (currentRng != zeraIRng)
                {
                    standardMeter.SetActualCurrentRange(currentRng);
                    Thread.Sleep(2000);
                }
            }
           
            //
            //  Configures the K1 and K2 keys (necessary with RMM3006)
            //  
            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            standardMeter.SetK1K2(false, true);
            Thread.Sleep(4000);

            standardMeter.GetMeasures();

            int i;
            for (i = 0; i < totalSteps; i++)
            {
                if (backgroundWorker.CancellationPending) return (false);

                UpdateError(0.0f, Color.Blue);
                Thread.Sleep(1000);

                progress = (i + 1) * 100 / totalSteps;
                UpdateText(progress + " % - Ajustando " + paramMsg[i]);
                UpdateProgress(progress);


                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestIRng = standardMeter.GetBestCurrentRange(iSp[i]);
                    int currentRng = bestIRng[0];
                    int zeraIRng = standardMeter.GetRangeStatus()[1];
                    

                    if (iSp[i] <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
                    {
                        if (isHighScale)
                        {          
                            standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                            standardMeter.SetK1K2(true, false); Thread.Sleep(4000);
                            isHighScale = false;
                        }
                    }

                    if (currentRng != zeraIRng)
                    {
                        
                        standardMeter.SetActualCurrentRange(currentRng);
                        Thread.Sleep(1000);
                        
                    }
                    
                    kh[i] = 750 * bestVRng[1] * bestIRng[1];
                }              

                standardMeter.SetPulseEnergyType(StandardMeter.EnergyType.ACTIVE);
                standardMeter.SetKd(StandardMeter.EnergyType.ACTIVE, kd[i], kh[i]);

                adr2000.ConfigureAccuracyTest(kd[i], pulses[i], turbo[i]);

                Thread.Sleep(2000);

                if (useCurrent[i])
                {
                    adr2000.EnableAccuracyTest();
                    Thread.Sleep(10000);
                }

                if (!ADR2000AdjustParameter(isGain[i], parameter[i], infLimit[i], supLimit[i], stdIndex[i], adrIndex[i], 
                    sampleSize[i], tolerance[i], offset[i], increment[i], standardMeter, adr2000))
                {
                    adr2000.StopAll();
                    return (false);
                }

                if (useCurrent[i]) adr2000.StopAll();
                //
                //  This delay assures that the user is able to see the approved error.
                //
                Thread.Sleep(3000);
            }

            return (true);
        }

        private void AdjustADR2000Process(string[] deviceInfo)
        {
            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();
            TcpClient tcpClient = new TcpClient();
            ADR2000 adr2000 = new ADR2000(null);
            ACP300 acpPowerSource = new ACP300();
            StandardMeter standardMeter = new StandardMeter(standardType);
            PowerSupply powerSupply = new PowerSupply(null);
            frVoltage formVoltage = new frVoltage(standardMeter, standardType);
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                //
                //  Necessary for the RMM3006
                //
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(false);


                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Recordset rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    uint VendorID = Convert.ToUInt32(((dynamic)rs.Fields["VID"]).Value);
                    uint ProductID = Convert.ToUInt32(((dynamic)rs.Fields["PID"]).Value);
                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    SimpleIOClass.InitMCP2200(VendorID, ProductID);
                    if (!SimpleIOClass.IsConnected())
                    {
                        throw new ApplicationException("Falha ao conectar e configurar MCP2200 para trafo com 5 saídas.");
                    }
                    else
                    {
                        SimpleIOClass.ConfigureIoDefaultOutput(0xF0, (byte)Transformer5Out._TRANSFORMER_OFF_);
                    }

                    MessageBox.Show("Mantenha pressionadas as teclas \u2191 e \u2193, clique em OK e aguarde o beep.", 
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    //  Transformer with 5 outputs
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        int[] bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                        }
                    }
                    else
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                    }
                    // Wait for ADR 2000 to turn on
                    Thread.Sleep(6000);
                }


                bluetoothClient.Connect(BluetoothAddress.Parse(deviceInfo[1]), BluetoothService.SerialPort);
                adr2000 = new ADR2000(bluetoothClient.GetStream());


                if (sourceType == _SUPPLIER)
                {
                    Recordset rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    string sourceIP = ((dynamic)rs.Fields["VAddr"]).Value.ToString();
                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    tcpClient.Connect(sourceIP, 502);
                    powerSupply = new PowerSupply(tcpClient.GetStream());
                }
                else if (sourceType == _ACP300)
                {
                    Recordset rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);

                    int baudRate = Convert.ToInt32(rs.Fields["BAUD_PowerSupply"].Value);
                    int stopBits = Convert.ToInt32(rs.Fields["StopBits_PowerSupply"].Value);
                    int parity = Convert.ToInt32(rs.Fields["Parity_PowerSupply"].Value);
                    int dataSize = Convert.ToInt32(rs.Fields["DataSize_PowerSupply"].Value);
                    string port = rs.Fields["COM_PowerSupply"].Value.ToString();

                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    acpPowerSource = new ACP300(port, baudRate, dataSize,
                        (Parity)parity, (StopBits)stopBits, false, false);

                    if (!acpPowerSource.InitSerial())
                    {
                        throw new IOException("Falha ao conectar com fonte de tensão ACP300");
                    }

                    Answer answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                    answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                    if (!answer.Status)
                    {
                        throw new IOException("Falha ao inicializar fonte de tensão ACP300");
                    }
                }

                if (!AdjustADR2000(Convert.ToInt32(deviceInfo[0].Substring(4)), standardMeter,
                    adr2000, powerSupply, formVoltage, db, new object[] { acpPowerSource }))
                {
                    MessageBox.Show("Processo de ajuste interrompido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                adr2000.StopAll();
                
                if (sourceType == _SUPPLIER)
                {
                    powerSupply.SendCommand(PowerSupply.Phase.All, 60.0f, PowerSupply.Command.PutSetPoint);
                }
                else if (sourceType == _ACP300)
                {
                    Answer answer = acpPowerSource.SendCommand(ACP300.SET_VOLTAGE, "103.000");
                    if (!answer.Status)
                    {
                        MessageBox.Show("Problemas no ajuste de tensão da fonte ACP300.", "Erro",
                             MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    answer = acpPowerSource.SendCommand(ACP300.STOP_REMOTE, "");
                    if (!answer.Status)
                    {
                        MessageBox.Show("Problemas na desconexão com a fonte ACP300.", "Erro",
                             MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                
                bluetoothClient.Dispose();
                tcpClient.Dispose();
                adr2000.Dispose();

                //
                //  Necessary for the RMM3006 case
                //
                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                standardMeter.TerminateConnection();

                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                }

                standardMeter.Dispose();
                powerSupply.Dispose();
                acpPowerSource.Dispose();
                formVoltage.Dispose();
                db.Dispose();

                timer.Stop();
                MessageBox.Show("Processo de ajuste finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {

                    adr2000.StopAll();
                    
                    //
                    //  Necessary for the RMM3006 case
                    //
                    standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                    standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                    standardMeter.TerminateConnection();

                    if ((sourceType == _5_OUTPUTS_TRANSFORMER) && !(ex is ApplicationException))
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                    }
                }
                catch (Exception)
                {
                    adr2000.Dispose();
                    standardMeter.Dispose();
                }

                if (adr2000 != null) adr2000.Dispose();
                if (bluetoothClient != null) bluetoothClient.Dispose();
                if (tcpClient != null) tcpClient.Dispose();

                if (standardMeter != null) standardMeter.Dispose();

                if (powerSupply != null) powerSupply.Dispose();
                if (acpPowerSource != null) acpPowerSource.Dispose();
                if (formVoltage != null) formVoltage.Dispose();
                db.Dispose();
            }
        }

        private void AdjustADR3000Process(string[] deviceInfo)
        {
            bool adrHasVoltageGeneration = false;

            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();
            TcpClient tcpClient = new TcpClient();
            ADR3000 adr3000 = new ADR3000(null);
            ACP300 acpPowerSource = new ACP300();
            StandardMeter standardMeter = new StandardMeter(standardType);
            PowerSupply powerSupply = new PowerSupply(null);
            frVoltage formVoltage = new frVoltage(standardMeter, standardType);
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                //
                //  Necessary for the RMM3006
                //
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(false);


                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Recordset rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    uint VendorID = Convert.ToUInt32(((dynamic)rs.Fields["VID"]).Value);
                    uint ProductID = Convert.ToUInt32(((dynamic)rs.Fields["PID"]).Value);
                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    SimpleIOClass.InitMCP2200(VendorID, ProductID);
                    if (!SimpleIOClass.IsConnected())
                    {
                        throw new ApplicationException("Falha ao conectar e configurar MCP2200 para trafo com 5 saídas.");
                    }
                    else
                    {
                        SimpleIOClass.ConfigureIoDefaultOutput(0xF0, (byte)Transformer5Out._TRANSFORMER_OFF_);
                    }

                    //  Transformer with 5 outputs
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        int[] bestVRng = standardMeter.GetBestVoltageRange(180.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                        }
                    }
                    else
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_180V_);
                    }
                    // Wait for ADR 3000 to turn on
                    Thread.Sleep(6000);
                }

                bluetoothClient.Connect(BluetoothAddress.Parse(deviceInfo[1]), BluetoothService.SerialPort);
                adr3000 = new ADR3000(bluetoothClient.GetStream());

                float[] measures = adr3000.GetMeasures();
                if (measures[ADR3000.V] < ADR3000.VOLTAGE_TH)
                {
                    adrHasVoltageGeneration = true;
                    adr3000.SetAdjustModeState(1);
                }


                if (!adrHasVoltageGeneration)
                {
                    if (sourceType == _SUPPLIER)
                    {
                        Recordset rs = new Recordset();
                        db.ConnectToDatabase();
                        db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                        string sourceIP = ((dynamic)rs.Fields["VAddr"]).Value.ToString();
                        if (rs != null) rs.Close();
                        db.CloseConnection();

                        tcpClient.Connect(sourceIP, 502);
                        powerSupply = new PowerSupply(tcpClient.GetStream());
                    }
                    else if (sourceType == _ACP300)
                    {
                        Recordset rs = new Recordset();
                        db.ConnectToDatabase();
                        db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);

                        int baudRate = Convert.ToInt32(rs.Fields["BAUD_PowerSupply"].Value);
                        int stopBits = Convert.ToInt32(rs.Fields["StopBits_PowerSupply"].Value);
                        int parity = Convert.ToInt32(rs.Fields["Parity_PowerSupply"].Value);
                        int dataSize = Convert.ToInt32(rs.Fields["DataSize_PowerSupply"].Value);
                        string port = rs.Fields["COM_PowerSupply"].Value.ToString();

                        if (rs != null) rs.Close();
                        db.CloseConnection();

                        acpPowerSource = new ACP300(port, baudRate, dataSize,
                            (Parity)parity, (StopBits)stopBits, false, false);

                        if (!acpPowerSource.InitSerial())
                        {
                            throw new IOException("Falha ao conectar com fonte de tensão ACP300");
                        }

                        Answer answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                        answer = acpPowerSource.SendCommand(ACP300.INIT_REMOTE, "");
                        if (!answer.Status)
                        {
                            throw new IOException("Falha ao inicializar fonte de tensão ACP300");
                        }
                    }
                }

                if (!AdjustADR3000(Convert.ToInt32(deviceInfo[0].Substring(4, 4)), standardMeter,
                    adr3000, powerSupply, formVoltage, db, new object[] { acpPowerSource, adrHasVoltageGeneration }))
                {
                    MessageBox.Show("Processo de ajuste interrompido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                adr3000.TurnOffI();
                adr3000.CancelTest();
                adr3000.SetEEPROMProtectionState(ADR3000.BLOCKED);

                if (!adrHasVoltageGeneration)
                {
                    if (sourceType == _SUPPLIER)
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, 60.0f, PowerSupply.Command.PutSetPoint);
                    }
                    else if (sourceType == _ACP300)
                    {
                        Answer answer = acpPowerSource.SendCommand(ACP300.SET_VOLTAGE, "103.000");
                        if (!answer.Status)
                        {
                            MessageBox.Show("Problemas no ajuste de tensão da fonte ACP300.", "Erro",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        answer = acpPowerSource.SendCommand(ACP300.STOP_REMOTE, "");
                        if (!answer.Status)
                        {
                            MessageBox.Show("Problemas na desconexão com a fonte ACP300.", "Erro",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    adr3000.TurnOffV();
                    adr3000.SetAdjustModeState(0);
                }

                bluetoothClient.Dispose();
                tcpClient.Dispose();
                adr3000.Dispose();

                //
                //  Necessary for the RMM3006 case
                //
                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                standardMeter.TerminateConnection();

                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                }

                standardMeter.Dispose();
                powerSupply.Dispose();
                acpPowerSource.Dispose();
                formVoltage.Dispose();
                db.Dispose();

                timer.Stop();
                MessageBox.Show("Processo de ajuste finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    if (adrHasVoltageGeneration) adr3000.TurnOffV();
                    adr3000.TurnOffI();
                    adr3000.CancelTest();
                    adr3000.SetEEPROMProtectionState(ADR3000.BLOCKED);
                    //
                    //  Necessary for the RMM3006 case
                    //
                    standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                    standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                    standardMeter.TerminateConnection();

                    if ((sourceType == _5_OUTPUTS_TRANSFORMER) && !(ex is ApplicationException))
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                    }
                }
                catch (Exception)
                {
                    adr3000.Dispose();
                    standardMeter.Dispose();
                }

                if (adr3000 != null) adr3000.Dispose();
                if (bluetoothClient != null) bluetoothClient.Dispose();
                if (tcpClient != null) tcpClient.Dispose();

                if (standardMeter != null) standardMeter.Dispose();

                if (powerSupply != null) powerSupply.Dispose();
                if (acpPowerSource != null) acpPowerSource.Dispose();
                if (formVoltage != null) formVoltage.Dispose();
                db.Dispose();
            }
        }

        private void AdjustADR3000LITEProcess(string[] deviceInfo)
        {            
            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();
            TcpClient tcpClient = new TcpClient();
            ADR3000 adr3000 = new ADR3000(null);           
            StandardMeter standardMeter = new StandardMeter(standardType);            
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                //
                //  Necessary for the RMM3006
                //
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(false);


                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Recordset rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
                    uint VendorID = Convert.ToUInt32(((dynamic)rs.Fields["VID"]).Value);
                    uint ProductID = Convert.ToUInt32(((dynamic)rs.Fields["PID"]).Value);
                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    SimpleIOClass.InitMCP2200(VendorID, ProductID);
                    if (!SimpleIOClass.IsConnected())
                    {
                        throw new ApplicationException("Falha ao conectar e configurar MCP2200 para trafo com 5 saídas.");
                    }
                    else
                    {
                        SimpleIOClass.ConfigureIoDefaultOutput(0xF0, (byte)Transformer5Out._TRANSFORMER_OFF_);
                        Thread.Sleep(3000);
                    }

                    //  Transformer with 5 outputs
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        int[] bestVRng = standardMeter.GetBestVoltageRange(120.0f);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_120V_);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_120V_);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_120V_);
                        }
                    }
                    else
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_120V_);
                    }
                    // Wait for ADR 3000 to turn on
                    Thread.Sleep(6000);
                }
                
                //MessageBox.Show("Ligue a fonte de 12 Vdc do ADR 3000 LITE.");
                bluetoothClient.Connect(BluetoothAddress.Parse(deviceInfo[1]), BluetoothService.SerialPort);
                adr3000 = new ADR3000(bluetoothClient.GetStream());

                adr3000.SetAdjustModeState(1);

                if (!AdjustADR3000LITE(Convert.ToInt32(deviceInfo[0].Substring(4, 4)), standardMeter, adr3000, db))
                {
                    MessageBox.Show("Processo de ajuste interrompido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                adr3000.TurnOffI();
                adr3000.TurnOffV();
                adr3000.CancelTest();
                adr3000.SetEEPROMProtectionState(ADR3000.BLOCKED);
                
                adr3000.SetAdjustModeState(0);
                

                bluetoothClient.Dispose();
                tcpClient.Dispose();
                adr3000.Dispose();

                //
                //  Necessary for the RMM3006 case
                //
                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                standardMeter.TerminateConnection();

                if (sourceType == _5_OUTPUTS_TRANSFORMER)
                {
                    Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                }

                standardMeter.Dispose();
                db.Dispose();

                timer.Stop();
                MessageBox.Show("Processo de ajuste finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    adr3000.TurnOffI();
                    adr3000.TurnOffV();
                    adr3000.CancelTest();
                    adr3000.SetEEPROMProtectionState(ADR3000.BLOCKED);
                    //
                    //  Necessary for the RMM3006 case
                    //
                    standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                    standardMeter.SetK1K2(false, true); Thread.Sleep(1000);
                    standardMeter.TerminateConnection();

                    if ((sourceType == _5_OUTPUTS_TRANSFORMER) && !(ex is ApplicationException))
                    {
                        Transformer5Out.EnableOutput(Transformer5Out._TRANSFORMER_OFF_);
                    }
                }
                catch (Exception)
                {
                    adr3000.Dispose();
                    standardMeter.Dispose();
                }

                if (adr3000 != null) adr3000.Dispose();
                if (bluetoothClient != null) bluetoothClient.Dispose();
                if (tcpClient != null) tcpClient.Dispose();
                if (standardMeter != null) standardMeter.Dispose();

                db.Dispose();
            }

        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            string argument = (string)e.Argument;
            string[] deviceInfo = argument.Split('|');

            switch (deviceInfo[0].Substring(0, 4))
            {
                case "AM2-":
                    AdjustADR2000Process(deviceInfo);
                    break;
                case "AL3-":
                    AdjustADR3000LITEProcess(deviceInfo);
                    break;
                default:
                    AdjustADR3000Process(deviceInfo);
                    break;
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer.Stop();
            time = "00:00:00";
            UpdateTimer(time);
            UpdateText("0 %");
            UpdateProgress(0);
            UpdateError(0.0f, Color.Red);
            UpdateControlsStatus(true);
        }

        private void BtInitAdjustment_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Iniciar ajuste do ADR 3000?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.No)
            {
                cbDevice.Enabled = false;
                btInitAdjustment.Enabled = false;
                btStopAdjustment.Enabled = true;

                timer.Start();
                backgroundWorker.RunWorkerAsync(cbDevice.Text);
            }
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateTimer(time = (TimeSpan.Parse(time) + new TimeSpan(0, 0, 1)).ToString());
        }

        private void BtStopAdjustment_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void FrAdjust_Load(object sender, EventArgs e)
        {/*
            BluetoothDeviceInfo[] deviceInfos = new BluetoothClient().DiscoverDevices(255, true, false, false, false);

            foreach (BluetoothDeviceInfo deviceInfo in deviceInfos)
            {
                if (deviceInfo.DeviceName.Length >= 8)
                {
                    if (deviceInfo.DeviceName.Substring(0, 4).Equals("AM3-") || deviceInfo.DeviceName.Substring(0, 4).Equals("AM2-") ||
                        deviceInfo.DeviceName.Substring(0, 4).Equals("AP3-") || deviceInfo.DeviceName.Substring(0, 4).Equals("AL3-")) 
                        cbDevice.Items.Add(deviceInfo.DeviceName + "|" + deviceInfo.DeviceAddress.ToString());
                }
            }

            if (cbDevice.Items.Count == 0)
            {
                MessageBox.Show("Não há ADRs pareados com o bluetooth local.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }

            cbDevice.Text = cbDevice.Items[0].ToString();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(Elapsed);
            timer.AutoReset = true;

            backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            backgroundWorker.WorkerSupportsCancellation = true;*/
        }

        private void FrAdjust_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker != null) backgroundWorker.Dispose();
            if (timer != null) timer.Dispose();
            Dispose();
        }
    }
}
