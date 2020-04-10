using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FC;

namespace FC
{
    public class SceneFader : MonoBehaviour
    {
        public float fadeSpeed = 1.5f;
        public int nextLevel = 1;

        private bool sceneStarting = true;
        private Image fadeImg;

        void Awake()
        {
            fadeImg = this.GetComponent<Image>();
        }

        private void Update()
        {
            if (sceneStarting)
            {
                StartScene();
            }
        }

        void FadeToClear()
        {
            fadeImg.color = Color.Lerp(fadeImg.color, Color.clear, fadeSpeed * Time.deltaTime);
        }
        void FadeToBlack()
        {
            fadeImg.color = Color.Lerp(fadeImg.color, Color.black, fadeSpeed * Time.deltaTime);
        }

        void StartScene()
        {
            FadeToClear();

            if(fadeImg.color.a <= 0.05F)
            {
                fadeImg.color = Color.clear;
                fadeImg.enabled = false;
                sceneStarting = false;
                transform.root.GetComponent<StartCountDown>().enabled = true;
            }
        }

        public void EndScene(bool next=true)
        {
            fadeImg.enabled = true;
            FadeToBlack();

            if(fadeImg.color.a >= 0.95f)
            {
                if (next)
                    SceneManager.LoadScene(nextLevel);
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}


