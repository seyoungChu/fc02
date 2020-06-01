using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FC;
namespace FC
{
    public class PlayerHealth : HealthBase
    {
        public float health = 100f;
        public float criticalHealth = 30f;
        public Transform healthHUD;
        public SoundList deathSound;
        public SoundList hitSound;
        public GameObject hurtPrefab;
        public float decayFactor = 0.8f;

        private float totalHealth;
        private RectTransform healthBar, placeHolderBar;
        private Text healthLabel;
        private float originalBarScale;
        private bool critical;
        
        //private BlinkHUD criticalHud; -> 2부에서.
        //private HurtHUD hurtHUD; -> 2부에서.

        void Awake()
        {
            myAnimator = GetComponent<Animator>();
            totalHealth = health;
            
            healthBar = healthHUD.Find("HealthBar/Bar").GetComponent<RectTransform>();
            placeHolderBar = healthHUD.Find("HealthBar/Placeholder").GetComponent<RectTransform>();
            healthLabel = healthHUD.Find("HealthBar/Label").GetComponent<Text>();
            originalBarScale = healthBar.sizeDelta.x;
            healthLabel.text = "" + (int)health;
            
            //criticalHud = healthHUD.Find("Bloodframe").GetComponent<BlinkHUD>(); -> 2부에서.
            //hurtHUD = this.gameObject.AddComponent<HurtHUD>(); -> 2부에서.
            //hurtHUD.Setup(healthHUD, hurtPrefab, decayFactor, this.transform); -> 2부에서.
        }

        void Update()
        {
            if (placeHolderBar.sizeDelta.x > healthBar.sizeDelta.x)
            {
                placeHolderBar.sizeDelta = Vector2.Lerp(placeHolderBar.sizeDelta, healthBar.sizeDelta, 2f * Time.deltaTime);
            }
        }

        public bool IsFullLife()
        {
            return Math.Abs(health - totalHealth) < float.Epsilon;
        }

        public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null,
            GameObject origin = null)
        {
            health -= damage;

            UpdateHealthBar();

            // if (hurtPrefab && healthHUD) -> 2부에서.
            // {
            //     hurtHUD.DrawHurtUI(origin.transform, origin.GetHashCode()); -> 2부에서.
            // }
                

            if (health <= 0)
            {
                Kill();
            }
            else if (health <= criticalHealth && !critical)
            {
                critical = true;
                //criticalHud.StartBlink(); -> 2부에서.
            }

            SoundManager.Instance.PlayOneShotEffect((int)hitSound, location, 0.1f);
            
        }

        private void UpdateHealthBar()
        {
            healthLabel.text = "" + (int)health;

            float scaleFactor = health / totalHealth;
            healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y);
        }

        private void Kill()
        {
            dead = true;
            gameObject.layer = TagAndLayer.GetLayerByName(TagAndLayer.LayerName.Default);
            gameObject.tag = TagAndLayer.TagName.Untagged;
            healthHUD.gameObject.SetActive(false);
            healthHUD.parent.Find("WeaponHUD").gameObject.SetActive(false);
            myAnimator.SetBool(AnimatorKey.Aim, false);
            myAnimator.SetBool(AnimatorKey.Cover, false);
            myAnimator.SetFloat(AnimatorKey.Speed, 0);
            foreach (GenericBehaviour behaviour in GetComponentsInChildren<GenericBehaviour>())
            {
                behaviour.enabled = false;
            }
            SpawnEffect spawnEffect = this.GetComponentInChildren<SpawnEffect>();
            if (spawnEffect != null)
            {
                spawnEffect.enabled = true;
            }

            SoundManager.Instance.PlayOneShotEffect((int)deathSound, transform.position, 5f);
        }
    }
}

