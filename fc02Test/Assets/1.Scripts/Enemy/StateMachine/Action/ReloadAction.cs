﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Reload")]
    public class ReloadAction : Action
    {
        public override void Act(StateController controller)
        {
            if(!controller.reloading && controller.bullets <= 0)
            {
                // Set reloading animation state.
                controller.enemyAnimation.anim.SetTrigger(AnimatorKey.Reload);
                controller.reloading = true;
                // Play weapon reloading sound clip.
                SoundManager.Instance.PlayOneShotEffect((int)SoundList.reloadWeapon,controller.enemyAnimation.gunMuzzle.position,2f);
            }
        }
    }
}

