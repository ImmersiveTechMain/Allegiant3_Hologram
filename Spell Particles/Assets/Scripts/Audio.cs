using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;



public class Audio : MonoBehaviour
{
    public class AudioChannel
    {
        public delegate void CALLBACK();
        public CALLBACK OnAudioEnds = delegate () { };
        public CALLBACK OnAudioUpdate = delegate () { };

        public enum Type
        {
            Music,
            SFX,
            Special
        }

        public AudioSource source { private set; get; }
        public Type type { private set; get; }
        public float volume { private set; get; }
        float _maxVolume = 1;
        public float maxVolume { set { _maxVolume = value; RecalculateVolume(); } get { return _maxVolume; } }
        bool _mute = false;
        public bool mute { private set { _mute = value; RecalculateVolume(); } get { return _mute; } }
        public bool isPlaying { get { return source == null ? false : source.isPlaying; } }
        public AudioClip clip { get { return source == null ? null : source.clip; } set { SetClip(value); } }
        Coroutine stopCoroutine;
        Coroutine playCoroutine;
        Coroutine loopCoroutine;

        public AudioChannel(Type type, string name = null)
        {
            this.type = type;
            Transform folder = null;
            switch (type)
            {
                case Type.Music: OnMusicMasterVolumeModified += (vol) => { RecalculateVolume(); }; folder = MusicChannelsFolder; break;
                case Type.SFX: OnSFXMasterVolumeModified += (vol) => { RecalculateVolume(); }; folder = SFXChannelsFolder; break;
                case Type.Special: folder = SpecialChannelsFolder; break;
            }
            OnMasterVolumeModified += (vol) => { RecalculateVolume(); };
            Transform sourceObj = new GameObject(string.IsNullOrEmpty(name) ? (type.ToString() + " - Channel") : name).transform;
            sourceObj.SetParent(folder);
            source = sourceObj.gameObject.AddComponent<AudioSource>();
            volume = 1;
            RecalculateVolume();
        }

        public void SetClip(AudioClip clip)
        {
            if (isPlaying) { Stop(); }
            source.clip = clip;
        }

        void RecalculateVolume()
        {
            float channelMaxVolume = 0;
            switch (type)
            {
                case Type.Music: channelMaxVolume = MusicMasterVolume; break;
                case Type.SFX: channelMaxVolume = SFXMasterVolume; break;
                case Type.Special: channelMaxVolume = MasterVolume; break;
            }
            float calculatedMaxVolume = MasterVolume * channelMaxVolume;

            source.volume = mute ? 0 : (calculatedMaxVolume * volume * maxVolume);
        }

        public void SetVolume(float volume)
        {
            this.volume = volume;
            RecalculateVolume();
        }

        public void SetPitch(float pitch)
        {
            if (source != null) { source.pitch = pitch; }
        }

        public void Stop(float fade = 0, System.Action then = null)
        {
            if (fade > 0 && Instance != null)
            {
                float startingVolume = volume;
                if (stopCoroutine != null) { Instance.StopCoroutine(stopCoroutine); stopCoroutine = null; }
                stopCoroutine = Instance.InterpolateCoroutine(fade, (n) => { SetVolume(startingVolume * (1 - n)); }, () => { if (source != null) { source.Stop(); } stopCoroutine = null; if (then != null) { then(); } });
            }
            else
            {
                source.Stop();
                if (then != null)
                {
                    then();
                }
            }
        }

        public void Play(float fade = 0)
        {
            source.Play();
            if (fade > 0)
            {
                float startingVolume = 0;
                if (playCoroutine != null) { Instance.StopCoroutine(playCoroutine); playCoroutine = null; }
                if (loopCoroutine != null) { Instance.StopCoroutine(loopCoroutine); loopCoroutine = null; }
                playCoroutine = Instance.InterpolateCoroutine(fade, (n) => { SetVolume(Mathf.Lerp(startingVolume, 1, n)); }, () => { playCoroutine = null; });
            }
            else
            {
                SetVolume(1);
            }


            if (type == Type.SFX && !source.loop)
            {
                loopCoroutine = Instance.ActionAfterCondition(() => { if (isPlaying && OnAudioUpdate != null) { OnAudioUpdate(); } return !isPlaying; }, () => { if (OnAudioEnds != null) { OnAudioEnds(); } });
            }
        }
    }
    static Audio Instance = null;

