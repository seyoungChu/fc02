using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class ThirdPersonOrbitCam : MonoBehaviour
    {
        public Transform player; // Player's reference.
        public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f); // Offset to repoint the camera.
        public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f); // Offset to relocate the camera related to the player position.

        public float smooth = 10f; // Speed of camera responsiveness.
        public float horizontalAimingSpeed = 6f; // Horizontal turn speed.
        public float verticalAimingSpeed = 6f; // Vertical turn speed.
        public float maxVerticalAngle = 30f; // Camera max clamp angle. 
        public float minVerticalAngle = -60f; // Camera min clamp angle.
        public float recoilAngleBounce = 5.0f;
        private float angleH = 0; // Float to store camera horizontal angle related to mouse movement.
        private float angleV = 0; // Float to store camera vertical angle related to mouse movement.
        private Transform cameraTransform; // This transform.
        private Camera myCamera;
        private Vector3 relCameraPos; // Current camera position relative to the player.
        private float relCameraPosMag; // Current camera distance to the player.
        private Vector3 smoothPivotOffset; // Camera current pivot offset on interpolation.
        private Vector3 smoothCamOffset; // Camera current offset on interpolation.
        private Vector3 targetPivotOffset; // Camera pivot offset target to iterpolate.
        private Vector3 targetCamOffset; // Camera offset target to interpolate.
        private float defaultFOV; // Default camera Field of View.
        private float targetFOV; // Target camera Field of View.
        private float targetMaxVerticalAngle; // Custom camera max vertical clamp angle.
        private float recoilAngle = 0f; // The angle to vertically bounce the camera in a recoil movement.

        // Get the camera horizontal angle.
        public float GetH
        {
            get { return angleH; }
        }

        void Awake()
        {
            // 캐싱.
            cameraTransform = transform;
            myCamera = cameraTransform.GetComponent<Camera>();
            // 카메라 기본 포지션 세.
            cameraTransform.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
            cameraTransform.rotation = Quaternion.identity;

            // 카메라와 플레이어간의 상대 벡터, 충돌 체크에 사용됩니다. Get camera position relative to the player, used for collision test.
            relCameraPos = cameraTransform.position - player.position;
            relCameraPosMag = relCameraPos.magnitude - 0.5f;

            // 참조 기본 값 세팅. Set up references and default values.
            smoothPivotOffset = pivotOffset;
            smoothCamOffset = camOffset;
            defaultFOV = myCamera.fieldOfView;
            angleH = player.eulerAngles.y;

            ResetTargetOffsets();
            ResetFOV();
            ResetMaxVerticalAngle();
        }
        
        void Update()
        {
            // Get mouse movement to orbit the camera.
            // 마우스 이동 값:
            angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed;
            angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed;
            
            // Set vertical movement limit.
            angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

            // Set vertical camera bounce.
            angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);

            // Set camera orientation.
            Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
            Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
            cameraTransform.rotation = aimRotation;

            // Set FOV.
            myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

            // Test for collision with the environment based on current camera position.
            Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
            Vector3 noCollisionOffset = targetCamOffset;
            for (float zOffset = targetCamOffset.z; zOffset <= 0; zOffset += 0.5f)
            {
                noCollisionOffset.z = zOffset;
                if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, Mathf.Abs(zOffset)) ||
                    zOffset == 0)
                {
                    break;
                }
            }

            // Repostition the camera.
            smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
            smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset, smooth * Time.deltaTime);

            cameraTransform.position = player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;

            // Amortize Camera vertical bounce.
            if (recoilAngle > 0.0f)
            {
                recoilAngle -= recoilAngleBounce * Time.deltaTime;
            }
            else if (recoilAngle < 0.0f)
            {
                recoilAngle += recoilAngleBounce * Time.deltaTime;
            }

        }
        
        // Bounce the camera vertically.
        public void BounceVertical(float degrees)
        {
            recoilAngle = degrees;
        }

        // Set camera offsets to custom values.
        public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
        {
            targetPivotOffset = newPivotOffset;
            targetCamOffset = newCamOffset;
        }

        // Reset camera offsets to default values.
        public void ResetTargetOffsets()
        {
            targetPivotOffset = pivotOffset;
            targetCamOffset = camOffset;
        }

        // Set custom Field of View.
        public void SetFOV(float customFOV)
        {
            this.targetFOV = customFOV;
        }

        // Reset Field of View to default value.
        public void ResetFOV()
        {
            this.targetFOV = defaultFOV;
        }
        
        // Reset max vertical camera rotation angle to default value.
        public void ResetMaxVerticalAngle()
        {
            this.targetMaxVerticalAngle = maxVerticalAngle;
        }

        // Double check for collisions: concave objects doesn't detect hit from outside, so cast in both directions.
        bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
        {
            float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
            return ViewingPosCheck(checkPos, playerFocusHeight) &&
                   ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
        }

        // Check for collision from camera to player.
        bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
        {
            // Cast target.
            Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);
            // If a raycast from the check position to the player hits something...
            if (Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
            {
                // ... if it is not the player...
                if (hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
                {
                    // This position isn't appropriate.
                    return false;
                }
            }

            // If we haven't hit anything or we've hit the player, this is an appropriate position.
            return true;
        }

        // Check for collision from player to camera.
        bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
        {
            // Cast origin.
            Vector3 origin = player.position + (Vector3.up * deltaPlayerHeight);
            if (Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
            {
                if (hit.transform != player && hit.transform != transform &&
                    !hit.transform.GetComponent<Collider>().isTrigger)
                {
                    return false;
                }
            }

            return true;
        }

        // Get camera magnitude.
        public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
        {
            return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
        }
    }

}
