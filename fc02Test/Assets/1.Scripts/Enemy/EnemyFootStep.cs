﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public class EnemyFootStep : MonoBehaviour
    {
        public SoundList[] stepSoundLists;

        private int index;
        private Animator anim;
        private bool isLeftFootAhead;
        private bool playedLeftFoot;
        private bool playedRightFoot;
        private Vector3 leftFootIKPos;
        private Vector3 rightFootIKPos;

        void Awake()
        {
            anim = this.GetComponent<Animator>();
        }
        void OnAnimatorIK()
        {
            leftFootIKPos = anim.GetIKPosition(AvatarIKGoal.LeftFoot);
            rightFootIKPos = anim.GetIKPosition(AvatarIKGoal.RightFoot);
        }
        
        void PlayFootStep()
        {
            int oldIndex = index;
            while (oldIndex == index)
            {
                index = Random.Range(0, stepSoundLists.Length);
            }
            SoundManager.Instance.PlayOneShotEffect((int)stepSoundLists[index],transform.position, 0.1f);
        }
        
        void Update()
        {
            float factor = 0.15f;

            if (anim.velocity.magnitude > 1.4f)
            {
                // Distance between the pivot position and the left foot
                if (Vector3.Distance(leftFootIKPos, anim.pivotPosition) <= factor && playedLeftFoot == false)
                {
                    PlayFootStep();
                    playedLeftFoot = true;
                    playedRightFoot = false;
                }

                // Distance between the pivot position and the right foot
                if (Vector3.Distance(rightFootIKPos, anim.pivotPosition) <= factor && playedRightFoot == false)
                {
                    PlayFootStep();
                    playedRightFoot = true;
                    playedLeftFoot = false;
                }
            }
        }

        
    }
}

