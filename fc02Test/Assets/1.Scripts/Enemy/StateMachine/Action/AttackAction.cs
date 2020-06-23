﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 총 4단계에 걸쳐 사격 :
    /// 1. 조준중이고 조준 유효각도안에 타겟이있거나 가깝냐
    /// 2. 발사 간격 딜레이가 되었다면 애니메이션 재생
    /// 3. 충돌 검출을 하는데 약간의 사격시 충격도 더해준다.
    /// 4. 총구 이펙트 및 총알 이펙트를 생성해준다.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Attack")]
    public class AttackAction : Action
    {
        private readonly float startShootDelay = 0.2f; // Delay before start shooting.
        private readonly float aimAngleGap = 30f; // Minimum angle gap between current and desired aim orientations.
        
        // The action on enable function, triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Setup initial values for the action.
            controller.variables.shotsInRound = Random.Range(controller.maximumBurst / 2, controller.maximumBurst);
            controller.variables.currentShots = 0;
            controller.variables.startShootTimer = 0f;
            controller.enemyAnimation.anim.ResetTrigger(AnimatorKey.Shooting);
            controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
            controller.variables.waitInCoverTimer = 0;
            controller.enemyAnimation.ActivatePendingAim();//조준 대기. 이제 시야에만 들려오면 조준 가능.
        }
        
        // Draw shot and extra assets.
        private void DoShot(StateController controller, Vector3 direction, Vector3 hitPoint,
            Vector3 hitNormal = default, bool organic = false, Transform target = null)
        {
            
            // Draw muzzle flash.
            GameObject muzzleFlash = EffectManager.Instance.EffectOneShot((int) EffectList.flash,Vector3.zero);
            muzzleFlash.transform.SetParent(controller.enemyAnimation.gunMuzzle);
            muzzleFlash.transform.localPosition = Vector3.zero;
            muzzleFlash.transform.localEulerAngles = Vector3.left * 90f;
            DestroyDelayed delayed = muzzleFlash.AddComponent<DestroyDelayed>();
            delayed.DelayedTime = 0.5f;

            // Draw shot tracer and smoke.
            GameObject shotTracer = EffectManager.Instance.EffectOneShot((int) EffectList.tracer, Vector3.zero);
            shotTracer.transform.SetParent(controller.enemyAnimation.gunMuzzle);
            Vector3 origin = controller.enemyAnimation.gunMuzzle.position;
            shotTracer.transform.position = origin;
            shotTracer.transform.rotation = Quaternion.LookRotation(direction);

            // Draw bullet hole and sparks, if target is not organic.
            if (target && !organic)
            {
                GameObject bulletHole =
                    EffectManager.Instance.EffectOneShot((int) EffectList.bulletHole, hitPoint + 0.01f * hitNormal);
                bulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

                GameObject instantSparks = EffectManager.Instance.EffectOneShot((int) EffectList.sparks, hitPoint);
            }
            // The object hit is organic, call take damage function.
            else if (target && organic)
            {
                HealthBase targetHealth = target.GetComponent<HealthBase>();
                if (targetHealth)
                {
                    targetHealth.TakeDamage(hitPoint, direction, controller.classStats.BulletDamage,
                        target.GetComponent<Collider>(), controller.gameObject);
                }
            }

            // Play shot audio clip at shot position.
            SoundManager.Instance.PlayShotSound(controller.classID,controller.enemyAnimation.gunMuzzle.position, 2f);
        }
        // Cast the shot.
        private void CastShot(StateController controller)
        {
            // Get shot imprecision vector.
            Vector3 imprecision =
                Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate)
                * controller.transform.right;

            imprecision += Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate)
                           * controller.transform.up;
            // Get shot desired direction.
            Vector3 shotDirection = controller.personalTarget - controller.enemyAnimation.gunMuzzle.position;
            // Cast shot.
            Ray ray = new Ray(controller.enemyAnimation.gunMuzzle.position, shotDirection.normalized + imprecision);
            if (Physics.Raycast(ray, out RaycastHit hit, controller.viewRadius, controller.generalStats.shotMask.value))
            {
                // Hit something organic? Consider all layers in target mask as organic.
                bool isOrganic = ((1 << hit.transform.root.gameObject.layer) & controller.generalStats.targetMask) != 0;
                DoShot(controller, ray.direction, hit.point, hit.normal, isOrganic, hit.transform);
            }
            else
            {
                // Hit nothing (miss shot), shot at desired direction with imprecision.
                DoShot(controller, ray.direction, ray.origin + (ray.direction * 500f));
            }
        }
        
        // Can the NPC shoot?
        private bool CanShoot(StateController controller)
        {
            float distance = (controller.personalTarget - 
                              controller.enemyAnimation.gunMuzzle.position).sqrMagnitude;
            // NPC is aiming and almost aligned with desired position?
            if (controller.Aiming &&
                (controller.enemyAnimation.currentAimAngleGap < aimAngleGap ||
                 // Or if the target is too close, shot anyway
                 distance <= 5.0f))
            {
                // All conditions match, check start delay.
                if (controller.variables.startShootTimer >= startShootDelay)
                {
                    return true;
                }
                else
                {
                    controller.variables.startShootTimer += Time.deltaTime;
                }
            }
            
            return false;
        }



        // Perform the shoot action.
        private void Shoot(StateController controller)
        {
            // Check interval between shots.
            if (Time.timeScale > 0 && controller.variables.shotTimer == 0f)
            {
                controller.enemyAnimation.anim.SetTrigger(AnimatorKey.Shooting);
                CastShot(controller);
            }
            // Update shot related variables and habilitate next shot.
            else if (controller.variables.shotTimer >= (0.1f + 2 * Time.deltaTime))
            {
                controller.bullets = Mathf.Max(--controller.bullets, 0);
                controller.variables.currentShots++;
                controller.variables.shotTimer = 0f;
                return;
            }

            controller.variables.shotTimer += controller.classStats.ShotRateFactor * Time.deltaTime;
        }

        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
            // Always focus on sight position.
            controller.focusSight = true;

            if (CanShoot(controller))
            {
                Shoot(controller);
            }

            // Accumulate blind engage timer.
            controller.variables.blindEngageTimer += Time.deltaTime;
        }
        
    }
}