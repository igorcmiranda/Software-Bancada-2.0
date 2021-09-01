using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ajustador_Calibrador_ADR3000.Delegates;
using Ajustador_Calibrador_ADR3000.Helpers;
using Ajustador_Calibrador_ADR3000.Devices;
using System.Threading;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using ADODB;
using System.Net.Sockets;
using System.IO;
using System.IO.Ports;
using SimpleIO;

namespace Ajustador_Calibrador_ADR3000.Forms
{
    public partial class frCalibration : Form
    {
        private struct LPW
        {
            public LPW(float _c, int _l) {
                _constant = _c;
                _lpw = _l;
            }
            public float _constant;
            public int _lpw;
        }

        private const int ACP300_POS = 0;

        private const int _DEVICE_INFO_ = 0;
        private const int _ESSAY_NAME_ = 1;
        private const int _MODE_ = 2;
        private const int _OSOP_ = 3;
        private const int _OSOP_NUMBER_ = 4;
        private const int _CODE_ = 5;
        private const int _REPORT_ = 6;
        private const int _CONSIDER_TOL_ = 7;
        
        private const int _VARIVOLT = 0;
        private const int _SUPPLIER = 1;
        private const int _ACP300 = 2;
        private const int _5_OUTPUTS_TRANSFORMER = 3;

