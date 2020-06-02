using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class PlayerFootStep : MonoBehaviour
    {
        //public AudioClip[] stepClips;
        public SoundList[] stepSounds;
        private Animator myAnimator;
        private int index;
        private Transform lFoot, rFoot;
        private float dist;
        private int groundedBool, coverBool, aimBool, crouchFloat;
        private bool grounded;

        private enum Foot
        {
            LEFT,
            RIGHT
        }

        private Foot step = Foot.LEFT;
        private float oldDist, maxDist = 0;

        void Awake()
        {
            myAnimator = this.GetComponent<Animator>();
            lFoot = myAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rFoot = myAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            groundedBool = Animator.StringToHash(AnimatorKey.Grounded);
            coverBool = Animator.StringToHash(AnimatorKey.Cover);
            aimBool = Animator.StringToHash(AnimatorKey.Aim);
            crouchFloat = Animator.StringToHash(AnimatorKey.Crouch);
        }
        
        private void PlayFootStep()
        {
            //기존 위치랑 동일하지 않다면, 즉 아직 움직이는 중이라면.
            if (oldDist < maxDist)
            {
                return;
            }

            oldDist = maxDist = 0;
            int oldIndex = index;
            while (oldIndex == index)
            {
                index = (int)Random.Range(0, stepSounds.Length - 1);
            }

            SoundManager.Instance.PlayOneShotEffect((int)stepSounds[index],transform.position,0.2f);
        }

        private void Update()
        {
            if (!grounded && myAnimator.GetBool(groundedBool))
            {
                PlayFootStep();
            }

            grounded = myAnimator.GetBool(groundedBool);

            float factor = 0.15f;
            // if (myAnimator.GetBool(coverBool) || myAnimator.GetBool(aimBool))
            // {
            //     if (myAnimator.GetFloat(crouchFloat) < 0.5f && !myAnimator.GetBool(aimBool))
            //         factor = 0.17f;
            //     else
            //         factor = 0.11f;
            // }

            if (grounded && myAnimator.velocity.magnitude > 1.6f)
            {
                oldDist = maxDist;
                switch (step)
                {
                    case Foot.LEFT:
                        dist = lFoot.position.y - transform.position.y;
                        maxDist = dist > maxDist ? dist : maxDist;
                        if (dist <= factor)
                        {
                            PlayFootStep();
                            step = Foot.RIGHT;
                        }

                        break;
                    case Foot.RIGHT:
                        dist = rFoot.position.y - transform.position.y;
                        maxDist = dist > maxDist ? dist : maxDist;
                        if (dist <= factor)
                        {
                            PlayFootStep();
                            step = Foot.LEFT;
                        }

                        break;
                }
            }
        }
    }
}

