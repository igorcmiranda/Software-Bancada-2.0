using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public class ACPCommand
    {
        public bool HasAnswer { get; }
        public string Comm { get; }

        public ACPCommand(bool hasAnswer, string comm)
        {
            HasAnswer = hasAnswer;
            Comm = comm;
        }
    }
}
