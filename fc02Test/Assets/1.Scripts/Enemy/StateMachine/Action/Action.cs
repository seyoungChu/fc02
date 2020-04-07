using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
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

