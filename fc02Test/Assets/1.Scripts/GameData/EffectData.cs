using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectData : BaseData
{
	public EffectClip[] effectClips = new EffectClip[0];

	public string clipPath = "Effects/"; //경로.
	private string xmlFilePath = ""; //데이터 파일 저장 경로.
	private string xmlFileName = "effectData.xml"; //데이터 파일 이름.
	private string dataPath = "Data/effectData";
	private static string EFFECT = "effect"; //저장 키.
	private static string CLIP = "clip"; //저장 키.

	public EffectData() { }
	/// <summary>
	/// 
	/// </summary>
	public void LoadData()
	{
		this.xmlFilePath = Application.dataPath + dataDirectory;

		TextAsset asset = (TextAsset)Resources.Load(dataPath, typeof(TextAsset));

		if (asset == null || asset.text == null)
		{
			this.AddEffect("NewEffect");
			return;
		}

		using (XmlTextReader reader = new XmlTextReader(new StringReader(asset.text)))
		{
			int currentID = 0;
			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "length": int length = int.Parse(reader.ReadString()); this.names = new string[length]; this.effectClips = new EffectClip[length]; break;
						case "id": currentID = int.Parse(reader.ReadString()); this.effectClips[currentID] = new EffectClip(); this.effectClips[currentID].realID = currentID; break;
						case "name": this.names[currentID] = reader.ReadString(); break;
						case "effectType": this.effectClips[currentID].effectType = (EffectType)Enum.Parse(typeof(EffectType), reader.ReadString()); break;
						case "effectName": this.effectClips[currentID].effectName = reader.ReadString(); break;
						case "effectPath": this.effectClips[currentID].effectPath = reader.ReadString(); break;
						case "beHitEffectPath": this.effectClips[currentID].beHitEffect_Path = reader.ReadString(); break;

					}
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void SaveData()
	{
		using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
		{
			xml.WriteStartDocument();
			xml.WriteStartElement(EFFECT);
			xml.WriteElementString("length", this.names.Length.ToString());

			for (int i = 0; i < this.names.Length; i++)
			{
				EffectClip clip = this.effectClips[i];
				xml.WriteStartElement(CLIP);
				xml.WriteElementString("id", i.ToString());
				xml.WriteElementString("name", this.names[i]);
				xml.WriteElementString("effectType", clip.effectType.ToString());
				xml.WriteElementString("effectName", clip.effectName);
				xml.WriteElementString("effectPath", clip.effectPath);
				xml.WriteElementString("beHitEffectPath", clip.beHitEffect_Path);

				xml.WriteEndElement();
			}
			xml.WriteEndElement();
			xml.WriteEndDocument();
		}
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="name"></param>
	public void AddEffect(string name)
	{
		if (this.names == null)
		{
			this.names = new string[] { name };
			this.effectClips = new EffectClip[] { new EffectClip() };
		}
		else
		{
			this.names = ArrayHelper.Add(name, this.names);
			this.effectClips = ArrayHelper.Add(new EffectClip(), this.effectClips);
		}
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="index"></param>
	public override void RemoveData(int index)
	{
		this.names = ArrayHelper.Remove(index, this.names);
		if (this.names.Length == 0)
		{
			this.names = null;
		}
		this.effectClips = ArrayHelper.Remove(index, this.effectClips);
	}

	public void ClearData()
	{
		foreach (EffectClip clip in this.effectClips)
		{
			clip.ReleaseEffect();
		}
		this.effectClips = new EffectClip[0];
		this.names = null;
	}

	public EffectClip GetCopy(int index)
	{
		if (index < 0 || index >= this.effectClips.Length)
		{
			return null;
		}

		EffectClip clip = new EffectClip();
		clip.realID = index;


		clip.PreLoad();
		return clip;
	}

	public override void Copy(int index)
	{
		this.names = ArrayHelper.Add(this.names[index], this.names);
		this.effectClips = ArrayHelper.Add(this.GetCopy(index), this.effectClips);
	}

}
