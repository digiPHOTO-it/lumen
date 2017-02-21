using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event
{
    public class EventManager : IEventManager
    {
        private static EventManager instance;

        private IEventManager iEventManager;

        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventManager();
                }
                return instance;
            }
        }

        public void setIEventManager(IEventManager iEventManager)
        {
            this.iEventManager = iEventManager;
        }

        public void Home()
        {
            if (iEventManager != null)
            {
                iEventManager.Home();
                MoveTimeCounter.Instance.updateLastTime();
            }
        }

        public void Next()
        {
            if (iEventManager != null)
            {
                iEventManager.Next();
                MoveTimeCounter.Instance.updateLastTime();
            }
        }

        public void Previous()
        {
            if (iEventManager != null)
            {
                iEventManager.Previous();
                MoveTimeCounter.Instance.updateLastTime();
            }
        }

        public void Go()
        {
            if (iEventManager != null)
            {
                iEventManager.Go();
                MoveTimeCounter.Instance.updateLastTime();
            }
        }
    }
}
