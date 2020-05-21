using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundClip
{
    public SoundPlayType playType = SoundPlayType.None;
    public string clipName = string.Empty;
    public string clipPath = string.Empty;
    public float maxVolume = 1.0f;
    public bool isLoop = false;
    public float[] checkTime = new float[0];
    public float[] setTime = new float[0];
    public int realID = 0;

    private AudioClip clip = null;
    public int currentLoop = 0;
    public float pitch = 1.0f;
    public float dopplerLevel = 1.0f;
    public AudioRolloffMode rollOffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 10000.0f;
    public float maxDistance = 50000.0f;
    public float spatialBlend = 1.0f;

    public float fadeTime1 = 0.0f;
    public float fadeTime2 = 0.0f;
    public Interpolate.Function interpolate_Func;
    public bool isFadeIn = false;
    public bool isFadeOut = false;

    public SoundClip() {}

    public SoundClip(string _clipPath, string _clipName)
    {
        this.clipPath = _clipPath;
        this.clipName = _clipName;
    }

    public void PreLoad()
    {
        if(this.clip == null)
        {
            string fullPath = this.clipPath + this.clipName;
            this.clip = ResourceManager.Load(fullPath) as AudioClip;
        }
    }

    /// <summary>
    /// 반복 추가.
    /// </summary>
    public void AddLoop()
    {
        this.checkTime = ArrayHelper.Add(0.0f, this.checkTime);
        this.setTime = ArrayHelper.Add(0.0f, this.setTime);
    }
    /// <summary>
    /// 반복 제거.
    /// </summary>
    public void RemoveLoop(int index)
    {
        this.checkTime = ArrayHelper.Remove(index, this.checkTime);
        this.setTime = ArrayHelper.Remove(index, this.setTime);
    }
    /// <summary>
    /// 사운드 클립 얻기.
    /// </summary>
    public AudioClip GetClip()
    {
        if (this.clip == null)
        {
            PreLoad();
        }
        if (this.clip == null && this.clipName != string.Empty)
        {
            Debug.LogWarning("Can Not Load Audio Clip Resource : " + this.clipName);
            return null;
        }
        return this.clip;
    }
    /// <summary>
    /// ReleaseClip
    /// </summary>
    public void ReleaseClip()
    {
        if (this.clip != null)
        {
            this.clip = null;
        }
    }
    /// <summary>
    /// 반복재생인가.
    /// </summary>
    public bool HasLoop()
    {
        return this.checkTime.Length > 0;
    }
    /// <summary>
    /// 다음 반복 구간 세팅.
    /// </summary>
    public void NextLoop()
    {
        this.currentLoop++;
        if (this.currentLoop >= this.checkTime.Length)
        {
            this.currentLoop = 0;
        }
    }
    /// <summary>
    /// 반복 체크.
    /// </summary>
    public void CheckLoop(AudioSource source)
    {
        if (this.checkTime.Length > 0 && source.time >= this.checkTime[this.currentLoop])
        {
            source.time = this.setTime[this.currentLoop];
            this.NextLoop();
        }
    }
    /// <summary>
    /// 페이드 인.
    /// </summary>
    public void FadeIn(float time, Interpolate.EaseType easeType)
    {
        this.isFadeOut = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeIn = true;
    }
    /// <summary>
    /// 페이드 아웃.
    /// </summary>
    public void FadeOut(float time, Interpolate.EaseType easeType)
    {
        this.isFadeIn = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeOut = true;
    }
    /// <summary>
    /// 페이드 프로세스.
    /// </summary>
    public void DoFade(float time, AudioSource audio)
    {
        if (this.isFadeIn == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, 0, this.maxVolume, this.fadeTime1, this.fadeTime2);
            if (this.fadeTime1 >= this.fadeTime2)
            {
                this.isFadeIn = false;
            }
        }
        else if (this.isFadeOut == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, this.maxVolume, 0 - this.maxVolume, this.fadeTime1, this.fadeTime2);
            if (this.fadeTime1 >= this.fadeTime2)
            {
                this.isFadeOut = false;
                audio.Stop();
            }
        }
    }

}
