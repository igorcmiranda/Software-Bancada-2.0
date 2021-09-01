using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    /// <summary>
    /// Helper class that represents an answer from a device that uses
    /// a string protocol (Words are always strings). 
    /// </summary>
    public class Answer
    {
        public bool Status { get; }
        public string Ans { get; }

        public Answer(bool _status, string _answer)
        {
            Status = _status;
            Ans = _answer;
        }
    }
}
