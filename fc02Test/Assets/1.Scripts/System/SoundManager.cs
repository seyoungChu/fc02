﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    public const string MasteGroupName = "Master";
    public const string EffectGroupName = "Effect";
    public const string BGMGroupName = "BGM";
    public const string UIGroupName = "UI";
    public const string MixerName = "AudioMixer";
    public const string ContainerName = "SoundContainer";
    public const string FadeA = "FadeA";
    public const string FadeB = "FadeB";
    public const string UI = "UI";
    public const string EffectVolumeParam = "Volume_Effect";
    public const string BGMVolumeParam = "Volume_BGM";
    public const string UIVolumeParam = "Volume_UI";

    public enum MusicPlayingtype
    {
        None = 0,
        SourceA = 1,
        SourceB = 2,
        AtoB = 3,
        BtoA = 4
    }

    public AudioMixer mixer = null;
    public Transform audioRoot = null;
    public AudioSource fadeA_audio = null;
    public AudioSource fadeB_audio = null;
    public AudioSource[] effect_audios = null;
    public AudioSource UI_audio = null;

    public float[] effect_PlayStartTime = null;
    private int EffectChannelCount = 5;
    private MusicPlayingtype currentPlayingType = MusicPlayingtype.None;
    private bool isTicking = false;
    private SoundClip currentSound = null;
    private SoundClip lastSound = null;
    private float minVolume = -80.0f;
    private float maxVolume = 0.0f;

    private void Start()
    {
        if (this.mixer == null)
        {
            this.mixer = Resources.Load(MixerName) as AudioMixer;
        }

        if (this.audioRoot == null)
        {
            this.audioRoot = new GameObject(ContainerName).transform;
            this.audioRoot.SetParent(transform);
            this.audioRoot.localPosition = Vector3.zero;
        }

        if (this.fadeA_audio == null)
        {
            GameObject fadeA_GO = new GameObject(FadeA, typeof(AudioSource));
            fadeA_GO.transform.SetParent(this.audioRoot);
            this.fadeA_audio = fadeA_GO.GetComponent<AudioSource>();
            this.fadeA_audio.playOnAwake = false;
        }

        if (this.fadeB_audio == null)
        {
            GameObject fadeB_GO = new GameObject(FadeB, typeof(AudioSource));
            fadeB_GO.transform.SetParent(this.audioRoot);
            this.fadeB_audio = fadeB_GO.GetComponent<AudioSource>();
            this.fadeB_audio.playOnAwake = false;
        }

        if (this.UI_audio == null)
        {
            GameObject UI_GO = new GameObject(UI, typeof(AudioSource));
            UI_GO.transform.SetParent(this.audioRoot);
            this.UI_audio = UI_GO.GetComponent<AudioSource>();
            this.UI_audio.playOnAwake = false;
        }

        if (this.effect_audios == null || this.effect_audios.Length == 0)
        {
            this.effect_PlayStartTime = new float[EffectChannelCount];
            this.effect_audios = new AudioSource[EffectChannelCount];
            for (int i = 0; i < EffectChannelCount; i++)
            {
                this.effect_PlayStartTime[i] = 0.0f;
                GameObject EFFECT_GO = new GameObject("Effect_" + i.ToString(), typeof(AudioSource));
                EFFECT_GO.transform.SetParent(this.audioRoot);
                this.effect_audios[i] = EFFECT_GO.GetComponent<AudioSource>();
                this.effect_audios[i].playOnAwake = false;
            }
        }

        if (this.mixer != null)
        {
            this.fadeA_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGroupName)[0];
            this.fadeB_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGroupName)[0];

            this.UI_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(UIGroupName)[0];
            for (int i = 0; i < this.effect_audios.Length; i++)
            {
                this.effect_audios[i].outputAudioMixerGroup = mixer.FindMatchingGroups(EffectGroupName)[0];
            }
        }
        VolumeInit();
    }

    public void SetBGMVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);

        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(BGMVolumeParam, volume);
        PlayerPrefs.SetFloat(BGMVolumeParam, volume);
    }

    public float GetBGMVolume()
    {
        if (PlayerPrefs.HasKey(BGMVolumeParam) == true)
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(BGMVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    public void SetEffectVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(EffectVolumeParam, volume);
        PlayerPrefs.SetFloat(EffectVolumeParam, volume);
    }

    public float GetEffectVolume()
    {
        if (PlayerPrefs.HasKey(EffectVolumeParam) == true)
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(EffectVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    public void SetUIVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(UIVolumeParam, volume);
        PlayerPrefs.SetFloat(UIVolumeParam, volume);
    }

    public float GetUIVolume()
    {
        if (PlayerPrefs.HasKey(UIVolumeParam) == true)
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(UIVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    void VolumeInit()
    {
        if (this.mixer != null)
        {
            this.mixer.SetFloat(BGMVolumeParam, GetBGMVolume());
            this.mixer.SetFloat(EffectVolumeParam, GetEffectVolume());
            this.mixer.SetFloat(UIVolumeParam, GetUIVolume());
        }
    }

    void PlayAudioSource(AudioSource source, SoundClip clip, float volume)
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.Stop();
        source.clip = clip.GetClip();
        source.volume = volume;
        source.loop = clip.isLoop;
        source.pitch = clip.pitch;
        source.dopplerLevel = clip.dopplerLevel;
        source.rolloffMode = clip.rollOffMode;
        source.minDistance = clip.minDistance;
        source.maxDistance = clip.maxDistance;
        source.spatialBlend = clip.spatialBlend;
        source.Play();
    }

    void PlayAudioSourceAtPoint(SoundClip clip, Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(clip.GetClip(), position, volume);
    }

    public bool IsPlaying()
    {
        return (int) this.currentPlayingType > 0;
    }

    public bool IsDifferentSound(SoundClip clip)
    {
        if (clip == null)
        {
            return false;
        }

        if (this.currentSound != null && this.currentSound.realID == clip.realID && IsPlaying() &&
            this.currentSound.isFadeOut == false)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator CheckProcess()
    {
        while (this.isTicking == true && IsPlaying() == true)
        {
            yield return new WaitForSeconds(0.05f);
            if (this.currentSound.HasLoop())
            {
                if (this.currentPlayingType == MusicPlayingtype.SourceA)
                {
                    this.currentSound.CheckLoop(this.fadeA_audio);
                }
                else if (this.currentPlayingType == MusicPlayingtype.SourceB)
                {
                    this.currentSound.CheckLoop(this.fadeB_audio);
                }
                else if (this.currentPlayingType == MusicPlayingtype.AtoB)
                {
                    this.lastSound.CheckLoop(this.fadeA_audio);
                    this.currentSound.CheckLoop(this.fadeB_audio);
                }
                else if (this.currentPlayingType == MusicPlayingtype.BtoA)
                {
                    this.lastSound.CheckLoop(this.fadeB_audio);
                    this.currentSound.CheckLoop(this.fadeA_audio);
                }
            }
        }
    }

    public void DoCheck()
    {
        StartCoroutine(CheckProcess());
    }

    public void FadeIn(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if (this.IsDifferentSound(clip) == true)
        {
            this.fadeA_audio.Stop();
            this.fadeB_audio.Stop();
            this.lastSound = this.currentSound;
            this.currentSound = clip;

            this.PlayAudioSource(this.fadeA_audio, this.currentSound, 0.0f);

            this.currentSound.FadeIn(time, ease);
            this.currentPlayingType = MusicPlayingtype.SourceA;
            if (this.currentSound.HasLoop() == true)
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }

    public void FadeIn(int index, float time, Interpolate.EaseType ease)
    {
        this.FadeIn(DataManager.SoundData().GetCopy(index), time, ease);
    }

    public void FadeOut(float time, Interpolate.EaseType ease)
    {
        if (this.currentSound != null)
        {
            this.currentSound.FadeOut(time, ease);
        }
    }

    private void Update()
    {
        //BGM Control
        if (this.currentSound == null)
        {
            return;
        }

        if (this.currentPlayingType == MusicPlayingtype.SourceA)
        {
            this.currentSound.DoFade(Time.deltaTime, this.fadeA_audio);
        }
        else if (this.currentPlayingType == MusicPlayingtype.SourceB)
        {
            this.currentSound.DoFade(Time.deltaTime, this.fadeB_audio);
        }
        else if (this.currentPlayingType == MusicPlayingtype.AtoB)
        {
            this.lastSound.DoFade(Time.deltaTime, this.fadeA_audio);
            this.currentSound.DoFade(Time.deltaTime, this.fadeB_audio);
        }
        else if (this.currentPlayingType == MusicPlayingtype.BtoA)
        {
            this.lastSound.DoFade(Time.deltaTime, this.fadeB_audio);
            this.currentSound.DoFade(Time.deltaTime, this.fadeA_audio);
        }

        if (this.fadeA_audio.isPlaying == true && this.fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingtype.SourceA;
        }
        else if (this.fadeB_audio.isPlaying == true && this.fadeA_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingtype.SourceB;
        }
        else if (this.fadeA_audio.isPlaying == false && this.fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingtype.None;
        }
    }

    public void FadeTo(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if (this.currentPlayingType == MusicPlayingtype.None)
        {
            this.FadeIn(clip, time, ease);
        }
        else if (this.IsDifferentSound(clip) == true)
        {
            if (this.currentPlayingType == MusicPlayingtype.AtoB)
            {
                this.fadeA_audio.Stop();
                this.currentPlayingType = MusicPlayingtype.SourceB;
            }
            else if (this.currentPlayingType == MusicPlayingtype.BtoA)
            {
                this.fadeB_audio.Stop();
                this.currentPlayingType = MusicPlayingtype.SourceA;
            }

            this.lastSound = this.currentSound;
            this.currentSound = clip;
            this.lastSound.FadeOut(time, ease);
            this.currentSound.FadeIn(time, ease);
            if (this.currentPlayingType == MusicPlayingtype.SourceA)
            {
                PlayAudioSource(fadeB_audio, this.currentSound, 0.0f);
                this.currentPlayingType = MusicPlayingtype.AtoB;
            }
            else if (this.currentPlayingType == MusicPlayingtype.SourceB)
            {
                PlayAudioSource(fadeA_audio, this.currentSound, 0.0f);
                this.currentPlayingType = MusicPlayingtype.BtoA;
            }

            if (this.currentSound.HasLoop())
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }

    public void FadeTo(int index, float time, Interpolate.EaseType ease)
    {
        this.FadeTo(DataManager.SoundData().GetCopy(index), time, ease);
    }

    public void FadeTo(string soundName, float time, Interpolate.EaseType ease)
    {
        try
        {
            SoundList list = (SoundList) Enum.Parse(typeof(SoundList), soundName);
            this.FadeTo(DataManager.SoundData().GetCopy((int) list), time, ease);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Plz Check Sound Name :" + soundName + " / " + ex.Message.ToString());
        }
    }

    public void PlayBGM(SoundClip clip)
    {
        if (this.IsDifferentSound(clip))
        {
            this.fadeB_audio.Stop();
            this.lastSound = this.currentSound;
            this.currentSound = clip;
            PlayAudioSource(this.fadeA_audio, clip, clip.maxVolume);
            if (this.currentSound.HasLoop())
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }

    public void PlayBGM(int index)
    {
        SoundClip tempAudio = DataManager.SoundData().GetCopy(index);
        PlayBGM(tempAudio);
    }

    public void PlayUISound(SoundClip clip)
    {
        PlayAudioSource(this.UI_audio, clip, clip.maxVolume);
    }

    public void PlayEffectSound(SoundClip clip)
    {
        bool isPlaySuccess = false;

        for (int i = 0; i < this.EffectChannelCount; i++)
        {
            if (this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSource(this.effect_audios[i], clip, clip.maxVolume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if (this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSource(this.effect_audios[i], clip, clip.maxVolume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }

        if (isPlaySuccess == false)
        {
            float maxTime = 0.0f;
            int selectIndex = 0;
            for (int i = 0; i < this.EffectChannelCount; i++)
            {
                if (this.effect_PlayStartTime[i] > maxTime)
                {
                    maxTime = this.effect_PlayStartTime[i];
                    selectIndex = i;
                }
            }

            PlayAudioSource(this.effect_audios[selectIndex], clip, clip.maxVolume);
        }
    }

    public void PlayEffectSound(SoundClip clip, Vector3 position, float volume)
    {
        bool isPlaySuccess = false;

        for (int i = 0; i < this.EffectChannelCount; i++)
        {
            if (this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSourceAtPoint(clip, position, volume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if (this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSourceAtPoint(clip, position, volume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }

        if (isPlaySuccess == false)
        {
            float maxTime = 0.0f;
            int selectIndex = 0;
            for (int i = 0; i < this.EffectChannelCount; i++)
            {
                if (this.effect_PlayStartTime[i] > maxTime)
                {
                    maxTime = this.effect_PlayStartTime[i];
                    selectIndex = i;
                }
            }

            PlayAudioSourceAtPoint(clip, position, volume);
        }
    }

    public void PlayOneShotEffect(int index, Vector3 position, float volume)
    {
        if (index == (int) SoundList.None)
        {
            return;
        }

        SoundClip clip = DataManager.SoundData().GetCopy(index);
        if (clip == null)
        {
            Debug.LogWarning("Sound Play Failed");
            return;
        }

        PlayEffectSound(clip, position, volume);
    }

    public void PlayShotSound(string classID, Vector3 position, float volume)
    {
        SoundList sound = (SoundList) Enum.Parse(typeof(SoundList), classID.ToLower());
        PlayOneShotEffect((int) sound, position, volume);
    }

    public void PlayOneShot(SoundClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Sound Play Failed");
            return;
        }

        switch (clip.playType)
        {
            case SoundPlayType.EFFECT:
                PlayEffectSound(clip);
                break;
            case SoundPlayType.BGM:
                PlayBGM(clip);
                break;
            case SoundPlayType.UI:
                PlayUISound(clip);
                break;
        }
    }

    public void PlayOneShot(int index)
    {
        if (index == (int) SoundList.None)
        {
            return;
        }

        this.PlayOneShot(DataManager.SoundData().GetCopy(index));
    }

    public void Stop(bool allStop = false)
    {
        if (allStop == true)
        {
            this.fadeA_audio.Stop();
            this.fadeB_audio.Stop();
        }

        this.FadeOut(0.5f, Interpolate.EaseType.Linear);
        this.currentPlayingType = MusicPlayingtype.None;
        this.isTicking = false;
        StopAllCoroutines();
    }
}