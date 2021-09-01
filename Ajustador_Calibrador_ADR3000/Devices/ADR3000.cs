using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    public class ADR3000 : IDisposable
    {
        //
        // Private constants
        //
        private const string WRONG_ANSWER = "Resposta incorreta do ADR 3000.";
        private const string ACCURACY_TEST = "Teste de exatidão finalizado.";
        private const string WRONG_CHECKSUM = "Checksum do ADR incorreto.";
        private const string PARAM_LIMITS = "Parâmetro fora dos limites permitidos.";
        private readonly string[] ADR_ERROR =
        {
            "",                                         //0x00
            "Erro ADR: Cheksum incorreto.",             //0x01
            "Erro ADR: Comando inválido.",              //0x02
            "Erro ADR: Permissão negada.",              //0x03
            "Erro ADR: Timeout.",                       //0x04
            "Erro ADR: Header incorreto",               //0x05
            "Erro ADR: Erro de tensão.",                //0x06
            "",                                         //0x07
            "Erro ADR: Comunicação com STPM34.",        //0x08
            "Erro ADR: Gate driver.",                   //0x09
            "Erro ADR: Parâmetro incorreto.",           //0x0A
            "",                                         //0x0B
            "",                                         //0x0C
            "",                                         //0x0D
            "",                                         //0x0E
            "",                                         //0x0F
            "Erro ADR: EEPROM.",                        //0x10
            "Erro ADR: Shunt aberto.",                  //0x11
            "Erro ADR: Conexão com shunt incorreta.",   //0x12
            "Erro ADR: Fase invertida na tensão.",      //0x13
            "Erro ADR: Bobina de tensão aberta.",       //0x14
            "Erro ADR: Bobina de tensão em curto."      //0x15
        };
        private const float MAX_AMP = 45.0f;
        private const float MAX_VOLTAGE = 300.0f;
        private const float MIN_PHI = -180.0f;
        private const float MAX_PHI = 180.0f;
        private const int MIN_TURNS = 1, MAX_TURNS = 65535;
        private const int MIN_SERIAL = 0, MAX_SERIAL = 9999;
        private const int MAX_REPORT_LENGTH = 16;
        private const int MAX_SHELF = 65535;
        private const int MAX_EEPROM_ADDRESS = 512;
        private const int MAX_LPW = 15;

        //
        // Private classes and variables to be used
        //
        private readonly NetworkStream stream; //Stream to handle communications
        //
        // Variables to handle disposing features
        //
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        //
        // Definition of a class to store the result of an accuracy test
        //
        public class AccuracyTestResult
        {
            public float Error { get; }
            public float KdEst{ get; }
            public string Report{ get; }
            public int CalibrationDate{ get; }
            public int CalibrationShelfLife{ get; }
            public int SerialNum{ get; }
            public float Energy{ get; }

            public AccuracyTestResult(byte[] buffer)
            {
                Error = BitConverter.ToSingle(new byte[] { buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                KdEst = BitConverter.ToSingle(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6] }, 0);
                Report = Encoding.ASCII.GetString(buffer, 10, 16);
                CalibrationDate = BitConverter.ToInt32(new byte[] { buffer[29], buffer[28], buffer[27], buffer[26] }, 0);
                CalibrationShelfLife = BitConverter.ToInt16(new byte[] { buffer[31], buffer[30] }, 0);
                SerialNum = BitConverter.ToInt16(new byte[] { buffer[33], buffer[32] }, 0);
                Energy = BitConverter.ToSingle(new byte[] { buffer[37], buffer[36], buffer[35], buffer[34] }, 0);
            }
        }

        //
        // Properties
        //
        public int ReadTimeout //Timeout for reading operations
        {
            get
            {
                return stream.ReadTimeout;
            }
            set
            {
                stream.ReadTimeout = value;
            }
        }
        public AccuracyTestResult MyAccuracyTestResult { get; private set; }

        //
        // Public constants
        //
        public const int V = 0;
        public const int I = 1;
        public const int P = 2;
        public const int S = 3;
        public const int Q = 4;
        public const int FP = 5;
        public const int F = 6;
        public const int PHI = 7;
        public const int NV = 8;
        public const int ENE = 9;

        public const int VOLTAGE_TH = 30;

        public const int _8b = 0;
        public const int _16b = 1;

        public const byte ACTIVE_ENERGY = 0;
        public const byte REACTIVE_ENERGY = 1;
        public const byte START_END = 0;
        public const byte PULSES_LAPS = 1;
        public const byte SINGLE_PHASE = 1;
        public const byte BIPHASIC = 2;
        public const byte THREE_PHASE = 3;
        public const byte _50Hz = 0;
        public const byte _60Hz = 1;
        public const byte TYPE_ACTIVE = 0;
        public const byte TYPE_REACTIVE = 2;
        public const byte UNBLOCKED = 0;
        public const byte BLOCKED = 1;
        public const byte ADJ_ON = 1;

        public const int CHV1  = 113;
        public const int CHV2  = 115;
        public const int CHC1H  = 156; //0x009C
        public const int CHC1L  = 151; //0x0097
        public const int CHC2H  = 166; //0x00A6
        public const int CHC2L  = 161; //0x00A1
        public const int PHV1H  = 158; //0x009E
        public const int PHV1L  = 153; //0x0099
        public const int PHV2H  = 168; //0x00A8
        public const int PHV2L  = 163; //0x00A3
        public const int PHC1H  = 159; //0x009F
        public const int PHC1L  = 154; //0x009A
        public const int PHC2H  = 169; //0x00A9
        public const int PHC2L  = 164; //0x00A4
        public const int OFA1  = 127;
        public const int OFA2  = 129;
        public const int OFAF1  = 131;
        public const int OFAF2  = 133;
        public const int OFS1  = 135;
        public const int OFS2  = 137;
        public const int OFR1  = 139;
        public const int OFR2  = 141;


        public const int EE_CHV1_0_LITE = 147;
        public const int EE_CHV1_1_LITE = 149;
        public const int EE_PHV1_0_LITE = 151;
        public const int EE_PHV1_1_LITE = 152;
        public const int EE_CHC10_0_LITE = 153;
        public const int EE_CHC10_1_LITE = 155;
        public const int EE_CHC10_2_LITE = 157;
        public const int EE_CHC10_3_LITE = 159;
        public const int EE_CHC11_0_LITE = 161;
        public const int EE_CHC11_1_LITE = 163;
        public const int EE_CHC11_2_LITE = 165;
        public const int EE_CHC11_3_LITE = 167;
        public const int EE_CHANNEL_OFFSET_LITE = 46;
        /// <summary>
        /// Constructor for the ADR3000 class.
        /// </summary>
        /// <param name="_stream">The stream that handles the communication with the ADR3000 device.</param>
        public ADR3000(NetworkStream _stream)
        {
            stream = _stream;
            if (stream != null) stream.ReadTimeout = 7000;
        }

        /// <summary>
        /// Eco for communication tests.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void Nop()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x80, 0x00, 0x00, 0x80 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x80, 0x00, 0x00, 0x80 })) throw new IOException(WRONG_ANSWER);

        }

        /// <summary>
        /// Gets the set of measurements that the ADR is measuring. This method also
        /// checks if an accuracy test has been finalized.
        /// </summary>
        /// <returns>The array of measures.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ApplicationException"/>
        public float[] GetMeasures()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x88, 0x00, 0x00, 0x88 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);

            buffer = new byte[256];
            int numOfBytesRead = stream.Read(buffer, 0, 1);
            numOfBytesRead += stream.Read(buffer, numOfBytesRead, 1);

            int numOfBytesToBeRead = buffer[1] + 4; // buffer[1] contains the length of the data
            while (numOfBytesRead < numOfBytesToBeRead) numOfBytesRead += stream.Read(buffer, numOfBytesRead, numOfBytesToBeRead - numOfBytesRead);

            //
            // Extracts the checksum and compares with the real checksum
            //
            int checkSum = BitConverter.ToInt32(new byte[] { buffer[numOfBytesRead - 1], buffer[numOfBytesRead - 2], 0, 0 }, 0);

            if (!VerifyChecksum(buffer, checkSum, numOfBytesRead - 2)) throw new IOException(WRONG_CHECKSUM);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Return value
            //
            float[] adrMeasures = new float[] { 0.0f };
            
            //
            // Checks if the answer is a set of measures or if it's a test result
            //
            switch (buffer[0] & 0x7F)
            {
                case 0x08:
                    adrMeasures = new float[10];
                    byte[] measures = new byte[38];
                    Buffer.BlockCopy(buffer, 2, measures, 0, measures.Length);
                    int j, k = 0;
                    for (j = 0; j < 32; j += 4)
                    {
                        adrMeasures[k++] = BitConverter.ToSingle(new byte[] { measures[j + 3], measures[j + 2], measures[j + 1], measures[j] }, 0);
                    }
                    adrMeasures[k++] = (((measures[j] << 8) & 0xFF00) | (measures[j + 1] & 0xFF));
                    j += 2;
                    adrMeasures[k] = BitConverter.ToSingle(new byte[] { measures[j + 3], measures[j + 2], measures[j + 1], measures[j] }, 0);

                    break;
                case 0x11:
                    MyAccuracyTestResult = new AccuracyTestResult(buffer);
                    throw new ApplicationException(ACCURACY_TEST);
            }

            return (adrMeasures);
        }

        /// <summary>
        /// Sets the RMS value of the current that the ADR 3000 will generate.
        /// </summary>
        /// <param name="setPoint">A floating point representing the RMS value.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetCurrentSetPoint(float setPoint)
        {
            //
            // Check the value of the parameter passed
            //
            if ((setPoint < 0.0f) || (setPoint > MAX_AMP)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bSetPoint = BitConverter.GetBytes(setPoint);

            byte[] buffer = new byte[8];
            buffer[0] = 0x02;
            buffer[1] = 0x04;
            buffer[2] = bSetPoint[3];
            buffer[3] = bSetPoint[2];
            buffer[4] = bSetPoint[1];
            buffer[5] = bSetPoint[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];
            
            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i< 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x82, 0x00, 0x00, 0x82 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the RMS value of the voltage that the ADR 3000 will generate.
        /// </summary>
        /// <param name="setPoint">A floating point representing the RMS value.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetVoltageSetPoint(float setPoint)
        {
            //
            // Check the value of the parameter passed
            //
            if ((setPoint < 0.0f) || (setPoint > MAX_VOLTAGE)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bSetPoint = BitConverter.GetBytes(setPoint);

            byte[] buffer = new byte[8];
            buffer[0] = 0x01;
            buffer[1] = 0x04;
            buffer[2] = bSetPoint[3];
            buffer[3] = bSetPoint[2];
            buffer[4] = bSetPoint[1];
            buffer[5] = bSetPoint[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x81, 0x00, 0x00, 0x81 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the value of the angle between the voltage and the current that the ADR 3000 will generate.
        /// </summary>
        /// <param name="setPoint">A floating point representing the angle value in degrees.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetPhiSetPoint(float setPoint)
        {
            //
            // Check the value of the parameter passed
            //
            if ((setPoint < MIN_PHI) || (setPoint > MAX_PHI)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bSetPoint = BitConverter.GetBytes(setPoint);

            byte[] buffer = new byte[8];
            buffer[0] = 0x07;
            buffer[1] = 0x04;
            buffer[2] = bSetPoint[3];
            buffer[3] = bSetPoint[2];
            buffer[4] = bSetPoint[1];
            buffer[5] = bSetPoint[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x87, 0x00, 0x00, 0x87 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Stops current generation and opens the current circuit.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void TurnOffI()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x84, 0x00, 0x00, 0x84 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x84, 0x00, 0x00, 0x84 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Stops voltage generation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void TurnOffV()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x83, 0x00, 0x00, 0x83 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x83, 0x00, 0x00, 0x83 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Close the current circuit and starts current generation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void TurnOnI()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x86, 0x00, 0x00, 0x86 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x86, 0x00, 0x00, 0x86 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Starts voltage generation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void TurnOnV()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x85, 0x00, 0x00, 0x85 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x85, 0x00, 0x00, 0x85 })) throw new IOException(WRONG_ANSWER);
        }

        public void CancelTest()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x99, 0x00, 0x00, 0x99 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x99, 0x00, 0x00, 0x99 })) throw new IOException(WRONG_ANSWER);
        }

        public void SetEEPROMProtectionState(byte state)
        {
            //
            // Check the value of the parameter passed
            //
            if (state > BLOCKED) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x20, 0x01, state, 0x00, (byte)(state + 0x21) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xA0, 0x00, 0x00, 0xA0 })) throw new IOException(WRONG_ANSWER);
        }

        public void SetAdjustModeState(byte state)
        {
            //
            // Check the value of the parameter passed
            //
            if (state > ADJ_ON) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x36, 0x01, state, 0x00, (byte)(state + 0x37) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xB6, 0x00, 0x00, 0xB6 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets nature of the energy to be tested by the ADR.
        /// </summary>
        /// <param name="energyType">The nature of the energy. It can be ACTIVE_ENERGY or REACTIVE_ENERGY</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetEnergyType(byte energyType)
        {
            //
            // Check the value of the parameter passed
            //
            if (energyType > 1) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x0C, 0x01, energyType, 0x00, (byte)(energyType + 0x0D) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x8C, 0x00, 0x00, 0x8C })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets nature of the pulse counting to be done by the ADR 3000.
        /// </summary>
        /// <param name="countingType">The type of counting. It can be START_END or PULSES_LAPS</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetCountingType(byte countingType)
        {
            //
            // Check the value of the parameter passed
            //
            if (countingType > 1) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x0D, 0x01, countingType, 0x00, (byte)(countingType + 0x0E) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x8D, 0x00, 0x00, 0x8D })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the number of elements.
        /// </summary>
        /// <param name="numOfElements">The number of elements. It can be SINGLE_PHASE, BIPHASIC or THREE_PHASE</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetNumberOfElements(byte numOfElements)
        {
            //
            // Check the value of the parameter passed
            //
            if ((numOfElements < SINGLE_PHASE) || (numOfElements > THREE_PHASE)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x0E, 0x01, numOfElements, 0x00, (byte)(numOfElements + 0x0F) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x8E, 0x00, 0x00, 0x8E })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the Wh/pulse constant of the pulse being inputed to the ADR 3000 (The watt-hour meter constant).
        /// </summary>
        /// <param name="kd">The constant in Wh/pulse.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetKd(float kd)
        {
            //
            // Check the value of the parameter passed
            //
            if (kd < 0.0f) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bKd = BitConverter.GetBytes(kd);

            byte[] buffer = new byte[8];
            buffer[0] = 0x0B;
            buffer[1] = 0x04;
            buffer[2] = bKd[3];
            buffer[3] = bKd[2];
            buffer[4] = bKd[1];
            buffer[5] = bKd[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x8B, 0x00, 0x00, 0x8B })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the number of pulses or laps to be performed in an accuracy test.
        /// </summary>
        /// <param name="numberOfTurns">The number of pulses or laps to be performed in an accuracy test.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetNumberOfTurns(int numberOfTurns)
        {
            //
            // Check the value of the parameter passed
            //
            if ((numberOfTurns < MIN_TURNS) || (numberOfTurns > MAX_TURNS)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bNv = BitConverter.GetBytes(numberOfTurns);

            byte[] buffer = new byte[6];
            buffer[0] = 0x09;
            buffer[1] = 0x02;
            buffer[2] = bNv[1];
            buffer[3] = bNv[0];

            int checkSum = 0, i;
            for (i = 0; i < 4; i++) checkSum += buffer[i];

            buffer[4] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[5] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x89, 0x00, 0x00, 0x89 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Increments the counting of turns in an accuracy test.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void IncrementActualTurns()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x8A, 0x00, 0x00, 0x8A };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x8A, 0x00, 0x00, 0x8A })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Starts an accuracy test.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void StartAccuracyTest()
        {
            //
            // Creates a buffer with the command to be sent to the device
            //
            byte[] buffer = new byte[] { 0x91, 0x00, 0x00, 0x91 };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);
            

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x91, 0x00, 0x00, 0x91 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the serial number of the ADR 3000.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetSerialNumber(int serialNumber)
        {
            //
            // Check the value of the parameter passed
            //
            if ((serialNumber < MIN_SERIAL) || (serialNumber > MAX_SERIAL)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bSerial = BitConverter.GetBytes(serialNumber);

            byte[] buffer = new byte[6];
            buffer[0] = 0x13;
            buffer[1] = 0x02;
            buffer[2] = bSerial[1];
            buffer[3] = bSerial[0];

            int checkSum = 0, i;
            for (i = 0; i < 4; i++) checkSum += buffer[i];

            buffer[4] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[5] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x93, 0x00, 0x00, 0x93 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the calibration report of the ADR 3000. It must have 16 characteres at maximum.
        /// </summary>
        /// <param name="calibrationReport">The report.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetCalibrationReport(string calibrationReport)
        {
            if ((calibrationReport.Length == 0) || (calibrationReport.Length > MAX_REPORT_LENGTH)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bReport = Encoding.ASCII.GetBytes(calibrationReport);

            int length = bReport.Length;

            byte[] buffer = new byte[4 + length];
            buffer[0] = 0x14;
            buffer[1] = (byte)length;
            Buffer.BlockCopy(bReport, 0, buffer, 2, bReport.Length);

            int checkSum = 0, i;
            for (i = 0; i < buffer.Length - 2; i++) checkSum += buffer[i];
            
            buffer[buffer.Length - 2] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[buffer.Length - 1] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x94, 0x00, 0x00, 0x94 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the date at wich the ADR 3000 was calibrated. 
        /// </summary>
        /// <param name="calibrationDate">An integer with the format: ddMMyyyy.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetCalibrationDate(int calibrationDate)
        {
            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bDate = BitConverter.GetBytes(calibrationDate);

            byte[] buffer = new byte[8];
            buffer[0] = 0x18;
            buffer[1] = 0x04;
            buffer[2] = bDate[3];
            buffer[3] = bDate[2];
            buffer[4] = bDate[1];
            buffer[5] = bDate[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x98, 0x00, 0x00, 0x98 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the RMS value of the rated current for the watt-hour meter.
        /// </summary>
        /// <param name="setPoint">A floating point representing the RMS value.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetNominalCurrent(float nominalCurrent)
        {
            //
            // Check the value of the parameter passed
            //
            if (nominalCurrent < 0.0f) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bNc = BitConverter.GetBytes(nominalCurrent);

            byte[] buffer = new byte[8];
            buffer[0] = 0x1B;
            buffer[1] = 0x04;
            buffer[2] = bNc[3];
            buffer[3] = bNc[2];
            buffer[4] = bNc[1];
            buffer[5] = bNc[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x9B, 0x00, 0x00, 0x9B })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the RMS value of the maximum current for the watt-hour meter.
        /// </summary>
        /// <param name="setPoint">A floating point representing the RMS value.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetMaximumCurrent(float maximumCurrent)
        {
            //
            // Check the value of the parameter passed
            //
            if (maximumCurrent < 0.0f) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bMc = BitConverter.GetBytes(maximumCurrent);

            byte[] buffer = new byte[8];
            buffer[0] = 0x1C;
            buffer[1] = 0x04;
            buffer[2] = bMc[3];
            buffer[3] = bMc[2];
            buffer[4] = bMc[1];
            buffer[5] = bMc[0];

            int checkSum = 0, i;
            for (i = 0; i < 6; i++) checkSum += buffer[i];

            buffer[6] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[7] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0x9C, 0x00, 0x00, 0x9C })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Configures the frequency of the pulses output of the ADR 3000.
        /// </summary>
        /// <param name="divider">The value to be written into the LPW register.</param>
        public void SetPulseOutputDivider(int divider)
        {
            //
            // Check the value of the parameter passed
            //
            if ((divider < 0) || (divider > MAX_LPW)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x27, 0x01, (byte)divider, 0x00, (byte)(divider + 0x28) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xA7, 0x00, 0x00, 0xA7 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Configures the type of energy that pulses out of the ADR 3000.
        /// </summary>
        /// <param name="energyType">The type of energy. Can be TYPE_ACTIVE or TYPE_REACTIVE.</param>
        public void SetPulseOutputEnergyType(byte energyType)
        {
            //
            // Check the value of the parameter passed
            //
            if ((energyType != TYPE_ACTIVE) && (energyType != TYPE_REACTIVE)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x2E, 0x01, energyType, 0x00, (byte)(energyType + 0x2F) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xAE, 0x00, 0x00, 0xAE })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets the shelf life of the calibration of the ADR 3000.
        /// </summary>
        /// <param name="shelfLife">The shelf life period.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetCalibrationShelfLife(int shelfLife)
        {
            //
            // Check the value of the parameter passed
            //
            if ((shelfLife < 0) || (shelfLife > MAX_SHELF)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] bShelf = new byte[] { (byte)((shelfLife >> 8) & 0xFF), (byte)(shelfLife & 0xFF) };

            byte[] buffer = new byte[8];
            buffer[0] = 0x25;
            buffer[1] = 0x02;
            buffer[2] = bShelf[0];
            buffer[3] = bShelf[1];

            int checkSum = 0, i;
            for (i = 0; i < 4; i++) checkSum += buffer[i];

            buffer[4] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[5] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xA5, 0x00, 0x00, 0xA5 })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Sets frequency for the signal to be generated.
        /// </summary>
        /// <param name="frequency">The frequency. It can be _50Hz or _60Hz</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void SetFrequency(byte frequency)
        {
            //
            // Check the value of the parameter passed
            //
            if (frequency > 1) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[] { 0x2F, 0x01, frequency, 0x00, (byte)(frequency + 0x30) };

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];

            int i;
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xAF, 0x00, 0x00, 0xAF })) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Takes 3 integer types and converts them to only one integer to store a date
        /// </summary>
        /// <param name="day">Day of the date.</param>
        /// <param name="month">Month of the date.</param>
        /// <param name="year">Year of the date.</param>
        /// <returns>The integer representing the whole date.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public int GetDate(long day, long month, long year)
        {
            if ((day < 0) || (month < 0) || (year < 0) || (year > 65535) || (month > 12)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            else if ((month == 1) || (month == 3) || (month == 5) || (month == 7) || (month == 8) || (month == 10) || (month == 12))
            {
                if (day > 31) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }
            else if ((month == 4) || (month == 6) || (month == 9) || (month == 11))
            {
                if (day > 30) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }
            if (month == 2)
            {
                bool isLeap = false;
                if ((year % 4) == 0)
                {
                    if ((year % 100) == 0)
                    {
                        if ((year % 400) == 0) isLeap = true;
                        else isLeap = false;
                    }
                    else isLeap = true;
                }
                else isLeap = false;

                if ((isLeap && (day > 29)) || ((!isLeap) && (day > 28))) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }

            long date = ((day << 24) & 0xFF000000);
            date |= ((month << 16) & 0xFF0000);
            date |= (year & 0xFFFF);

            return ((int)date);
        }

        /// <summary>
        /// Gets a factor from the EEPROM of the ADR 3000.
        /// </summary>
        /// <param name="address">The starting address of the factor.</param>
        /// <param name="factorSize">The size in bytes of the factor.</param>
        /// <returns>The factor as an unsigned int.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public int GetFactor(int address, int factorSize)
        {
            //
            // Check the value of the parameter passed
            //
            if ((factorSize != _8b) && (factorSize != _16b)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Reads the addresses in the EEPROM relative to the factor and gets the factor
            //
            byte[] factor;
            int retVal = 0;
            if (factorSize == _8b)
            {
                factor = ReadEEPROM((ushort)address, 1);
                retVal = factor[2];
            }
            else
            {
                factor = ReadEEPROM((ushort)address, 2);
                retVal = BitConverter.ToUInt16(factor, 2);
            }

            return (retVal);
        }

        /// <summary>
        /// Writes a factor to a specified address.
        /// </summary>
        /// <param name="address">The first address that data will be written into.</param>
        /// <param name="factor">The factor.</param>
        /// <param name="factorSize">The size of the factor. It must be _8b or _16b.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void WriteFactor(int address, int factor, int factorSize)
        {
            //
            // Check the value of the parameter passed
            //
            if ((factorSize != _8b) && (factorSize != _16b)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs a set of bytes according to the size of the factor
            //
            byte[] bFactor;
            if (factorSize == _8b) bFactor = BitConverter.GetBytes((byte)factor);
            else bFactor = BitConverter.GetBytes((ushort)factor);

            //
            // Attempts to write the factor into the EEPROM
            //
            WriteEEPROM((ushort)address, bFactor);
        }

        /// <summary>
        /// Calculates a checksum of a byte array and compares it with a previous value.
        /// </summary>
        /// <param name="buffer">The buffer with the elements to be summed.</param>
        /// <param name="checkSum">The previous value of the checksum.</param>
        /// <param name="length">The number of elements to be summed.</param>
        /// <returns>True if the sum equals the previous value, false otherwise.</returns>
        private bool VerifyChecksum(byte[] buffer, int checkSum, int length)
        {
            int cks = 0, i;

            for (i = 0; i < length; i++) cks += buffer[i];

            if (cks != checkSum) return (false);

            return (true);
        }

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

        /// <summary>
        /// Reads a number of bytes from the EEPROM.
        /// </summary>
        /// <param name="address">The first address to be read.</param>
        /// <param name="numOfBytes">The quantity of bytes to be read.</param>
        /// <returns>The answer from the device.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private byte[] ReadEEPROM(ushort address, byte numOfBytes)
        {
            //
            // Check the value of the parameter passed
            //
            if (address > MAX_EEPROM_ADDRESS) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] addr = BitConverter.GetBytes(address);
            byte[] buffer = new byte[] { 0x21, 0x03, addr[1], addr[0], numOfBytes, 0x00, 0x00 };

            int checkSum = 0;
            foreach(byte b in buffer) checkSum += b;

            buffer[buffer.Length - 2] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[buffer.Length - 1] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[numOfBytes + 4];

            int i;
            for (i = 0; i < 4 + numOfBytes; i++) stream.Read(buffer, i, 1);

            //
            // Extracts the checksum and compares with the real checksum
            //
            checkSum = BitConverter.ToInt32(new byte[] { buffer[buffer.Length - 1], buffer[buffer.Length - 2], 0, 0 }, 0);

            if (!VerifyChecksum(buffer, checkSum, buffer.Length - 2)) throw new IOException(WRONG_CHECKSUM);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            return (buffer);
        }

        /// <summary>
        /// Writes a set of bytes into the EEPROM.
        /// </summary>
        /// <param name="address">The first address that data will be written into.</param>
        /// <param name="data">The set of bytes to be written.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private void WriteEEPROM(ushort address, byte[] data) 
        {
            //
            // Check the value of the parameter passed
            //
            if ((address > MAX_EEPROM_ADDRESS) || (data.Length == 0)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);

            //
            // Constructs the frame to be sent to the ADR 3000
            //
            byte[] buffer = new byte[6 + data.Length];

            byte[] bAddr = BitConverter.GetBytes(address);
            
            buffer[0] = 0x22;
            buffer[1] = (byte)(2 + data.Length);
            buffer[2] = bAddr[1];
            buffer[3] = bAddr[0];

            int i, checkSum = 0;
            for (i = 0; i < data.Length; i++) buffer[i + 4] = data[i];
            for (i = 0; i < buffer.Length - 2; i++) checkSum += buffer[i];

            buffer[buffer.Length - 2] = (byte)((checkSum >> 8) & 0x00FF);
            buffer[buffer.Length - 1] = (byte)(checkSum & 0x00FF);

            //
            // Sends the command to the device and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[4];
            for (i = 0; i < 4; i++) stream.Read(buffer, i, 1);

            //
            // Checks if the ADR 3000 has sent some error code
            //
            if (buffer[0] == 0x7F) throw new IOException(ADR_ERROR[buffer[2]]);

            //
            // Checks if the information received is correct
            //
            if (!CheckArrays(buffer, new byte[] { 0xA2, 0x00, 0x00, 0xA2 })) throw new IOException(WRONG_ANSWER);
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
                }
                catch (Exception) { }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Disposes this instace of the ADR3000 class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
