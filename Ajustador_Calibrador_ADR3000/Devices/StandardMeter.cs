using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Management;
using System.Diagnostics;
using Ajustador_Calibrador_ADR3000.Helpers;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    /// <summary>
    /// Class to communicate with a standard reference meter. It encapsulates the Radian, GFUVE and Zera standards.
    /// </summary>
    public class StandardMeter : IDisposable
    {
        private const int GF333B_MEASURES_SIZE = 39;
        private const int RMM3006_MEASURES_SIZE = 36;

        private const int MAX_RMM3006_TRY = 2;  //  Max attempts = n + 1 = 3
        private const int MAX_GF333B_TRY = 5;   //  Max attempts = n + 1 = 6
        private const int MAX_RD20_TRY = 4;     //  Max attempts = n + 1 = 3
        private const int MAX_PRESCALER_TRY = 4;//  Max attempts = n + 1 = 3
        //
        // Valid values for the measurement mode
        //
        public const int _4Wa = 0;
        public const int _4Wr = 1;
        public const int _4Wrc = 2;
        public const int _3Wa = 3;
        public const int _3Wr = 4;
        public const int _3Wrca = 5;
        public const int _3Wrcb = 6;
        public const int _4Wap = 7;
        public const int _4Wr60 = 8;
        public const int _3Wap = 9;
        public const int _4Q60c = 10;
        public const int _3WQ60 = 11;
        public const int _3Q60c = 12;
        public const int _4Wapg = 13;
        public const int _3Wapg = 14;
        public const int _4Wrg = 15;
        public const int _3Wrg = 16;

        public const int U480V = 0;
        public const int U240V = 1;
        public const int U120V = 2;
        public const int U60V = 3;

        public const int I200A = 0;
        public const int I100A = 1;
        public const int I50A = 2;
        public const int I20A = 3;
        public const int I10A = 4;
        public const int I5A = 5;
        public const int I2A = 6;
        public const int I1A = 7;
        public const int I500mA = 8;
        public const int I200mA = 9;
        public const int I100mA = 10;
        public const int I50mA = 11;
        public const int I20mA = 12;
        public const int I10mA = 13;
        public const int I5mA = 14;

        public const int LOW_TO_HIGH_THRESHOLD = 16;

        //
        // Public variables, classes, structures and attributes to be used
        //
        public enum StandardType
        {
            /// <summary>
            /// GFUVE GF333B standard meter.
            /// </summary>
            GF333B = 0,

            /// <summary>
            /// Radian RD20 standard meter.
            /// </summary>
            RD20 = 1,

            /// <summary>
            /// Zera RMM3006 standard meter.
            /// </summary>
            RMM3006 = 2,

            GF333BM = 3
        };

        public enum EnergyType
        {
            /// <summary>
            /// The active energy type.
            /// </summary>
            ACTIVE = 0,

            /// <summary>
            /// The reactive energy type.
            /// </summary>
            REACTIVE = 1
        };


        public enum RangeType
        {
            MANUAL,
            AUTOMATIC
        }

        //
        // Private variables, classes, structures and attributes to be used
        //
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private SerialPort port, portCounter;
        private readonly StandardType standardType;
        private byte[] myStdBuffer = new byte[1024];
        private string rMM30006_Answer = "";
        private string gf333B_Answer = "";
        private int mIndex = 0;


        public StandardType Standard
        {
            get
            {
                return standardType;
            }
        }

        /// <summary>
        /// Constructor for the StandardMeter class.
        /// </summary>
        /// <param name="type">The type of the standard.</param>
        public StandardMeter(StandardType type)
        {
            standardType = type;
        }

        /// <summary>
        /// Verifies wich standard is being used and connects to the correct one.
        /// </summary>
        public void ConnectToStandard(bool hasPrescaler)
        {
            switch (standardType)
            {
                case StandardType.GF333B:
                case StandardType.GF333BM:
                    ConnectToGF333B(hasPrescaler);
                    break;
                case StandardType.RD20:
                    ConnectToRD20();
                    break;
                case StandardType.RMM3006:
                    ConnectToRMM3006(hasPrescaler);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Requests measures from the standard being used by the StandardMeter class.
        /// </summary>
        /// <returns>A float array containing the measures.</returns>
        public float[] GetMeasures()
        {
            //
            // A float array to be returned by the method
            //
            bool keepTrying;
            float[] measures = new float[] { 0.0f };
            int i, j, k;
            int tryCount = 0;
            //
            // Checks what type of standard is being used and performs the correct action
            //
            switch (standardType)
            {
                case StandardType.GF333B:
                case StandardType.GF333BM:
                    /*Measures:
                    0 - U1  3 - U12 6 - I1  9 -  phi1   12 - phi+       15 - phiU1U3    18 - phiU1I3    21 - P3    24 - Q2      27 - freq   30 - ES     33 - Ea     36 - N  
                    1 - U2  4 - U23 7 - I2  10 - phi2   13 - phiU1U1    16 - phiU1I1    19 - P1         22 - P+    25 - Q3      28 - US     31 - Ep     34 - TM     37 - ERROR
                    2 - U3  5 - U31 8 - I3  11 - phi3   14 - phiU1U2    17 - phiU1I2    20 - P2         23 - Q1    26 - Q+      29 - IS     32 - Eq     35 - C      38 - Cmeter*/
                    while (true)
                    {
                        try
                        {
                            //
                            // Clears the output buffer, sends a command, reads and clears the input buffer
                            //
                            keepTrying = false;

                            Interlocked.Exchange(ref gf333B_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write("$$00000068!!");

                            uint ticks = GetTickCount();
                            
                            while (!gf333B_Answer.Contains("!!"))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_GF333B_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else throw new IOException("Problemas na comunicação com o padrão GF333B");
                                }
                            }

                            if (!keepTrying) break;
                        }
                        catch (Exception)
                        {
                            if (tryCount < MAX_GF333B_TRY) tryCount++;
                            else throw new IOException("Problemas na comunicação com o padrão GF333B");
                        }
                    }

                    string answer = gf333B_Answer;
                    measures = new float[GF333B_MEASURES_SIZE];
                    
                    //
                    //  First gets the measures values
                    //
                    i = 8; 
                    j = 0;
                    while (j < 28)
                    {
                        measures[j++] = ASCIItoFloat(answer.Substring(i, 8));
                        i += 8;
                    }
                    //
                    //  Converts W to kW, var to kvar and VA to kVA
                    //
                    for (i = 19; i < 27; i++) measures[i] /= 1000;

                    //
                    //  Status words for voltage, current and extended
                    //
                    i = 29 * 8; // i = 8 + 28 * 8
                    for (j = 0; j < 3; j++)
                    {
                        measures[28 + j] = ASCIItoInteger(answer.Substring(i, 4));
                        i += 4;
                    }
                    //
                    //  Energies in kWh, kvarh and kVA
                    //
                    for (j = 0; j < 3; j++)
                    {
                        measures[31 + j] = ASCIItoFloat(answer.Substring(i, 8));
                        i += 8;
                    }

                    //
                    //  Energy time in ms
                    //
                    measures[34] = ASCIItoInteger(answer.Substring(i, 8));
                    i += 8;
                    //
                    //  GF333B-M constant
                    //
                    measures[35] = ASCIItoFloat(answer.Substring(i, 8));
                    i += 8;
                    //
                    //  Present circle number
                    //
                    measures[36] = ASCIItoInteger(answer.Substring(i, 4));
                    i += 4;
                    //
                    //  Error
                    //
                    measures[37] = ASCIItoFloat(answer.Substring(i, 8));
                    i += 8;
                    //
                    //  Meter constant
                    //
                    measures[38] = ASCIItoFloat(answer.Substring(i, 8));
                    

                    break;

                case StandardType.RD20:

                    
                    //
                    // Instantiates the buffer to be sent to the standard
                    //
                    byte[] buffer;
                    int numOfBytesRead = 0;
                    int checkSum;

                    TRY: buffer = new byte[] { 0xA6, 0x0D, 0x00, 0x08, 0x00, 0x24, 0x00, 0x00, 0x00, 0x14, 0xFF, 0xFD, 0x02, 0xEF };

                    //
                    // Clears the output buffer, sends a command, reads and clears the input buffer
                    //
                    port.DiscardOutBuffer();
                    port.Write(buffer, 0, buffer.Length);
                    Thread.Sleep(250);

                    buffer = new byte[42];
                    numOfBytesRead = 0;

                    for (i = 0; i < 42; i++)
                    {
                        try
                        {
                            numOfBytesRead += port.Read(buffer, i, 1);
                        }
                        catch (Exception ex)
                        {
                            if (tryCount < MAX_RD20_TRY)
                            {
                                tryCount++;
                                port.DiscardInBuffer();
                                goto TRY;
                            }
                            else throw new IOException(ex.Message + Environment.NewLine + "StandardMeter.GetMeasures().");
                        }
                    }
                    port.DiscardInBuffer();

                    //
                    // Verifies the integrity of the answer based on the checksum sent by the RD20
                    //
                    checkSum = BitConverter.ToInt32(new byte[] { buffer[numOfBytesRead - 1], buffer[numOfBytesRead - 2], 0, 0 }, 0);


                    if ((uint)checkSum != CheckSum(buffer, numOfBytesRead - 2))
                    {
                        if (tryCount < MAX_RD20_TRY)
                        {
                            tryCount++;
                            port.DiscardInBuffer();
                            goto TRY;
                        }
                        else throw new IOException("Checksum incorreto enviado pelo padrão RD20");
                    }

                    //
                    // Creates a buffer to store only data related to measures, excluding header, footer and checksum
                    //
                    byte[] bMeasures = new byte[36], data;

                    Buffer.BlockCopy(buffer, 4, bMeasures, 0, bMeasures.Length);

                    //
                    // Allocates the necessary memory to store the floating point numbers
                    //
                    measures = new float[9];

                    //
                    // Converts each group of four bytes into one IEEE 754 floating point number
                    //
                    k = 0;
                    for (j = 0; j < bMeasures.Length; j += 4)
                    {
                        data = new byte[] { bMeasures[j], bMeasures[j + 1], bMeasures[j + 2], bMeasures[j + 3] };
                        measures[k++] = bytesTofloat(data);
                    }

                    break;
                case StandardType.RMM3006:
                    while (true)
                    {
                        try
                        {
                            keepTrying = false;

                            measures = new float[RMM3006_MEASURES_SIZE];
                            string endChar = "MEACK\r";

                            Interlocked.Exchange(ref rMM30006_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write("ME\r");

                            uint ticks = GetTickCount();
                            while (!rMM30006_Answer.Contains(endChar))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_RMM3006_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else throw new IOException("Problemas na comunicação com o padrão RMM3006");
                                }
                            }


                            if (!keepTrying) break;
                        }
                        catch (Exception)
                        {
                            if (tryCount < MAX_RMM3006_TRY) tryCount++;
                            else throw new IOException("Problemas na comunicação com o padrão RMM3006");
                        }
                    }
                    
                    string[] values = rMM30006_Answer.Split('\r');

                            
                    for (i = 0; i < measures.Length; i++)
                    {
                        string[] measure = values[i].Split(';');
                        measures[i] = float.Parse(measure[1], System.Globalization.NumberStyles.Float);
                    }
                    
                    break;

                default:
                    break;
            }

            return (measures);
        }

        /// <summary>
        /// Get information about the ranges of the standard (voltage and current).
        /// </summary>
        /// <returns>An int array with the information about the ranges.</returns>
        public int[] GetRangeStatus()
        {
            int[] Ranges = new int[3];
            bool keepTrying;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    while (true)
                    {
                        try
                        {
                            keepTrying = false;

                            string endChar = "STACK\r";

                            Interlocked.Exchange(ref rMM30006_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write("ST\r");

                            uint ticks = GetTickCount();
                            while (!rMM30006_Answer.Contains(endChar))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_RMM3006_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else throw new IOException("Problemas na comunicação com o padrão RMM3006");
                                }
                            }

                            if (!keepTrying) break;
                        }
                        catch (Exception)
                        {
                            if (tryCount < MAX_RMM3006_TRY) tryCount++;
                            else throw new IOException("Problemas na comunicação com o padrão RMM3006");
                        }
                    }

                    //
                    // Treats the standard's answer
                    //
                    string[] values = rMM30006_Answer.Split('\r');

                    string vRng = values[0].Substring(3, 3);
                    string iRng = values[1].Substring(3, 3);
                    string mMode = values[2].Substring(2, 5);

                    switch (vRng)
                    {
                        case "240": Ranges[0] = U240V; break;
                        case "120": Ranges[0] = U120V; break;
                        case " 60": Ranges[0] = U60V; break;
                        default: Ranges[0] = U480V; break;
                    }

                    switch (iRng)
                    {
                        case "100": Ranges[1] = I100A; break;
                        case " 50": Ranges[1] = I50A; break;
                        case " 20": Ranges[1] = I20A; break;
                        case " 10": Ranges[1] = I10A; break;
                        case "  5": Ranges[1] = I5A; break;
                        case "  2": Ranges[1] = I2A; break;
                        case "  1": Ranges[1] = I1A; break;
                        case "0.5": Ranges[1] = I500mA; break;
                        case "0.2": Ranges[1] = I200mA; break;
                        case "0.1": Ranges[1] = I100mA; break;
                        case "50m": Ranges[1] = I50mA; break;
                        case "20m": Ranges[1] = I20mA; break;
                        case "10m": Ranges[1] = I10mA; break;
                        case " 5m": Ranges[1] = I5mA; break;
                        default: Ranges[1] = I200A; break;
                    }

                    switch (mMode)
                    {
                        case "  4Wa": Ranges[2] = _4Wa; break;
                        case "  4Wr": Ranges[2] = _4Wr; break;
                        case " 4Wrc": Ranges[2] = _4Wrc; break;
                        case "  3Wa": Ranges[2] = _3Wa; break;
                        case "  3Wr": Ranges[2] = _3Wr; break;
                        case "3Wrca": Ranges[2] = _3Wrca; break;
                        case "3Wrcb": Ranges[2] = _3Wrcb; break;
                        case " 4Wap": Ranges[2] = _4Wap; break;
                        case "4Wr60": Ranges[2] = _4Wr60; break;
                        case " 3Wap": Ranges[2] = _3Wap; break;
                        case "4Q60c": Ranges[2] = _4Q60c; break;
                        case "3WQ60": Ranges[2] = _3WQ60; break;
                        case "3Q60c": Ranges[2] = _3Q60c; break;
                        case "4Wapg": Ranges[2] = _4Wapg; break;
                        case "3Wapg": Ranges[2] = _3Wapg; break;
                        case " 4Wrg": Ranges[2] = _4Wrg; break;
                        default: Ranges[2] = _3Wrg; break;
                    }

                    break;
                default:
                    break;
            }

            return Ranges;
        }

        /// <summary>
        /// Gets the best voltage range that suits the standard given a voltage value.
        /// </summary>
        /// <param name="voltage">A voltage value.</param>
        /// <returns>An int array with the range and the constant multiplier.</returns>
        public int[] GetBestVoltageRange(float voltage)
        {
            int UR = U480V;
            int vRngMult = 1;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    if (voltage < 60)
                    {
                        UR = U60V;
                        vRngMult = 8;
                    }
                    else if ((voltage >= 60) && (voltage < 120))
                    {
                        UR = U120V;
                        vRngMult = 4;
                    }
                    else if ((voltage >= 120) && (voltage < 240))
                    {
                        UR = U240V;
                        vRngMult = 2;
                    }
                    else if ((voltage >= 240) && (voltage < 480))
                    {
                        UR = U480V;
                        vRngMult = 1;
                    }
                    else
                    {
                        throw new ArgumentException("Valor de tensão do ponto inválido.");

                    }
                    break;
                default:
                    break;
            }

            return (new int[] { UR, vRngMult });
        }

        /// <summary>
        /// Gets the best current range that suits the standard given a current value.
        /// </summary>
        /// <param name="current">A current value.</param>
        /// <returns>An int array with the range and the constant multiplier.</returns>
        public int[] GetBestCurrentRange(float current)
        {
            int iRngMult = 1;
            int IR = I200A;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    if ((current >= 0.005f) && (current < 0.010f))
                    {
                        IR = I10mA;
                        iRngMult = 20000;
                    }
                    else if ((current >= 0.010f) && (current < 0.020f))
                    {
                        IR = I20mA;
                        iRngMult = 10000;
                    }
                    else if ((current >= 0.020f) && (current < 0.050f))
                    {
                        IR = I50mA;
                        iRngMult = 4000;
                    }
                    else if ((current >= 0.050f) && (current < 0.100f))
                    {
                        IR = I100mA;
                        iRngMult = 2000;
                    }
                    else if ((current >= 0.100f) && (current < 0.200f))
                    {
                        IR = I200mA;
                        iRngMult = 1000;
                    }
                    else if ((current >= 0.200f) && (current < 0.500f))
                    {
                        IR = I500mA;
                        iRngMult = 400;
                    }
                    else if ((current >= 0.500f) && (current < 1.0f))
                    {
                        IR = I1A;
                        iRngMult = 200;
                    }
                    else if ((current >= 1.0f) && (current < 2.0f))
                    {
                        IR = I2A;
                        iRngMult = 100;
                    }
                    else if ((current >= 2.0f) && (current < 5.0f))
                    {
                        IR = I5A;
                        iRngMult = 40;
                    }
                    else if ((current >= 5.0f) && (current < 10.0f))
                    {
                        IR = I10A;
                        iRngMult = 20;
                    }
                    else if ((current >= 10.0f) && (current < 16.0f))
                    {
                        IR = I20A;
                        iRngMult = 10;
                    }
                    else if ((current >= 16.0f) && (current < 50.0f))
                    {
                        IR = I50A;
                        iRngMult = 4;
                    }
                    else if ((current >= 50.0f) && (current < 100.0f))
                    {
                        IR = I100A;
                        iRngMult = 2;
                    }
                    else if ((current >= 100.0f) && (current < 160.0f))
                    {
                        IR = I200A;
                        iRngMult = 1;
                    }
                    else
                    {
                        throw new ArgumentException("Valor de corrente inválido");
                    }
                    break;
                default:
                    break;
            }

            return (new int[] { IR, iRngMult });
        }

        /// <summary>
        /// Method to configure the voltage range of the standard.
        /// </summary>
        /// <param name="vRng">An integer representing the range.</param>
        /// <returns>true if success, false otherwise.</returns>
        public bool SetActualVoltageRange(int vRng)
        {
            bool keepTrying;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    if ((vRng < 0) || (vRng > U60V)) return (false);

                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "UBACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");
                        
                        port.DiscardOutBuffer();
                        port.Write("UB" + vRng.ToString() + "\r");

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else return (false);
                            }
                        }

                        if (!keepTrying) break;
                    }

                    break;
                default:
                    break;
            }


            return (true);
        }

        /// <summary>
        /// Method to configure the current range of the standard.
        /// </summary>
        /// <param name="iRng">An integer representing the range.</param>
        /// <returns>true if success, false otherwise.</returns>
        public bool SetActualCurrentRange(int iRng)
        {
            bool keepTrying;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    if ((iRng < 0) || (iRng > I5mA)) return (false);

                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "IBACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");
                        
                        port.DiscardOutBuffer();
                        port.Write("IB" + iRng.ToString() + "\r");

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else return (false);
                            }
                        }

                        
                        if (!keepTrying) break;
                    }
                    break;
                default:
                    break;
            }


            return (true);
        }

        /// <summary>
        /// Requests the interface connection from the standard.
        /// </summary>
        /// <returns>
        /// Returns true if the operation was successful, false otherwise.
        /// </returns>
        public bool RequestConnection()
        {
            bool keepTrying;
            bool retVal = true;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "RQACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");
                        
                        port.DiscardOutBuffer();
                        port.Write("RQ\r");

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else
                                {
                                    retVal = false;
                                    break;
                                }
                            }
                        }

                        if (!keepTrying) break;
                    }
                    break;
                default:
                    break;
            }

            return (retVal);
        }
        
        public void SetK1K2(bool low_k1, bool high_k2)
        {
            //
            //  This method configures the keys for the current path.
            //  There is one for the high current scale (4 A < I <= 45 A) and
            //  another for the low current scale (I <= 4 A).
            //
            int command = 0x11;
            long data = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:

                    if (low_k1) data |= 0x02;
                    if (high_k2) data |= 0x01;

                    if (!SendCommandToPrescaller(command, data))
                    {
                        throw new IOException("Problemas na comunicação com o prescaller.");
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Terminates the interface connection from the standard.
        /// </summary>
        /// <returns>
        /// Returns true if the operation was successful, false otherwise.
        /// </returns>
        public bool TerminateConnection()
        {
            bool keepTrying;
            bool retVal = true;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "NRACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");

                        port.DiscardOutBuffer();
                        port.Write("NR\r");

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else
                                {
                                    retVal = false;
                                    break;
                                }
                            }
                        }

                        if (!keepTrying) break;
                    }
                    break;
                default:
                    break;
            }

            return (retVal);
        }

        /// <summary>
        /// Configures the pulse constant in Wh/pulse.
        /// </summary>
        /// <param name="type">The nature of the energy pulse. Can be active or reactive.</param>
        /// <param name="kd">The constant in Wh/pulse.</param>
        /// <param name="kh">In the case of the GF333B, it must be passed the kh in pulses/Wh of the current scale</param>
        public void SetKd(EnergyType type, float kd, int kh)
        {
            int numOfBytesRead = 0, i, tryCount = 0;
            int command;
            long data;
            switch (standardType)
            {
                case StandardType.RMM3006:

                    command = 0x01;

                    data = (long)(kd * kh);

                    if (!SendCommandToPrescaller(command, data)) throw new IOException("Problemas na comunicação com o prescaller.");

                    break;
                case StandardType.GF333B:
                case StandardType.GF333BM:

                    if (type == EnergyType.ACTIVE) command = 0x01;
                    else command = 0x04;

                    data = (long)(kd * kh);

                    if (!SendCommandToPrescaller(command, data)) throw new IOException("Problemas na comunicação com o prescaller.");

                    break;

                case StandardType.RD20:

                    //
                    // Creates a buffer to store the bytes related to the TI floating point number
                    //
                    byte[] tikd = new byte[4];

                    //
                    // Converts the IEEE 754 floating point to 4 bytes representing a TI floating point
                    //
                    floatTobytes(kd, tikd);

                    //
                    // Creates a buffer to be sent to the RD20 containing the command.
                    //
                    byte[] RD20Buffer;
                    uint cks;
                    TRY: RD20Buffer = new byte[] { 0xA6, 0x32, 0x00, 0x07, 0x00, (byte)type, 0x00, tikd[0], tikd[1], tikd[2], tikd[3], 0x00, 0x00 };

                    cks = CheckSum(RD20Buffer, 11);

                    RD20Buffer[11] = (byte)((cks >> 8) & 0xFF);
                    RD20Buffer[12] = (byte)(cks & 0xFF);

                    //
                    // Clears the output buffer, sends a command, reads and clears the input buffer
                    //
                    port.DiscardOutBuffer();
                    port.Write(RD20Buffer, 0, RD20Buffer.Length);
                    Thread.Sleep(250);

                    RD20Buffer = new byte[4];
                    for (i = 0; i < 4; i++)
                    {
                        try
                        {
                            numOfBytesRead += port.Read(RD20Buffer, i, 1);
                        }
                        catch (Exception ex)
                        {
                            if (tryCount < MAX_RD20_TRY)
                            {
                                tryCount++;
                                port.DiscardInBuffer();
                                goto TRY;
                            }
                            else throw new IOException(ex.Message + Environment.NewLine + "StandardMeter.SetKd().");
                        }
                    }
                    port.DiscardInBuffer();

                    //
                    // Verifies the integrity of the answer sent by the RD20
                    //
                    if (!CheckArrays(RD20Buffer, new byte[] { 0xA3, 0x32, 0x00, 0xD5 }))
                    {
                        if (tryCount < MAX_RD20_TRY)
                        {
                            tryCount++;
                            port.DiscardInBuffer();
                            goto TRY;
                        }
                        else throw new IOException("Resposta incorreta enviada pelo padrão RD20.");
                    }

                    break;
                default:
                    break;
            }
        }

        public void SetPulseOutputFrequency(ushort scale, ushort currentScales)
        {
            
        }

        /// <summary>
        /// Sets the nature of the energy being represented by the pulse output of the standard.
        /// </summary>
        /// <param name="type">The type of energy. It can be ACTIVE or REACTIVE.</param>
        public void SetPulseEnergyType(EnergyType type)
        {
            bool keepTrying;
            int numOfBytesRead = 0, i, tryCount = 0;
            switch (standardType)
            {
                case StandardType.RMM3006:
                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "MSACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");
                        
                        port.DiscardOutBuffer();
                        if (type == EnergyType.ACTIVE) port.Write("MS0\r");
                        else port.Write("MS1\r");

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else
                                {
                                    throw new IOException("Problemas na comunicação com o padrão.");
                                }
                            }

                        }

                        if (!keepTrying) break;
                    }
                    break;
                case StandardType.GF333BM:
                    break;
                case StandardType.RD20:

                    //
                    // Creates a buffer to be sent to the RD20 containing the command.
                    //
                    byte[] answer = new byte[] { 0xAC, 0x1D, 0x00, 0x02, 0x01, 0xCC, 0x01, 0x98 };

                    byte[] buffer;

                    TRY: buffer = new byte[] { 0xA6, 0x1D, 0x00, 0x02, 0x02, (byte)type, 0x00, (byte)(0xC7 + type) };
                    //
                    // Clears the output buffer, sends a command, reads and clears the input buffer
                    //
                    port.DiscardOutBuffer();
                    port.Write(buffer, 0, buffer.Length);
                    Thread.Sleep(250);

                    buffer = new byte[8];
                    for (i = 0; i < 8; i++)
                    {
                        try
                        {
                            numOfBytesRead += port.Read(buffer, i, 1);
                        }
                        catch (Exception ex)
                        {
                            if (tryCount < MAX_RD20_TRY)
                            {
                                tryCount++;
                                port.DiscardInBuffer();
                                goto TRY;
                            }
                            else throw new IOException(ex.Message + Environment.NewLine + "StandardMeter.SetPulseEnergyType().");
                        }
                    }
                    port.DiscardInBuffer();
                    //
                    // Verifies the integrity of the answer sent by the RD20
                    //
                    if (!CheckArrays(buffer, answer))
                    {
                        if (tryCount < MAX_RD20_TRY)
                        {
                            tryCount++;
                            port.DiscardInBuffer();
                            goto TRY;
                        }
                        else throw new IOException("Resposta incorreta enviada pelo padrão RD20.");
                    }

                    break;
                default:
                    break;
            }
        }
        
        public void EnablePulseInputToCalibrateMeter(bool enable)
        {
            float[] measures;
            string cmdChar;
            bool keepTrying;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.GF333B:
                case StandardType.GF333BM:

                    measures = GetMeasures();

                    ushort US = Convert.ToUInt16(measures[28]);
                    ushort IS = Convert.ToUInt16(measures[29]);
                    ushort ES = Convert.ToUInt16(measures[30]);

                    //
                    //  Enables or disables pulse input
                    //
                    if (enable)
                    {
                        ES |= 0x0002;
                    }
                    else
                    {
                        ES &= 0xFFFD;
                    }

                    cmdChar = "$$01000E" + 
                                UShortToASCII(US) + 
                                UShortToASCII(IS) + 
                                UShortToASCII(ES) + "00";

                    string gfCks = GF333BCheckSum(cmdChar);

                    cmdChar += gfCks + "!!";

                    while (true)
                    {
                        
                        //
                        // Clears the output buffer, sends a command, reads and clears the input buffer
                        //
                        keepTrying = false;

                        Interlocked.Exchange(ref gf333B_Answer, "");

                        port.DiscardOutBuffer();
                        port.Write(cmdChar);

                        uint ticks = GetTickCount();

                        while (!gf333B_Answer.Contains("!!"))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_GF333B_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else throw new IOException("Problemas na comunicação com o padrão GF333B");
                            }
                        }

                        if (!keepTrying) break;
                        
                    }

                    break;
                default:
                    break;
            }
        }

        public void ConfigRangeType(RangeType rangeType)
        {
            bool keepTrying;
            string cmdChar;
            int tryCount = 0;

            switch (standardType)
            {
                case StandardType.RMM3006:
                    if (rangeType == RangeType.MANUAL) cmdChar = "AMMMM\r";
                    else cmdChar = "AMAAA\r";

                    //
                    // Sends interface connection request to the standard (best of 3)
                    //
                    while (true)
                    {
                        keepTrying = false;
                        string endChar = "AMACK\r";

                        Interlocked.Exchange(ref rMM30006_Answer, "");

                        port.DiscardOutBuffer();
                        port.Write(cmdChar);

                        uint ticks = GetTickCount();
                        while (!rMM30006_Answer.Contains(endChar))
                        {
                            if (GetTickCount() - ticks >= port.ReadTimeout)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                    keepTrying = true;
                                    break;
                                }
                                else
                                {
                                    throw new IOException("Problemas na comunicação com o padrão.");
                                }
                            }
                        }

                        if (!keepTrying) break;
                    }

                    break;
                default:
                    break;
            }
        }

        public float ExecuteCalibration(EnergyType energyType, float totalPower, 
            int duration, float khMeter, float tol, bool considerTolerance)
        {
            float[] measures;
            bool keepTrying, keepTesting;
            float error = -1.71714748f;
            int tryCount = 0;
            int numOfTurns;
            string cmdChar, outCondition;
            int testTimeout = Convert.ToInt32(3 * 1000 * duration);
            uint ticks;
            string[] sValues = new string[] { "00", "0.000", "-1.71714748" };

            switch (standardType)
            {
                case StandardType.RMM3006:

                    outCondition = "0";
                    tryCount = 0;

                    numOfTurns = Convert.ToInt32(khMeter * totalPower * duration / 3600);

                    cmdChar = "EP" + numOfTurns +
                        ";0;" + Convert.ToSingle(khMeter).ToString("0.000000") + "e+003\r";

                    keepTesting = true;

                    while (keepTesting)
                    {
                        //
                        // Sends command to the standard (best of 3)
                        //
                        while (true)
                        {
                            keepTrying = false;

                            string endChar = "EPACK\r";

                            Interlocked.Exchange(ref rMM30006_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write(cmdChar);

                            ticks = GetTickCount();
                            while (!rMM30006_Answer.Contains(endChar))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_RMM3006_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else
                                    {
                                        throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                    }
                                }
                            }

                            if (!keepTrying) break;
                        }
                        //
                        //  Selects accuracy test
                        //
                        cmdChar = "EB0\r";
                        tryCount = 0;
                        //
                        // Sends command start the test (best of 3)
                        //
                        while (true)
                        {
                            keepTrying = false;
                            string endChar = "EBACK\r";

                            Interlocked.Exchange(ref rMM30006_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write(cmdChar);

                            ticks = GetTickCount();
                            while (!rMM30006_Answer.Contains(endChar))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_RMM3006_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else
                                    {
                                        throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                    }
                                }
                            }

                            if (!keepTrying) break;
                        }
                        //
                        //  Starts requesting the test status to the standard.
                        //  There is a timeout of 2 x duration of the test.
                        //
                        cmdChar = "ES\r";
                        ticks = GetTickCount();
                        while (!outCondition.Equals("3") && (GetTickCount() - ticks < testTimeout))
                        {
                            while (true)
                            {
                                keepTrying = false;
                                string endChar = "ESACK\r";

                                Interlocked.Exchange(ref rMM30006_Answer, "");

                                port.DiscardOutBuffer();
                                port.Write(cmdChar);

                                uint mTicks = GetTickCount();
                                while (!rMM30006_Answer.Contains(endChar))
                                {
                                    if (GetTickCount() - mTicks >= port.ReadTimeout)
                                    {
                                        if (tryCount < MAX_RMM3006_TRY)
                                        {
                                            tryCount++;
                                            keepTrying = true;
                                            break;
                                        }
                                        else
                                        {
                                            throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                        }
                                    }
                                }

                                if (!keepTrying) break;
                            }

                            sValues = rMM30006_Answer.Split('\r');
                            outCondition = sValues[0].Substring(1, 1);
                        }

                        if (GetTickCount() - ticks >= testTimeout)
                        {
                            throw new IOException("Problemas na comunicação com o padrão. Timeout. StandardMeter.ExecuteCalibration().");
                        }

                        error = Convert.ToSingle(sValues[2]);

                        if (considerTolerance)
                        {
                            if (Math.Abs(error) > tol)
                            {
                                if (tryCount < MAX_RMM3006_TRY)
                                {
                                    tryCount++;
                                }
                                else keepTesting = false;
                            }
                            else keepTesting = false;
                        }
                        else keepTesting = false;
                    }

                    break;
                case StandardType.GF333B:
                case StandardType.GF333BM:

                    numOfTurns = Convert.ToInt32(khMeter * totalPower * duration / 3600);
                    int statusWord = 0x08;

                    
                    if (energyType == EnergyType.REACTIVE) statusWord = 0x09;

                    keepTesting = true;

                    while (keepTesting)
                    {
                        //
                        //  First parametrizes the test for the GF333B or GF333B-M
                        //
                        while (true)
                        {
                            keepTrying = false;

                            cmdChar = "$$020010" +
                                       UShortToASCII(Convert.ToUInt16(numOfTurns & 0xFFFF)) +
                                       FloatToASCII(khMeter * 1000) +
                                       UShortToASCII(Convert.ToUInt16(statusWord & 0xFFFF));

                            string gfCks = GF333BCheckSum(cmdChar);

                            cmdChar += gfCks + "!!";

                            Interlocked.Exchange(ref gf333B_Answer, "");

                            port.DiscardOutBuffer();
                            port.Write(cmdChar);

                            ticks = GetTickCount();
                            while (!gf333B_Answer.Contains("!!"))
                            {
                                if (GetTickCount() - ticks >= port.ReadTimeout)
                                {
                                    if (tryCount < MAX_GF333B_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else
                                    {
                                        throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                    }
                                }
                            }

                            if (!keepTrying) break;
                        }

                        while (true)
                        {
                            keepTrying = false;
                            measures = GetMeasures();
                            float presentCircleNumber = measures[36];
                            ticks = GetTickCount();
                            while (presentCircleNumber == GetMeasures()[36])
                            {
                                if (GetTickCount() - ticks >= (2000 * duration))
                                {
                                    if (tryCount < MAX_GF333B_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else
                                    {
                                        throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                    }
                                }
                            }

                            if (keepTrying) continue;

                            while (presentCircleNumber > GetMeasures()[36])
                            {
                                if (GetTickCount() - ticks >= (2000 * duration))
                                {
                                    if (tryCount < MAX_GF333B_TRY)
                                    {
                                        tryCount++;
                                        keepTrying = true;
                                        break;
                                    }
                                    else
                                    {
                                        throw new IOException("Problemas na comunicação com o padrão. StandardMeter.ExecuteCalibration().");
                                    }
                                }
                            }

                            if (!keepTrying) break;
                        }


                        //
                        //  Gets the calibration error
                        //
                        error = GetMeasures()[37];

                        if (considerTolerance)
                        {
                            if (Math.Abs(error) > tol)
                            {
                                if (tryCount < MAX_GF333B_TRY)
                                {
                                    tryCount++;
                                }
                                else keepTesting = false;
                            }
                            else keepTesting = false;
                        }
                        else keepTesting = false;
                    }

                    break;
                case StandardType.RD20:

                    int numOfPulses = Convert.ToInt32(khMeter * totalPower * duration / 3600);

                    
                    byte[] tiKd = new byte[4];
                    byte[] bNumPulses = BitConverter.GetBytes(numOfPulses);

                    
                    float kdOfAdr = 1.0f / khMeter;
                    floatTobytes(kdOfAdr, tiKd);
                    //
                    // Creates a buffer to be sent to the RD20 containing the command.
                    //
                    byte[] buffer, inBuffer;
                    uint cks;
                    int i;
                    int pulsesLeft;

                    TRY: buffer = new byte[] {  0xA6, 0x39, 0x00, 0x10, (byte)energyType, 0x01,
                                                0x00, 0x00, 0xFF, 0xFF, tiKd[0], tiKd[1], tiKd[2], tiKd[3], 0x03, 0x00,
                                                bNumPulses[1], bNumPulses[0], 0x00, 0x01, 0x00, 0x00 };

                    cks = CheckSum(buffer, 20);

                    buffer[20] = (byte)((cks >> 8) & 0xFF);
                    buffer[21] = (byte)(cks & 0xFF);

                    pulsesLeft = -1;
                    //
                    // Starts the test
                    //
                    port.DiscardOutBuffer();
                    port.Write(buffer, 0, buffer.Length); 
                    Thread.Sleep(250);

                    //
                    // Reads ACK from RD-20
                    //
                    inBuffer = new byte[4];
                    for (i = 0; i < 4; i++)
                    {
                        try
                        {
                            port.Read(inBuffer, i, 1);
                        }
                        catch (Exception ex)
                        {
                            if (tryCount < MAX_RD20_TRY)
                            {
                                tryCount++;
                                port.DiscardInBuffer();
                                goto TRY;
                            }
                            else throw new IOException(ex.Message + Environment.NewLine + "StandardMeter.ExecuteCalibration().");
                        }
                    }
                    port.DiscardInBuffer();

                    if ((inBuffer[0] != 0xA6) && (inBuffer[0] != 0xAC) && (inBuffer[0] != 0xA3))
                    {
                        if (tryCount < MAX_RD20_TRY)
                        {
                            tryCount++;
                            port.DiscardInBuffer();
                            goto TRY;
                        }
                        else throw new IOException("RD-20 respondeu NAK. StandardMeter.ExecuteCalibration().");
                    }

                    buffer[5] &= 0xFE; //asks for status

                    cks = CheckSum(buffer, 20);

                    buffer[20] = (byte)((cks >> 8) & 0xFF);
                    buffer[21] = (byte)(cks & 0xFF);

                    
                    while (pulsesLeft != 0)
                    {
                        port.DiscardOutBuffer();
                        port.Write(buffer, 0, buffer.Length);
                        Thread.Sleep(250);

                        inBuffer = new byte[28];
                        for (i = 0; i < 28; i++)
                        {
                            try
                            {
                                port.Read(inBuffer, i, 1);
                            }
                            catch (Exception ex)
                            {
                                if (tryCount < MAX_RD20_TRY)
                                {
                                    tryCount++;
                                    port.DiscardInBuffer();
                                    goto TRY;
                                }
                                else throw new IOException(ex.Message + Environment.NewLine + "StandardMeter.ExecuteCalibration().");
                            }
                        }
                        port.DiscardInBuffer();
                        pulsesLeft = BitConverter.ToInt32(new byte[] { inBuffer[21], inBuffer[20], inBuffer[19], inBuffer[18] }, 0);
                    }


                    error = bytesTofloat(new byte[] { inBuffer[6], inBuffer[7], inBuffer[8], inBuffer[9] });

                    if (considerTolerance)
                    {
                        if (Math.Abs(error) > tol)
                        {
                            if (tryCount < MAX_RD20_TRY)
                            {
                                tryCount++;
                                port.DiscardInBuffer();
                                goto TRY;
                            }
                        }
                    }                   

                    break;
                default:
                    break;
            }

            return (error);
        }

        /// <summary>
        /// Connects to the GF333B standard and connects to the prescaller needed in conjunction with the GF333B.
        /// </summary>
        private void ConnectToGF333B(bool hasPrescaler)
        {
            StreamReader reader = new StreamReader("RS232.txt");
            int start, length;
            
            string stdPortName = reader.ReadLine();
            string stdBaudRate = reader.ReadLine();
            string stdDataSize = reader.ReadLine();
            string stdStopBits = reader.ReadLine();
            string stdParity = reader.ReadLine();
            string stdDtr = reader.ReadLine();
            string stdRts = reader.ReadLine();

            string prePortName = reader.ReadLine();
            string preBaudRate = reader.ReadLine();
            string preDataSize = reader.ReadLine();
            string preStopBits = reader.ReadLine();
            string preParity = reader.ReadLine();
            string preDtr = reader.ReadLine();
            string preRts = reader.ReadLine();

            reader.Close();

            start = stdPortName.IndexOf("=") + 1;
            length = stdPortName.Length - start;

            stdPortName = stdPortName.Substring(start, length);

            start = stdBaudRate.IndexOf("=") + 1;
            length = stdBaudRate.Length - start;

            int _stdBaudRate = Convert.ToInt32(stdBaudRate.Substring(start, length));

            start = stdDataSize.IndexOf("=") + 1;
            length = stdDataSize.Length - start;

            int _stdDataSize = Convert.ToInt32(stdDataSize.Substring(start, length));

            start = stdStopBits.IndexOf("=") + 1;
            length = stdStopBits.Length - start;

            int _stdStopBits = Convert.ToInt32(stdStopBits.Substring(start, length));

            start = stdParity.IndexOf("=") + 1;
            length = stdParity.Length - start;

            int _stdParity = Convert.ToInt32(stdParity.Substring(start, length));

            bool _stdDtr = false, _stdRts = false;

            start = stdDtr.IndexOf("=") + 1;
            length = stdDtr.Length - start;

            stdDtr = stdDtr.Substring(start, length);
            if (stdDtr.Equals("TRUE")) _stdDtr = true;

            start = stdRts.IndexOf("=") + 1;
            length = stdRts.Length - start;

            stdRts = stdRts.Substring(start, length);
            if (stdRts.Equals("TRUE")) _stdRts = true;




            start = prePortName.IndexOf("=") + 1;
            length = prePortName.Length - start;

            prePortName = prePortName.Substring(start, length);

            start = preBaudRate.IndexOf("=") + 1;
            length = preBaudRate.Length - start;

            int _preBaudRate = Convert.ToInt32(preBaudRate.Substring(start, length));

            start = preDataSize.IndexOf("=") + 1;
            length = preDataSize.Length - start;

            int _preDataSize = Convert.ToInt32(preDataSize.Substring(start, length));

            start = preStopBits.IndexOf("=") + 1;
            length = preStopBits.Length - start;

            int _preStopBits = Convert.ToInt32(preStopBits.Substring(start, length));

            start = preParity.IndexOf("=") + 1;
            length = preParity.Length - start;

            int _preParity = Convert.ToInt32(preParity.Substring(start, length));

            bool _preDtr = false, _preRts = false;

            start = preDtr.IndexOf("=") + 1;
            length = preDtr.Length - start;

            preDtr = preDtr.Substring(start, length);
            if (preDtr.Equals("TRUE")) _preDtr = true;

            start = preRts.IndexOf("=") + 1;
            length = preRts.Length - start;

            preRts = preRts.Substring(start, length);
            if (preRts.Equals("TRUE")) _preRts = true;


            port = new SerialPort
            {
                PortName = stdPortName,
                BaudRate = _stdBaudRate,
                DataBits = _stdDataSize,
                StopBits = (StopBits)_stdStopBits,
                Parity = (Parity)_stdParity,
                DtrEnable = _stdDtr,
                RtsEnable = _stdRts,
                ReadTimeout = 5000
            };
            port.Open();

            if (port.IsOpen)
            {
                new SerialPortReader(port, 1024, NewDataReceivedForGF333B, ErrorOnReceiveGF333B);
            }

            try
            {
                GetMeasures();
            }
            catch (Exception)
            {
                throw new IOException("Falha na comunicação com padrão GF333B");
            }


            if (hasPrescaler)
            {
                portCounter = new SerialPort
                {
                    PortName = prePortName,
                    BaudRate = _preBaudRate,
                    DataBits = _preDataSize,
                    StopBits = (StopBits)_preStopBits,
                    Parity = (Parity)_preParity,
                    DtrEnable = _preDtr,
                    RtsEnable = _preRts,
                    ReadTimeout = 5000
                };

                portCounter.Open();

                if (portCounter.IsOpen)
                {
                    new SerialPortReader(portCounter, myStdBuffer.Length, NewDataReceivedFromPrescaler, ErrorOnReceivePrescaler);
                }

                try
                {
                    if (!SendCommandToPrescaller(1, 324))
                    {
                        throw new IOException("Problemas na comunicação com o prescaler.");
                    }
                }
                catch (Exception)
                {
                    throw new IOException("Problemas na comunicação com o prescaler.");
                }
            }

        }

        /// <summary>
        /// Connects to the Radian RD20 standard.
        /// </summary>
        private void ConnectToRD20()
        {
            StreamReader reader = new StreamReader("RS232.txt");
            int start, length;

            string stdPortName = reader.ReadLine();
            string stdBaudRate = reader.ReadLine();
            string stdDataSize = reader.ReadLine();
            string stdStopBits = reader.ReadLine();
            string stdParity = reader.ReadLine();
            string stdDtr = reader.ReadLine();
            string stdRts = reader.ReadLine();            

            reader.Close();

            start = stdPortName.IndexOf("=") + 1;
            length = stdPortName.Length - start;

            stdPortName = stdPortName.Substring(start, length);

            start = stdBaudRate.IndexOf("=") + 1;
            length = stdBaudRate.Length - start;

            int _stdBaudRate = Convert.ToInt32(stdBaudRate.Substring(start, length));

            start = stdDataSize.IndexOf("=") + 1;
            length = stdDataSize.Length - start;

            int _stdDataSize = Convert.ToInt32(stdDataSize.Substring(start, length));

            start = stdStopBits.IndexOf("=") + 1;
            length = stdStopBits.Length - start;

            int _stdStopBits = Convert.ToInt32(stdStopBits.Substring(start, length));

            start = stdParity.IndexOf("=") + 1;
            length = stdParity.Length - start;

            int _stdParity = Convert.ToInt32(stdParity.Substring(start, length));

            bool _stdDtr = false, _stdRts = false;

            start = stdDtr.IndexOf("=") + 1;
            length = stdDtr.Length - start;

            stdDtr = stdDtr.Substring(start, length);
            if (stdDtr.Equals("TRUE")) _stdDtr = true;

            start = stdRts.IndexOf("=") + 1;
            length = stdRts.Length - start;

            stdRts = stdRts.Substring(start, length);
            if (stdRts.Equals("TRUE")) _stdRts = true;            

            port = new SerialPort
            {
                PortName = stdPortName,
                BaudRate = _stdBaudRate,
                DataBits = _stdDataSize,
                StopBits = (StopBits)_stdStopBits,
                Parity = (Parity)_stdParity,
                DtrEnable = _stdDtr,
                RtsEnable = _stdRts,
                ReadTimeout = 5000
            };
            port.Open();

            if (port.IsOpen)
            {
                try
                {
                    GetMeasures();
                }
                catch (Exception)
                {
                    throw new IOException("Falha na comunicação com padrão RD-20");
                }
            }

            
        }

        /// <summary>
        /// Connects to the Zera RMM3006 standard.
        /// </summary>
        private void ConnectToRMM3006(bool hasPrescaler)
        {
            StreamReader reader = new StreamReader("RS232.txt");
            int start, length;

            string stdPortName = reader.ReadLine();
            string stdBaudRate = reader.ReadLine();
            string stdDataSize = reader.ReadLine();
            string stdStopBits = reader.ReadLine();
            string stdParity = reader.ReadLine();
            string stdDtr = reader.ReadLine();
            string stdRts = reader.ReadLine();

            string prePortName = reader.ReadLine();
            string preBaudRate = reader.ReadLine();
            string preDataSize = reader.ReadLine();
            string preStopBits = reader.ReadLine();
            string preParity = reader.ReadLine();
            string preDtr = reader.ReadLine();
            string preRts = reader.ReadLine();

            reader.Close();

            start = stdPortName.IndexOf("=") + 1;
            length = stdPortName.Length - start;

            stdPortName = stdPortName.Substring(start, length);

            start = stdBaudRate.IndexOf("=") + 1;
            length = stdBaudRate.Length - start;

            int _stdBaudRate = Convert.ToInt32(stdBaudRate.Substring(start, length));

            start = stdDataSize.IndexOf("=") + 1;
            length = stdDataSize.Length - start;

            int _stdDataSize = Convert.ToInt32(stdDataSize.Substring(start, length));

            start = stdStopBits.IndexOf("=") + 1;
            length = stdStopBits.Length - start;

            int _stdStopBits = Convert.ToInt32(stdStopBits.Substring(start, length));

            start = stdParity.IndexOf("=") + 1;
            length = stdParity.Length - start;

            int _stdParity = Convert.ToInt32(stdParity.Substring(start, length));

            bool _stdDtr = false, _stdRts = false;

            start = stdDtr.IndexOf("=") + 1;
            length = stdDtr.Length - start;

            stdDtr = stdDtr.Substring(start, length);
            if (stdDtr.Equals("TRUE")) _stdDtr = true;

            start = stdRts.IndexOf("=") + 1;
            length = stdRts.Length - start;

            stdRts = stdRts.Substring(start, length);
            if (stdRts.Equals("TRUE")) _stdRts = true;




            start = prePortName.IndexOf("=") + 1;
            length = prePortName.Length - start;

            prePortName = prePortName.Substring(start, length);

            start = preBaudRate.IndexOf("=") + 1;
            length = preBaudRate.Length - start;

            int _preBaudRate = Convert.ToInt32(preBaudRate.Substring(start, length));

            start = preDataSize.IndexOf("=") + 1;
            length = preDataSize.Length - start;

            int _preDataSize = Convert.ToInt32(preDataSize.Substring(start, length));

            start = preStopBits.IndexOf("=") + 1;
            length = preStopBits.Length - start;

            int _preStopBits = Convert.ToInt32(preStopBits.Substring(start, length));

            start = preParity.IndexOf("=") + 1;
            length = preParity.Length - start;

            int _preParity = Convert.ToInt32(preParity.Substring(start, length));

            bool _preDtr = false, _preRts = false;

            start = preDtr.IndexOf("=") + 1;
            length = preDtr.Length - start;

            preDtr = preDtr.Substring(start, length);
            if (preDtr.Equals("TRUE")) _preDtr = true;

            start = preRts.IndexOf("=") + 1;
            length = preRts.Length - start;

            preRts = preRts.Substring(start, length);
            if (preRts.Equals("TRUE")) _preRts = true;



            port = new SerialPort
            {
                PortName = stdPortName,
                BaudRate = _stdBaudRate,
                DataBits = _stdDataSize,
                StopBits = (StopBits)_stdStopBits,
                Parity = (Parity)_stdParity,
                DtrEnable = _stdDtr,
                RtsEnable = _stdRts,
                ReadTimeout = 5000
            };
            port.Open();

            if (port.IsOpen)
            {
                new SerialPortReader(port, 1024, NewDataReceivedForRMM3006, ErrorOnReceiveRMM3006);
            }

            try
            {
                GetMeasures();
            }
            catch (Exception)
            {
                throw new IOException("Falha na comunicação com padrão GF333B");
            }


            if (hasPrescaler)
            {
                portCounter = new SerialPort
                {
                    PortName = prePortName,
                    BaudRate = _preBaudRate,
                    DataBits = _preDataSize,
                    StopBits = (StopBits)_preStopBits,
                    Parity = (Parity)_preParity,
                    DtrEnable = _preDtr,
                    RtsEnable = _preRts,
                    ReadTimeout = 5000
                };

                portCounter.Open();

                if (portCounter.IsOpen)
                {
                    new SerialPortReader(portCounter, myStdBuffer.Length, NewDataReceivedFromPrescaler, ErrorOnReceivePrescaler);
                }

                try
                {
                    if (!SendCommandToPrescaller(1, 324))
                    {
                        throw new IOException("Problemas na comunicação com o prescaler.");
                    }
                }
                catch (Exception)
                {
                    throw new IOException("Problemas na comunicação com o prescaler.");
                }
            }
        }

        private void NewDataReceivedForRMM3006(byte[] dataReceived)
        {
        
            Interlocked.Exchange(ref rMM30006_Answer,
                rMM30006_Answer + Encoding.ASCII.GetString(dataReceived));
        }

        private void NewDataReceivedForGF333B(byte[] dataReceived)
        {

            Interlocked.Exchange(ref gf333B_Answer,
                gf333B_Answer + Encoding.ASCII.GetString(dataReceived));

            if ((gf333B_Answer.Length > 2) && (!gf333B_Answer.Contains("##")))
            {
                Interlocked.Exchange(ref gf333B_Answer, "");
            }
        }

        private void ErrorOnReceiveRMM3006(Exception ex)
        {

        }

        private void ErrorOnReceiveGF333B(Exception ex)
        {

        }

        private void NewDataReceivedFromPrescaler(byte[] dataReceived)
        {
            Buffer.BlockCopy(dataReceived, 0, myStdBuffer, mIndex, dataReceived.Length);

            Interlocked.Exchange( ref mIndex,
                (mIndex + dataReceived.Length) % myStdBuffer.Length );
        }

        private void ErrorOnReceivePrescaler(Exception ex)
        {

        }

        /// <summary>
        /// Sends a command to the prescaller and verifies its answer.
        /// </summary>
        /// <param name="command">The command code.</param>
        /// <param name="data">The data to be sent with the command.</param>
        /// <returns>True if the answer is correct, false otherwise.</returns>
        private bool SendCommandToPrescaller(int command, long data)
        {
            bool keepTrying;
            //
            // Instantiates a buffer to be sent to the prescaller according to its protocol
            //
            byte[] buffer = new byte[] { 0x09,
                                        (byte)command,
                                        (byte)((data & 0x000000FF00000000) >> 32),
                                        (byte)((data & 0x00000000FF000000) >> 24),
                                        (byte)((data & 0x0000000000FF0000) >> 16),
                                        (byte)((data & 0x000000000000FF00) >> 8),
                                        (byte)( data & 0x00000000000000FF),
                                        0x00,
                                        0x00 };
            //
            // Calculates the checksum to be sent
            //
            int checkSum = 0, i;
            for (i = 0; i < 7; i++) checkSum += buffer[i];
            buffer[7] = (byte)((checkSum >> 8) & 0xFF);
            buffer[8] = (byte)(checkSum & 0xFF);
            int tryCount = 0;

            //
            // Clears the outbuffer and attempts to send the buffer
            //
            int framesize = 9;
            while (true)
            {
                keepTrying = false;
                //
                //  Clears used variables
                //
                Interlocked.Exchange(ref mIndex, 0);
                myStdBuffer = new byte[1024];

                portCounter.DiscardOutBuffer();
                portCounter.Write(buffer, 0, buffer.Length);

                uint ticks = GetTickCount();
                while (mIndex < framesize)
                {
                    if (GetTickCount() - ticks >= portCounter.ReadTimeout)
                    {
                        if (tryCount < MAX_PRESCALER_TRY)
                        {
                            tryCount++;
                            keepTrying = true;
                            break;
                        }
                        else throw new IOException("Problemas na comunicação com o prescaler");
                    }
                }

                if (!keepTrying) break;
            }

            byte[] inBuffer = new byte[framesize];

            Buffer.BlockCopy(myStdBuffer, 0, inBuffer, 0, framesize);

            return (CheckArrays(buffer, inBuffer));
        }

        private int getNPP()
        {
            byte[] buffer = new byte[] { 0x09, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12 };

            portCounter.DiscardOutBuffer();
            portCounter.Write(buffer, 0, buffer.Length);

            //
            // Reads the answer according to the prescaller's protocol and clears the input buffer
            //
            int i;
            for (i = 0; i < 9; i++) portCounter.Read(buffer, i, 1);
            portCounter.DiscardInBuffer();

            //
            // Returns the result of the comparison between the checksums
            //
            
            return (BitConverter.ToInt32(new byte[] { buffer[6], buffer[5], buffer[4], buffer[3] }, 0));
        }

        private int getNPA()
        {
            byte[] buffer = new byte[] { 0x09, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x13 };

            portCounter.DiscardOutBuffer();
            portCounter.Write(buffer, 0, buffer.Length);

            //
            // Reads the answer according to the prescaller's protocol and clears the input buffer
            //
            int i;
            for (i = 0; i < 9; i++) portCounter.Read(buffer, i, 1);
            portCounter.DiscardInBuffer();

            //
            // Returns the result of the comparison between the checksums
            //
            return (BitConverter.ToInt32(new byte[] { buffer[6], buffer[5], buffer[4], buffer[3] }, 0));
        }

        /// <summary>
        /// Converts a hexadecimal array to a floating point number.
        /// </summary>
        /// <param name="str">The string to be converted.</param>
        /// <returns>The floating point number.</returns>
        private float ASCIItoFloat(string str)
        {
            uint num;
            byte[] floatVal;
            
            num = uint.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            
            floatVal = BitConverter.GetBytes(num);
            return (BitConverter.ToSingle(floatVal, 0));
        }

        /// <summary>
        /// Converts ASCII hex to an integer.
        /// </summary>
        /// <param name="str">The hex integer in string format.</param>
        /// <returns>The corresponding integer.</returns>
        private int ASCIItoInteger(string str)
        {
            int num;

            num = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            
            return num;
        }

        /// <summary>
        /// Converts a floating point to a hexadecimal array, or string.
        /// </summary>
        /// <param name="value">The floating point number to be converted.</param>
        /// <returns>An string representing the float.</returns>
        private string FloatToASCII(float value)
        {
            byte[] buffer;
            byte[] aux = new byte[4];
            string str;
            buffer = BitConverter.GetBytes(value);
            aux[0] = buffer[3];
            aux[1] = buffer[2];
            aux[2] = buffer[1];
            aux[3] = buffer[0];
            str = BitConverter.ToString(aux).Replace("-", "");
            return (str);
        }

        /// <summary>
        /// Converts an unsigned short (16 bit length unsigned integer) to a hexadecimal array, or string.
        /// </summary>
        /// <param name="value">The integer number to be converted.</param>
        /// <returns>An string representing the ushort.</returns>
        private string UShortToASCII(ushort value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            byte[] aux = new byte[2];
            aux[0] = buffer[1];
            aux[1] = buffer[0];
            string str = BitConverter.ToString(aux).Replace("-", "");
            return (str);
        }

        /// <summary>
        /// Converts an unsigned int (32 bit length unsigned integer) to a hexadecimal array, or string.
        /// </summary>
        /// <param name="value">The integer number to be converted.</param>
        /// <returns>An string representing the uint.</returns>
        private string UIntToASCII(uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            byte[] aux = new byte[4];
            aux[0] = buffer[3];
            aux[1] = buffer[2];
            aux[2] = buffer[1];
            aux[3] = buffer[0];
            string str = BitConverter.ToString(aux).Replace("-", "");
            return (str);
        }

        /// <summary>
        /// Method that returns the system ticks counting.
        /// </summary>
        /// <returns>
        /// An unsigned integer representing the tick
        /// counting.
        /// </returns>
        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        /// <summary>
        /// Converts an array of 4 bytes representing a TI float into an actual IEEE 754 floating point number.
        /// </summary>
        /// <param name="data">The 4 bytes array of the TI float. It must be in Big Endian</param>
        /// <returns>A IEEE 754 float</returns>
        [DllImport("lib_RD20/RD20COMM.dll", EntryPoint = "DDeviceMemStructsbytesToFloatTMS320@4")]
        static extern float bytesTofloat(byte[] data);

        /// <summary>
        /// Converts an IEEE 754 floating point number into and array of 4 bytes representing a TI float.
        /// </summary>
        /// <param name="value">A floating point number in the IEEE 754 format.</param>
        /// <param name="data">The array to keep the bytes of the TI float.</param>
        [DllImport("lib_RD20/RD20COMM.dll", EntryPoint = "DDeviceMemStructsfloatToBytesTMS320@8")]
        static extern void floatTobytes(float value, byte[] data);

        /// <summary>
        /// Calculates the unsigned sum of the bytes of a byte array.
        /// </summary>
        /// <param name="data">An array of bytes to be summed.</param>
        /// <param name="size">The number of bytes to be summed.</param>
        /// <returns>The calculated checksum.</returns>
        [DllImport("lib_RD20/RD20COMM.dll", EntryPoint = "CalculateChecksum@8")]
        static extern uint CheckSum(byte[] data, int size);

        /// <summary>
        /// Compares two arrays element by element.
        /// </summary>
        /// <param name="a1">The first array.</param>
        /// <param name="a2">The second array.</param>
        /// <returns>True if the arrays are identic, false otherwise.</returns>
        private bool CheckArrays(Array a1, Array a2)
        {
            //
            // Checks if the arrays are the same reference
            //
            if (a1 == a2) return (true);

            //
            // Checks if one of them is a null reference
            //
            if ((a1 == null) || (a2 == null)) return (false);

            //
            // Checks if they have the same length
            //
            if (a1.Length != a2.Length) return (false);

            //
            // Verifies each element of the arrays and if the arrays are not identical to each other
            // returns false
            //
            int i;
            for (i = 0; i < a1.Length; i++)
            {
                if ((byte)a1.GetValue(i) != (byte)a2.GetValue(i)) return (false);
            }

            //
            // If there is no problem returns true
            //
            return (true);
        }

        private string GF333BCheckSum(string str)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            uint cSum = 0x0;
            //calcula checksum
            foreach (byte b in buffer) cSum += b;
            buffer = BitConverter.GetBytes(cSum);
            string ret = BitConverter.ToString(buffer).Replace("-", "");
            return (ret.Substring(0, 2));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                try
                {
                    if (handle != null) handle.Dispose();
                    if (port != null) port.Dispose();
                    if (portCounter != null) portCounter.Dispose();
                }
                catch (Exception) { }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Disposes this instace of the StandardMeter class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
