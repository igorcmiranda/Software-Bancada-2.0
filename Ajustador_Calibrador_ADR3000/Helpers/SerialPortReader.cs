using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public class SerialPortReader
    {
        // http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
        // https://stackoverflow.com/questions/6759938/stop-stream-beginread
        private readonly SerialPort _port;
        private readonly byte[] _buffer;
        private readonly Action<byte[]> _dataReceivedAction;
        private readonly Action<Exception> _serialErrorAction;
        private readonly Action _kickoffRead = null;
        private IAsyncResult _ar;
        public SerialPortReader(SerialPort port, int bufferSize, Action<byte[]> dataReceivedAction, Action<Exception> serialErrorAction)
        {
            _port = port;
            _buffer = new byte[bufferSize];
            _dataReceivedAction = dataReceivedAction;
            _serialErrorAction = serialErrorAction;

            _kickoffRead =
            delegate ()
            {
                try
                {
                    _port.BaseStream.BeginRead(_buffer, 0, _buffer.Length,
                    delegate (IAsyncResult ar)
                    {
                        _ar = ar;
                        try
                        {
                            if (!_port.IsOpen)
                            {
                                // the port has been closed, so exit quietly

                                return;
                            }

                            int actualLength = _port.BaseStream.EndRead(ar);
                            if (actualLength > 0)
                            {
                                byte[] received = new byte[actualLength];
                                Buffer.BlockCopy(_buffer, 0, received, 0, actualLength);
                                _dataReceivedAction(received);
                            }
                            
                            _kickoffRead();
                        }
                        catch (Exception e)
                        {
                            _serialErrorAction(e);
                        }
                    },
                    null);
                }
                catch (Exception ex)
                {
                    _serialErrorAction(ex);
                }
            };

            _kickoffRead();
        }
    }
}
