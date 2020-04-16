using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public class ParticleSystemAutoDestory : MonoBehaviour
    {
        private ParticleSystem ps;

        public void Start()
        {
            // Set up the references.
            ps = GetComponent<ParticleSystem>();
        }

        public void Update()
        {
            // Check if lifetime has ended to destroy it.
            if (ps)
            {
                if (!ps.IsAlive())
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}