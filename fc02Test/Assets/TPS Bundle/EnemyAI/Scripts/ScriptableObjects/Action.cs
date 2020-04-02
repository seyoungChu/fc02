using UnityEngine;
using EnemyAI;

// A template scriptable object for n FSM action.
// Any custom FSM action must inherit from this class.
public abstract class Action : ScriptableObject
{
    // The act function, called on Update() (State controller - current state - action).
    public abstract void Act(StateController controller);

    // The action on enable function, triggered once after a FSM state transition.
    //ready로 이름을 바꾸면 더 이해가 쉬울 듯 하다.
    public virtual void OnEnableAction(StateController controller)
    {
    }
}