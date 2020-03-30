using UnityEngine;

// NPC class specific stats.
[CreateAssetMenu(menuName = "PluggableAI/Class Stats")]
public class ClassStats : ScriptableObject
{
	[Header("Animation")]
	[Tooltip("Post animation aim rotation offset for custom NPC avatar.")]
	public Vector3 aimOffset;
	[Header("Cover")]
	[Tooltip("Chance to change current cover to some spot that is near the target.")]
	[Range(0, 100)] public int changeCoverChance;
	[Header("Shoot")]
	[Tooltip("NPC weapon type (1: short, 2: long)")]
	public WeaponType weaponType = WeaponType.NONE;
	[Tooltip("Weapon bullet damage.")]
	public float bulletDamage;
	[Tooltip("Weapon shot and reload sounds.")]
	public AudioClip shotSound, reloadSound;
	[Tooltip("Weapon shot rate factor (higher is faster rate).")]
	public float shotRateFactor;
	[Tooltip("NPC accuracy (0 is perfect, higher is worse).")]
	public float shotErrorRate;
	[Tooltip("Game objects for shot effects.")]
	public GameObject muzzleFlash, shot, sparks, bulletHole;
	// Weapon types, related to NPC's shooting animations.
	public enum WeaponType
	{
		NONE,
		SHORT,
		LONG
	}
}
