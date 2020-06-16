using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 장애물로 이동할 수 있는 상황인지 아닌지에 따라 판단.
    /// 쏴야할 총알이 남아 있거나, 엄폐물로 이동하기 전에 대기시간이 남아 있거나,
    /// 근처에 숨을만한 엄폐물이 없을경우 실패
    /// 그외에는 장애물로 이동.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Take Cover")]
    public class TakeCoverDecision : Decision
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // NPC still have shots to fire on current engage round?
            // Or the cover period for this cover round has ended?
            // Or there is no current cover spot?
            //지금 쏴야할 총알이 남아있거나, 대기 시간이 더 필요하거나, 커버 위치를 못찾았다면
            if (controller.variables.currentShots < controller.variables.shotsInRound
                || controller.variables.waitInCoverTimer > controller.variables.coverTime
                || Equals(controller.CoverSpot, Vector3.positiveInfinity))
            {
                return false;
            }
            // Otherwise, NPC will take cover. 
            //그외에는 장애물로 이동.
            else
            {
                return true;
            }
        }
    }
}