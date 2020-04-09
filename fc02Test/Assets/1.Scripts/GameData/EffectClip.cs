using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectClip
{
	public int realID = 0;

	public EffectType effectType = EffectType.NORMAL;
	public GameObject effectPrefab = null;
	public string effectName = string.Empty;
	public string effectPath = string.Empty;
	
	public EffectClip() { }

	public string effect_fullPath = string.Empty;

	public void PreLoad()
	{
		this.effect_fullPath = effectPath + effectName;

		if (this.effect_fullPath != string.Empty && this.effectPrefab == null)
		{
			this.effectPrefab = ResourceManager.Load(effect_fullPath) as GameObject;
		}
	}

	public void ReleaseEffect()
	{
		if (this.effectPrefab != null)
		{
			this.effectPrefab = null;
		}
	}

	public GameObject Instantiate(Vector3 pos)
	{
		if (this.effectPrefab == null)
		{
			this.PreLoad();
		}
		if (this.effectPrefab != null)
		{
			GameObject effect = GameObject.Instantiate(this.effectPrefab, pos, Quaternion.identity) as GameObject;

			return effect;
		}

		return null;
	}

}
