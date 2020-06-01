using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FC;

namespace FC
{
    /// <summary>
    /// 콜라이더를 생성해 무기를 줏을 수 있도록 한다.
    /// 루팅했으면 콜라이더는 삭제.
    /// 무기를 다시 버릴수도 있어야 하며, 버릴때는 다시 줏을때를 대비해 콜라이더를 생성해준다.
    /// 관련해서 UI도 같이 컨트롤하는 것이 좋고
    /// ShootBehaviour에 줏은 무기를 넣어준다. (인벤토리 개념)
    /// </summary>
    public class InteractiveWeapon : MonoBehaviour
    {
        public string label; //무기 이름. The weapon name. Same name will treat weapons as same regardless game object's name.

        public SoundList shotSound,
            reloadSound, // Audio clips for shoot and reload.
            pickSound,
            dropSound,
            noBulletSound; // Audio clips for pickweapon , drop weapon, and no bullet shot try.

        public Sprite weaponSprite; //무기 스프라이트. Weapon srpite to show on screen HUD.
        public Vector3 rightHandPosition; //플레이어 오른손에 따른 포지션. Position offsets relative to the player's right hand.
        public Vector3 relativeRotation; //플레이어 오른손에 따른 회전. Rotation Offsets relative to the player's right hand.
        public float bulletDamage = 10f; //총알 데미지. Damage of one shot.
        public float recoilAngle; //사격반동 각도. Angle of weapon recoil.
        
        public enum WeaponType // Weapon types, related to player's shooting animations.
        {
            NONE,
            SHORT,
            LONG
        }

        public enum WeaponMode // Weapon shooting modes.
        {
            SEMI,
            BURST,
            AUTO
        }

        public WeaponType type = WeaponType.NONE; // Default weapon type, change in Inspector.
        public WeaponMode mode = WeaponMode.SEMI; // Default weapon mode, change in Inspector.
        public int burstSize = 3; //버스트샷 숫자? How many shot are fired on burst mode.

        //현재 탄창 양과 소지하고 있는 전체 총알 량.
        [SerializeField]
        private int currentMagCapacity, totalBullets; // Current mag capacity and total amount of bullets being carried.
        //재장전시의 꽉찬 탄창양과 한번에 채울수 있는 최대 총알 량
        private int fullMag, maxBullets; // Default mag capacity and total bullets for reset purposes.
        private GameObject player, gameController; // References to the player and the game controller.
        private ShootBehaviour playerInventory; // 플레이어 인벤토리 역할. Player's inventory to store weapons.
        private SphereCollider interactiveRadius; // 플레이어와 인터랙션 가능한 구체충돌체In-game radius of interaction with player.
        private BoxCollider weaponCollider; // 무기 충돌체 Weapon collider.
        private Rigidbody weaponRigidbody; // 무기 리지드바디.Weapon rigidbody.
        private bool pickable; //줏어서 소지가 가능한지의 여부. Boolean to store whether or not the weapon is pickable (player within radius).

        public GameObject screenHUD;
        public WeaponUIManager weaponHUD; // 무기 UI.Reference to on-screen weapon HUD.
        private Transform pickupHUD; // 줏은 무기 UI.Reference to the weapon pickup in-game label.
        public Text pickupHUDLabel;
        
        [Tooltip("muzzle Transform")] [SerializeField]
        private Transform muzzleTransform;
        
        void Awake()
        {
            // Set up the references.
            this.gameObject.name = this.label;
            this.gameObject.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
            foreach (Transform t in this.transform)
            {
                t.gameObject.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
            }

            player = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.Player);
            playerInventory = player.GetComponent<ShootBehaviour>();
            gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);

            if (weaponHUD == null)
            {
                if (screenHUD == null)
                {
                    screenHUD = GameObject.Find("ScreenHUD");
                }
                weaponHUD = screenHUD.GetComponent<WeaponUIManager>();
            }

            if (pickupHUD == null)
            {
                pickupHUD = gameController.transform.Find("PickupHUD");    
            }
            // Create physics components and radius of interaction.
            weaponCollider = this.transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
            CreateInteractiveRadius(weaponCollider.center);
            this.weaponRigidbody = this.gameObject.AddComponent<Rigidbody>();

            // Assert that an weapon slot is set up.
            if (this.type == WeaponType.NONE)
            {
                Debug.LogWarning("Set correct weapon slot ( 1 - small/ 2- big)");
                type = WeaponType.SHORT;
            }

