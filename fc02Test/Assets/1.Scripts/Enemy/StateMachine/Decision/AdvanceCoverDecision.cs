﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Advance Cover")]
    public class AdvanceCoverDecision : Decision
    {
        public int waitRounds = 1; // Engaging rounds to wait before consider advancing.

        [Header("Extra Decisions")] [Tooltip("The NPC near sense decision.")]
        public FocusDecision targetNear;

        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // NPC has not performed minimum engage rounds yet.
            if (controller.variables.waitRounds <= waitRounds)
            {
                return false;
            }


            controller.variables.waitRounds = 0;
            // Return previusly calculated probability to move to another cover spot.
            // If target is near, the NPC will not advance.
            return controller.variables.advanceCoverDecision && !targetNear.Decide(controller);
        }

        // The decision on enable function, triggered once after a FSM state transition.
        public override void OnEnableDecision(StateController controller)
        {
            // Accumulate an engage round.
            controller.variables.waitRounds += 1;
            // Calculate this round probability to advance to another cover spot.
            controller.variables.advanceCoverDecision =
                Random.Range(0, 1f) < controller.classStats.ChangeCoverChance / 100f;
        }
    }
}