    public delegate void FLOATCALLBACK(float value);
    static FLOATCALLBACK OnMasterVolumeModified = delegate (float value) { };
    static FLOATCALLBACK OnSFXMasterVolumeModified = delegate (float value) { };
    static FLOATCALLBACK OnMusicMasterVolumeModified = delegate (float value) { };

    static Transform MusicChannelsFolder;
    static Transform SpecialChannelsFolder;
    static Transform SFXChannelsFolder;

    static float _MusicMasterVolume = 1;
    static float _SFXMasterVolume = 1;
    static float _MasterVolume = 1;
    public static float MusicMasterVolume { set { float final = Mathf.Clamp(value, 0, 1f); _MusicMasterVolume = final; OnMusicMasterVolumeModified(final); } get { return _MusicMasterVolume; } }
    public static float SFXMasterVolume { set { float final = Mathf.Clamp(value, 0, 1f); _SFXMasterVolume = final; OnSFXMasterVolumeModified(final); } get { return _SFXMasterVolume; } }
    public static float MasterVolume { set { float final = Mathf.Clamp(value, 0, 1f); _MasterVolume = final; OnMasterVolumeModified(final); } get { return mute ? 0 : _MasterVolume; } }

    public static AudioChannel MusicChannel { private set; get; }
    public static AudioChannel SpecialChannel { private set; get; }
    public static List<AudioChannel> SFXChannels { get { return _SFXChannels; } }
    static List<AudioChannel> _SFXChannels = new List<AudioChannel>();

    public static bool mute;

