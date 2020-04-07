﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public class EnemyAnimation : MonoBehaviour
    {
        [HideInInspector] public Animator anim; // Reference to the NPC Animator component.

        [HideInInspector]
        public float currentAimAngleGap; // Gap between current aim direction and on target aim direction

        [HideInInspector] public Transform gunMuzzle; // NPC weapon gun muzzle reference.
        [HideInInspector] public float angularSpeed; // NPC angular speed (used to turn).

        private StateController controller; // Reference to the NPC state controller (with the FSM).
        private UnityEngine.AI.NavMeshAgent nav; // Reference to the NPC NavMesh agent.
        private bool pendingAim; // Boolean to determine if an aim animation activation is pending.
        private Transform hips, spine; // Avatar bone transforms.
        private Vector3 initialRootRotation; // Initial root bone local rotation.
        private Vector3 initialHipsRotation; // Initial hips rotation related to the root bone.
        private Vector3 initialSpineRotation; // Initial spine rotation related to the hips bone.
        private Quaternion lastRotation; // Last frame spine rotation.
        private float timeCountAim, timeCountGuard; // Timers to rotate the spine to the current desired rotation.
        private readonly float turnSpeed = 25f; // NPC turn speed when strafing (focus movement).

        void Awake()
        {
            // Set up the references.
            controller = GetComponent<StateController>();
            nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            anim = GetComponent<Animator>();
            nav.updateRotation = false;

            // Get avatar bones for rotation reference.
            hips = anim.GetBoneTransform(HumanBodyBones.Hips);
            spine = anim.GetBoneTransform(HumanBodyBones.Spine);

            // Get initial bone rotation values.
            initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
            initialHipsRotation = hips.localEulerAngles;
            initialSpineRotation = spine.localEulerAngles;

            // Trigger equip weapon animation states.
            anim.SetTrigger(AnimatorKey.ChangeWeapon);
            anim.SetInteger(AnimatorKey.Weapon,
                (int) Enum.Parse(typeof(WeaponType), controller.classStats.WeaponType));

            // Find the NPC gun muzzle.
            foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
            {
                gunMuzzle = child.Find("muzzle");
                if (gunMuzzle != null)
                {
                    break;
                }
            }

            // Set ragdoll rigidbodies as kinematic while NPC is alive.
            foreach (Rigidbody member in GetComponentsInChildren<Rigidbody>())
            {
                member.isKinematic = true;
            }
        }

        void Update()
        {
            // Check speed and orientation at each frame.
            NavAnimSetup();
        }

        // Move and rotate the NPC based on the animation values.
        void OnAnimatorMove()
        {
            if (Time.timeScale > 0 && Time.deltaTime > 0)
            {
                nav.velocity = anim.deltaPosition / Time.deltaTime;
                if (!controller.Strafing)
                {
                    transform.rotation = anim.rootRotation;
                }
            }
        }

        private void LateUpdate()
        {
            // Aim adjustments.
            if (controller.Aiming)
            {
                // Calculate desired rotation.
                Quaternion targetRotation = Quaternion.LookRotation(controller.personalTarget - spine.position);
                // Apply parent bones initial rotation offsets
                targetRotation *= Quaternion.Euler(initialRootRotation);
                targetRotation *= Quaternion.Euler(initialHipsRotation);
                targetRotation *= Quaternion.Euler(initialSpineRotation);
                // Apply extra rotation offsets (depends on the NPC avatar).
                targetRotation *= Quaternion.Euler(VectorHelper.ToVector(controller.classStats.AimOffset));
                // Set rotation for the frame.
                Quaternion frameRotation = Quaternion.Slerp(lastRotation, targetRotation, timeCountAim);

                // Simulate a simple bone constraint on upper body rotation.
                // Is spine rotation relative to the hips is less than 60 degrees?
                if (Quaternion.Angle(frameRotation, hips.rotation) <= 60f)
                {
                    // Set desired rotation.
                    spine.rotation = frameRotation;
                    timeCountAim += Time.deltaTime;
                }
                // Avoid unrealistic rotation, related to hips and spine.
                else
                {
                    // Deal with over twist stuck situation, due to async rotation of spine and hips.
                    if (timeCountAim == 0 && Quaternion.Angle(frameRotation, hips.rotation) > 70f)
                    {
                        // Stop aiming and aim again after body realignment (1 second interval).
                        StartCoroutine(controller.UnstuckAim(1f));
                    }

                    // No over twist, freeze spine rotation until the desired one is 60 degrees or less, relative to hips.
                    spine.rotation = lastRotation;
                    timeCountAim = 0;
                }

                // Grab rotation for next frame.
                lastRotation = spine.rotation;

                // Measure remain angle gap to desired aim orientation.
                Vector3 target = controller.personalTarget - gunMuzzle.position;
                Vector3 fwd = -gunMuzzle.right;
                currentAimAngleGap = Vector3.Angle(target, fwd);

                timeCountGuard = 0;
            }
            // Guard position adjustments.
            else
            {
                lastRotation = spine.rotation;
                // Slowly reduce aim offset when exiting aim position.
                spine.rotation *= Quaternion.Slerp(Quaternion.Euler(VectorHelper.ToVector(controller.classStats.AimOffset)),
                    Quaternion.identity, timeCountGuard);
                timeCountGuard += Time.deltaTime;
            }
        }

        // Set NPC orientation and speed on the animator contoller.
        void NavAnimSetup()
        {
            float speed;
            float angle;
            // Grab speed per frame, based nav desired velocity and current forward
            speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;
            // Target is on sight, focus orientation on him.
            if (controller.focusSight)
            {
                Vector3 dest = (controller.personalTarget - transform.position);
                dest.y = 0;
                angle = Vector3.SignedAngle(transform.forward, dest, transform.up);

                //Calculate facing direction when strafing.
                if (controller.Strafing)
                {
                    dest = dest.normalized;
                    Quaternion targetStrafeRotation = Quaternion.LookRotation(dest);
                    transform.rotation =
                        Quaternion.Lerp(transform.rotation, targetStrafeRotation, turnSpeed * Time.time);
                }
            }
            // Target is not on sight, use navmesh agent values as reference (ex.: waypoint navigation).
            else
            {
                if (nav.desiredVelocity == Vector3.zero)
                    angle = 0;
                else
                    angle = Vector3.SignedAngle(transform.forward, nav.desiredVelocity, transform.up);
            }

            // Use angle deadzone (clearance) to avoid flickering when facing player.
            if (!controller.Strafing && Mathf.Abs(angle) < controller.generalStats.angleDeadzone)
            {
                transform.LookAt(transform.position + nav.desiredVelocity);
                angle = 0f;
                // Only trigger pending aim when NPC is facing player.
                if (pendingAim && controller.focusSight)
                {
                    controller.Aiming = true;
                    pendingAim = false;
                }
            }

            // Strafe direction.
            Vector3 direction = nav.desiredVelocity;
            direction.y = 0.0f;
            direction = direction.normalized;
            direction = Quaternion.Inverse(transform.rotation) * direction;
            // Setup values on animator.
            Setup(speed, angle, direction);
        }

        // Set aim animation start as pending (called externally).
        public void ActivatePendingAim()
        {
            pendingAim = true;
        }

        // Abort aim animation start.
        public void AbortPendingAim()
        {
            pendingAim = false;
            controller.Aiming = false;
        }

        // Set speed and orientation angle on the NPC animator, using damping values.
        void Setup(float speed, float angle, Vector3 strafeDirection)
        {
            angle *= Mathf.Deg2Rad;
            angularSpeed = angle / controller.generalStats.angleResponseTime;

            anim.SetFloat(AnimatorKey.Speed, speed, controller.generalStats.speedDampTime, Time.deltaTime);
            anim.SetFloat(AnimatorKey.AngularSpeed, angularSpeed, controller.generalStats.angularSpeedDampTime,
                Time.deltaTime);

            // Set 2D direction for strafing.
            anim.SetFloat(AnimatorKey.Horizontal, strafeDirection.x, controller.generalStats.speedDampTime,
                Time.deltaTime);
            anim.SetFloat(AnimatorKey.Vertical, strafeDirection.z, controller.generalStats.speedDampTime,
                Time.deltaTime);
        }
    }
}