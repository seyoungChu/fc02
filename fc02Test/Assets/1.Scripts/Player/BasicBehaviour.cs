using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class BasicBehaviour : MonoBehaviour //0
    {
        private List<GenericBehaviour> behaviours; //2
        private List<GenericBehaviour> overridingBehaviours; //2
        private int currentBehaviour;
        private int defaultBehaviour;
        private int behaviourLocked;

        //캐싱//
        public Transform playerCamera;
        private Animator myAnimator;
        private Rigidbody myRigidbody;
        private ThirdPersonOrbitCam camScript; // Reference to the third person camera script.

        private float h; // Horizontal Axis.
        private float v; // Vertical Axis.
        public float turnSmoothing = 0.06f; // Speed of turn when moving to match camera facing.
        private bool changedFOV; // Boolean to store when the sprint action has changed de camera FOV.
        public float sprintFOV = 100f; // the FOV to use on the camera when player is sprinting.
        private Vector3 lastDirection; // Last direction the player was moving.
        private bool sprint; // Boolean to determine whether or not the player activated the sprint mode.        
        private int hFloat; // Animator variable related to Horizontal Axis.
        private int vFloat; // Animator variable related to Vertical Axis.
        private int groundedBool; // Animator variable related to whether or not the player is on the ground.
        private Vector3 colExtents; // Collider extents for ground test. 
                                    // Get current horizontal and vertical axes.
        public float GetH { get => h; }
        public float GetV { get => v; }
        public ThirdPersonOrbitCam GetCamScript { get => camScript; }// Get the player camera script.        
        public Rigidbody GetRigidBody { get => myRigidbody; }// Get the player's rigid body.        
        public Animator GetAnim { get => myAnimator; }// Get the player's animator controller.        
        public int GetDefaultBehaviour { get => defaultBehaviour; }// Get current default behaviour.

        void Awake()
        {
            // Set up the references.
            behaviours = new List<GenericBehaviour>();
            overridingBehaviours = new List<GenericBehaviour>();
            myAnimator = GetComponent<Animator>();
            hFloat = Animator.StringToHash("H");
            vFloat = Animator.StringToHash("V");
            camScript = playerCamera.GetComponent<ThirdPersonOrbitCam>();
            myRigidbody = GetComponent<Rigidbody>();

            // Grounded verification variables.
            groundedBool = Animator.StringToHash("Grounded");
            colExtents = GetComponent<Collider>().bounds.extents;
        }
        // Check if the player is moving.
        public bool IsMoving()
        {
            return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
            //return (h != 0) || (v != 0);
        }

        // Check if the player is moving on the horizontal plane.
        public bool IsHorizontalMoving()
        {
            return Mathf.Abs(h) > Mathf.Epsilon;
            //return h != 0;
        }

        // Check if player is sprinting.
        public virtual bool IsSprinting()
        {
            return sprint && IsMoving() && CanSprint();
        }

        // Check if player can sprint (all behaviours must allow).
        public bool CanSprint()
        {
            foreach (GenericBehaviour behaviour in behaviours)
            {
                if (!behaviour.AllowSprint)
                {
                    return false;
                }

            }

            foreach (GenericBehaviour behaviour in overridingBehaviours)
            {
                if (!behaviour.AllowSprint)
                {
                    return false;
                }

            }

            return true;
        }
        // Function to tell whether or not the player is on ground.
        public bool IsGrounded()
        {
            //머리 위에서 아래로 레이를 만들어서
            Ray ray = new Ray(this.transform.position + Vector3.up * 2 * colExtents.x, Vector3.down);
            return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
        }

        void Update()
        {
            // Store the input axes.
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            // Set the input axes on the Animator Controller.
            myAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
            myAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

            // Toggle sprint by input.
            sprint = Input.GetButton(ButtonName.Sprint);

            // Set the correct camera FOV for sprint mode.
            if (IsSprinting())
            {
                changedFOV = true;
                camScript.SetFOV(sprintFOV);
            }
            else if (changedFOV)
            {
                camScript.ResetFOV();
                changedFOV = false;
            }

            // Set the grounded test on the Animator Controller.
            myAnimator.SetBool(groundedBool, IsGrounded());
        }

        // Put the player on a standing up position based on last direction faced.
        public void Repositioning()
        {
            if (lastDirection != Vector3.zero)
            {
                lastDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
                Quaternion newRotation = Quaternion.Slerp(myRigidbody.rotation, targetRotation, turnSmoothing);
                myRigidbody.MoveRotation(newRotation);
            }
        }

        // Call the FixedUpdate functions of the active or overriding behaviours.
        void FixedUpdate()
        {
            // Call the active behaviour if no other is overriding.
            bool isAnyBehaviourActive = false;
            if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
            {
                foreach (GenericBehaviour behaviour in behaviours)
                {
                    if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                    {
                        isAnyBehaviourActive = true;
                        behaviour.LocalFixedUpdate();
                    }
                }
            }
            // Call the overriding behaviours if any.
            else
            {
                foreach (GenericBehaviour behaviour in overridingBehaviours)
                {
                    behaviour.LocalFixedUpdate();
                }
            }

            // Ensure the player will stand on ground if no behaviour is active or overriding.
            if (!isAnyBehaviourActive && overridingBehaviours.Count == 0)
            {
                myRigidbody.useGravity = true;
                Repositioning();
            }
        }

        // Call the LateUpdate functions of the active or overriding behaviours.
        private void LateUpdate()
        {
            // Call the active behaviour if no other is overriding.
            if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
            {
                foreach (GenericBehaviour behaviour in behaviours)
                {
                    if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                    {
                        behaviour.LocalLateUpdate();
                    }
                }
            }
            // Call the overriding behaviours if any.
            else
            {
                foreach (GenericBehaviour behaviour in overridingBehaviours)
                {
                    behaviour.LocalLateUpdate();
                }
            }
        }

        // Put a new behaviour on the behaviours watch list.
        public void SubscribeBehaviour(GenericBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }

        // Set the default player behaviour.
        public void RegisterDefaultBehaviour(int behaviourCode)
        {
            defaultBehaviour = behaviourCode;
            currentBehaviour = behaviourCode;
        }

        // Attempt to set a custom behaviour as the active one.
        // Always changes from default behaviour to the passed one.
        public void RegisterBehaviour(int behaviourCode)
        {
            if (currentBehaviour == defaultBehaviour)
            {
                currentBehaviour = behaviourCode;
            }
        }

        // Attempt to deactivate a player behaviour and return to the default one.
        public void UnregisterBehaviour(int behaviourCode)
        {
            if (currentBehaviour == behaviourCode)
            {
                currentBehaviour = defaultBehaviour;
            }
        }

        // Attempt to override any active behaviour with the behaviours on queue.
        // Use to change to one or more behaviours that must overlap the active one (ex.: aim behaviour).
        public bool OverrideWithBehaviour(GenericBehaviour behaviour)
        {
            // Behaviour is not on queue.
            if (!overridingBehaviours.Contains(behaviour))
            {
                // No behaviour is currently being overridden.
                if (overridingBehaviours.Count == 0)
                {
                    // Call OnOverride function of the active behaviour before overrides it.
                    foreach (GenericBehaviour overriddenBehaviour in behaviours)
                    {
                        if (overriddenBehaviour.isActiveAndEnabled &&
                            currentBehaviour == overriddenBehaviour.GetBehaviourCode)
                        {
                            overriddenBehaviour.OnOverride();
                            break;
                        }
                    }
                }

                // Add overriding behaviour to the queue.
                overridingBehaviours.Add(behaviour);
                return true;
            }

            return false;
        }

        // Attempt to revoke the overriding behaviour and return to the active one.
        // Called when exiting the overriding behaviour (ex.: stopped aiming).
        public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
        {
            if (overridingBehaviours.Contains(behaviour))
            {
                overridingBehaviours.Remove(behaviour);
                return true;
            }

            return false;
        }


        // Check if any or a specific behaviour is currently overriding the active one.
        public bool IsOverriding(GenericBehaviour behaviour = null)
        {
            if (behaviour == null)
                return overridingBehaviours.Count > 0;
            return overridingBehaviours.Contains(behaviour);
        }

        // Check if the active behaviour is the passed one.
        public bool IsCurrentBehaviour(int behaviourCode)
        {
            return this.currentBehaviour == behaviourCode;
        }

        // Check if any other behaviour is temporary locked.
        public bool GetTempLockStatus(int behaviourCodeIgnoreSelf = 0)
        {
            return (behaviourLocked != 0 && behaviourLocked != behaviourCodeIgnoreSelf);
        }

        // Atempt to lock on a specific behaviour.
        //  No other behaviour can overrhide during the temporary lock.
        // Use for temporary transitions like jumping, entering/exiting aiming mode, etc.
        public void LockTempBehaviour(int behaviourCode)
        {
            if (behaviourLocked == 0)
            {
                behaviourLocked = behaviourCode;
            }
        }

        // Attempt to unlock the current locked behaviour.
        // Use after a temporary transition ends.
        public void UnlockTempBehaviour(int behaviourCode)
        {
            if (behaviourLocked == behaviourCode)
            {
                behaviourLocked = 0;
            }
        }

        // Get the last player direction of facing.
        public Vector3 GetLastDirection()
        {
            return lastDirection;
        }

        // Set the last player direction of facing.
        public void SetLastDirection(Vector3 direction)
        {
            lastDirection = direction;
        }
    }

    [RequireComponent(typeof(BasicBehaviour), typeof(Animator))]
    public abstract class GenericBehaviour : MonoBehaviour //1
    {
        protected int speedFloat;
        protected BasicBehaviour basicBehaviour;
        protected int behaviourCode;
        protected bool canSprint;

        private void Awake()
        {
            basicBehaviour = GetComponent<BasicBehaviour>();
            speedFloat = Animator.StringToHash(AnimatorKey.Speed);
            canSprint = true;

            behaviourCode = this.GetType().GetHashCode();
        }

        public int GetBehaviourCode
        {
            get => behaviourCode;
        }
        public bool AllowSprint
        {
            get => canSprint;
        }

        public virtual void LocalLateUpdate()
        {

        }

        public virtual void LocalFixedUpdate()
        {

        }

        public virtual void OnOverride()
        {

        }
    }



}
