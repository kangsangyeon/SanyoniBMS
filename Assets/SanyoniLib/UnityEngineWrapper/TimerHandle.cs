using System;
using System.Collections;
using UnityEngine;


namespace SanyoniLib.UnityEngineWrapper
{

    public class TimerHandle
    {
        private MonoBehaviour m_Owner;
        private Coroutine m_Coroutine;
        public Action m_Action;
        public float m_Delay;
        public bool m_Loop;

        private TimerHandle(MonoBehaviour _owner, Action _action, float _delay, bool _loop)
        {
            this.m_Owner = _owner;
            this.m_Action = _action;
            this.m_Delay = _delay;
            this.m_Loop = _loop;
        }

        public static TimerHandle CreateHandle(MonoBehaviour _owner, Action _action, float _delay, bool _loop)
        {
            TimerHandle timer = new TimerHandle(_owner, _action, _delay, _loop);
            return timer;
        }

        public void Start()
        {
            if (this.m_Coroutine != null)
            {
                Debug.Log("Timer is already started.");
            }

            this.m_Coroutine = this.m_Owner.StartCoroutine(CInvokeAction());
        }

        private IEnumerator CInvokeAction()
        {
            WaitForSeconds waitDelay = new WaitForSeconds(this.m_Delay);

            do
            {
                if (this.m_Action != null) this.m_Action.Invoke();
                yield return waitDelay;
            }
            while (this.m_Loop == true);

            this.m_Coroutine = null;
        }

        public void Stop()
        {
            if (this.m_Coroutine != null)
            {
                this.m_Owner.StopCoroutine(this.m_Coroutine);
                this.m_Coroutine = null;
            }
        }

    }

}