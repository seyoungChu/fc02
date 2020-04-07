﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public class AimBehaviour : GenericBehaviour
    {
        public Texture2D crosshair; // Crosshair texture.
        public float aimTurnSmoothing = 0.15f; // Speed of turn response when aiming to match camera facing.
        public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0f); // Offset to repoint the camera when aiming.
        public Vector3 aimCamOffset = new Vector3(0f, 0.4f, -0.7f); // Offset to relocate the camera when aiming.

        private int aimBool; // Animator variable related to aiming.
        private bool aim; // Boolean to determine whether or not the player is aiming.
        private int cornerBool; // Animator variable related to cover corner..
        private bool peekCorner; // Boolean to get whether or not the player is on a cover corner.
        private Vector3 initialRootRotation; // Initial root bone local rotation.
        private Vector3 initialHipsRotation; // Initial hips rotation related to the root bone.
        private Vector3 initialSpineRotation; // Initial spine rotation related to the root bone.

        // Start is always called after any Awake functions.
        void Start()
        {
            // Set up the references.
            aimBool = Animator.StringToHash(AnimatorKey.Aim);

            cornerBool = Animator.StringToHash(AnimatorKey.Corner);

            // Get initial bone rotation values.
            Transform hips = basicBehaviour.GetAnim.GetBoneTransform(HumanBodyBones.Hips);
            initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
            initialHipsRotation = hips.localEulerAngles;
            initialSpineRotation = basicBehaviour.GetAnim.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
        }
        

        // Rotate the player to match correct orientation, according to camera.
        void Rotating()
        {
            Vector3 forward = basicBehaviour.playerCamera.TransformDirection(Vector3.forward);
            // Player is moving on ground, Y component of camera facing is not relevant.
            forward.y = 0.0f;
            forward = forward.normalized;

            // Always rotates the player according to the camera horizontal rotation in aim mode.
            Quaternion targetRotation = Quaternion.Euler(0, basicBehaviour.GetCamScript.GetH, 0);

            float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

            // Peeking corner situation.
            if (peekCorner)
            {
                // Rotate only player upper body when peeking a corner.
                transform.rotation = Quaternion.LookRotation(-basicBehaviour.GetLastDirection());
                targetRotation *= Quaternion.Euler(initialRootRotation);
                targetRotation *= Quaternion.Euler(initialHipsRotation);
                targetRotation *= Quaternion.Euler(initialSpineRotation);
                Transform spine = basicBehaviour.GetAnim.GetBoneTransform(HumanBodyBones.Spine);
                spine.rotation = targetRotation;
            }
            else
            {
                // Rotate entire player to face camera.
                basicBehaviour.SetLastDirection(forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, minSpeed * Time.deltaTime);
            }
        }
        // Handle aim parameters when aiming is active.
        void AimManagement()
        {
            // Deal with the player orientation when aiming.
            Rotating();
        }


        // Co-rountine to start aiming mode with delay.
        private IEnumerator ToggleAimOn()
        {
            yield return new WaitForSeconds(0.05f);
            // Aiming is not possible.
            if (basicBehaviour.GetTempLockStatus(this.behaviourCode) || basicBehaviour.IsOverriding(this))
            {
                yield return false;
            }
            else // Start aiming.
            {
                aim = true;
                int signal = 1;
                if (peekCorner)
                {
                    signal = (int)Mathf.Sign(basicBehaviour.GetH);
                }

                aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
                aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
                yield return new WaitForSeconds(0.1f);
                basicBehaviour.GetAnim.SetFloat(speedFloat, 0);
                // This state overrides the active one.
                basicBehaviour.OverrideWithBehaviour(this);
            }
        }

        // Co-rountine to end aiming mode with delay.
        private IEnumerator ToggleAimOff()
        {
            aim = false;
            yield return new WaitForSeconds(0.3f);
            basicBehaviour.GetCamScript.ResetTargetOffsets();
            basicBehaviour.GetCamScript.ResetMaxVerticalAngle();
            yield return new WaitForSeconds(0.05f);
            basicBehaviour.RevokeOverridingBehaviour(this);
        }

        // LocalFixedUpdate overrides the virtual function of the base class.
        public override void LocalFixedUpdate()
        {
            // Set camera position and orientation to the aim mode parameters.
            if (aim)
            {
                basicBehaviour.GetCamScript.SetTargetOffsets(aimPivotOffset, aimCamOffset);
            }

        }

        // LocalLateUpdate: manager is called here to set player rotation after camera rotates, avoiding flickering.
        public override void LocalLateUpdate()
        {
            AimManagement();
        }

        // Update is used to set features regardless the active behaviour.
        void Update()
        {
            peekCorner = basicBehaviour.GetAnim.GetBool(cornerBool);

            // Activate/deactivate aim by input.
            if (Input.GetAxisRaw(ButtonName.Aim) != 0 && !aim)
            {
                StartCoroutine(ToggleAimOn());
            }
            else if (aim && Input.GetAxisRaw(ButtonName.Aim) == 0)
            {
                StartCoroutine(ToggleAimOff());
            }

            // No sprinting while aiming.
            canSprint = !aim;

            // Toggle camera aim position left or right, switching shoulders.
            if (aim && Input.GetButtonDown(ButtonName.Shoulder) && !peekCorner)
            {
                aimCamOffset.x = aimCamOffset.x * (-1);
                aimPivotOffset.x = aimPivotOffset.x * (-1);
            }

            // Set aim boolean on the Animator Controller.
            basicBehaviour.GetAnim.SetBool(aimBool, aim);
        }

        // Draw the crosshair when aiming.
        void OnGUI()
        {
            if (crosshair)
            {
                float mag = basicBehaviour.GetCamScript.GetCurrentPivotMagnitude(aimPivotOffset);
                if (mag < 0.05f)
                {
                    GUI.DrawTexture(new Rect(Screen.width / 2 - (crosshair.width * 0.5f),
                        Screen.height / 2 - (crosshair.height * 0.5f),
                        crosshair.width, crosshair.height), crosshair);
                }                    
            }
        }
    }
}