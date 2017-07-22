using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using Aimtec.SDK.Menu;

    public interface IFeature
    {
        void OnLoad(Menu rootMenu);
    }
}
