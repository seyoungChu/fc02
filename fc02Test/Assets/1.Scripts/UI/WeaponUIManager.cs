using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
using UnityEngine.UI;
namespace FC
{
    public class WeaponUIManager : MonoBehaviour
    {
        public Color bulletColor = Color.white;             // Color of the available bullets inside weapon HUD.
        public Color emptyBulletColor = Color.black;        // Color of the empty  bullets inside weapon HUD.
    
        private Color nobulletColor;                        // Transparent color to hide extra capacity bullet slots.
        [SerializeField] private Image weaponHud;                            // The weapon draw inside HUD.
        [SerializeField] private GameObject bulletMag;                       // The bullets draw inside HUD.
        [SerializeField] private Text totalBulletsHud;                       // The bullets amount label inside HUD.
    
        void Start ()
        {
            // Set up references and default values.
            nobulletColor = new Color(0, 0, 0, 0);
            if (weaponHud == null)
            {
                weaponHud = this.transform.Find("WeaponHUD/Weapon").GetComponent<Image>();    
            }
            if (bulletMag == null)
            {
                bulletMag = this.transform.Find("WeaponHUD/Data/Mag").gameObject;    
            }
            if (totalBulletsHud == null)
            {
                totalBulletsHud = this.transform.Find("WeaponHUD/Data/Label").GetComponent<Text>();    
            }

            // Player begins unarmed, hide weapon HUD.
            Toggle(false);
        }
    
        // Manage on-screen HUD visibility.
        public void Toggle(bool active)
        {
            weaponHud.transform.parent.gameObject.SetActive(active);
        }
    
        // Update the weapon HUD features.
        public void UpdateWeaponHUD(Sprite weaponSprite, int bulletsLeft, int fullMag, int extraBullets)
        {
            // Update the weapon draw.
            if(weaponSprite != null && weaponHud.sprite != weaponSprite)
            {
                weaponHud.sprite = weaponSprite;
                weaponHud.type = Image.Type.Filled;
                weaponHud.fillMethod = Image.FillMethod.Horizontal;
            }
            // Update bullet draws.
            int b = 0;
            foreach(Transform bullet in bulletMag.transform)
            {
                if(b < bulletsLeft)
                {    //남은거
                    bullet.GetComponent<Image>().color = bulletColor;
                }
                else if(b >= fullMag)
                {    //넘치는 건 일단 그리지 않습니다.
                    bullet.GetComponent<Image>().color = nobulletColor;
                }
                else
                {    //사용한 탄.
                    bullet.GetComponent<Image>().color = emptyBulletColor;
                }
                b++;
            }
    
            // Update bullet count label.
            totalBulletsHud.text = bulletsLeft + "/" + extraBullets;
        }
    }

}
