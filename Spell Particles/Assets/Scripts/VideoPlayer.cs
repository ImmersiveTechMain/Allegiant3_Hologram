using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
public class VideoPlayer : MonoBehaviour
{
    public delegate void CALLBACK();
    ///public CALLBACK OnVideoEnds = delegate () { };
    public UnityEngine.Video.VideoPlayer Player;
    public GameObject VideoObject;
    public GameObject loadingCover;
    public RawImage RawImage;
    public AudioSource audioSource; // optional
    public bool IsPlaying { get { return Player.isPlaying; } }
    public bool IsOpen { get { return VideoObject.activeInHierarchy; } }
    public float volume { get { return Player.GetDirectAudioVolume(0); } set { Player.SetDirectAudioVolume(0, value); } }
    public bool createCloneOfTexture = false;
    bool cloneCreated = false;

    private void Awake()
    {
        if (createCloneOfTexture && !cloneCreated)
        {
            RawImage.texture = Instantiate(RawImage.texture);
            Player.targetTexture = RawImage.texture as RenderTexture;
            cloneCreated = true;
        }
        Player.loopPointReached += (player) => { CallTheCallback(); if (!player.isLooping) { Close(); } };
        Close();
    }

    private void Update()
    {
        if (this.gameObject.activeInHierarchy)
        {
            Player.playbackSpeed = Mathf.Clamp(Player.playbackSpeed, 0f, 3f);
            if (audioSource != null)
                audioSource.pitch = Player.playbackSpeed;
            if (IsOpen && IsPlaying && loadingCover != null)
                loadingCover.SetActive(false);
        }
    }
    void AddAudioComponent()
    {
        if (audioSource == null)
        {
            if (GetComponent<AudioSource>() == null)
            {
                this.gameObject.AddComponent<AudioSource>();
            }
            audioSource = GetComponent<AudioSource>();
        }
    }
    bool AudioExists(AudioClip clip, AudioSource component)
    {
        return clip != null && component != null;
    }
    void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
    void PlayAudio(AudioClip clip, AudioSource audioSource)
    {
        if (audioSource == null) { return; }
        if (AudioExists(clip, audioSource))
        {
            audioSource.Play();
        }
    }
    System.Action _then = null;
    public void PlayVideo(VideoClip video, bool isLooping = false, AudioClip audio = null, bool isMuted = false, Action then = null)
    {
        loadingCover.SetActive(true);
        if (IsPlaying)
        {
            Player.Stop();
            StopAudio();
        }
        if (video == null)
        {
            Close();
            return;
        }
        if (audio != null && audioSource != null)
        {
            audioSource.clip = audio;
            StopAudio();
        }
        Player.clip = video;
        Player.isLooping = isLooping;
        Open();
        if (Player.isPrepared)
        {
            Player.Play();
            PlayAudio(audio, audioSource);
            this.ActionAfterFrameDelay(1, () =>
            {
                loadingCover.SetActive(false);
            });
        }
        else
        {
            Player.Prepare();
            bool done = false;
            Player.prepareCompleted += (player) =>
            {
                if (!done)
                {
                    done = true;
                    Player.Play();
                    PlayAudio(audio, audioSource);
                    this.ActionAfterCondition(() => { return player.time > 0.0f; }, () => { loadingCover.SetActive(false); });
                }
            };
        }
        _then = then;
        volume = isMuted ? 0f : 1f;
    }
    void CallTheCallback()
    {
        if (_then == null) { return; }
        Action then = (Action)_then.Clone();
        _then = null;
        if (then != null) then();
    }
    public void Open()
    {
        loadingCover.SetActive(true);
        VideoObject.SetActive(true);
    }
    public void Close(bool triggerPendingCallbacks = false)
    {
        Player.isLooping = false;
        if (triggerPendingCallbacks)
        {
            CallTheCallback();
        }
        loadingCover.SetActive(true);
        VideoObject.SetActive(false);
        if (IsPlaying)
        {
            Player.Stop();
            StopAudio();
        }
    }
}