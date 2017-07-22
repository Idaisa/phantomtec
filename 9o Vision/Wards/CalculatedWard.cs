using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using System.Drawing;
    using Aimtec;

    class CalculatedWard
    {
        public readonly float EndTime;
        public readonly Vector3 Position;
        public readonly Color Color;

        public CalculatedWard(float endTime, Vector3 position, Color color)
        {
            EndTime = endTime;
            Position = position;
            Color = color;
        }
    }
}
