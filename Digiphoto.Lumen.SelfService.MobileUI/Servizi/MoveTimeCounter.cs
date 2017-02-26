using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Digiphoto.Lumen.SelfService.MobileUI.Servizi
{
    public class MoveTimeCounter
    {
        private long _DEFAULT_MAX_ELASPED_TIME_SECONDS = 60;

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
            if (sw.ElapsedMilliseconds < (_DEFAULT_MAX_ELASPED_TIME_SECONDS * 1000))
            {
                sw.Start();
                return true;
            }
            sw.Start();
            return false;
        }
    }
}
