using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.IO;

/// <summary>
/// 
/// </summary>
public class SoundData : BaseData
{
    public SoundClip[] soundClips = new SoundClip[0];

    public string clipPath = "Sound/"; //경로.
    private string xmlFilePath = ""; //데이터 파일 저장 경로.
    private string xmlFileName = "soundData.xml"; //데이터 파일 이름.
    private string dataPath = "Data/soundData";
    private static string SOUND = "sound"; //저장 키.
    private static string CLIP = "clip"; //저장 키.
    /// <summary>
    /// 
    /// </summary>
    public SoundData() { }

    /// <summary>
    /// 데이터 로드함수.
    /// </summary>
    public void LoadData()
    {
        this.xmlFilePath = Application.dataPath + dataDirectory;

        TextAsset asset = (TextAsset)Resources.Load(dataPath, typeof(TextAsset));

        if (asset == null || asset.text == null)
        {
            this.AddSound("NewSound");
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
                        case "length":
                            int length = int.Parse(reader.ReadString());
                            this.names = new string[length];
                            this.soundClips = new SoundClip[length];
                            break;
                        case "clip":
                            break;
                        case "id":
                            currentID = int.Parse(reader.ReadString());
                            this.soundClips[currentID] = new SoundClip();
                            this.soundClips[currentID].realID = currentID;
                            break;
                        case "name":
                            this.names[currentID] = reader.ReadString();
                            break;
                        case "loops":
                            int count = int.Parse(reader.ReadString());
                            this.soundClips[currentID].checkTime = new float[count];
                            this.soundClips[currentID].setTime = new float[count];
                            break;
                        case "maxvol":
                            this.soundClips[currentID].maxVolume = float.Parse(reader.ReadString());
                            break;
                        case "pitch":
                            this.soundClips[currentID].pitch = float.Parse(reader.ReadString());
                            break;
                        case "dopplerlevel":
                            this.soundClips[currentID].dopplerLevel = float.Parse(reader.ReadString());
                            break;
                        case "rolloffmode":
                            this.soundClips[currentID].rollOffMode = (AudioRolloffMode)Enum.Parse(typeof(AudioRolloffMode), reader.ReadString());
                            break;
                        case "mindistance":
                            this.soundClips[currentID].minDistance = float.Parse(reader.ReadString());
                            break;
                        case "maxdistance":
                            this.soundClips[currentID].maxDistance = float.Parse(reader.ReadString());
                            break;
                        case "spatialblend":
                            this.soundClips[currentID].spatialBlend = float.Parse(reader.ReadString());
                            break;
                        case "loop":
                            this.soundClips[currentID].isLoop = true;
                            break;
                        case "clippath":
                            this.soundClips[currentID].clipPath = reader.ReadString();
                            break;
                        case "clipname":
                            this.soundClips[currentID].clipName = reader.ReadString();
                            break;
                        case "checktimecount":
                            break;
                        case "checktime":
                            this.SetLoopTime(true, this.soundClips[currentID], reader.ReadString());
                            break;
                        case "settimecount":
                            break;
                        case "settime":
                            this.SetLoopTime(false, this.soundClips[currentID], reader.ReadString());
                            break;
                        case "type":
                            this.soundClips[currentID].playType = (SoundPlayType)System.Enum.Parse(typeof(SoundPlayType), reader.ReadString());
                            break;


                    }
                }
            }
        }

        //! Preload Test
        foreach (SoundClip clip in soundClips)
        {
            clip.PreLoad();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void SaveData()
    {
		Debug.LogWarning("xmlFilePath :" + xmlFilePath);
		Debug.LogWarning("xmlFileName :" + xmlFileName);
        using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(SOUND);
            xml.WriteElementString("length", this.names.Length.ToString());
            xml.WriteWhitespace("\n");

            for (int i = 0; i < this.names.Length; i++)
            {
                SoundClip clip = this.soundClips[i];
                xml.WriteStartElement(CLIP);
                xml.WriteElementString("id", i.ToString());
                xml.WriteElementString("name", this.names[i]);
                xml.WriteElementString("loops", clip.checkTime.Length.ToString());
                xml.WriteElementString("maxvol", clip.maxVolume.ToString());
                xml.WriteElementString("pitch", clip.pitch.ToString());
                xml.WriteElementString("dopplerlevel", clip.dopplerLevel.ToString());
                xml.WriteElementString("rolloffmode", clip.rollOffMode.ToString());
                xml.WriteElementString("mindistance", clip.minDistance.ToString());
                xml.WriteElementString("maxdistance", clip.maxDistance.ToString());
                xml.WriteElementString("spatialblend", clip.spatialBlend.ToString());
                if (clip.isLoop == true)
                {
                    xml.WriteElementString("loop", "true");
                }
                xml.WriteElementString("clippath", clip.clipPath);
                xml.WriteElementString("clipname", clip.clipName);
                xml.WriteElementString("checktimecount", clip.checkTime.Length.ToString());

                string str = "";
                foreach (float t in clip.checkTime)
                {
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("checktime", str);
                str = "";
                xml.WriteElementString("settimecount", clip.setTime.Length.ToString());
                foreach (float t in clip.setTime)
                {
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("settime", str);
                xml.WriteElementString("type", clip.playType.ToString());

                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddSound(string name)
    {
        if (this.names == null)
        {
            this.names = new string[] { name };
            this.soundClips = new SoundClip[] { new SoundClip() };
        }
        else
        {
            this.names = ArrayHelper.Add(name, this.names);
            this.soundClips = ArrayHelper.Add(new SoundClip(), this.soundClips);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddSound(string name, string clipPath, string clipName)
    {
        if (this.names == null)
        {
            this.names = new string[] { name };
            this.soundClips = new SoundClip[] { new SoundClip(clipPath, clipName) };
        }
        else
        {
            this.names = ArrayHelper.Add(name, this.names);
            this.soundClips = ArrayHelper.Add(new SoundClip(clipPath, clipName), this.soundClips);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public override void RemoveData(int index)
    {
        this.names = ArrayHelper.Remove(index, this.names);
        if (this.names.Length == 0)
        {
            this.names = null;
        }
        this.soundClips = ArrayHelper.Remove(index, this.soundClips);
    }
    /// <summary>
    /// 
    /// </summary>
    public void ClearData()
    {
        foreach (SoundClip clip in this.soundClips)
        {
            if (clip.GetClip() != null)
            {
                clip.ReleaseClip();
            }
        }
        this.soundClips = new SoundClip[0];
        this.names = null;
    }
    /// <summary>
    /// 
    /// </summary>
    public SoundClip GetCopy(int index)
    {
        if (index < 0 || index >= this.soundClips.Length)
        {
            return null;
        }
        SoundClip clip = new SoundClip();
        clip.realID = index;
        clip.clipPath = this.soundClips[index].clipPath;
        clip.clipName = this.soundClips[index].clipName;
        clip.maxVolume = this.soundClips[index].maxVolume;
        clip.pitch = this.soundClips[index].pitch;
        clip.dopplerLevel = this.soundClips[index].dopplerLevel;
        clip.rollOffMode = this.soundClips[index].rollOffMode;
        clip.minDistance = this.soundClips[index].minDistance;
        clip.maxDistance = this.soundClips[index].maxDistance;
        clip.spatialBlend = this.soundClips[index].spatialBlend;
        clip.isLoop = this.soundClips[index].isLoop;
        clip.checkTime = new float[this.soundClips[index].checkTime.Length];
        clip.setTime = new float[this.soundClips[index].setTime.Length];
        clip.playType = this.soundClips[index].playType;
        for (int i = 0; i < clip.checkTime.Length; i++)
        {
            clip.checkTime[i] = this.soundClips[index].checkTime[i];
            clip.setTime[i] = this.soundClips[index].setTime[i];
        }
        clip.PreLoad();
        return clip;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Copy(int index)
    {
        this.names = ArrayHelper.Add(this.names[index], this.names);
        this.soundClips = ArrayHelper.Add(this.GetCopy(index), this.soundClips);
    }

    void SetLoopTime(bool ischeck, SoundClip clip, string timestring)
    {
        string timeString = timestring;
        string[] time = timeString.Split('/');
        for (int i = 0; i < time.Length; i++)
        {
            if (time[i] != string.Empty)
            {
                if (ischeck == true)
                {
                    clip.checkTime[i] = float.Parse(time[i]);
                }
                else
                {
                    clip.setTime[i] = float.Parse(time[i]);
                }
            }
        }
    }

}
