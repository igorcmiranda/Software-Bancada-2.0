using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO.Ports;
using Ajustador_Calibrador_ADR3000.Helpers;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    /// <summary>
    /// Class to remotely control the AC power supply ACP 300
    /// </summary>
    public class ACP300 : IDisposable
    {
        private const int bufferSize = 1024;
        private const int MAX_ACP300_TRY = 2;   //  Max attempts = n + 1 = 3

        //
        //  Commands
        //
        public static ACPCommand SET_MODE { get; } = new ACPCommand(false, "CALC:FORM ");
        public static ACPCommand GET_MODE { get; } = new ACPCommand(true, "CALC:FORM?");
        public static ACPCommand SET_CONSTRAST { get; } = new ACPCommand(false, "DISP:CONT ");
        public static ACPCommand GET_CONSTRAST { get; } = new ACPCommand(true, "DISP:CONT?");
        public static ACPCommand GET_VALUES { get; } = new ACPCommand(true, "FETCH?");
        public static ACPCommand SET_OUTPUT { get; } = new ACPCommand(false, "OUTP ");
        public static ACPCommand GET_OUTPUT { get; } = new ACPCommand(true, "OUTP?");
        public static ACPCommand SET_OVER_CURR { get; } = new ACPCommand(false, "SOUR:CURR:LIM:HIGH ");
        public static ACPCommand GET_OVER_CURR { get; } = new ACPCommand(true, "SOUR:CURR:LIM:HIGH?");
        public static ACPCommand SET_FREQUENCY { get; } = new ACPCommand(false, "SOUR:FREQ ");
        public static ACPCommand GET_FREQUENCY { get; } = new ACPCommand(true, "SOUR:FREQ?");
        public static ACPCommand SET_FREQ_LIM { get; } = new ACPCommand(false, "SOUR:FREQ:LIM:HIGH ");
        public static ACPCommand GET_FREQ_LIM { get; } = new ACPCommand(true, "SOUR:FREQ:LIM:HIGH?");
        public static ACPCommand SET_FREQ_RANGE { get; } = new ACPCommand(false, "SOUR:FREQ:RANG ");
        public static ACPCommand GET_FREQ_RANGE { get; } = new ACPCommand(true, "SOUR:FREQ:RANG?");
        public static ACPCommand SET_RAMP_MODE { get; } = new ACPCommand(false, "SOUR:RTEN ");
        public static ACPCommand GET_RAMP_MODE { get; } = new ACPCommand(true, "SOUR:RTEN?");
        public static ACPCommand SET_RAMP_TIME { get; } = new ACPCommand(false, "SOUR:RTIM:UP ");
        public static ACPCommand GET_RAMP_TIME { get; } = new ACPCommand(true, "SOUR:RTIM:UP?");
        public static ACPCommand SET_VOLTAGE { get; } = new ACPCommand(false, "SOUR:VOLT ");
        public static ACPCommand GET_VOLTAGE { get; } = new ACPCommand(true, "SOUR:VOLT?");
        public static ACPCommand SET_VOLT_LIM { get; } = new ACPCommand(false, "SOUR:VOLT:LIM:HIGH ");
        public static ACPCommand GET_VOLT_LIM { get; } = new ACPCommand(true, "SOUR:VOLT:LIM:HIGH?");
        public static ACPCommand SET_VOLT_RANGE { get; } = new ACPCommand(false, "SOUR:VOLT:RANG ");
        public static ACPCommand GET_VOLR_RANGE { get; } = new ACPCommand(true, "SOUR:VOLT:RANG?");
        public static ACPCommand GET_SYSTEM_ERR { get; } = new ACPCommand(true, "SYST:ERR?");
        public static ACPCommand SET_KEY_LOCK { get; } = new ACPCommand(false, "SYST:KLOC ");
        public static ACPCommand GET_KEY_LOCK { get; } = new ACPCommand(true, "SYST:KLOC?");
        public static ACPCommand INIT_REMOTE { get; } = new ACPCommand(false, "SYST:REM");
        public static ACPCommand STOP_REMOTE { get; } = new ACPCommand(false, "SYST:LOC");
        public static ACPCommand GET_VERSION { get; } = new ACPCommand(true, "SYST:VERS?");
        //
        // Parameters
        //
        public const string MODE_WATT = "WATT";
        public const string MODE_VA = "VA";
        public const string MODE_PF = "PF";
        public const string _ON = "1";
        public const string _OFF = "0";
        public const string RAMP_ON = "RAMP";
        public const string RAMP_OFF = "ZERO";
        public const string F_RANG_60HZ = "60HZ";
        public const string F_RANG_50HZ = "50HZ";
        public const string F_RANG_400HZ = "400HZ";
        public const string F_RANG_HZ = "HZ";
        //
        //  Errors
        //


        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private SerialPort serialPort;
        private readonly string port;
        private readonly int baud;
        private readonly int dataBits;
        private readonly Parity parity;
        private readonly StopBits stopBits;
        private readonly bool dtr;
        private readonly bool rts;
        private string answer;

        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        
        public int ReadTimeout { get; set; } = 3000;

        public ACP300() { }

        public ACP300(string _port, int _baud, int _dataBits, Parity _parity, StopBits _stopBits, bool _dtr, bool _rts)
        {
            port = _port;
            baud = _baud;
            dataBits = _dataBits;
            parity = _parity;
            stopBits = _stopBits;
            dtr = _dtr;
            rts = _rts;
        }

        /// <summary>
        /// Initializes a new connection with the serial port via RS-232 protocol.
        /// </summary>
        /// <returns>True if everything was ok, False otherwise.</returns>
        public bool InitSerial()
        {
            try
            {
                //
                // Initializes the serial port component
                //
                serialPort = new SerialPort
                {
                    PortName = port,
                    BaudRate = baud,
                    DataBits = dataBits,
                    Parity = parity,
                    StopBits = stopBits,
                    DtrEnable = dtr,
                    RtsEnable = rts
                };
                serialPort.Open();

                serialPort.ReadTimeout = int.MaxValue;

                if (serialPort.IsOpen)
                {
                    //
                    //  Instantiates a reader for the serial port
                    //
                    new SerialPortReader(serialPort, bufferSize, NewDataReceived, ErrorOnRead);
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Answer SendCommand(ACPCommand command, string param)
        {
            bool keepTrying;
            int tryCount = 0;
            //
            // Sends interface connection request to the standard (best of 3)
            //
            while (true)
            {
                keepTrying = false;

                if (command.HasAnswer) Interlocked.Exchange(ref answer, "");
                else Interlocked.Exchange(ref answer, "\n");

                serialPort.DiscardOutBuffer();
                
                serialPort.Write(command.Comm + param + "\n");

                uint ticks = GetTickCount();
                
                while (!answer.Contains("\n"))
                {
                    if (GetTickCount() - ticks >= ReadTimeout)
                    {
                        if (tryCount < MAX_ACP300_TRY)
                        {
                            tryCount++;
                            keepTrying = true;
                            break;
                        }
                        else
                        {
                            return new Answer(false, "");
                        }
                    }
                }

                if (!keepTrying) break;
            }

            return new Answer(true, answer);

        }

        private void NewDataReceived(byte[] dataReceived)
        {
            Interlocked.Exchange(ref answer,
                answer + Encoding.ASCII.GetString(dataReceived));
        }

        private void ErrorOnRead(Exception e)
        {

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
                    if (serialPort != null) serialPort.Dispose();
                    if (handle != null) handle.Dispose();
                }
                catch (Exception) { }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
