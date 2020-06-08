using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/GeneralStats")]
    public class GeneralStats : ScriptableObject
    {
        [Header("General")]
        [Tooltip("NPC 정찰 속도(clear state).")]
        public float patrolSpeed = 2f;
        [Tooltip("NPC 검색 속도 (warning state).")]
        public float chaseSpeed = 5f;
        [Tooltip("NPC 회피 속도 (engage state).")]
        public float evadeSpeed = 15f;
        [Tooltip("웨이포인트에서 대기시간.")]
        public float patrolWaitTime = 2f;
        [Tooltip("장애물 레이어마스크")]
        public LayerMask obstacleMask;
        [Header("Animation")]
        [Tooltip("조준시 깜빡임을 피하기위한 확정 앵글(deadzone)")]
        public float angleDeadzone = 5f;
        [Tooltip("속도 댐핑 시간.")]
        public float speedDampTime = 0.4f;
        [Tooltip("각속도 댐핑 시간.")]
        public float angularSpeedDampTime = 0.2f;
        [Tooltip("각속도 안에서 각도 회전에 대한 반응 시간")]
        public float angleResponseTime = 0.2f;
        [Header("Cover")]
        [Tooltip("장애물에 숨었을때 고려해야할 최소 높이값.")]
        public float aboveCoverHeight = 1.5f;
        [Tooltip("장애물 레이어 마스크 The cover layer mask.")]
        public LayerMask coverMask;
        [Header("Shoot")]
        [Tooltip("사격 레이어마스크 Layer mask to cast shots.")]
        public LayerMask shotMask;
        [Tooltip("타겟 레이어마스크 Layer mask of target(s).")]
        public LayerMask targetMask;
    }

}
