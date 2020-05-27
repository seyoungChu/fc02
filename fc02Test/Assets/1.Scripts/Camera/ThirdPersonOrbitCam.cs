using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    
    //카메라의 포지션에서 오프셋 포지션 + 피봇 포지션이 존재하는데 (오프셋은 충돌처리용 포지션,피봇 포지션은 시선이동에 사용.)
    //충돌 체크는 이중 충돌 (캐릭터로 부터 카메라로,카메라로 부터 캐릭터로)
    //리코일 (사격 반동)과
    //FOV 변경 기능.    
    public class ThirdPersonOrbitCam : MonoBehaviour
    {
        public Transform player; // 플레이어 트랜스폼 캐싱.
        public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f); // 카메라 포인트 오프셋. Offset to repoint the camera.
        public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f); // 플레이어로부터의 카메라 재배치를 위한 오프셋.Offset to relocate the camera related to the player position.

        public float smooth = 10f; // 카메라 반응속도.Speed of camera responsiveness.
        public float horizontalAimingSpeed = 6f; // 수평 회전 속도.Horizontal turn speed.
        public float verticalAimingSpeed = 6f; // 수직 회전 속도.Vertical turn speed.
        public float maxVerticalAngle = 30f; // 카메라의 수직 최대 각도.Camera max clamp angle. 
        public float minVerticalAngle = -60f; // 카메라의 수직 최소 각도.Camera min clamp angle.
        public float recoilAngleBounce = 5.0f; // 사격 반동 바운스 값.
        private float angleH = 0f; // 마우스이동에 따른 카메라 수평이동 수치. Float to store camera horizontal angle related to mouse movement.
        private float angleV = 0f; // 마우스이동에 따른 카메라 수직이동 수치.Float to store camera vertical angle related to mouse movement.
        private Transform cameraTransform; // 트랜스폼 캐싱.This transform.
        private Camera myCamera; //카메라 캐싱.
        private Vector3 relCameraPos; // 플레이어로부터 카메라까지의 위치. Current camera position relative to the player.
        private float relCameraPosMag; // 플레이어와 카메라사이의 거리. Current camera distance to the player.
        private Vector3 smoothPivotOffset; // 카메라 포인트 보간 오프셋. Camera current pivot offset on interpolation.
        private Vector3 smoothCamOffset; // 카메라 재배치 보간 오프셋.Camera current offset on interpolation.
        private Vector3 targetPivotOffset; // 카메라 포인트 보간 타겟 오프셋.Camera pivot offset target to iterpolate.
        private Vector3 targetCamOffset; // 카메라 재배치 보간 타겟 오프셋.Camera offset target to interpolate.
        private float defaultFOV; // 기본. Default camera Field of View.
        private float targetFOV; // 타겟. Target camera Field of View.
        private float targetMaxVerticalAngle; //카메라 수직 최대 각도 커스텀 값. Custom camera max vertical clamp angle.
        private float recoilAngle = 0f; // 사격 반동 각도. The angle to vertically bounce the camera in a recoil movement.

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

        // Reset camera offsets to default values.
        public void ResetTargetOffsets()
        {
            targetPivotOffset = pivotOffset;
            targetCamOffset = camOffset;
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

        // Set custom Field of View.
        public void SetFOV(float customFOV)
        {
            this.targetFOV = customFOV;
        }

        // 카메라와 플레이어간 충돌 검출 Check for collision from camera to player.
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


        // Double check for collisions: concave objects doesn't detect hit from outside, so cast in both directions.
        bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
        {
            float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
            return ViewingPosCheck(checkPos, playerFocusHeight) &&
                   ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
        }
        
        void Update()
        {
            // Get mouse movement to orbit the camera.
            // 마우스 이동 값:
            angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
            angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;
            
            // 수직 이동 제한. Set vertical movement limit.
            angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

            // 수직 카메라 바운스.Set vertical camera bounce.
            angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);

            // Set camera orientation.
            Quaternion camYRotation = Quaternion.Euler(0f, angleH, 0f);
            Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0f);
            cameraTransform.rotation = aimRotation;

            // Set FOV.
            myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

            // 카메라와 배경의 충돌 테스트 Test for collision with the environment based on current camera position.
            Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
            Vector3 noCollisionOffset = targetCamOffset; //에임을 할때 카메라의 오프셋값. 평소와 조준중엔 다르다!.
            for (float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f)
            {
                noCollisionOffset.z = zOffset;
                if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, Mathf.Abs(zOffset)) ||
                    zOffset == 0f)
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
        // Get camera magnitude.
        public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
        {
            return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
        }
    }

}
