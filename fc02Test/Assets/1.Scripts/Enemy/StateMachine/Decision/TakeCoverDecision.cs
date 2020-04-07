﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "PluggableAI/Actions/Take Cover")]
    public class TakeCoverDecision : Decision
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // NPC still have shots to fire on current engage round?
            // Or the cover period for this cover round has ended?
            // Or there is no current cover spot?
            if (controller.variables.currentShots < controller.variables.shotsInRound
                || controller.variables.waitInCoverTimer > controller.variables.coverTime
                || Equals(controller.CoverSpot, Vector3.positiveInfinity))
            {
                return false;
            }
            // Otherwise, NPC will take cover.
            else
            {
                return true;
            }
        }
    }
}