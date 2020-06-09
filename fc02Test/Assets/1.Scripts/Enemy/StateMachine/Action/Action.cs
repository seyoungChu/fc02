using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 실제 행동을 하는 업데이트 스테이트.
    /// </summary>
    public abstract class Action : ScriptableObject
    {
        // The act function, called on Update() (State controller - current state - action).
        public abstract void Act(StateController controller);

        // The action on enable function, triggered once after a FSM state transition.
        public virtual void OnReadyAction(StateController controller)
        {
        }
    }
}

