﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
using Random = UnityEngine.Random;

namespace FC
{
    /// <summary>
    /// 사격 기능 : 사격이 가능한지 막혀 있는지 여부를 체크,
    /// 발사 키 입력 받아서 애니메이션과 이펙트도 생성, 충돌 체크 기능도 갖고 있다.
    /// UI 관련해서 십자선 텍스처를 변경해주고
    /// 발사 속도 조정
    /// 캐릭터 상체를 IK를 이용, 조준 시점에 맞춰 회전 조절.
    /// 벽이나 충돌체에 부딪쳤을경우 피탄 이펙트를 생성(무한은 아님)
    /// 인벤토리 역활, 무기를 소지하고 있는 지 확인.
    /// 재장전과 무기 교체 기능도 포함.
    /// </summary>
    public class ShootBehaviour : GenericBehaviour
    {
        public Texture2D aimCrosshair, shootCrosshair; //십자선 텍스쳐. Crosshair textures for aiming and shooting.
        public GameObject muzzleFlash, shot, sparks; //발사 이펙트. Game objects for shot effects.
        public Material bulletHole; //총알 구멍 매트리얼. Material for the bullet hole placed on target shot.
        public int maxBulletHoles = 50; //씬에서 그릴수 있는 최대 총알 구멍 Max bullet holes to draw on scene.
        public float shotErrorRate = 0.01f; // 발사 실패율 Shooting error margin. 0 is most acurate.
        public float shotRateFactor = 1f; // 발사 속도 Rate of fire parameter. Higher is faster rate.

        public float armsRotation = 8f; // 조준시 팔의 위치 회전.  Rotation of arms to align with aim, according player heigh.

        //Ignore Raycast, Ignore Shot, Cover Invisible, Player,
        // Layer mask to cast shots.
        public LayerMask shotMask = //~((1 << 2) | (1 << 9) | (1 << 10) | (1 << 11));
            ~(TagAndLayer.LayerMasking.IgnoreRaycastLayer | TagAndLayer.LayerMasking.IgnoreShotLayer |
              TagAndLayer.LayerMasking.CoverInvisible | TagAndLayer.LayerMasking.Player);

        //생명체인지 아닌지. 생명체가 아니면 총알 구멍을.
        public LayerMask organicMask; // Layer mask to define organic matter.

        [Header("Advanced Rotation Adjustments")]
        //짧은 총, 피스톨 같은 총을 들었을때는 조준시 왼팔의 위치를 보정.
        public Vector3
            LeftArmShortAim = new Vector3(-4.0f, 0.0f, 2.0f); // Local left arm rotation when aiming with a short gun.

        private int activeWeapon = 0; //사용무기 인덱스.  Index of the active weapon.
        private int weaponTypeInt; //애니메이터 밸류, 무기 타입관련 Animator variable related to the weapon type.
        private int changeWeaponTrigger; //애니메이터 밸류, 무기 교체 Animator trigger for changing weapon.
        private int shootingTrigger; //애니메이터 트리거, 무기 발사. Animator trigger for shooting weapon.

        private List<InteractiveWeapon> weapons; //무기 인벤토리 Weapons inventory.

        //애니메이터 밸류, 조준, 조준 블럭, 재장전.
        private int aimBool, // Animator variables related to aiming.
            blockedAimBool, // Animator variable related to blocked aim.
            reloadBool; // Animator variable related to reloading.

        //조준 중인지, 조준 블럭당했는지 여부.
        private bool isAiming, // Boolean to get whether or not the player is aiming.
            isAimBlocked; // Boolean to determine whether or not the aim is blocked.

        //총구의 월드 포지션.
        private Transform gunMuzzle; // World position of the gun muzzle.
        private float distToHand; //목에서 손까지의 거리. Distance from neck to hand.

        private Vector3 castRelativeOrigin; //블럭 조준 테스트를 위한 목의 위치. Position of neck to cast for blocked aim test.

        //무기 유형을 인벤토리 슬롯에 지정하도록 매핑.
        private Dictionary<InteractiveWeapon.WeaponType, int>
            slotMap; // Map to designate weapon types to inventory slots.

        //아바타 뼈대 본 트랜스폼
        private Transform hips, spine, chest, rightHand, leftArm; // Avatar bone transforms.
        private Vector3 initialRootRotation; // Initial root bone local rotation.
        private Vector3 initialHipsRotation; // Initial hips rotation related to the root bone.
        private Vector3 initialSpineRotation; // Initial spine rotation related to the hips bone.
        private Vector3 initialChestRotation; // Initial chest rotation related to the spine bone.