    [Header("Default Volume")]
    [Range(0, 1f)]
    public float MusicVolume = 1;
    public float SFXVolume = 1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        SFXMasterVolume = SFXVolume;
        MusicMasterVolume = MusicVolume;
        DontDestroyOnLoad(gameObject);
        Instance = this;
        CreateFolders();
    }

    public static void DestoyAllSFXChannels()
    {
        if (SFXChannels != null && SFXChannels.Count > 0)
        {
            AudioChannel[] SFX_Channels_Array = SFXChannels.ToArray();
            for (int i = 0; i < SFX_Channels_Array.Length; i++)
            {
                if (SFX_Channels_Array[i] != null && SFX_Channels_Array[i].source != null && SFX_Channels_Array[i].source.gameObject != null)
                {
                    Destroy(SFX_Channels_Array[i].source.gameObject);
                }
            }
            SFXChannels.Clear();
        }
    }

    public static void PlayMusic(AudioClip clip, bool fadeIN = true, bool fadeOUT = true, float fadeDuration = 0)
    {
        if (MusicChannel == null) { MusicChannel = CreateChannel("Main Music Channel", AudioChannel.Type.Music); MusicChannel.source.loop = true; }
        PlayChannel(MusicChannel, clip, fadeIN, fadeOUT, fadeDuration);
    }

    public static void PlaySpecial(AudioClip clip, bool fadeIN = true, bool fadeOUT = true, float fadeDuration = 0)
    {
        if (clip != null)
        {
            if (SpecialChannel == null) { SpecialChannel = CreateChannel("Main Special Channel", AudioChannel.Type.Special); }
            PlayChannel(SpecialChannel, clip, fadeIN, fadeOUT, fadeDuration);
        }
    }

    public static AudioChannel PlaySFX(AudioClip clip, bool reuse = true, float defaultVolume = 1)
    {
        AudioChannel RequestedSFXChannel = null;
        if (clip != null)
        {
            bool needsANewChannel = true;
            if (reuse)
            {
                AudioChannel oldChannel = FindSFX(clip);
                if (oldChannel != null)
                {
                    oldChannel.Stop();
                    oldChannel.SetVolume(defaultVolume);
                    oldChannel.Play();
                    needsANewChannel = false;
                    RequestedSFXChannel = oldChannel;
                }
            }

            if (needsANewChannel)
            {
                RequestedSFXChannel = CreateChannel("SFX Channel [" + SFXChannels.Count + "] - " + clip.name, AudioChannel.Type.SFX);
                RequestedSFXChannel.SetClip(clip);
                RequestedSFXChannel.SetVolume(defaultVolume);
                RequestedSFXChannel.Play();
                SFXChannels.Add(RequestedSFXChannel);
            }
        }
        return RequestedSFXChannel;
    }

    public static void PlayChannel(AudioChannel channel, AudioClip clip, bool fadeIN = true, bool fadeOUT = true, float fadeDuration = 0)
    {
        if (channel.isPlaying)
        {
            channel.Stop(fadeOUT ? fadeDuration : 0, () =>
            {
                channel.SetClip(clip);
                channel.Play(fadeIN ? fadeDuration : 0);
            });
        }
        else
        {
            channel.SetClip(clip);
            channel.Play(fadeIN ? fadeDuration : 0);
        }
    }

    public static AudioChannel CreateChannel(string name, AudioChannel.Type type)
    {
        AudioChannel channel = new AudioChannel(type, name);
        return channel;
    }

    public static void DestroyAllSounds()
    {
        if (MusicChannelsFolder != null) { DestroyImmediate(MusicChannelsFolder.gameObject); MusicChannelsFolder = null; MusicChannel = null; }
        if (SFXChannelsFolder != null) { DestoyAllSFXChannels(); DestroyImmediate(SFXChannelsFolder.gameObject);  }
        if (SpecialChannelsFolder != null) { DestroyImmediate(SpecialChannelsFolder.gameObject); SpecialChannel = null; SpecialChannel = null; }
        if (Instance != null)
        {
            Instance.CreateFolders();
        }
        OnMusicMasterVolumeModified = delegate (float v) { };
        OnMasterVolumeModified = delegate (float v) { };
        OnSFXMasterVolumeModified = delegate (float v) { };
    }

    public void CreateFolders()
    {
        if (MusicChannelsFolder == null)
        {
            MusicChannelsFolder = new GameObject("Music Channels").transform;
            MusicChannelsFolder.SetParent(transform, false);
        }
        if (SFXChannelsFolder == null)
        {
            SFXChannelsFolder = new GameObject("SFX Channels").transform;
            SFXChannelsFolder.SetParent(transform, false);
        }
        if (SpecialChannelsFolder == null)
        {
            SpecialChannelsFolder = new GameObject("Special Channels").transform;
            SpecialChannelsFolder.SetParent(transform, false);
        }

    }

    public static AudioChannel FindSFX(AudioClip clip)
    {
        for (int i = 0; i < SFXChannels.Count; i++)
        {
            if (SFXChannels[i].clip == clip) { return SFXChannels[i]; }
        }
        return null;
    }

    public static void DrawAudioWave(AudioClip audioClip, Material material)
    {
        const float PI2 = Mathf.PI * 2;
        const int maxArraySizeAllowedInShader = 1023;

        float[] audioData = new float[audioClip.samples * audioClip.channels];
        float[] fakeArray = new float[720];
        float[] arrayInUse;


        audioClip.GetData(audioData, 0);
        for (int i = 0; i < fakeArray.Length; i++)
        {
            fakeArray[i] = Mathf.Sin((i / (float)fakeArray.Length) * PI2);
        }

        arrayInUse = audioData;

        float[] simplifiedArray = arrayInUse.Length > maxArraySizeAllowedInShader ? new float[maxArraySizeAllowedInShader] : arrayInUse;

        if (arrayInUse.Length > maxArraySizeAllowedInShader)
        {
            float interval = arrayInUse.Length / (float)maxArraySizeAllowedInShader;
            for (int i = 0; i < simplifiedArray.Length; i++)
            {
                int current = Mathf.FloorToInt(interval * i);
                int next = Mathf.FloorToInt(interval * (i + 1));
                float avarage = 0;
                int counter = 0;
                for (int a = current; a < next; a++)
                {
                    if (a < arrayInUse.Length) { avarage += arrayInUse[a]; counter++; }
                }

                if (counter > 0) { avarage /= (float)counter; } else { avarage = Mathf.Abs(arrayInUse[current]); }

                simplifiedArray[i] = avarage;
            }
        }

        material.SetFloatArray("s_Data", simplifiedArray);
        float[] floatArray = material.GetFloatArray("s_Data");
        float maxValueFound = 0;
        Debug.Log("Arrays sizes match [" + simplifiedArray.Length + "," + floatArray.Length + "] = " + (simplifiedArray.Length == floatArray.Length));
        for (int i = 0; i < floatArray.Length; i++)
        {
            maxValueFound = Mathf.Max(maxValueFound, Mathf.Abs(floatArray[i]));
        }
        material.SetFloat("s_HighestValue", maxValueFound);
        material.SetFloat("s_ArrayLength", floatArray.Length);
    }

}
