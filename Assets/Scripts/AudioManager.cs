using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager m_Instance;
    public static AudioManager GetInstance
    {
        get { return m_Instance; }
    }
    //配置 音频文件
    public enum AudioType
    {
        BGM_StartScene,
        BGM_MainScene,
        /// <summary>
        /// 普通消除 3消
        /// </summary>
        Destroy_Normal,
        /// <summary>
        /// 整行、整列消除
        /// </summary>
        Destroy_Any,
        /// <summary>
        /// 同色消除
        /// </summary>
        Destroy_Same,
        win
    }
    [System.Serializable]
    public struct AudioStruct
    {
        public AudioType audioType;
        public AudioClip audioClip;
    }
    public AudioStruct[] audioManager;
    public Dictionary<AudioType, AudioClip> dicAudio;

    //背景音乐播放器
    private AudioSource audioSource_BGM;
    //音效播放器
    private AudioSource audioSource_Sound;

    public void Awake()
    {
        AudioManager[] audioManagers = GameObject.FindObjectsOfType<AudioManager>();
        if (audioManagers.Length == 1)
            DontDestroyOnLoad(this);


        m_Instance = this;
    }
    public void Start()
    {
        Init();
    }

    private void Init()
    {
        //初始化音频字典
        dicAudio = new Dictionary<AudioType, AudioClip>();
        for (int i = 0; i < audioManager.Length; i++)
        {
            dicAudio.Add(audioManager[i].audioType, audioManager[i].audioClip);
        }

        audioSource_BGM = gameObject.AddComponent<AudioSource>();
        audioSource_BGM.loop = true;
        audioSource_BGM.volume = 0.7f;
        audioSource_Sound = gameObject.AddComponent<AudioSource>();
        audioSource_Sound.loop = false;

    }

    public void PlayAudio(AudioType audioType)
    {
        AudioClip ac;
        dicAudio.TryGetValue(audioType, out ac);
        if (!ac)
        {
            Debug.LogError("音频源缺失！");
            return;
        }
        if (audioType == AudioType.BGM_MainScene || audioType == AudioType.BGM_StartScene)
        {
            audioSource_BGM.clip = ac;
            audioSource_BGM.Play();
        }
        else
        {
            audioSource_Sound.clip = ac;
            audioSource_Sound.Play();
        }
    }

    public void AudioVolume(float value,bool isBGMAudioSourse)
    { 
        if (isBGMAudioSourse)
        {
            audioSource_BGM.volume = value;
        }
        else
        {
            audioSource_Sound.volume = value;
        }
    }

}
