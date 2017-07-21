using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using Aimtec.SDK.Events;

    class Program
    {
        static void Main(string[] args)
        {
            GameEvents.GameStart += new WardTracker().OnLoad;
        }
    }
}
