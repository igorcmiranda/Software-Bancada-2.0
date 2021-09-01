using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    public class PowerSupply : IDisposable
    {
        //
        // Private constants
        //
        private const string WRONG_ANSWER = "Resposta incorreta da fonte.";
        //
        // Private classes and variables to be used
        //
        private readonly int FS;
        private readonly NetworkStream stream; //Stream to handle communications

        //
        // Variables to handle disposing features
        //
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        //
        // Public variables, classes, structures and attributes to be used
        //
        public enum Phase
        {
            /// <summary>
            /// All the three phases.
            /// </summary>
            All = 0,

            /// <summary>
            /// The A phase of the power supply.
            /// </summary>
            Phase_A = 1,

            /// <summary>
            /// The B phase of the power supply.
            /// </summary>
            Phase_B = 2,

            /// <summary>
            /// The C phase of the power supply.
            /// </summary>
            Phase_C = 3
        };

        public enum Command
        {
            /// <summary>
            /// The command to change the set point for the signal to be generated.
            /// </summary>
            PutSetPoint = 0xCD,

            /// <summary>
            /// The command to configure the frequency for the signal to be generated.
            /// </summary>
            SetFrequency = 0xD0,

            /// <summary>
            /// The command to change the phase shift between signals.
            /// </summary>
            PhaseShift = 0xD9
        }

        /// <summary>
        /// Constructor for the PowerSupply class
        /// </summary>
        /// <param name="_stream">The stream for communications with the power supply</param>
        public PowerSupply(NetworkStream _stream)
        {
            stream = _stream;
            if (stream != null) stream.ReadTimeout = 7000;
            FS = 130;
        }

        /// <summary>
        /// Sends a command to the power supply.
        /// </summary>
        /// <param name="phase">One of the three phases of the three-phase system or all phases.</param>
        /// <param name="setPoint">The value of the set point for the command.</param>
        /// <param name="command">The command to be sent.</param>
        public void SendCommand(Phase phase, float setPoint, Command command)
        {
            //
            // Allocates necessary data to use
            //
            byte[] bSetPoint = BitConverter.GetBytes(Convert.ToInt16(setPoint * FS)); 
            byte[] buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x10, (byte)phase, (byte)command, 0x00, 0x01, 0x02, bSetPoint[1], bSetPoint[0] };
            byte[] auxBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x10, (byte)phase, (byte)command, 0x00, 0x01 };

            //
            // Sends data to the Power Supply and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int numOfBytesRead = 0, i = 0;
            buffer = new byte[128];

            Thread.Sleep(500);  //Waits for 500 ms for the power supply to answer the command

            while (stream.DataAvailable)
            {
                numOfBytesRead += stream.Read(buffer, i++, 1);
            }

            //
            // Checks the veracity of the answer
            //
            byte[] answer = new byte[numOfBytesRead];
            Buffer.BlockCopy(buffer, 0, answer, 0, numOfBytesRead);

            //if (!CheckArrays(answer, auxBuffer)) throw new IOException(WRONG_ANSWER);
        }

        public void TurnOn()
        {
            //
            // Allocates necessary data to use
            //
            byte[] buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x10, 0x00, 0xCA, 0x00, 0x01, 0x02, 0x00, 0x00 };
            byte[] auxBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x10, 0x00, 0xCA, 0x00, 0x01 };

            //
            // Sends data to the Power Supply and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int numOfBytesRead = 0, i = 0;
            buffer = new byte[128];

            Thread.Sleep(500);  //Waits for 500 ms for the power supply to answer the command

            while (stream.DataAvailable)
            {
                numOfBytesRead += stream.Read(buffer, i++, 1);
            }

            //
            // Checks the veracity of the answer
            //
            byte[] answer = new byte[numOfBytesRead];
            Buffer.BlockCopy(buffer, 0, answer, 0, numOfBytesRead);

            //if (!CheckArrays(answer, auxBuffer)) throw new IOException(WRONG_ANSWER);
        }

        public void TurnOff()
        {
            //
            // Allocates necessary data to use
            //
            byte[] buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x10, 0x00, 0xCC, 0x00, 0x01, 0x02, 0x00, 0x00 };
            byte[] auxBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x10, 0x00, 0xCC, 0x00, 0x01 };

            //
            // Sends data to the Power Supply and reads the answer
            //
            stream.Write(buffer, 0, buffer.Length);
            int numOfBytesRead = 0, i = 0;
            buffer = new byte[128];

            Thread.Sleep(500);  //Waits for 500 ms for the power supply to answer the command

            while (stream.DataAvailable)
            {
                numOfBytesRead += stream.Read(buffer, i++, 1);
            }

            //
            // Checks the veracity of the answer
            //
            byte[] answer = new byte[numOfBytesRead];
            Buffer.BlockCopy(buffer, 0, answer, 0, numOfBytesRead);

            //if (!CheckArrays(answer, auxBuffer)) throw new IOException(WRONG_ANSWER);
        }

        /// <summary>
        /// Request the operating status of the power supply.
        /// </summary>
        /// <returns>A byte array with the requested status.</returns>
        public byte[] GetStatus()
        {
            //
            // Allocates necessary data
            //
            byte[] buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, 0x00, 0xD5, 0x00, 0x00 };
            byte[] answer;

            //
            // Sends command to the power supply
            //
            stream.Write(buffer, 0, buffer.Length);
            int numOfBytesRead = 0, i = 0;
            answer = new byte[128];

            Thread.Sleep(500); //Waits 500 ms for the power supply to answer

            while (stream.DataAvailable)
            {
                numOfBytesRead += stream.Read(answer, i++, 1);
            }

            //
            // Returns the status array
            //
            return (answer);
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
