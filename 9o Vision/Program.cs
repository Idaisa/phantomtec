using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using Aimtec.SDK.Events;
    using Aimtec.SDK.Menu;

    class Program
    {
        static void Main(string[] args)
        {
            GameEvents.GameStart += OnLoad;
        }

        private static void OnLoad()
        {
            var rootMenu = new Menu($"{nameof(_9o_Vision)}", "9o Vision", true);
            rootMenu.Attach();
            var features = new IFeature[] { new WardTracker(), new GankAlerter.GankAlerter(), new RecallTracker.RecallTracker(), new AntiConfusion.AntiConfusion(), new Ranges.Ranges(), };

            foreach (var feature in features)
            {
                feature.OnLoad(rootMenu);
            }

            
        }
    }
}
