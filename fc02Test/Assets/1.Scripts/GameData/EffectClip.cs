using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이펙트 프리팹과 경로와 타입을 데이터를 가지고 있으며
//프리팹 프리로딩 기능을 갖고 있다. - 사전 로딩 기능은 풀링을 위한 기능이기도하다.
//이펙트 인스턴스 기능도 갖고 있으며 추후 풀링 매니저를 만들 경우 연관해서 사용하면 된다.

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