            // Set default values.
            fullMag = currentMagCapacity;
            maxBullets = totalBullets;
            pickupHUD.gameObject.SetActive(false);
            if (muzzleTransform == null)
            {
                muzzleTransform = transform.Find("muzzle");
            }
        }

        // Create the sphere of interaction with player.
        private void CreateInteractiveRadius(Vector3 center)
        {
            interactiveRadius = this.gameObject.AddComponent<SphereCollider>();
            interactiveRadius.center = center;
            interactiveRadius.radius = 1f;
            interactiveRadius.isTrigger = true;
        }
        
        // Draw in-game weapon pickup label.
        private void TooglePickupHUD(bool toogle)
        {
            pickupHUD.gameObject.SetActive(toogle);
            if (toogle)
            {
                pickupHUD.position = this.transform.position + Vector3.up * 0.5f;
                Vector3 direction = player.GetComponent<BehaviourController>().playerCamera.forward;
                direction.y = 0f;
                pickupHUD.rotation = Quaternion.LookRotation(direction);
                pickupHUDLabel.text = "Pick " + this.gameObject.name;
            }
        }
        // Update weapon screen HUD.
        private void UpdateHUD()
        {
            weaponHUD.UpdateWeaponHUD(weaponSprite, currentMagCapacity, fullMag, totalBullets);
        }

        // Manage weapon active status.
        public void Toggle(bool active)
        {
            if (active)
            {
                //AudioSource.PlayClipAtPoint(pickSound, transform.position, 0.5f);
                SoundManager.Instance.PlayOneShotEffect((int) pickSound, transform.position, 0.5f);
            }

            weaponHUD.Toggle(active);
            UpdateHUD();
        }
        

        void Update()
        {
            // Handle player pick weapon action.
            if (this.pickable && Input.GetButtonDown(ButtonName.Pick))
            {
                // Disable weapon physics.
                weaponRigidbody.isKinematic = true;
                this.weaponCollider.enabled = false;

                // Setup weapon and add in player inventory.
                playerInventory.AddWeapon(this);
                Destroy(interactiveRadius);
                this.Toggle(true);
                this.pickable = false;

                // Change active weapon HUD.
                TooglePickupHUD(false);
            }
        }

        // Handle weapon collision with environment.
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject != player &&
                Vector3.Distance(transform.position, player.transform.position) <= 5f)
            {
                //AudioSource.PlayClipAtPoint(dropSound, transform.position, 0.5f);
                SoundManager.Instance.PlayOneShotEffect((int) dropSound, transform.position, 0.5f);
            }
        }

        // Handle player exiting radius of interaction.
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == player)
            {
                pickable = false;
                TooglePickupHUD(false);
            }
        }

        // Handle player within radius of interaction.
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject == player && playerInventory && playerInventory.isActiveAndEnabled)
            {
                pickable = true;
                TooglePickupHUD(true);
            }
        }

        // Manage the drop action.
        public void Drop()
        {
            this.gameObject.SetActive(true);
            this.transform.position += Vector3.up;
            weaponRigidbody.isKinematic = false;
            this.transform.parent = null;
            CreateInteractiveRadius(weaponCollider.center);
            this.weaponCollider.enabled = true;
            weaponHUD.Toggle(false);
        }

        // Start the reload action (called by shoot behaviour).
        public bool StartReload()
        {
            if (currentMagCapacity == fullMag || totalBullets == 0)
            {
                return false;
            }
            else if (totalBullets < fullMag - currentMagCapacity)
            {
                currentMagCapacity += totalBullets;
                totalBullets = 0;
            }
            else
            {
                totalBullets -= fullMag - currentMagCapacity;
                currentMagCapacity = fullMag;
            }

            return true;
        }

        // End the reload action (called by shoot behaviour).
        public void EndReload()
        {
            UpdateHUD();
        }

        // Manage shoot action.
        public bool Shoot(bool firstShot = true)
        {
            if (currentMagCapacity > 0)
            {
                currentMagCapacity--;
                UpdateHUD();
                return true;
            }

            if (firstShot && noBulletSound != SoundList.None)
            {
                //AudioSource.PlayClipAtPoint(noBulletSound, this.transform.Find("muzzle").position, 5f);
                SoundManager.Instance.PlayOneShotEffect((int) noBulletSound, muzzleTransform.position, 5f);
            }

            return false;
        }

        // Reset the bullet parameters.
        public void ResetBullets()
        {
            currentMagCapacity = fullMag;
            totalBullets = maxBullets;
        }

    }
}