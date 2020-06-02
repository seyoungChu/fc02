using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class HealthBase : MonoBehaviour
    {
        // Class to encapsulate damage parameters for the callback function.
        public class DamageInfo
        {
            public Vector3 location, direction;      // Hit location and direction.
            public float damage;                     // Damage ammount.
            public Collider bodyPart;               // The body part (Collider) that was hit (optional).
            public GameObject origin;                // The game object that generated the hit (optional).

            public DamageInfo(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
            {
                this.location = location;
                this.direction = direction;
                this.damage = damage;
                this.bodyPart = bodyPart;
                this.origin = origin;
            }
        }

        [HideInInspector] public bool isDead;          // Is this entity dead?
        protected Animator myAnimator;
        // This is the mandatory function that receives damage from shots.
        // You may remove the 'virtual' keyword before coding the content.
        public virtual void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
        {
        }

        // This is the message receiver for damage taken by a child gameObject rigidbody (ex.: ragdoll)
        public void HitCallback(DamageInfo damageInfo)
        {
            this.TakeDamage(damageInfo.location, damageInfo.direction, damageInfo.damage, damageInfo.bodyPart, damageInfo.origin);
        }
    }
}

