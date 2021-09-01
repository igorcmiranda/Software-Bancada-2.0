using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleIO;
using System.Threading;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    /// <summary>
    /// Class to perform configurations on the 5 outputs transformer command circuit
    /// </summary>
    public class Transformer5Out
    {
        /// <summary>
        /// Time to wait for the relay to turn off
        /// </summary>
        private const int _RELAY_OFF_TIME_MS_ = 55;
        /// <summary>
        /// Time to wait for the relay to turn on
        /// </summary>
        private const int _RELAY_ON_TIME_MS_ = 500;

        /// <summary>
        /// No voltage, open circuit
        /// </summary>
        public const uint _TRANSFORMER_OFF_ = 0x0F;
        /// <summary>
        /// RL5 - 100 V
        /// </summary>
        public const uint _TRANSFORMER_100V_ = 0x0C;
        /// <summary>
        /// RL4 - 120 V
        /// </summary>
        public const uint _TRANSFORMER_120V_ = 0x04;
        /// <summary>
        /// RL3 - 180 V
        /// </summary>
        public const uint _TRANSFORMER_180V_ = 0x01;
        /// <summary>
        /// RL2 - 220 V
        /// </summary>
        public const uint _TRANSFORMER_220V_ = 0x08;
        /// <summary>
        /// RL1 - 240 V
        /// </summary>
        public const uint _TRANSFORMER_240V_ = 0x05;

        /// <summary>
        /// Get the output value for the MCP2200 pins according to a real voltage level
        /// </summary>
        /// <param name="voltage">The voltage level</param>
        /// <returns>MCP2200 gpio value</returns>
        public static uint GetVoltageDiscreteLevel(float voltage)
        {
            if (voltage <= 100) return _TRANSFORMER_100V_;
            else if ((voltage > 100) && (voltage <= 120)) return _TRANSFORMER_120V_;
            else if ((voltage > 120) && (voltage <= 180)) return _TRANSFORMER_180V_;
            else if ((voltage > 180) && (voltage <= 220)) return _TRANSFORMER_220V_;
            else return _TRANSFORMER_240V_;
        }

        /// <summary>
        /// Get the real voltage value according to a MCP2200 gpio value
        /// </summary>
        /// <param name="discreteLevel">MCP2200 gpio value</param>
        /// <returns>Real voltage value</returns>
        public static float GetVoltageRealValue(uint discreteLevel)
        {
            switch (discreteLevel)
            {
                case _TRANSFORMER_100V_: return 100.0f;
                case _TRANSFORMER_120V_: return 120.0f;
                case _TRANSFORMER_180V_: return 180.0f;
                case _TRANSFORMER_220V_: return 220.0f;
                case _TRANSFORMER_240V_: return 240.0f;
                default: return 0.0f;
            }
        }

        /// <summary>
        /// Set the MCP2200 gpio to wanted value
        /// </summary>
        /// <param name="output">Wanted value</param>
        public static void EnableOutput(uint output)
        {
            SimpleIOClass.WritePort(_TRANSFORMER_OFF_);
            Thread.Sleep(_RELAY_OFF_TIME_MS_);
            SimpleIOClass.WritePort(output);
            Thread.Sleep(_RELAY_ON_TIME_MS_);
        }
    }
}
