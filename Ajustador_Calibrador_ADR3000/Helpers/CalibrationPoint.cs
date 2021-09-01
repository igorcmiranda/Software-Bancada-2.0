using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public struct CalibrationPoint
    {
        public int codigo;
        public float voltage;
        public float current;
        public float spv;
        public float spi;
        public float phi;
        public string elements;
        public string energy;
        public int n;
        public int tempo;
        public float classe;
        public bool check;
    }
}
