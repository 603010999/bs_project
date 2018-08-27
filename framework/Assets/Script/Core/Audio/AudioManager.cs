using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : BehaviourSingleton<AudioManager>
{
    private static float _mGlobalVolume = 1;
    /// <summary>
    /// 全局音量，范围从0到1，与音效音量和音乐音量是相乘关系
    /// </summary>
    public static float m_globalVolume
    {
        get { return _mGlobalVolume; }
        set
        {
            _mGlobalVolume = Mathf.Clamp01(value);
            OnMusicVolumeChange();
            OnSoundVolumeChange();
        }
    }

    private static float _musicVolume = 1;
    /// <summary>
    /// 音乐音量，范围从0到1
    /// </summary>
    public static float m_musicVolume
    {
        get { return _musicVolume * m_globalVolume; }
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            OnMusicVolumeChange();
        }
    }
   
    private static float _soundVolume = 1;
    /// <summary>
    /// 音效音量，范围从0到1
    /// </summary>
    public static float m_soundVolume
    {
        get { return _soundVolume * m_globalVolume; }
        set
        {
            _soundVolume = Mathf.Clamp01(value);
            OnSoundVolumeChange();
        }
    }

    //2D 音乐  背景音等
    public static AudioSource m_2Dmusic;
    
    //是否在播放
    public static bool m_musicIsPlaying = false;
    
    //2D音效列表
    private static List<AudioSource> m_2Dplayers = new List<AudioSource>();
    
    //3D音效列表
    private static List<AudioSource> m_3Dplayers = new List<AudioSource>();

    //音效播放事件回调
    //音乐播放结束
    public static AudioCallBack m_onMusicComplete;
    
    //音乐音量变化
    public static AudioCallBack m_onMusicVolumeChange;
    
    //音效音量变化
    public static AudioCallBack m_onSoundVolumeChange;

    public AudioManager()
    {
        Init2DPlayer(5);
    }
    

    //初始化2D音效播放    
    private static void Init2DPlayer(int count)
    {
        AudioSource AudioSourceTmp = null;
        for (int i = 0; i < count; i++)
        {
            AudioSourceTmp = Instance.gameObject.AddComponent<AudioSource>();
            m_2Dplayers.Add(AudioSourceTmp);
        }
    }

    public void Update()
    {
        if (m_2Dmusic == null || !m_musicIsPlaying)
        {
            return;
        }
       
        if (!m_2Dmusic.isPlaying)
        {
            m_musicIsPlaying = false;
            
            try
            {
                if (m_onMusicComplete != null)
                {
                    m_onMusicComplete(SoundType.Music);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    /// <summary>
    /// 播放一个2D音乐
    /// </summary>
    /// <param name="l_musicName">音乐名</param>
    /// <param name="l_isLoop">是否循环</param>
    public static AudioSource PlayMusic2D(string l_musicName, bool l_isLoop)
    {
        m_musicIsPlaying = true;

        AudioSource audioTmp = GetAudioSource2D(SoundType.Music);
        audioTmp.clip = GetAudioClip(l_musicName);
        audioTmp.loop = l_isLoop;
        audioTmp.volume = m_musicVolume;
        if (l_isLoop)
        {
            audioTmp.Play();
        }
        else
        {
            audioTmp.PlayOneShot(audioTmp.clip);
        }
            
        
        return audioTmp;
    }

    /// <summary>
    /// 播放一个2D音效
    /// </summary>
    /// <param name="l_soundName">音效名</param>
    public static AudioSource PlaySound2D(string l_soundName)
    {
        AudioSource audioTmp = PlaySound2D(l_soundName,1,false);

        return audioTmp;
    }

    /// <summary>
    /// 播放一个2D音效, 可变音调
    /// </summary>
    /// <param name="l_soundName">音效名</param>
    public static AudioSource PlaySound2D(string l_soundName, float l_pitch, bool l_isLoop = false )
    {
        AudioSource audioTmp = GetAudioSource2D(SoundType.Sound);
        audioTmp.clip = GetAudioClip(l_soundName);
        audioTmp.loop = l_isLoop;
        audioTmp.volume = m_soundVolume;
        audioTmp.PlayOneShot(audioTmp.clip);
        audioTmp.pitch = l_pitch;
        return audioTmp;
    }


    /// <summary>
    /// 延时播放一个2D音效
    /// </summary>
    /// <param name="l_soundName">音效名</param>
    public static void PlaySound2D(string l_soundName,float l_delay )
    {
        if (l_delay == 0)
        {
            PlaySound2D(l_soundName);
        }
        else
        {
            ApplicationManager.Instance.StartCoroutine(DelayPlay(l_soundName, l_delay));
        }
    }

    static IEnumerator DelayPlay(string l_soundName, float l_delay)
    {
        yield return new WaitForSeconds(l_delay);
        PlaySound2D(l_soundName);
    }

    /// <summary>
    /// 播放一个3D音效
    /// </summary>
    /// <param name="l_soundName">音效名</param>
    /// <param name="l_gameObject">音效绑在哪个对象上</param>
    public static AudioSource PlaySound3D(string l_soundName, GameObject l_gameObject)
    {
        AudioSource audioTmp = GetAudioSource3D(l_gameObject);
        audioTmp.clip = GetAudioClip(l_soundName);
        audioTmp.loop = false;
        audioTmp.volume = m_soundVolume;
        return audioTmp;
    }

    public static AudioSource GetAudioSource2D(SoundType l_SoundType)
    {
        if (l_SoundType == SoundType.Music)
        {
            if(m_2Dmusic == null)
            {
                m_2Dmusic = Instance.gameObject.AddComponent<AudioSource>();
            }

            return m_2Dmusic;
        }
        else
        {
            AudioSource AudioSourceTmp = null;
            for (int i = 0; i < m_2Dplayers.Count;i++ )
            {
                AudioSourceTmp = m_2Dplayers[i];
                if(AudioSourceTmp.isPlaying == false)
                {
                    return AudioSourceTmp;
                }
            }

            AudioSourceTmp = Instance.gameObject.AddComponent<AudioSource>();

            m_2Dplayers.Add(AudioSourceTmp);

            return AudioSourceTmp;
        }
    }

    public static AudioSource GetAudioSource3D(GameObject l_obj)
    {
        AudioSource[] l_players = l_obj.GetComponents<AudioSource>();

        for (int i = 0; i < l_players.Length; i++)
        {
            if (!l_players[i].isPlaying)
            {
                return l_players[i];
            }
        }

        AudioSource l_newAudioPlayer = l_obj.AddComponent<AudioSource>();

        return l_newAudioPlayer;
    }

    public static AudioClip GetAudioClip(string l_soundName)
    {
        AudioClip clipTmp = null;

        clipTmp = ResourceManager.LoadAudioPrefab<AudioClip>(l_soundName);

        if (clipTmp == null)
        {
            Debug.LogError("AudioManager GetAudioClip error: " + l_soundName + "is not AudioClip ! ");
        }

        return clipTmp;
    }

    static void OnMusicVolumeChange()
    {
        if(m_2Dmusic != null)
        {
            m_2Dmusic.volume = m_musicVolume;
        }

        try
        {
            if (m_onMusicVolumeChange != null)
            {
                m_onMusicVolumeChange(SoundType.Music);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    static void OnSoundVolumeChange()
    {
        for (int i = 0; i < m_2Dplayers.Count; i++)
        {
            if (m_2Dplayers[i].isPlaying)
            {
                m_2Dplayers[i].volume = m_soundVolume;
            }
            else
            {
                m_2Dplayers.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < m_3Dplayers.Count; i++)
        {
            if (m_3Dplayers[i] != null && m_3Dplayers[i].isPlaying)
            {
                m_3Dplayers[i].volume = m_soundVolume;
            }
            else
            {
                m_3Dplayers.RemoveAt(i);
                i--;
            }
        }

        try
        {
            if (m_onSoundVolumeChange != null)
            {
                m_onSoundVolumeChange(SoundType.Sound);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
}

public enum SoundType { Sound, Music, };
public delegate void AudioCallBack(SoundType l_soundType);