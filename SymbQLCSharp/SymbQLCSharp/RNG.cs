using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public static class RNG
    {
        private static Random _random;

        static RNG()
        {
            Reset();
        }

        public static Random Random()
        {
            return _random;
        }

        public static void Reset()
        {
            _random = new Random();
        }
    }
}
