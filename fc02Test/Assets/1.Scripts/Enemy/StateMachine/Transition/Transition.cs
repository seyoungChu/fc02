using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    //
    [Serializable]
    public class Transition
    {
        [Tooltip("The decision to trigger the corresponding transition.")]
        public Decision decision;
        [Tooltip("The state to go in case the decision is true")]
        public State trueState;
        [Tooltip("The state to go in case the decision is false")]
        public State falseState;
    }
}

