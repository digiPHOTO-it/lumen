using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService.MobileUI.Servizi
{
    public class MoveTimeCounter
    {
        private long _DEFAULT_MAX_ELASPED_TIME = 3600;

       Stopwatch sw;

        private static MoveTimeCounter instance;

        public static MoveTimeCounter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MoveTimeCounter();
                }
                return instance;
            }
        }

        private MoveTimeCounter()
        {
            sw = new Stopwatch();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void updateLastTime()
        {
            sw.Restart();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool evaluateTime()
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds < _DEFAULT_MAX_ELASPED_TIME)
            {
                return true;
            }

            return false;
        }
    }
}