        private readonly int sourceType;
        private readonly string connectionString;
        private readonly string userName;
        private readonly StandardMeter.StandardType standardType;
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private readonly System.Timers.Timer timer = new System.Timers.Timer(1000);
        private string time = "00:00:00";
        public frCalibration(int _sourceType, string _connectionString, StandardMeter.StandardType _standardType, string _userName)
        {
            InitializeComponent();

            sourceType = _sourceType;
            connectionString = _connectionString;
            standardType = _standardType;
            userName = _userName;
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

        private void UpdateControlsStatus(bool status)
        {
            if (InvokeRequired) Invoke(new MyHandler(() => {
                cbDevice.Enabled = status;
                cbEssay.Enabled = status;
                listBoxPoints.Enabled = status;
                cbMode.Enabled = status;
                cbOSOP.Enabled = status;
                tbOSOPNum.Enabled = status;
                tbReport.Enabled = status;
                btInitCalibration.Enabled = status;
                //ckbConsiderTolerance.Enabled = status;
                btStopCalibration.Enabled = !status;
            }));
            else
            {
                cbDevice.Enabled = status;
                cbEssay.Enabled = status;
                listBoxPoints.Enabled = status;
                cbMode.Enabled = status;
                cbOSOP.Enabled = status;
                tbOSOPNum.Enabled = status;
                tbReport.Enabled = status;
                btInitCalibration.Enabled = status;
                //ckbConsiderTolerance.Enabled = status;
                btStopCalibration.Enabled = !status;
            }
        }

        private void UpdateTimer(string time)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { lbTimer.Text = time; }));
            else lbTimer.Text = time;
        }

        private void UpdateText(string text)
        {
            if (InvokeRequired) Invoke(new UpdateTextHandler(() => { gbProgress.Text = text; }));
            else gbProgress.Text = text;
        }

        private void UpdateProgress(int progress)
        {
            if (InvokeRequired) Invoke(new UpdateProgressHandler(() => { pbCalibration.Value = progress; }));
            else pbCalibration.Value = progress;
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

        private bool CalibrateADR3000(StandardMeter standardMeter, ADR3000 adr3000, PowerSupply powerSupply, 
            frVoltage formVoltage, MSAccess db, object _argument, object args)
        {
            object[] obj = new object[1];
            if (args != null)
            {
                obj = (object[])args;
            }

            bool adrHasVoltageGeneration = (bool)obj[1];

            bool isHighScale;

            object[] argument = (object[])_argument;

            string[] deviceInfo = ((string)argument[_DEVICE_INFO_]).Split('|');
            string essayName = (string)argument[_ESSAY_NAME_];
            bool[] codigo = (bool[])argument[_CODE_];
            string mode = (string)argument[_MODE_];
            string osop = (string)argument[_OSOP_];
            string osopNumber = (string)argument[_OSOP_NUMBER_];
            string report = (string)argument[_REPORT_];
            bool considerTolerance = (bool)argument[_CONSIDER_TOL_];

            LPW[] dividers = new LPW[]
            {
                new LPW(2048, 0),
                new LPW(1024, 1),
                new LPW(512, 2),
                new LPW(256, 3),
                new LPW(128, 4),
                new LPW(64, 5),
                new LPW(32, 6),
                new LPW(16, 7),
                new LPW(8, 8),
                new LPW(4, 9),
                new LPW(2, 10),
                new LPW(1, 11),
                new LPW(0.5f, 12),
                new LPW(0.25f, 13),
                new LPW(0.125f, 14),
                new LPW(0.0625f, 15)
            };

            bool laudoSent = false;

            int[] bestVRng = new int[] { 1, 1 };
            int[] bestIRng = new int[] { 1, 1 };
            
            // Assure that the process generates minimum number of relay comutations
            string essayCondition = "WHERE Nome = '" + essayName + "' ORDER BY Corrente, Tensao, Energia, Elementos";            
            
            int lastLaudo;

            List<string> fields = new List<string>() { "Data_", "Laudo", "Nome", "Tensao", "Corrente", "Elemento", "Energia", "PF", "erro", "dp", "Usuario", "n_amostra", "Clamp", "OSOP", "numOSOP", "Ano", "Classe" };

            if (backgroundWorker.CancellationPending) return (false);

            db.ConnectToDatabase();
            Recordset rs = new Recordset();
            db.MaxRecord("Calibracao", "Laudo", "WHERE Ano = " + DateTime.Today.Year, ref rs);
            if (rs.Fields[0].Value.Equals(DBNull.Value)) lastLaudo = 0;
            else lastLaudo = Convert.ToInt32(((dynamic)rs.Fields[0]).Value.Substring(1, 4));

            if (rs != null) rs.Close();

            string laudo;

            rs = new Recordset();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            int setup = ((dynamic)rs.Fields["Setup"]).Value;
            //
            // Configures kh
            //
            int lpw = ((dynamic)rs.Fields["LPW"]).Value;
            float khADR3000 = ((dynamic)rs.Fields["khADR3000"]).Value;
            float pulseFreq = ((dynamic)rs.Fields["pulseFreq"]).Value;

            if (report == "")
            {
                lastLaudo++;
                laudo = setup.ToString() + lastLaudo.ToString("0000") + "/" + DateTime.Today.Year;
            }
            else laudo = report;

            if (rs != null) rs.Close();

            rs = new Recordset();

            db.GetRecords("Ensaios", new List<string>() { "*" }, essayCondition, ref rs);
            essayCondition = "WHERE Nome = '" + essayName + "'";
            long numOfPoints = db.RecordsCount("Ensaios", "Nome", essayCondition);


            CalibrationPoint[] points = new CalibrationPoint[numOfPoints];
            int stepsDone = 0, totalSteps = 0;
            int i;
            for (i = 0; i < numOfPoints; i++)
            {
                points[i] = new CalibrationPoint();
                points[i].codigo = ((dynamic)rs.Fields["Código"]).Value;
                points[i].voltage = ((dynamic)rs.Fields["Tensao"]).Value;
                points[i].current = ((dynamic)rs.Fields["Corrente"]).Value;
                points[i].spv = ((dynamic)rs.Fields["SPV"]).Value;
                points[i].spi = ((dynamic)rs.Fields["SPI"]).Value;
                points[i].phi = ((dynamic)rs.Fields["PHI"]).Value;
                points[i].elements = ((dynamic)rs.Fields["Elementos"]).Value;
                points[i].energy = ((dynamic)rs.Fields["Energia"]).Value;
                points[i].n = ((dynamic)rs.Fields["n"]).Value;
                points[i].tempo = ((dynamic)rs.Fields["Tempo"]).Value;
                points[i].classe = ((dynamic)rs.Fields["Classe"]).Value;
                points[i].check = codigo[i];
                if (points[i].check) totalSteps += points[i].n;
                rs.MoveNext();
            }
            if (rs != null) rs.Close();
            db.CloseConnection();

            List<string> values = new List<string>()
            {
                "'" + DateTime.Today.ToString("dd/MM/yyyy") + "'", //0 Data 
                "'" + laudo + "'",    //1 Laudo
                "'" + deviceInfo[0] + "'",    //2 Nome ADR
                " ",    //3 Tensão
                " ",    //4 Corrente
                " ",    //5 Elemento
                " ",    //6 Energia
                " ",    //7 fator de potência
                " ",    //8 erro
                " ",    //9 desvio padrão
                "'" + userName + "'",  //10 usuário
                " ",    //11 número de testes
                "'TC 200 A'",    //12 tipo de clamp
                " ",   //13 OS ou OP
                " ", //14 Número da OS ou OP
                "'" + DateTime.Today.Year + "'", //15 Ano
                " "     //16 Classe do ponto calibrado
            };

            for (i = 0; i < numOfPoints; i++)
            {
                if (points[i].check) break;
            }

            float currentVoltage = points[i].voltage;

            if (!adrHasVoltageGeneration)
            {
                if (sourceType == _SUPPLIER)
                {
                    float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                    }
                    else
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                    }

                    Thread.Sleep(5000);
                }
                else if (sourceType == _ACP300)
                {
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                    }
                    else
                    {
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                    }

                    Thread.Sleep(5000);
                }
                else if (sourceType == _VARIVOLT)
                {
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            formVoltage.Voltage = currentVoltage;
                            formVoltage.ShowDialog();
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            formVoltage.Voltage = currentVoltage;
                            formVoltage.ShowDialog();
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            formVoltage.Voltage = currentVoltage;
                            formVoltage.ShowDialog();
                        }
                    }
                    else
                    {
                        formVoltage.Voltage = currentVoltage;
                        formVoltage.ShowDialog();
                    }
                }
                else
                {
                    uint voltageLevel = Transformer5Out.GetVoltageDiscreteLevel(currentVoltage);
                    currentVoltage = Transformer5Out.GetVoltageRealValue(voltageLevel);
                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                        int voltageVRng = bestVRng[0];
                        int zeraVRng = standardMeter.GetRangeStatus()[0];

                        if (voltageVRng > zeraVRng)
                        {
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                            Transformer5Out.EnableOutput(voltageLevel);
                        }
                        else if (voltageVRng < zeraVRng)
                        {
                            Transformer5Out.EnableOutput(voltageLevel);
                            standardMeter.SetActualVoltageRange(voltageVRng);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Transformer5Out.EnableOutput(voltageLevel);
                        }
                    }
                    else
                    {
                        Transformer5Out.EnableOutput(voltageLevel);
                    }
                }
            }
            else
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        adr3000.SetVoltageSetPoint(currentVoltage);
                        adr3000.TurnOnV();
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        adr3000.SetVoltageSetPoint(currentVoltage);
                        adr3000.TurnOnV();
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        adr3000.SetVoltageSetPoint(currentVoltage);
                        adr3000.TurnOnV();
                    }
                }
                else
                {
                    adr3000.SetVoltageSetPoint(currentVoltage);
                    adr3000.TurnOnV();
                }

                Thread.Sleep(5000);
            }

            adr3000.CancelTest();
            adr3000.SetCountingType(ADR3000.PULSES_LAPS);
            adr3000.SetNumberOfElements(ADR3000.SINGLE_PHASE);

            if (standardType == StandardMeter.StandardType.RMM3006)
            {
                int currentRng = standardMeter.GetBestCurrentRange(points[i].current)[0];
                int zeraIRng = standardMeter.GetRangeStatus()[1];

                if (currentRng != zeraIRng)
                {
                    standardMeter.SetActualCurrentRange(currentRng);
                    Thread.Sleep(2000);
                }
            }

            adr3000.SetCurrentSetPoint(points[i].current);
            //
            //  Configures the keys K1 and K2 according to the
            //  first point
            //
            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            if (points[i].current <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
            {
                standardMeter.SetK1K2(true, false);
                Thread.Sleep(4000);
                isHighScale = false;
            }
            else
            {
                standardMeter.SetK1K2(false, true);
                Thread.Sleep(4000);
                isHighScale = true;
            }

            adr3000.TurnOnI();            

            adr3000.SetPulseOutputDivider(lpw);

            Thread.Sleep(5000);

            float kdForInputTest;

            for (i = 0; i < numOfPoints; i++)
            {
                if (backgroundWorker.CancellationPending) return (false);
                if (points[i].check)
                {
                    UpdateError(0.0f, Color.Blue);
                    if (points[i].voltage != currentVoltage)
                    {
                        currentVoltage = points[i].voltage;
                        if (!adrHasVoltageGeneration)
                        {
                            if (sourceType == _SUPPLIER)
                            {
                                float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                                if (standardType == StandardMeter.StandardType.RMM3006)
                                {
                                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                    int voltageVRng = bestVRng[0];
                                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                                    if (voltageVRng > zeraVRng)
                                    {
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                        powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                    }
                                    else if (voltageVRng < zeraVRng)
                                    {
                                        powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                    }
                                    else powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                }
                                else
                                {
                                    powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                }

                                Thread.Sleep(5000);
                            }
                            else if (sourceType == _ACP300)
                            {
                                if (standardType == StandardMeter.StandardType.RMM3006)
                                {
                                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                    int voltageVRng = bestVRng[0];
                                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                                    if (voltageVRng > zeraVRng)
                                    {
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                    }
                                    else if (voltageVRng < zeraVRng)
                                    {
                                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                    }
                                    else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                }
                                else
                                {
                                    ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                }

                                Thread.Sleep(5000);
                            }
                            else if (sourceType == _VARIVOLT)
                            {
                                if (standardType == StandardMeter.StandardType.RMM3006)
                                {
                                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                    int voltageVRng = bestVRng[0];
                                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                                    if (voltageVRng > zeraVRng)
                                    {
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                        formVoltage.Voltage = currentVoltage;
                                        formVoltage.ShowDialog();
                                    }
                                    else if (voltageVRng < zeraVRng)
                                    {
                                        formVoltage.Voltage = currentVoltage;
                                        formVoltage.ShowDialog();
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        formVoltage.Voltage = currentVoltage;
                                        formVoltage.ShowDialog();
                                    }
                                }
                                else
                                {
                                    formVoltage.Voltage = currentVoltage;
                                    formVoltage.ShowDialog();
                                }
                            }
                            else
                            {
                                uint voltageLevel = Transformer5Out.GetVoltageDiscreteLevel(currentVoltage);
                                currentVoltage = Transformer5Out.GetVoltageRealValue(voltageLevel);
                                if (standardType == StandardMeter.StandardType.RMM3006)
                                {
                                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                    int voltageVRng = bestVRng[0];
                                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                                    if (voltageVRng > zeraVRng)
                                    {
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                        Transformer5Out.EnableOutput(voltageLevel);
                                    }
                                    else if (voltageVRng < zeraVRng)
                                    {
                                        Transformer5Out.EnableOutput(voltageLevel);
                                        standardMeter.SetActualVoltageRange(voltageVRng);
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        Transformer5Out.EnableOutput(voltageLevel);
                                    }
                                }
                                else
                                {
                                    Transformer5Out.EnableOutput(voltageLevel);
                                }
                            }
                        }
                        else
                        {
                            if (standardType == StandardMeter.StandardType.RMM3006)
                            {
                                bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                int voltageVRng = bestVRng[0];
                                int zeraVRng = standardMeter.GetRangeStatus()[0];

                                if (voltageVRng > zeraVRng)
                                {
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                    adr3000.SetVoltageSetPoint(currentVoltage);
                                }
                                else if (voltageVRng < zeraVRng)
                                {
                                    adr3000.SetVoltageSetPoint(currentVoltage);
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    adr3000.SetVoltageSetPoint(currentVoltage);
                                }
                            }
                            else
                            {
                                adr3000.SetVoltageSetPoint(currentVoltage);
                            }

                            Thread.Sleep(5000);
                        }
                    }


                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestIRng = standardMeter.GetBestCurrentRange(points[i].current);
                        int currentRng = bestIRng[0];
                        int zeraIRng = standardMeter.GetRangeStatus()[1];
                        bool isCurrentOff = false;

                        if (points[i].current <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
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
                        else
                        {
                            if (!isHighScale)
                            {
                                adr3000.TurnOffI();
                                isCurrentOff = true;
                                standardMeter.SetK1K2(false, false); Thread.Sleep(1000);
                                standardMeter.SetK1K2(false, true); Thread.Sleep(4000);
                                isHighScale = true;
                            }
                        }

                        if (currentRng != zeraIRng)
                        {
                            adr3000.TurnOffI();
                            standardMeter.SetActualCurrentRange(currentRng);
                            Thread.Sleep(2000);
                            adr3000.SetCurrentSetPoint(points[i].current);
                            adr3000.TurnOnI();
                            //
                            //  If the RMM3006 changes current scale waits 5 s
                            //
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            if (i > 0)
                            {
                                if ((points[i - 1].current <= 4.0f && points[i].current > 4.0f) ||
                                    (points[i - 1].current > 4.0f && points[i].current <= 4.0f))
                                {
                                    adr3000.TurnOffI();
                                    isCurrentOff = true;
                                    Thread.Sleep(5000);
                                }
                            }
                            adr3000.SetCurrentSetPoint(points[i].current);
                            if (isCurrentOff)
                            {
                                adr3000.TurnOnI();
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    else
                    {
                        bool isCurrentOff = false;
                        if (i > 0)
                        {
                            if ((points[i - 1].current <= 4.0f && points[i].current > 4.0f) ||
                                (points[i - 1].current > 4.0f && points[i].current <= 4.0f))
                            {
                                adr3000.TurnOffI();
                                isCurrentOff = true;
                                Thread.Sleep(5000);
                            }
                        }
                        adr3000.SetCurrentSetPoint(points[i].current);
                        if (isCurrentOff)
                        {
                            adr3000.TurnOnI();
                            Thread.Sleep(5000);
                        }
                    }

                    adr3000.SetPhiSetPoint(points[i].phi);

                    Thread.Sleep(10000);

                    float powerFactor, tolerance;
                    StandardMeter.EnergyType stdEnergyType;
                    byte adrEnergyType;
                    byte adrOutputPulseEnergyType;

                    if (points[i].energy == "Ativa")
                    {
                        adrEnergyType = ADR3000.ACTIVE_ENERGY;
                        adrOutputPulseEnergyType = ADR3000.TYPE_ACTIVE;
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Cos(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.ACTIVE;
                        values[6] = "'A'";
                    }
                    else
                    {
                        
                        adrEnergyType = ADR3000.REACTIVE_ENERGY;
                        adrOutputPulseEnergyType = ADR3000.TYPE_REACTIVE;
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Sin(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.REACTIVE;
                        values[6] = "'R'";
                    }

                    adr3000.SetEnergyType(adrEnergyType);
                    adr3000.SetPulseOutputEnergyType(adrOutputPulseEnergyType);

                    standardMeter.SetPulseEnergyType(stdEnergyType);

                    string spf;
                    if ((points[i].phi > 0) && (points[i].phi <= 180)) spf = "ind";
                    else if (points[i].phi < 0) spf = "cap";
                    else spf = "";

                    tolerance = points[i].classe / powerFactor;

                    float[] error = new float[points[i].n];
                    float avgError = 0.0f;

                    // teste com RD-20
                    int khStd;// = 720;
                    //kdForInputTest = 0.1f;
                    if (points[i].current < 1.1f)
                    {
                        khStd = 7200;
                        kdForInputTest = 0.01f;
                    }
                    else if ((points[i].current >= 1.1f) && (points[i].current < 10.5f))
                    {
                        khStd = 720;
                        kdForInputTest = 0.1f;
                    }
                    else
                    {
                        khStd = 72;
                        kdForInputTest = 1.0f;
                    }

                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        khStd = 750 * bestVRng[1] * bestIRng[1];
                    }

                    adr3000.SetKd(kdForInputTest);
                    standardMeter.SetKd(stdEnergyType, kdForInputTest, khStd);


                    float totalPower = points[i].elements.Length * currentVoltage * points[i].current * powerFactor;

                    float pulseConstant = pulseFreq * 3600.0f / totalPower;
                    LPW result = SmallerDifference(dividers, pulseConstant);

                    adr3000.SetPulseOutputDivider(result._lpw);
                    khADR3000 = result._constant;


                    string msg = points[i].voltage + " V/" + points[i].current + " A/" + points[i].phi + "° ";
                    int testCount = 0;
                    int j;
                    if (mode.Equals("Saída de pulsos"))
                    {
                        //
                        //  Dummy calibration to discard the first test
                        //
                        standardMeter.ExecuteCalibration(stdEnergyType, totalPower,
                            points[i].tempo, khADR3000, points[i].classe, false);
                        Thread.Sleep(1000);

                        for (j = 0; j < points[i].n; j++)
                        {
                            stepsDone++;
                            testCount++;
                            string msgN = testCount + " de " + points[i].n;
                            int progress = stepsDone * 100 / totalSteps;
                            UpdateText( progress + " % - " + msg + msgN);
                            UpdateProgress(progress);

                            if (backgroundWorker.CancellationPending) return (false);
                            //
                            // ExecuteCalibration blocks until end of test
                            //
                            error[j] = standardMeter.ExecuteCalibration(stdEnergyType, totalPower, 
                                points[i].tempo, khADR3000, points[i].classe, considerTolerance);
                            avgError += error[j];

                            Color color;
                            if (Math.Abs(error[j]) > tolerance) color = Color.Red;
                            else color = Color.Green;

                            UpdateError(error[j], color);

                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        int numOfTurns = Convert.ToInt32(totalPower * points[i].tempo / (3600 * kdForInputTest));
                        adr3000.SetNumberOfTurns(numOfTurns);
                        
                        if (standardType == StandardMeter.StandardType.RMM3006)
                        {
                            //
                            //  Dummy calibration to discard the first test
                            //
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
                                
                            }
                        }

                        for (j = 0; j < points[i].n; j++)
                        {
                            stepsDone++;
                            testCount++;
                            string msgN = testCount + " de " + points[i].n;
                            int progress = stepsDone * 100 / totalSteps;
                            UpdateText(progress + " % - " + msg + msgN);
                            UpdateProgress(progress);

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
                                error[j] = adr3000.MyAccuracyTestResult.Error;
                                avgError += error[j];

                                Color color;
                                if (Math.Abs(error[j]) > tolerance) color = Color.Red;
                                else color = Color.Green;

                                UpdateError(error[j], color);
                            }
                        }
                    }

                    avgError /= points[i].n;

                    float sigma = 0.0f;
                    for (j = 0; j < points[i].n; j++) sigma += Convert.ToSingle(Math.Pow(error[j] - avgError, 2));
                    if (points[i].n > 1) sigma /= (points[i].n - 1);
                    sigma = Convert.ToSingle(Math.Sqrt(sigma));

                    values[3] = "'" + points[i].voltage + "'";
                    values[4] = "'" + points[i].current + "'";
                    values[5] = "'" + points[i].elements + "'";
                    values[7] = "'" + powerFactor.ToString("0.0") + " " + spf + "'";
                    values[8] = "'" + avgError + "'";
                    values[9] = "'" + sigma + "'";
                    values[11] = "'" + points[i].n + "'";
                    values[13] = "'" + osop + "'";
                    values[14] = "'" + osopNumber + "'";
                    values[16] = "'" + points[i].classe + "'";

                    string cond = "WHERE (Laudo = " + values[1] + " AND Nome = " + values[2] + " AND Tensao = " + points[i].voltage + " AND Corrente = " + points[i].current + " AND Elemento = " + values[5] + " AND Energia = " + values[6] +
                    " AND PF = " + values[7] + " AND Clamp = " + values[12] + ")";

                    rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Calibracao", new List<string>() { "*" }, cond, ref rs);

                    if (rs.BOF && rs.EOF) db.InsertData("Calibracao", fields, values);
                    else db.UpdateData("Calibracao", new List<string>() { "erro", "dp" }, new List<string>() { avgError.ToString(), sigma.ToString() }, cond);

                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    if (!laudoSent)
                    {
                        adr3000.SetCalibrationReport(laudo);
                        adr3000.SetCalibrationDate(adr3000.GetDate(DateTime.Today.Day, DateTime.Today.Month, DateTime.Today.Year));
                        laudoSent = true;
                    }

                    Thread.Sleep(1000);
                }
            }

           
            return (true);
        }


        private bool CalibrateADR3000LITE(StandardMeter standardMeter, ADR3000 adr3000, MSAccess db, object _argument)
        {           
            bool isHighScale;

            object[] argument = (object[])_argument;

            string[] deviceInfo = ((string)argument[_DEVICE_INFO_]).Split('|');
            string essayName = (string)argument[_ESSAY_NAME_];
            bool[] codigo = (bool[])argument[_CODE_];
            string mode = (string)argument[_MODE_];
            string osop = (string)argument[_OSOP_];
            string osopNumber = (string)argument[_OSOP_NUMBER_];
            string report = (string)argument[_REPORT_];
            bool considerTolerance = (bool)argument[_CONSIDER_TOL_];

            LPW[] dividers = new LPW[]
            {
                new LPW(4096, 0),
                new LPW(2048, 1),
                new LPW(1024, 2),
                new LPW(512, 3),
                new LPW(256, 4),
                new LPW(128, 5),
                new LPW(64, 6),
                new LPW(32, 7),
                new LPW(16, 8),
                new LPW(8, 9),
                new LPW(4, 10),
                new LPW(2, 11),
                new LPW(1.0f, 12),
                new LPW(0.5f, 13),
                new LPW(0.250f, 14),
                new LPW(0.125f, 15)
            };

            bool laudoSent = false;

            int[] bestVRng = new int[] { 1, 1 };
            int[] bestIRng = new int[] { 1, 1 };

            // Assure that the process generates minimum number of relay comutations
            string essayCondition = "WHERE Nome = '" + essayName + "' ORDER BY Tensao, Corrente, Energia, Elementos";

            int lastLaudo;

            List<string> fields = new List<string>() { "Data_", "Laudo", "Nome", "Tensao", "Corrente", "Elemento", "Energia", "PF", "erro", "dp", "Usuario", "n_amostra", "Clamp", "OSOP", "numOSOP", "Ano", "Classe" };

            if (backgroundWorker.CancellationPending) return false;

            db.ConnectToDatabase();
            Recordset rs = new Recordset();
            db.MaxRecord("Calibracao", "Laudo", "WHERE Ano = " + DateTime.Today.Year, ref rs);
            if (rs.Fields[0].Value.Equals(DBNull.Value)) lastLaudo = 0;
            else lastLaudo = Convert.ToInt32(((dynamic)rs.Fields[0]).Value.Substring(1, 4));

            if (rs != null) rs.Close();

            string laudo;

            rs = new Recordset();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            int setup = ((dynamic)rs.Fields["Setup"]).Value;
            //
            // Configures kh
            //
            int lpw = ((dynamic)rs.Fields["LPW"]).Value;
            float khADR3000;
            float pulseFreq = ((dynamic)rs.Fields["pulseFreq"]).Value;

            if (report == "")
            {
                lastLaudo++;
                laudo = setup.ToString() + lastLaudo.ToString("0000") + "/" + DateTime.Today.Year;
            }
            else laudo = report;

            if (rs != null) rs.Close();

            rs = new Recordset();

            db.GetRecords("Ensaios", new List<string>() { "*" }, essayCondition, ref rs);
            essayCondition = "WHERE Nome = '" + essayName + "'";
            long numOfPoints = db.RecordsCount("Ensaios", "Nome", essayCondition);


            CalibrationPoint[] points = new CalibrationPoint[numOfPoints];
            int stepsDone = 0, totalSteps = 0;
            int i;
            for (i = 0; i < numOfPoints; i++)
            {
                points[i] = new CalibrationPoint();
                points[i].codigo = ((dynamic)rs.Fields["Código"]).Value;
                points[i].voltage = ((dynamic)rs.Fields["Tensao"]).Value;
                points[i].current = ((dynamic)rs.Fields["Corrente"]).Value;
                points[i].spv = ((dynamic)rs.Fields["SPV"]).Value;
                points[i].spi = ((dynamic)rs.Fields["SPI"]).Value;
                points[i].phi = ((dynamic)rs.Fields["PHI"]).Value;
                points[i].elements = ((dynamic)rs.Fields["Elementos"]).Value;
                points[i].energy = ((dynamic)rs.Fields["Energia"]).Value;
                points[i].n = ((dynamic)rs.Fields["n"]).Value;
                points[i].tempo = ((dynamic)rs.Fields["Tempo"]).Value;
                points[i].classe = ((dynamic)rs.Fields["Classe"]).Value;
                points[i].check = codigo[i];
                if (points[i].check) totalSteps += points[i].n;
                rs.MoveNext();
            }
            if (rs != null) rs.Close();
            db.CloseConnection();

            List<string> values = new List<string>()
            {
                "'" + DateTime.Today.ToString("dd/MM/yyyy") + "'", //0 Data 
                "'" + laudo + "'",    //1 Laudo
                "'" + deviceInfo[0] + "'",    //2 Nome ADR
                " ",    //3 Tensão
                " ",    //4 Corrente
                " ",    //5 Elemento
                " ",    //6 Energia
                " ",    //7 fator de potência
                " ",    //8 erro
                " ",    //9 desvio padrão
                "'" + userName + "'",  //10 usuário
                " ",    //11 número de testes
                "'TC 200 A'",    //12 tipo de clamp
                " ",   //13 OS ou OP
                " ", //14 Número da OS ou OP
                "'" + DateTime.Today.Year + "'", //15 Ano
                " "     //16 Classe do ponto calibrado
            };

            for (i = 0; i < numOfPoints; i++)
            {
                if (points[i].check) break;
            }

            float currentVoltage = 0.0f;
            float currentCurrent = 0.0f;

            adr3000.CancelTest();
            adr3000.SetCountingType(ADR3000.PULSES_LAPS);
            adr3000.SetNumberOfElements(ADR3000.SINGLE_PHASE);

            adr3000.SetPulseOutputDivider(lpw);

            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            standardMeter.SetK1K2(true, false);
            Thread.Sleep(4000);
            isHighScale = false;
            

            float kdForInputTest;

            for (i = 0; i < numOfPoints; i++)
            {
                if (backgroundWorker.CancellationPending) return (false);
                if (points[i].check)
                {
                    bool turnedOffI = false;
                    UpdateError(0.0f, Color.Blue);
                    if (points[i].voltage != currentVoltage)
                    {
                        currentVoltage = points[i].voltage;
                        adr3000.TurnOffI();
                        turnedOffI = true;
                        adr3000.TurnOffV();
                        if (standardType == StandardMeter.StandardType.RMM3006)
                        {
                            bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                            int voltageVRng = bestVRng[0];
                            int zeraVRng = standardMeter.GetRangeStatus()[0];

                            if (voltageVRng > zeraVRng)
                            {
                                standardMeter.SetActualVoltageRange(voltageVRng);                                
                                adr3000.SetVoltageSetPoint(currentVoltage);
                            }
                            else if (voltageVRng < zeraVRng)
                            {
                                adr3000.SetVoltageSetPoint(currentVoltage);
                                standardMeter.SetActualVoltageRange(voltageVRng);
                            }
                            else
                            {
                                adr3000.SetVoltageSetPoint(currentVoltage);
                            }
                        }
                        else
                        {
                            adr3000.SetVoltageSetPoint(currentVoltage);
                        }

                        adr3000.TurnOnV();
                        Thread.Sleep(5000);
                    }

                    if (points[i].current != currentCurrent)
                    {
                        currentCurrent = points[i].current;
                        adr3000.TurnOffI();
                        adr3000.SetCurrentSetPoint(points[i].current);
                        turnedOffI = true;
                        if (standardType == StandardMeter.StandardType.RMM3006)
                        {
                            bestIRng = standardMeter.GetBestCurrentRange(points[i].current);
                            int currentRng = bestIRng[0];
                            int zeraIRng = standardMeter.GetRangeStatus()[1];                        

                            if (points[i].current <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
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
                                Thread.Sleep(2000);                                                        
                            }
                        }
                    }

                    if (turnedOffI) adr3000.TurnOnI();

                    Thread.Sleep(5000);

                    float powerFactor, tolerance;
                    StandardMeter.EnergyType stdEnergyType;
                    byte adrEnergyType;
                    byte adrOutputPulseEnergyType;

                    if (points[i].energy == "Ativa")
                    {
                        adrEnergyType = ADR3000.ACTIVE_ENERGY;
                        adrOutputPulseEnergyType = ADR3000.TYPE_ACTIVE;
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Cos(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.ACTIVE;
                        values[6] = "'A'";
                    }
                    else
                    {

                        adrEnergyType = ADR3000.REACTIVE_ENERGY;
                        adrOutputPulseEnergyType = ADR3000.TYPE_REACTIVE;
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Sin(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.REACTIVE;
                        values[6] = "'R'";
                    }

                    adr3000.SetEnergyType(adrEnergyType);
                    adr3000.SetPulseOutputEnergyType(adrOutputPulseEnergyType);

                    standardMeter.SetPulseEnergyType(stdEnergyType);

                    string spf;
                    if ((points[i].phi > 0) && (points[i].phi <= 180)) spf = "ind";
                    else if (points[i].phi < 0) spf = "cap";
                    else spf = "";

                    tolerance = points[i].classe / powerFactor;

                    float[] error = new float[points[i].n];
                    float avgError = 0.0f;

                    // teste com RD-20
                    int khStd;// = 720;
                    //kdForInputTest = 0.1f;
                    if (points[i].current < 1.1f)
                    {
                        khStd = 7200;
                        kdForInputTest = 0.01f;
                    }
                    else if ((points[i].current >= 1.1f) && (points[i].current < 10.5f))
                    {
                        khStd = 720;
                        kdForInputTest = 0.1f;
                    }
                    else
                    {
                        khStd = 72;
                        kdForInputTest = 1.0f;
                    }

                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        khStd = 750 * bestVRng[1] * bestIRng[1];
                    }

                    adr3000.SetKd(kdForInputTest);
                    standardMeter.SetKd(stdEnergyType, kdForInputTest, khStd);


                    float totalPower =  currentVoltage * points[i].current * powerFactor * points[i].elements.Length / 2;

                    float pulseConstant = pulseFreq * 3600.0f / totalPower;
                    LPW result = SmallerDifference(dividers, pulseConstant);

                    adr3000.SetPulseOutputDivider(result._lpw);
                    khADR3000 = result._constant;


                    string msg = points[i].voltage + " V/" + points[i].current + " A/" + points[i].phi + "° ";
                    int testCount = 0;
                    int j;
                    if (mode.Equals("Saída de pulsos"))
                    {
                        //
                        //  Dummy calibration to discard the first test
                        //
                        standardMeter.ExecuteCalibration(stdEnergyType, totalPower,
                            points[i].tempo, khADR3000, points[i].classe, false);
                        Thread.Sleep(1000);

                        for (j = 0; j < points[i].n; j++)
                        {
                            stepsDone++;
                            testCount++;
                            string msgN = testCount + " de " + points[i].n;
                            int progress = stepsDone * 100 / totalSteps;
                            UpdateText(progress + " % - " + msg + msgN);
                            UpdateProgress(progress);

                            if (backgroundWorker.CancellationPending) return false;
                            //
                            // ExecuteCalibration blocks until end of test
                            //
                            error[j] = standardMeter.ExecuteCalibration(stdEnergyType, totalPower,
                                points[i].tempo, khADR3000, points[i].classe, considerTolerance);
                            avgError += error[j];

                            Color color;
                            if (Math.Abs(error[j]) > tolerance) color = Color.Red;
                            else color = Color.Green;

                            UpdateError(error[j], color);

                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        int numOfTurns = Convert.ToInt32(totalPower * points[i].tempo / (3600 * kdForInputTest));
                        adr3000.SetNumberOfTurns(numOfTurns);

                        if (standardType == StandardMeter.StandardType.RMM3006)
                        {
                            //
                            //  Dummy calibration to discard the first test
                            //
                            adr3000.StartAccuracyTest();
                            try
                            {
                                while (true)
                                {
                                    adr3000.GetMeasures();
                                    if (backgroundWorker.CancellationPending) return false;
                                }
                            }
                            catch (ApplicationException)
                            {

                            }
                        }

                        for (j = 0; j < points[i].n; j++)
                        {
                            stepsDone++;
                            testCount++;
                            string msgN = testCount + " de " + points[i].n;
                            int progress = stepsDone * 100 / totalSteps;
                            UpdateText(progress + " % - " + msg + msgN);
                            UpdateProgress(progress);

                            adr3000.StartAccuracyTest();
                            try
                            {
                                while (true)
                                {
                                    adr3000.GetMeasures();
                                    if (backgroundWorker.CancellationPending) return false;
                                }
                            }
                            catch (ApplicationException)
                            {
                                error[j] = adr3000.MyAccuracyTestResult.Error;
                                avgError += error[j];

                                Color color;
                                if (Math.Abs(error[j]) > tolerance) color = Color.Red;
                                else color = Color.Green;

                                UpdateError(error[j], color);
                            }
                        }
                    }

                    avgError /= points[i].n;

                    float sigma = 0.0f;
                    for (j = 0; j < points[i].n; j++) sigma += Convert.ToSingle(Math.Pow(error[j] - avgError, 2));
                    if (points[i].n > 1) sigma /= (points[i].n - 1);
                    sigma = Convert.ToSingle(Math.Sqrt(sigma));

                    values[3] = "'" + points[i].voltage + "'";
                    values[4] = "'" + points[i].current + "'";
                    values[5] = "'" + points[i].elements + "'";
                    values[7] = "'" + powerFactor.ToString("0.0") + " " + spf + "'";
                    values[8] = "'" + avgError + "'";
                    values[9] = "'" + sigma + "'";
                    values[11] = "'" + points[i].n + "'";
                    values[13] = "'" + osop + "'";
                    values[14] = "'" + osopNumber + "'";
                    values[16] = "'" + points[i].classe + "'";

                    string cond = "WHERE (Laudo = " + values[1] + " AND Nome = " + values[2] + " AND Tensao = " + points[i].voltage + " AND Corrente = " + points[i].current + " AND Elemento = " + values[5] + " AND Energia = " + values[6] +
                    " AND PF = " + values[7] + " AND Clamp = " + values[12] + ")";

                    rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Calibracao", new List<string>() { "*" }, cond, ref rs);

                    if (rs.BOF && rs.EOF) db.InsertData("Calibracao", fields, values);
                    else db.UpdateData("Calibracao", new List<string>() { "erro", "dp" }, new List<string>() { avgError.ToString(), sigma.ToString() }, cond);

                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    if (!laudoSent)
                    {
                        adr3000.SetCalibrationReport(laudo);
                        adr3000.SetCalibrationDate(adr3000.GetDate(DateTime.Today.Day, DateTime.Today.Month, DateTime.Today.Year));
                        laudoSent = true;
                    }

                    Thread.Sleep(1000);
                }
            }


            return true;
        }

        private bool CalibrateADR2000(StandardMeter standardMeter, ADR2000 adr2000, PowerSupply powerSupply,
            frVoltage formVoltage, MSAccess db, object _argument, object args)
        {
            object[] obj = new object[1];
            if (args != null)
            {
                obj = (object[])args;
            }

            bool isHighScale;

            object[] argument = (object[])_argument;

            string[] deviceInfo = ((string)argument[_DEVICE_INFO_]).Split('|');
            string essayName = (string)argument[_ESSAY_NAME_];
            bool[] codigo = (bool[])argument[_CODE_];
            string mode = (string)argument[_MODE_];
            string osop = (string)argument[_OSOP_];
            string osopNumber = (string)argument[_OSOP_NUMBER_];
            string report = (string)argument[_REPORT_];
            bool considerTolerance = (bool)argument[_CONSIDER_TOL_];
            

            bool laudoSent = false;

            int[] bestVRng = new int[] { 1, 1 };
            int[] bestIRng = new int[] { 1, 1 };

            string essayCondition = "WHERE Nome = '" + essayName + "' ORDER BY Corrente, Tensao, Energia, Elementos";

            int lastLaudo;

            List<string> fields = new List<string>() { "Data_", "Laudo", "Nome", "Tensao", "Corrente", "Elemento", "Energia", "PF", "erro", "dp", "Usuario", "n_amostra", "Clamp", "OSOP", "numOSOP", "Ano", "Classe" };

            if (backgroundWorker.CancellationPending) return (false);

            db.ConnectToDatabase();
            Recordset rs = new Recordset();
            db.MaxRecord("Calibracao", "Laudo", "WHERE Ano = " + DateTime.Today.Year, ref rs);
            if (rs.Fields[0].Value.Equals(DBNull.Value)) lastLaudo = 0;
            else lastLaudo = Convert.ToInt32(((dynamic)rs.Fields[0]).Value.Substring(1, 4));

            if (rs != null) rs.Close();

            string laudo;

            rs = new Recordset();
            db.GetRecords("Parametros", new List<string>() { "*" }, "", ref rs);
            int setup = ((dynamic)rs.Fields["Setup"]).Value;
            //
            // Configures kh
            //
            
            float khADR2000 = ((dynamic)rs.Fields["khADR2000"]).Value;
            

            if (report == "")
            {
                lastLaudo++;
                laudo = setup.ToString() + lastLaudo.ToString("0000") + "/" + DateTime.Today.Year;
            }
            else laudo = report;

            if (rs != null) rs.Close();

            rs = new Recordset();

            db.GetRecords("Ensaios", new List<string>() { "*" }, essayCondition, ref rs);
            essayCondition = "WHERE Nome = '" + essayName + "'";
            long numOfPoints = db.RecordsCount("Ensaios", "Nome", essayCondition);

            float[] kd = new float[numOfPoints];
            CalibrationPoint[] points = new CalibrationPoint[numOfPoints];
            int stepsDone = 0, totalSteps = 0;
            int i;
            for (i = 0; i < numOfPoints; i++)
            {
                points[i] = new CalibrationPoint();
                points[i].codigo = ((dynamic)rs.Fields["Código"]).Value;
                points[i].voltage = ((dynamic)rs.Fields["Tensao"]).Value;
                points[i].current = ((dynamic)rs.Fields["Corrente"]).Value;
                points[i].spv = ((dynamic)rs.Fields["SPV"]).Value;
                points[i].spi = ((dynamic)rs.Fields["SPI"]).Value;          /*!< For the ADR 2000 this value is the KD used for the test. This is because
                                                                             * the power used by the ADR is a function of the configured KD. */
                kd[i] = points[i].spi;
                points[i].phi = ((dynamic)rs.Fields["PHI"]).Value;
                points[i].elements = ((dynamic)rs.Fields["Elementos"]).Value;
                points[i].energy = ((dynamic)rs.Fields["Energia"]).Value;
                points[i].n = ((dynamic)rs.Fields["n"]).Value;
                points[i].tempo = ((dynamic)rs.Fields["Tempo"]).Value;
                points[i].classe = ((dynamic)rs.Fields["Classe"]).Value;
                points[i].check = codigo[i];
                if (points[i].check) totalSteps += points[i].n;
                rs.MoveNext();
            }
            if (rs != null) rs.Close();
            db.CloseConnection();

            List<string> values = new List<string>()
            {
                "'" + DateTime.Today.ToString("dd/MM/yyyy") + "'", //0 Data 
                "'" + laudo + "'",    //1 Laudo
                "'" + deviceInfo[0] + "'",    //2 Nome ADR
                " ",    //3 Tensão
                " ",    //4 Corrente
                " ",    //5 Elemento
                " ",    //6 Energia
                " ",    //7 fator de potência
                " ",    //8 erro
                " ",    //9 desvio padrão
                "'" + userName + "'",  //10 usuário
                " ",    //11 número de testes
                "'TC 200 A'",    //12 tipo de clamp
                " ",   //13 OS ou OP
                " ", //14 Número da OS ou OP
                "'" + DateTime.Today.Year + "'", //15 Ano
                " "     //16 Classe do ponto calibrado
            };

            for (i = 0; i < numOfPoints; i++)
            {
                if (points[i].check) break;
            }

            float currentVoltage = points[i].voltage;

            if (sourceType == _SUPPLIER)
            {
                float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                }
                else
                {
                    powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                }

                Thread.Sleep(5000);
            }
            else if (sourceType == _ACP300)
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                }
                else
                {
                    ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                }

                Thread.Sleep(5000);
            }
            else if (sourceType == _VARIVOLT)
            {
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        formVoltage.Voltage = currentVoltage;
                        formVoltage.ShowDialog();
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        formVoltage.Voltage = currentVoltage;
                        formVoltage.ShowDialog();
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        formVoltage.Voltage = currentVoltage;
                        formVoltage.ShowDialog();
                    }
                }
                else
                {
                    formVoltage.Voltage = currentVoltage;
                    formVoltage.ShowDialog();
                }
            }
            else
            {
                uint voltageLevel = Transformer5Out.GetVoltageDiscreteLevel(currentVoltage);
                currentVoltage = Transformer5Out.GetVoltageRealValue(voltageLevel);
                if (standardType == StandardMeter.StandardType.RMM3006)
                {
                    bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                    int voltageVRng = bestVRng[0];
                    int zeraVRng = standardMeter.GetRangeStatus()[0];

                    if (voltageVRng > zeraVRng)
                    {
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                        Transformer5Out.EnableOutput(voltageLevel);
                    }
                    else if (voltageVRng < zeraVRng)
                    {
                        Transformer5Out.EnableOutput(voltageLevel);
                        standardMeter.SetActualVoltageRange(voltageVRng);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Transformer5Out.EnableOutput(voltageLevel);
                    }
                }
                else
                {
                    Transformer5Out.EnableOutput(voltageLevel);
                }
            }

            adr2000.StopAll();

            if (standardType == StandardMeter.StandardType.RMM3006)
            {
                int currentRng = standardMeter.GetBestCurrentRange(points[i].current)[0];
                int zeraIRng = standardMeter.GetRangeStatus()[1];

                if (currentRng != zeraIRng)
                {
                    standardMeter.SetActualCurrentRange(currentRng);
                    Thread.Sleep(2000);
                }
            }
            
            //
            //  Configures the keys K1 and K2 according to the
            //  first point
            //
            standardMeter.SetK1K2(false, false);
            Thread.Sleep(1000);
            if (points[i].current <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
            {
                standardMeter.SetK1K2(true, false);
                Thread.Sleep(4000);
                isHighScale = false;
            }
            else
            {
                standardMeter.SetK1K2(false, true);
                Thread.Sleep(4000);
                isHighScale = true;
            }            

            for (i = 0; i < numOfPoints; i++)
            {
                if (backgroundWorker.CancellationPending) return (false);
                if (points[i].check)
                {
                    UpdateError(0.0f, Color.Blue);
                    if (points[i].voltage != currentVoltage)
                    {
                        currentVoltage = points[i].voltage;
                        if (sourceType == _SUPPLIER)
                        {
                            float sqrt3 = Convert.ToSingle(Math.Sqrt(3));

                            if (standardType == StandardMeter.StandardType.RMM3006)
                            {
                                bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                int voltageVRng = bestVRng[0];
                                int zeraVRng = standardMeter.GetRangeStatus()[0];

                                if (voltageVRng > zeraVRng)
                                {
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                    powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                }
                                else if (voltageVRng < zeraVRng)
                                {
                                    powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                }
                                else powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                            }
                            else
                            {
                                powerSupply.SendCommand(PowerSupply.Phase.All, currentVoltage / sqrt3, PowerSupply.Command.PutSetPoint);
                            }

                            Thread.Sleep(5000);
                        }
                        else if (sourceType == _ACP300)
                        {
                            if (standardType == StandardMeter.StandardType.RMM3006)
                            {
                                bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                int voltageVRng = bestVRng[0];
                                int zeraVRng = standardMeter.GetRangeStatus()[0];

                                if (voltageVRng > zeraVRng)
                                {
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                    ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                }
                                else if (voltageVRng < zeraVRng)
                                {
                                    ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                }
                                else ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                            }
                            else
                            {
                                ((ACP300)obj[ACP300_POS]).SendCommand(ACP300.SET_VOLTAGE, currentVoltage.ToString("0.000"));
                            }

                            Thread.Sleep(5000);
                        }
                        else if (sourceType == _VARIVOLT)
                        {
                            if (standardType == StandardMeter.StandardType.RMM3006)
                            {
                                bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                int voltageVRng = bestVRng[0];
                                int zeraVRng = standardMeter.GetRangeStatus()[0];

                                if (voltageVRng > zeraVRng)
                                {
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                    formVoltage.Voltage = currentVoltage;
                                    formVoltage.ShowDialog();
                                }
                                else if (voltageVRng < zeraVRng)
                                {
                                    formVoltage.Voltage = currentVoltage;
                                    formVoltage.ShowDialog();
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    formVoltage.Voltage = currentVoltage;
                                    formVoltage.ShowDialog();
                                }
                            }
                            else
                            {
                                formVoltage.Voltage = currentVoltage;
                                formVoltage.ShowDialog();
                            }
                        }
                        else
                        {
                            uint voltageLevel = Transformer5Out.GetVoltageDiscreteLevel(currentVoltage);
                            currentVoltage = Transformer5Out.GetVoltageRealValue(voltageLevel);
                            if (standardType == StandardMeter.StandardType.RMM3006)
                            {
                                bestVRng = standardMeter.GetBestVoltageRange(currentVoltage);
                                int voltageVRng = bestVRng[0];
                                int zeraVRng = standardMeter.GetRangeStatus()[0];

                                if (voltageVRng > zeraVRng)
                                {
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                    Transformer5Out.EnableOutput(voltageLevel);
                                }
                                else if (voltageVRng < zeraVRng)
                                {
                                    Transformer5Out.EnableOutput(voltageLevel);
                                    standardMeter.SetActualVoltageRange(voltageVRng);
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    Transformer5Out.EnableOutput(voltageLevel);
                                }
                            }
                            else
                            {
                                Transformer5Out.EnableOutput(voltageLevel);
                            }
                        }
                    }


                    if (standardType == StandardMeter.StandardType.RMM3006)
                    {
                        bestIRng = standardMeter.GetBestCurrentRange(points[i].current);
                        int currentRng = bestIRng[0];
                        int zeraIRng = standardMeter.GetRangeStatus()[1];
                        

                        if (points[i].current <= StandardMeter.LOW_TO_HIGH_THRESHOLD)
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
                            //
                            //  If the RMM3006 changes current scale waits 5 s
                            //
                            Thread.Sleep(5000);
                        }                        
                    }

                    adr2000.ConfigureAccuracyTest(kd[i], 10, points[i].voltage > ADR2000.VOLTAGE_TH && kd[i] > 5 ? true : false);
                    adr2000.EnableAccuracyTest();

                    Thread.Sleep(10000);

                    float powerFactor, tolerance;
                    StandardMeter.EnergyType stdEnergyType;
                    

                    if (points[i].energy == "Ativa")
                    {                        
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Cos(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.ACTIVE;
                        values[6] = "'A'";
                    }
                    else
                    {                        
                        powerFactor = Convert.ToSingle(Math.Round(Math.Abs(Math.Sin(points[i].phi * Math.PI / 180) * 1000)) / 1000);
                        stdEnergyType = StandardMeter.EnergyType.REACTIVE;
                        values[6] = "'R'";
                    }

                    standardMeter.SetPulseEnergyType(stdEnergyType);

                    string spf;
                    if ((points[i].phi > 0) && (points[i].phi <= 180)) spf = "ind";
                    else if (points[i].phi < 0) spf = "cap";
                    else spf = "";

                    tolerance = points[i].classe / powerFactor;

                    float[] error = new float[points[i].n];
                    float avgError = 0.0f;                    

                    float totalPower = points[i].elements.Length * currentVoltage * points[i].current * powerFactor;                      

                    string msg = points[i].voltage + " V/" + points[i].current + " A/" + points[i].phi + "° ";
                    int testCount = 0;
                    int j;
                    if (mode.Equals("Saída de pulsos"))
                    {
                        //
                        //  Dummy calibration to discard the first test
                        //
                        standardMeter.ExecuteCalibration(stdEnergyType, totalPower,
                            points[i].tempo, khADR2000, points[i].classe, false);
                        Thread.Sleep(1000);

                        for (j = 0; j < points[i].n; j++)
                        {
                            stepsDone++;
                            testCount++;
                            string msgN = testCount + " de " + points[i].n;
                            int progress = stepsDone * 100 / totalSteps;
                            UpdateText(progress + " % - " + msg + msgN);
                            UpdateProgress(progress);

                            if (backgroundWorker.CancellationPending) return (false);
                            //
                            // ExecuteCalibration blocks until end of test
                            //
                            error[j] = standardMeter.ExecuteCalibration(stdEnergyType, totalPower,
                                points[i].tempo, khADR2000, points[i].classe, considerTolerance);
                            avgError += error[j];

                            Color color;
                            if (Math.Abs(error[j]) > tolerance) color = Color.Red;
                            else color = Color.Green;

                            UpdateError(error[j], color);

                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Modo não suportado.");
                    }

                    adr2000.CancelTest();
                    Thread.Sleep(2000);
                    adr2000.CoolResistance();
                    Thread.Sleep(8000);

                    avgError /= points[i].n;

                    float sigma = 0.0f;
                    for (j = 0; j < points[i].n; j++) sigma += Convert.ToSingle(Math.Pow(error[j] - avgError, 2));
                    if (points[i].n > 1) sigma /= (points[i].n - 1);
                    sigma = Convert.ToSingle(Math.Sqrt(sigma));

                    values[3] = "'" + points[i].voltage + "'";
                    values[4] = "'" + points[i].current + "'";
                    values[5] = "'" + points[i].elements + "'";
                    values[7] = "'" + powerFactor.ToString("0.0") + " " + spf + "'";
                    values[8] = "'" + avgError + "'";
                    values[9] = "'" + sigma + "'";
                    values[11] = "'" + points[i].n + "'";
                    values[13] = "'" + osop + "'";
                    values[14] = "'" + osopNumber + "'";
                    values[16] = "'" + points[i].classe + "'";

                    string cond = "WHERE (Laudo = " + values[1] + " AND Nome = " + values[2] + " AND Tensao = " + points[i].voltage + " AND Corrente = " + points[i].current + " AND Elemento = " + values[5] + " AND Energia = " + values[6] +
                    " AND PF = " + values[7] + " AND Clamp = " + values[12] + ")";

                    rs = new Recordset();
                    db.ConnectToDatabase();
                    db.GetRecords("Calibracao", new List<string>() { "*" }, cond, ref rs);

                    if (rs.BOF && rs.EOF) db.InsertData("Calibracao", fields, values);
                    else db.UpdateData("Calibracao", new List<string>() { "erro", "dp" }, new List<string>() { avgError.ToString(), sigma.ToString() }, cond);

                    if (rs != null) rs.Close();
                    db.CloseConnection();

                    if (!laudoSent)
                    {
                        //
                        //  Writes the calibration report and the calibration date into ADR 2000's memory
                        //
                        adr2000.WriteMemoryString(ADR2000.EE_REPORT, laudo);
                        adr2000.WriteMemoryLong(ADR2000.EE_CAL_DATE, adr2000.GetDate(DateTime.Today.Day, DateTime.Today.Month, DateTime.Today.Year));
                        laudoSent = true;
                    }

                    Thread.Sleep(1000);
                }
            }


            return (true);
        }


        private LPW SmallerDifference(LPW[] constants, float pulseConstant)
        {
            LPW ret = new LPW(0, 0);
            int i;
            float diff = Math.Abs(constants[0]._constant - pulseConstant);
            float aux;

            for (i = 1; i < constants.Length; i++)
            {
                if ((aux = Math.Abs(constants[i]._constant - pulseConstant)) < diff)
                {
                    ret = constants[i];
                }
                diff = aux;
            }

            return (ret);
        }

        private void CalibrationProcessADR3000(object argument, string[] deviceInfo)
        {
            bool adrHasVoltageGeneration = false;

            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();
            TcpClient tcpClient = new TcpClient();
            ADR3000 adr3000 = new ADR3000(null);
            StandardMeter standardMeter = new StandardMeter(standardType);
            PowerSupply powerSupply = new PowerSupply(null);
            ACP300 acpPowerSource = new ACP300();
            frVoltage formVoltage = new frVoltage(standardMeter, standardType);
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(true);

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

                bluetoothClient.Connect(BluetoothAddress.Parse(deviceInfo[1]), BluetoothService.SerialPort);
                adr3000 = new ADR3000(bluetoothClient.GetStream());

                float[] measures = adr3000.GetMeasures();
                if (measures[ADR3000.V] < ADR3000.VOLTAGE_TH)
                {
                    adrHasVoltageGeneration = true;
                    adr3000.SetAdjustModeState(0);
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

                if (!CalibrateADR3000(standardMeter, adr3000, powerSupply,
                    formVoltage, db, argument, new object[] { acpPowerSource, adrHasVoltageGeneration }))
                {
                    MessageBox.Show("Processo de calibração interrompido", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                adr3000.TurnOffI();
                adr3000.CancelTest();

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
                MessageBox.Show("Processo de calibração finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    if (adrHasVoltageGeneration) adr3000.TurnOffV();
                    adr3000.TurnOffI();
                    adr3000.CancelTest();

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

                if (bluetoothClient != null) bluetoothClient.Dispose();
                if (tcpClient != null) tcpClient.Dispose();

                if (standardMeter != null) standardMeter.Dispose();


                if (adr3000 != null) adr3000.Dispose();
                if (powerSupply != null) powerSupply.Dispose();
                if (acpPowerSource != null) acpPowerSource.Dispose();
                if (formVoltage != null) formVoltage.Dispose();
                db.Dispose();
            }
        }

        private void CalibrationProcessADR3000LITE(object argument, string[] deviceInfo)
        {            

            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();            
            ADR3000 adr3000 = new ADR3000(null);
            StandardMeter standardMeter = new StandardMeter(standardType);           
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(true);

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

               
                adr3000.SetAdjustModeState(0);               

                if (!CalibrateADR3000LITE(standardMeter, adr3000, db, argument))
                {
                    MessageBox.Show("Processo de calibração interrompido", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                adr3000.TurnOffI();
                adr3000.TurnOffV();
                adr3000.CancelTest();                

                bluetoothClient.Dispose();
                
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
                MessageBox.Show("Processo de calibração finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    adr3000.TurnOffI();
                    adr3000.TurnOffV();
                    adr3000.CancelTest();

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

                if (bluetoothClient != null) bluetoothClient.Dispose();
                if (standardMeter != null) standardMeter.Dispose();
                if (adr3000 != null) adr3000.Dispose();               
                db.Dispose();
            }

        }

        private void CalibrationProcessADR2000(object argument, string[] deviceInfo)
        {
            UpdateText("0 % - Conectando com dispositivos");

            BluetoothClient bluetoothClient = new BluetoothClient();
            TcpClient tcpClient = new TcpClient();
            ADR2000 adr2000 = new ADR2000(null);
            StandardMeter standardMeter = new StandardMeter(standardType);
            PowerSupply powerSupply = new PowerSupply(null);
            ACP300 acpPowerSource = new ACP300();
            frVoltage formVoltage = new frVoltage(standardMeter, standardType);
            MSAccess db = new MSAccess(connectionString);

            try
            {
                standardMeter.ConnectToStandard(true);
                standardMeter.RequestConnection();
                standardMeter.ConfigRangeType(StandardMeter.RangeType.MANUAL);
                standardMeter.EnablePulseInputToCalibrateMeter(true);

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

                if (!CalibrateADR2000(standardMeter, adr2000, powerSupply,
                    formVoltage, db, argument, new object[] { acpPowerSource }))
                {
                    MessageBox.Show("Processo de calibração interrompido", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Processo de calibração finalizado com sucesso", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            object[] argument = (object[])e.Argument;
            string[] deviceInfo = ((string)argument[_DEVICE_INFO_]).Split('|');

            switch (deviceInfo[0].Substring(0, 4))
            {
                case "AM2-":
                    CalibrationProcessADR2000(argument, deviceInfo);
                    break;
                case "AL3-":
                    CalibrationProcessADR3000LITE(argument, deviceInfo);
                    break;
                default:
                    CalibrationProcessADR3000(argument, deviceInfo);
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

        private void FrCalibration_SizeChanged(object sender, EventArgs e)
        {
            CenterControlInParent(pnCalibration);
        }

        private void FrCalibration_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker != null) backgroundWorker.Dispose();
            if (timer != null) timer.Dispose();
            Dispose();
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateTimer(time = (TimeSpan.Parse(time) + new TimeSpan(0, 0, 1)).ToString());
        }

        private void BtStopCalibration_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void BtInitCalibration_Click(object sender, EventArgs e)
        {
            /*if (cbOSOP.Text == "OS")
            {   
                if (!int.TryParse(tbOSOPNum.Text.Replace(" ", ""), out int osNumber))
                {
                    MessageBox.Show("Número inválido para ordem de serviço", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (!tbOSOPNum.Text.Contains("-"))
                {
                    MessageBox.Show("Código inválido para ordem de produção.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string[] text = tbOSOPNum.Text.Replace(" ", "").Split('-');

                if ((!int.TryParse(text[0], out int num1)) || (!int.TryParse(text[1], out int num2)))
                {
                    MessageBox.Show("Código inválido para ordem de produção.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }*/
            if (tbOSOPNum.Text.Contains("/"))
            {
                MessageBox.Show("Número da OS/OP não deve conter \"/\".", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Iniciar calibração do ADR 3000?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.No)
            {
                UpdateControlsStatus(false);

                timer.Start();

                bool[] codigo = new bool[listBoxPoints.Items.Count];
                int i;
                for (i = 0; i < listBoxPoints.Items.Count; i++)
                {
                    if (listBoxPoints.GetSelected(i)) codigo[i] = true;
                    else codigo[i] = false;
                }

                backgroundWorker.RunWorkerAsync(new object[] { cbDevice.Text, cbEssay.Text, cbMode.Text, cbOSOP.Text, 
                    tbOSOPNum.Text, codigo, tbReport.Text, ckbConsiderTolerance.Checked });
            }
        }

        private void FrCalibration_Load(object sender, EventArgs e)
        {/*
            BluetoothDeviceInfo[] deviceInfos = new BluetoothClient().DiscoverDevices(255, true, false, false, false);

            foreach (BluetoothDeviceInfo deviceInfo in deviceInfos)
            {
                if (deviceInfo.DeviceName.Length >= 8)
                {
                    if (deviceInfo.DeviceName.Substring(0, 4).Equals("AM3-") ||
                        deviceInfo.DeviceName.Substring(0, 4).Equals("AM2-") ||
                        deviceInfo.DeviceName.Substring(0, 4).Equals("AP3-") ||
                        deviceInfo.DeviceName.Substring(0, 4).Equals("AL3-")) 
                        cbDevice.Items.Add(deviceInfo.DeviceName + "|" + deviceInfo.DeviceAddress.ToString());
                }
            }

            if (cbDevice.Items.Count == 0)
            {
                MessageBox.Show("Não há ADR 3000 algum pareado com o bluetooth local.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }

            cbDevice.Text = cbDevice.Items[0].ToString();

            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            db.ConnectToDatabase();
            db.GetDistinctData("Ensaios", "Nome", ref rs);
            if (rs.BOF && rs.EOF)
            {
                MessageBox.Show("Não há ensaios de calibração cadastrados.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (rs != null) rs.Close();
                db.CloseConnection();
                db.Dispose();
                Close();
            }

            while (!rs.EOF)
            {
                //carrega informações no comboBox
                string item = ((dynamic)rs.Fields["Nome"]).Value.ToString();
                cbEssay.Items.Add(item);
                if (item.Length * cbEssay.Font.Size > cbEssay.DropDownWidth) cbEssay.DropDownWidth = Convert.ToInt32(item.Length * cbEssay.Font.Size);
                rs.MoveNext();
            }

            cbEssay.Text = cbEssay.Items[0].ToString();
            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();

            cbMode.Text = cbMode.Items[0].ToString();
            cbOSOP.Text = cbOSOP.Items[0].ToString();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(Elapsed);
            timer.AutoReset = true;

            backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            backgroundWorker.WorkerSupportsCancellation = true;*/
        }

        private void CbEssay_SelectedIndexChanged(object sender, EventArgs e)
        {
            MSAccess db = new MSAccess(connectionString);
            Recordset rs = new Recordset();

            listBoxPoints.Items.Clear();

            string condition = "WHERE Nome = '" + cbEssay.Text + "' ORDER BY Corrente, Tensao, Energia, Elementos";

            db.ConnectToDatabase();
            db.GetRecords("Ensaios", new List<string>() { "*" }, condition, ref rs);

            while (!rs.EOF)
            {
                string text = ((dynamic)rs.Fields["Energia"]).Value.ToString() + "/" +
                        ((dynamic)rs.Fields["Tensao"]).Value.ToString() + "/" +
                        ((dynamic)rs.Fields["Corrente"]).Value.ToString() + "/" +
                        ((dynamic)rs.Fields["PHI"]).Value.ToString() + "/" +
                        ((dynamic)rs.Fields["Elementos"]).Value.ToString();
                listBoxPoints.Items.Add(text);
                listBoxPoints.SetSelected(listBoxPoints.Items.Count - 1, true);
                rs.MoveNext();
            }
            if (rs != null) rs.Close();
            db.CloseConnection();
            db.Dispose();

        }
    }
}
