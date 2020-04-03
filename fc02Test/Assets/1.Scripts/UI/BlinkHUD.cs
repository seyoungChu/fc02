using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using FC;

namespace FC
{
    public class BlinkHUD : MonoBehaviour
    {
        public float blinkFadeStep = 1f;

        private Image hud;
        private Color originalColor, noAplhacolor;
        private float fadeTimer;
        private float colorGradeTimer;
        private int fadeFactor = 1;
        private int colorGradeFactor = 1;
        private bool blink, lastTime;

        private ColorGrading colorGradingLayer;
        private Bloom bloomLayer;
        private float dyingSaturation;
        private float dyingBrightness;

        void Start()
        {
            hud = GetComponent<Image>();
            hud.enabled = true;
            originalColor = noAplhacolor = hud.color;
            noAplhacolor.a = 0f;
            gameObject.SetActive(false);
            dyingSaturation = -30f;
            dyingBrightness = -10f;
            GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
            PostProcessVolume postProcessVolume = gameController.GetComponent<PostProcessVolume>();
            postProcessVolume.profile.TryGetSettings(out colorGradingLayer);
            postProcessVolume.profile.TryGetSettings(out bloomLayer);
        }

        void Update()
        {
            if (blink)
            {
                colorGradingLayer.saturation.value = Mathf.Lerp(0, dyingSaturation, colorGradeTimer / blinkFadeStep);
                colorGradingLayer.brightness.value = Mathf.Lerp(0, dyingBrightness, colorGradeTimer / blinkFadeStep);
                bloomLayer.intensity.value = Mathf.Lerp(0, 7f, colorGradeTimer / blinkFadeStep);

                colorGradeTimer += colorGradeFactor * Time.deltaTime;

                hud.color = Color.Lerp(noAplhacolor, originalColor, fadeTimer / blinkFadeStep);
                fadeTimer += fadeFactor * Time.deltaTime;
                if (fadeTimer >= blinkFadeStep || fadeTimer <= 0f)
                {
                    fadeFactor = -fadeFactor;
                    colorGradeFactor = 0;
                    if (fadeTimer < 0f)
                    {
                        fadeTimer = 0f;
                    }
                    else if (fadeTimer >= blinkFadeStep && lastTime)
                    {
                        blink = lastTime = false;
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        public void StartBlink()
        {
            gameObject.SetActive(true);
            blink = true;
            lastTime = false;
            colorGradeFactor = 1;
        }

        public void ApplyRespawnFilter()
        {
            colorGradingLayer.saturation.value = -40;
            colorGradingLayer.brightness.value = 0;
            bloomLayer.intensity.value = 0;
        }
    }
}


