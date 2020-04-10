using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
using UnityEngine.UI;

namespace FC
{
    public class StartCountDown : MonoBehaviour
    {
        public Text countdown;

        public float timeToStart = 3f;

        //public AudioClip countdownTick, startTick;
        public SoundList countdownTickSound, startTickSound;
        private int currentTime;
        private GameObject player;
        private Color fullAlpha, noAlpha;

        void Awake()
        {
            currentTime = (int) timeToStart;
            player = GameObject.FindGameObjectWithTag("Player");
            foreach (GenericBehaviour behaviour in player.GetComponentsInChildren<GenericBehaviour>())
            {
                behaviour.enabled = false;
            }

            if (player.GetComponent<BasicBehaviour>() != null)
            {
                player.GetComponent<BasicBehaviour>().enabled = false;
            }

            fullAlpha = noAlpha = Color.white;
            noAlpha.a = 0f;
        }

        void Update()
        {
            if (currentTime == timeToStart)
            {
                //AudioSource.PlayClipAtPoint(countdownTick, player.transform.position);
                SoundManager.Instance.PlayOneShotEffect((int)countdownTickSound, player.transform.position,1.0f);
            }

            timeToStart -= Time.deltaTime;
            if (timeToStart < (currentTime - 1))
            {
                currentTime--;
                if (timeToStart > 0)
                {
                    //AudioSource.PlayClipAtPoint(countdownTick, player.transform.position);
                    SoundManager.Instance.PlayOneShotEffect((int)countdownTickSound, player.transform.position,1.0f);
                }
                else
                {
                    //AudioSource.PlayClipAtPoint(startTick, player.transform.position);
                    SoundManager.Instance.PlayOneShotEffect((int)startTickSound, player.transform.position,1.0f);
                }
            }

            if (timeToStart <= 0)
            {
                foreach (GenericBehaviour behaviour in player.GetComponentsInChildren<GenericBehaviour>())
                {
                    behaviour.enabled = true;
                }

                if (player.GetComponent<BasicBehaviour>() != null)
                {
                    player.GetComponent<BasicBehaviour>().enabled = true;
                }

                GetComponent<TimeTrial>().enabled = true;
                this.enabled = false;
            }
        }

        private Color UpdateColorAlpha()
        {
            return Color.Lerp(fullAlpha, noAlpha, currentTime - timeToStart);
        }

        public void OnGUI()
        {
            countdown.color = UpdateColorAlpha();
            countdown.text = currentTime.ToString();
        }
    }
}