using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 더블 체크를 하는데 근처에 장애물이나 엄폐물이 가깝게 있는지 체크 한번.
    /// 타겟 목표까지 장애물이나 엄폐물이 있는지 체크.. 만약 플레이어 충돌체가 검출되면 타겟까지 막힌게 없다는 뜻. 
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Clear Shot")]
    public class ClearShotDecision : Decision
    {
        [Header("Extra Decisions")]
        [Tooltip("The NPC near sense decision.")]
        public FocusDecision targetNear;
        
        // Cast sphere for near obstacles, and line to personal target (not the aim target) for clean shot.
        private bool HaveClearShot(StateController controller)
        {
            Vector3 shotOrigin = controller.transform.position + 
                                 Vector3.up * (controller.generalStats.aboveCoverHeight + controller.nav.radius);
            Vector3 shotDirection = controller.personalTarget - shotOrigin;

            // Cast sphere in target direction to check for obstacles in near radius.
            bool blockedShot = Physics.SphereCast(shotOrigin, controller.nav.radius, shotDirection, out RaycastHit hit,
                controller.nearRadius, 
                controller.generalStats.coverMask | controller.generalStats.obstacleMask);
            if (!blockedShot)
            {
                // No near obstacles, cast line to target position and check for clear shot.
                blockedShot = Physics.Raycast(shotOrigin, shotDirection, out hit, shotDirection.magnitude,
                    controller.generalStats.coverMask | controller.generalStats.obstacleMask);
                // Hit something, is it the target? If true, shot is clear.
                if (blockedShot)
                {
                    //레이캐스트 결과가 타겟이라면 타겟까지 막힌게 없다는 뜻.
                    //타겟이 아니라면 몬가에 막혔다는 뜻.
                    blockedShot = !(hit.transform.root == controller.aimTarget.root);
                }
            }
            return !blockedShot;
        }

        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            return targetNear.Decide(controller) || HaveClearShot(controller);
        }

    }
}

