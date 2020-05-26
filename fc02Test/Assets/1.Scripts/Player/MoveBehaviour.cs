using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class MoveBehaviour : GenericBehaviour
    {
        public float walkSpeed = 0.15f; // Default walk speed.
        public float runSpeed = 1.0f; // Default run speed.
        public float sprintSpeed = 2.0f; // Default sprint speed.
        public float speedDampTime = 0.1f; // Default damp time to change the animations based on current speed.

        public float jumpHeight = 1.5f; // Default jump height.
        public float jumpIntertialForce = 10f; // Default horizontal inertial force when jumping.

        private float speed, speedSeeker; // Moving speed.
        private int jumpBool; // Animator variable related to jumping.
        private int groundedBool; // Animator variable related to whether or not the player is on ground.
        private bool jump; // Boolean to determine whether or not the player started a jump.
        private bool isColliding; // Boolean to determine if the player has collided with an obstacle.
        private CapsuleCollider capsuleCollider;
        // Start is always called after any Awake functions.
        void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            // Set up the references.
            jumpBool = Animator.StringToHash(AnimatorKey.Jump);
            groundedBool = Animator.StringToHash(AnimatorKey.Grounded);
            BehaviourController.GetAnim.SetBool(groundedBool, true);

            // Subscribe and register this behaviour as the default behaviour.
            BehaviourController.SubscribeBehaviour(this);
            BehaviourController.RegisterDefaultBehaviour(this.behaviourCode);
            speedSeeker = runSpeed;
        }
        // Rotate the player to match correct orientation, according to camera and key pressed.
        Vector3 Rotating(float horizontal, float vertical)
        {
            // Get camera forward direction, without vertical component.
            Vector3 forward = BehaviourController.playerCamera.TransformDirection(Vector3.forward);

            // Player is moving on ground, Y component of camera facing is not relevant.
            forward.y = 0.0f;
            forward = forward.normalized;

            // Calculate target direction based on camera forward and direction key.
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 targetDirection;
            targetDirection = forward * vertical + right * horizontal;

            // Lerp current direction to calculated target direction.
            if ((BehaviourController.IsMoving() && targetDirection != Vector3.zero))
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                Quaternion newRotation = Quaternion.Slerp(BehaviourController.GetRigidBody.rotation, targetRotation,
                    BehaviourController.turnSmoothing);
                BehaviourController.GetRigidBody.MoveRotation(newRotation);
                BehaviourController.SetLastDirection(targetDirection);
            }

            // If idle, Ignore current camera facing and consider last moving direction.
            if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
            {
                BehaviourController.Repositioning();
            }

            return targetDirection;
        }

        // Collision detection.
        private void OnCollisionStay(Collision collision)
        {
            isColliding = true;
            // Slide on vertical obstacles
            if (BehaviourController.IsCurrentBehaviour(this.GetBehaviourCode) && collision.GetContact(0).normal.y <= 0.1f)
            {
                float vel = BehaviourController.GetAnim.velocity.magnitude;
                Vector3 tangentMove = Vector3.ProjectOnPlane(transform.forward, collision.GetContact(0).normal).normalized *
                                      vel;
                BehaviourController.GetRigidBody.AddForce(tangentMove, ForceMode.VelocityChange);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            isColliding = false;
        }

                // Remove vertical rigidbody velocity.
        private void RemoveVerticalVelocity()
        {
            Vector3 horizontalVelocity = BehaviourController.GetRigidBody.velocity;
            horizontalVelocity.y = 0;
            BehaviourController.GetRigidBody.velocity = horizontalVelocity;
        }

        // Execute the idle and walk/run jump movements.
        void JumpManagement()
        {
            // Start a new jump.
            if (jump && !BehaviourController.GetAnim.GetBool(jumpBool) && BehaviourController.IsGrounded())
            {
                // Set jump related parameters.
                BehaviourController.LockTempBehaviour(this.behaviourCode);
                BehaviourController.GetAnim.SetBool(jumpBool, true);
                // Is a locomotion jump?
                if (BehaviourController.GetAnim.GetFloat(speedFloat) > 0.1f)
                {
                    // Temporarily change player friction to pass through obstacles.
                    capsuleCollider.material.dynamicFriction = 0f;
                    capsuleCollider.material.staticFriction = 0f;
                    // Remove vertical velocity to avoid "super jumps" on slope ends.
                    RemoveVerticalVelocity();
                    // Set jump vertical impulse velocity.
                    float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                    velocity = Mathf.Sqrt(velocity);
                    BehaviourController.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
                }
            }
            // Is already jumping?
            else if (BehaviourController.GetAnim.GetBool(jumpBool))
            {
                // Keep forward movement while in the air.
                if (!BehaviourController.IsGrounded() && !isColliding && BehaviourController.GetTempLockStatus())
                {
                    BehaviourController.GetRigidBody.AddForce(
                        transform.forward * jumpIntertialForce * Physics.gravity.magnitude * sprintSpeed,
                        ForceMode.Acceleration);
                }

                // Has landed?
                if ((BehaviourController.GetRigidBody.velocity.y < 0) && BehaviourController.IsGrounded())
                {
                    BehaviourController.GetAnim.SetBool(groundedBool, true);
                    // Change back player friction to default.
                    capsuleCollider.material.dynamicFriction = 0.6f;
                    capsuleCollider.material.staticFriction = 0.6f;
                    // Set jump related parameters.
                    jump = false;
                    BehaviourController.GetAnim.SetBool(jumpBool, false);
                    BehaviourController.UnlockTempBehaviour(this.behaviourCode);
                }
            }
        }

        // Deal with the basic player movement
        void MovementManagement(float horizontal, float vertical)
        {
            // On ground, obey gravity.
            if (BehaviourController.IsGrounded())
            {
                BehaviourController.GetRigidBody.useGravity = true;
            }
            // Avoid takeoff when reached a slope end.
            else if (!BehaviourController.GetAnim.GetBool(jumpBool) && BehaviourController.GetRigidBody.velocity.y > 0)
            {
                RemoveVerticalVelocity();
            }

            // Call function that deals with player orientation.
            Rotating(horizontal, vertical);

            // Set proper speed.
            Vector2 dir = new Vector2(horizontal, vertical);
            speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
            // This is for PC only, gamepads control speed via analog stick.
            speedSeeker += Input.GetAxis("Mouse ScrollWheel");
            speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
            speed *= speedSeeker;
            if (BehaviourController.IsSprinting())
            {
                speed = sprintSpeed;
            }

            BehaviourController.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
        }



        // Update is used to set features regardless the active behaviour.
        void Update()
        {
            // Get jump input.
            if (!jump && Input.GetButtonDown(ButtonName.Jump) && BehaviourController.IsCurrentBehaviour(this.behaviourCode) &&
                !BehaviourController.IsOverriding())
            {
                jump = true;
            }
        }
        // LocalFixedUpdate overrides the virtual function of the base class.
        public override void LocalFixedUpdate()
        {
            // Call the basic movement manager.
            MovementManagement(BehaviourController.GetH, BehaviourController.GetV);

            // Call the jump manager.
            JumpManagement();
        }
    }

}