        private float
            shotInterval, originalShotInterval = 0.5f; //기본 총알 수명 Default shot lifetime. Use shotRateFactor to modify speed.

        private List<GameObject> bulletHoles; //피탄 구멍 Bullet holes scene buffer.
        private int bulletHoleSlot = 0; //씬에서의 피탄 구멍 숫자. Number of active bullet holes on scene.
        private int burstShotCount = 0; //산탄총 숫자. Number of burst shots fired.
        private AimBehaviour aimBehaviour; //조준 비해비어 캐싱. Reference to the aim behaviour.
        private Texture2D originalCrosshair; //기본 십자선. Original unarmed aim behaviour crosshair.
        private bool isShooting = false; //사격중인지 Boolean to determine if player is holding shoot button.

        private bool
            isChangingWeapon = false; //무기를 변경중인지 Boolean to determine if player is holding change weapon button.

        private bool
            isShotAlive = false; //씬에 사라지지않은 총알이 있는지 확인. Boolean to determine if there is any active shot on scene.

        // Start is always called after any Awake functions.
        void Start()
        {
            // Set up the references.
            weaponTypeInt = Animator.StringToHash(AnimatorKey.Weapon);
            aimBool = Animator.StringToHash(AnimatorKey.Aim);
            blockedAimBool = Animator.StringToHash(AnimatorKey.BlockedAim);
            changeWeaponTrigger = Animator.StringToHash(AnimatorKey.ChangeWeapon);
            shootingTrigger = Animator.StringToHash(AnimatorKey.Shooting);
            reloadBool = Animator.StringToHash(AnimatorKey.Reload);
            weapons = new List<InteractiveWeapon>(new InteractiveWeapon[3]);
            aimBehaviour = this.GetComponent<AimBehaviour>();
            bulletHoles = new List<GameObject>();

            // Hide shot effects on scene.
            muzzleFlash.SetActive(false);
            shot.SetActive(false);
            sparks.SetActive(false);

            // Create weapon slots. Different weapon types can be added in the same slot - ex.: (LONG_SPECIAL, 2) for a rocket launcher.
            slotMap = new Dictionary<InteractiveWeapon.WeaponType, int>
            {
                {InteractiveWeapon.WeaponType.SHORT, 1},
                {InteractiveWeapon.WeaponType.LONG, 2}
            };

            // Get player's avatar bone transforms for IK.
            Transform neck = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.Neck);
            if (!neck)
            {
                neck = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.Head).parent;
            }

            hips = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.Hips);
            spine = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.Spine);
            chest = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.Chest);
            rightHand = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.RightHand);
            leftArm = BehaviourController.GetAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);

            // Set default values.
            initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
            initialHipsRotation = hips.localEulerAngles;
            initialSpineRotation = spine.localEulerAngles;
            initialChestRotation = chest.localEulerAngles;
            originalCrosshair = aimBehaviour.crosshair;
            shotInterval = originalShotInterval;
            castRelativeOrigin = neck.position - this.transform.position;
            distToHand = (rightHand.position - neck.position).magnitude * 1.5f;
        }

        // 발사 비주얼 이펙트 담당. Manage the shot visual effects.
        private void DrawShoot(GameObject weapon, Vector3 destination, Vector3 targetNormal, Transform parent,
            bool placeSparks = true, bool placeBulletHole = true)
        {
            Vector3 origin = gunMuzzle.position - gunMuzzle.right * 0.5f;

            // Draw the flash at the gun muzzle position.
            muzzleFlash.SetActive(true);
            muzzleFlash.transform.SetParent(gunMuzzle);
            muzzleFlash.transform.localPosition = Vector3.zero;
            muzzleFlash.transform.localEulerAngles = Vector3.back * 90f;

            // Create the shot tracer and smoke trail particle.
            GameObject instantShot =
                EffectManager.Instance.EffectOneShot((int) EffectList.tracer,
                    origin); //Object.Instantiate<GameObject>(shot);
            instantShot.SetActive(true);
            instantShot.transform.rotation = Quaternion.LookRotation(destination - origin);
            instantShot.transform.parent = shot.transform.parent;

            // Create the shot sparks at target.
            if (placeSparks)
            {
                GameObject instantSparks =
                    EffectManager.Instance.EffectOneShot((int) EffectList.sparks, destination);
                instantSparks.SetActive(true);
                instantSparks.transform.parent = sparks.transform.parent;
            }

            // Put bullet hole on the target.
            if (placeBulletHole)
            {
                Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, targetNormal);
                GameObject bullet = null;
                if (bulletHoles.Count < maxBulletHoles)
                {
                    // Instantiate new bullet if an empty slot is available.
                    bullet = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    bullet.GetComponent<MeshRenderer>().material = bulletHole;
                    bullet.GetComponent<Collider>().enabled = false;
                    bullet.transform.localScale = Vector3.one * 0.07f;
                    bullet.name = "BulletHole";
                    bulletHoles.Add(bullet);
                }
                else
                {
                    // Cycle through bullet slots to reposition the oldest one.
                    bullet = bulletHoles[bulletHoleSlot];
                    bulletHoleSlot++;
                    bulletHoleSlot %= maxBulletHoles;
                }

                bullet.transform.position = destination + 0.01f * targetNormal;
                bullet.transform.rotation = hitRotation;
                bullet.transform.SetParent(parent);
            }
        }

        // Shoot the weapon.
        private void ShootWeapon(int weapon, bool firstShot = true)
        {
            // Check conditions to shoot.
            if (!isAiming || isAimBlocked || BehaviourController.GetAnim.GetBool(reloadBool) ||
                !weapons[weapon].Shoot(firstShot))
            {
                return;
            }
            else
            {
                // Update parameters: burst count, trigger for animation, crosshair change and recoil camera bounce.
                //버스트샷 카운트 증가, 발사 애니메이션 트리거 발동, 십자선 이미지 교체,
                //반동 추가.
                burstShotCount++;
                BehaviourController.GetAnim.SetTrigger(shootingTrigger);
                aimBehaviour.crosshair = shootCrosshair;
                BehaviourController.GetCamScript.BounceVertical(weapons[weapon].recoilAngle);

                // Cast the shot to find a target.
                //살짝 부정확하게 만든다.
                Vector3 imprecision = Random.Range(-shotErrorRate, shotErrorRate) *
                                      BehaviourController.playerCamera.forward;
                Ray ray = new Ray(BehaviourController.playerCamera.position,
                    BehaviourController.playerCamera.forward + imprecision);
                RaycastHit hit = default(RaycastHit);
                // Target was hit.
                if (Physics.Raycast(ray, out hit, 500f, shotMask))
                {
                    if (hit.collider.transform != this.transform)
                    {
                        // Is the target organic?
                        bool isOrganic = (organicMask == (organicMask |
                                                          (1 << hit.transform.root.gameObject.layer)));
                        // Handle shot effects on target.
                        DrawShoot(weapons[weapon].gameObject, hit.point,
                            hit.normal, hit.collider.transform, !isOrganic,
                            !isOrganic);

                        // Call the damage behaviour of target if exists.
                        if (hit.collider)
                        {
                            hit.collider.SendMessageUpwards("HitCallback",
                                new HealthBase.DamageInfo(
                                    hit.point, ray.direction, weapons[weapon].bulletDamage, hit.collider),
                                SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                // No target was hit.
                else
                {
                    Vector3 destination = (ray.direction * 500f) - ray.origin;
                    // Handle shot effects without a specific target.
                    DrawShoot(weapons[weapon].gameObject, destination, Vector3.up, null, false, 
                        false);
                }

                // Play shot sound.
                SoundManager.Instance.PlayOneShotEffect((int) weapons[weapon].shotSound, gunMuzzle.position, 5f);
                // Trigger alert callback - AlertChecker 가 필요하다.
                GameObject gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);
                gameController.SendMessage("RootAlertNearby", ray.origin,
                    SendMessageOptions.DontRequireReceiver);
                // Reset shot lifetime.
                shotInterval = originalShotInterval;
                isShotAlive = true;
            }
        }

        // Handle reload weapon end (called by animation).
        public void EndReloadWeapon()
        {
            BehaviourController.GetAnim.SetBool(reloadBool, false);
            weapons[activeWeapon].EndReload();
        }

        // Change HUD crosshair when aiming.
        private void SetWeaponCrosshair(bool armed)
        {
            if (armed)
            {
                aimBehaviour.crosshair = aimCrosshair;
            }
            else
            {
                aimBehaviour.crosshair = originalCrosshair;
            }
        }

        // Handle the shot parameters during its lifetime.
        private void ShotProgress()
        {
            // Update parameters on imminent shot death.
            if (shotInterval > 0.2f)
            {
                shotInterval -= shotRateFactor * Time.deltaTime;
                if (shotInterval <= 0.4f)
                {
                    // Return crosshair to normal aim mode and hide shot flash.
                    SetWeaponCrosshair(activeWeapon > 0);
                    muzzleFlash.SetActive(false);

                    if (activeWeapon > 0)
                    {
                        // Set camera bounce return on recoil end.
                        BehaviourController.GetCamScript.BounceVertical(-weapons[activeWeapon].recoilAngle * 0.1f);

                        // Handle next shot for burst or auto mode.
                        if (shotInterval <= (0.4f - 2 * Time.deltaTime))
                        {
                            // Auto mode, keep firing while shoot button is pressed.
                            if (weapons[activeWeapon].mode == InteractiveWeapon.WeaponMode.AUTO &&
                                Input.GetAxisRaw(ButtonName.Shoot) != 0)
                            {
                                ShootWeapon(activeWeapon, false);
                            }
                            // Burst mode, keep shooting until reach weapon burst capacity.
                            else if (weapons[activeWeapon].mode == InteractiveWeapon.WeaponMode.BURST &&
                                     burstShotCount < weapons[activeWeapon].burstSize)
                            {
                                ShootWeapon(activeWeapon, false);
                            }
                            // Reset burst count for other modes.
                            else if (weapons[activeWeapon].mode != InteractiveWeapon.WeaponMode.BURST)
                            {
                                burstShotCount = 0;
                            }
                        }
                    }
                }
            }
            // Shot is dead, reset parameters.
            else
            {
                isShotAlive = false;
                BehaviourController.GetCamScript.BounceVertical(0);
                burstShotCount = 0;
            }
        }

        // Change the active weapon.
        private void ChangeWeapon(int oldWeapon, int newWeapon)
        {
            // Previously armed? Disable weapon.
            if (oldWeapon > 0)
            {
                weapons[oldWeapon].gameObject.SetActive(false);
                gunMuzzle = null;
                weapons[oldWeapon].Toggle(false);
            }

            // Cycle trought empty slots to find next existing weapon or the no weapon slot.
            while (weapons[newWeapon] == null && newWeapon > 0)
            {
                newWeapon = (newWeapon + 1) % weapons.Count;
            }

            // Next weapon exists? Activate it.
            if (newWeapon > 0)
            {
                weapons[newWeapon].gameObject.SetActive(true);
                gunMuzzle = weapons[newWeapon].transform.Find("muzzle");
                weapons[newWeapon].Toggle(true);
            }

            activeWeapon = newWeapon;

            // Call change weapon animation if new weapon type is different.
            if (oldWeapon != newWeapon)
            {
                BehaviourController.GetAnim.SetTrigger(changeWeaponTrigger);
                BehaviourController.GetAnim.SetInteger(weaponTypeInt,
                    weapons[newWeapon] ? (int) weapons[newWeapon].type : 0);
            }

            // Set crosshair if armed.
            SetWeaponCrosshair(newWeapon > 0);
        }

        // Update is used to set features regardless the active behaviour.
        private void Update()
        {
            // Handle shoot weapon action.
            float shootTrigger = Math.Abs(Input.GetAxisRaw(ButtonName.Shoot));
            if (shootTrigger > Mathf.Epsilon && !isShooting && activeWeapon > 0 && burstShotCount == 0)
            {
                isShooting = true;
                ShootWeapon(activeWeapon);
            }
            else if (isShooting && shootTrigger < Mathf.Epsilon)
            {
                isShooting = false;
            }
            // Handle reload weapon action.
            else if (Input.GetButtonUp(ButtonName.Reload) && activeWeapon > 0)
            {
                if (weapons[activeWeapon].StartReload())
                {
                    //AudioSource.PlayClipAtPoint(weapons[activeWeapon].reloadSound, gunMuzzle.position, 0.5f);
                    SoundManager.Instance.PlayOneShotEffect((int) weapons[activeWeapon].reloadSound, gunMuzzle.position,
                        0.5f);
                    BehaviourController.GetAnim.SetBool(reloadBool, true);
                }
            }
            // Handle drop weapon action.
            else if (Input.GetButtonDown(ButtonName.Drop) && activeWeapon > 0)
            {
                // End reload paramters, drop weapon and change to another one in inventory.
                EndReloadWeapon();
                int weaponToDrop = activeWeapon;
                ChangeWeapon(activeWeapon, 0);
                weapons[weaponToDrop].Drop();
                weapons[weaponToDrop] = null;
            }
            // Handle change weapon action.
            else
            {
                if ((Math.Abs(Input.GetAxisRaw(ButtonName.Change)) > Mathf.Epsilon && !isChangingWeapon))
                {
                    isChangingWeapon = true;
                    int nextWeapon = activeWeapon + 1;
                    ChangeWeapon(activeWeapon, (nextWeapon) % weapons.Count);
                }
                else if (Math.Abs(Input.GetAxisRaw(ButtonName.Change)) < Mathf.Epsilon)
                {
                    isChangingWeapon = false;
                }
            }

            // Manage shot parameters after shooting action.
            if (isShotAlive)
            {
                ShotProgress();
            }

            isAiming = BehaviourController.GetAnim.GetBool(aimBool);
        }

        // Add a new weapon to inventory (called by weapon object).
        public void AddWeapon(InteractiveWeapon newWeapon)
        {
            // Position new weapon in player's hand.
            newWeapon.gameObject.transform.SetParent(rightHand);
            newWeapon.transform.localPosition = newWeapon.rightHandPosition;
            newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.relativeRotation);

            // Handle inventory slot conflict.
            if (this.weapons[slotMap[newWeapon.type]])
            {
                // Same weapon type, recharge bullets and destroy duplicated object.
                if (this.weapons[slotMap[newWeapon.type]].label == newWeapon.label)
                {
                    this.weapons[slotMap[newWeapon.type]].ResetBullets();
                    ChangeWeapon(activeWeapon, slotMap[newWeapon.type]);
                    GameObject.Destroy(newWeapon.gameObject);
                    return;
                }
                // Different weapon type, grab the new one and drop the weapon in inventory.
                else
                {
                    this.weapons[slotMap[newWeapon.type]].Drop();
                }
            }

            // Call change weapon action.
            this.weapons[slotMap[newWeapon.type]] = newWeapon;
            ChangeWeapon(activeWeapon, slotMap[newWeapon.type]);
        }

        // Check if aim is blocked by obstacles.
        private bool CheckforBlockedAim()
        {
            isAimBlocked = Physics.SphereCast(this.transform.position + castRelativeOrigin, 0.1f,
                BehaviourController.GetCamScript.transform.forward, out RaycastHit hit, distToHand - 0.1f);
            isAimBlocked = isAimBlocked && hit.collider.transform != this.transform;
            BehaviourController.GetAnim.SetBool(blockedAimBool, isAimBlocked);
            Debug.DrawRay(this.transform.position + castRelativeOrigin,
                BehaviourController.GetCamScript.transform.forward * distToHand, isAimBlocked ? Color.red : Color.cyan);

            return isAimBlocked;
        }

        // Manage inverse kinematic parameters.
        public void OnAnimatorIK(int layerIndex)
        {
            if (isAiming && activeWeapon > 0)
            {
                if (CheckforBlockedAim())
                {
                    return;
                }


                // Orientate upper body where camera  is targeting.
                Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                targetRot *= Quaternion.Euler(initialRootRotation);
                targetRot *= Quaternion.Euler(initialHipsRotation);
                targetRot *= Quaternion.Euler(initialSpineRotation);
                // Set upper body horizontal orientation.
                BehaviourController.GetAnim.SetBoneLocalRotation(HumanBodyBones.Spine,
                    Quaternion.Inverse(hips.rotation) * targetRot);

                // Keep upper body orientation regardless strafe direction.
                float xCamRot = Quaternion.LookRotation(BehaviourController.playerCamera.forward).eulerAngles.x;
                targetRot = Quaternion.AngleAxis(xCamRot + armsRotation, this.transform.right);
                if (weapons[activeWeapon] && weapons[activeWeapon].type == InteractiveWeapon.WeaponType.LONG)
                {
                    // Correction for long weapons.
                    targetRot *= Quaternion.AngleAxis(9f, this.transform.right);
                    targetRot *= Quaternion.AngleAxis(20f, this.transform.up);
                }

                targetRot *= spine.rotation;
                targetRot *= Quaternion.Euler(initialChestRotation);
                // Set upper body vertical orientation.
                BehaviourController.GetAnim.SetBoneLocalRotation(HumanBodyBones.Chest,
                    Quaternion.Inverse(spine.rotation) * targetRot);
            }
        }

        // Manage post animation step corrections.
        private void LateUpdate()
        {
            // Correct left arm position when aiming with a short gun.
            if (isAiming && weapons[activeWeapon] && weapons[activeWeapon].type == InteractiveWeapon.WeaponType.SHORT)
            {
                //leftArm.Rotate(new Vector3(leftleft, leftDown, leftBack));
                leftArm.localEulerAngles = leftArm.localEulerAngles + LeftArmShortAim;
            }
        }
    }
}