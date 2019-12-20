using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHintsUDP : MonoBehaviour
{
    public delegate void OnHintPlayedCALLBACK(int hintIndex, AudioClip clip);
    public OnHintPlayedCALLBACK OnHintPlayed = delegate (int hintIndex, AudioClip clip) { };

    [Header("Settings")]
    public string UDP_ToReceive_PlayHint = "HINT_A1_"; // -HINT_A1_X
    public bool startingCountFrom1 = false;
    [Header("Data")]
    public AudioClip[] audioHints;

    public void UDP_Commands(string command)
    {
        if (command != null && command.Length > UDP_ToReceive_PlayHint.Length && command.ToLower().Substring(0, UDP_ToReceive_PlayHint.Length) == UDP_ToReceive_PlayHint.ToLower())
        {
            string index_STR = command.ToLower().Substring(UDP_ToReceive_PlayHint.Length);
            int index = 0;
            if (int.TryParse(index_STR, out index))
            {
                if (startingCountFrom1) { index--; }
                PlayHint(index);
            }
        }
    }

    public void PlayHint(int index)
    {
        if (index >= 0 && index < audioHints.Length && audioHints[index] != null)
        {
            Audio.SFXMasterVolume = 0.3f;
            Audio.MusicMasterVolume = 0.05f;
            AudioClip hint = audioHints[index];
            Audio.PlaySpecial(hint);
            Audio.AudioChannel channel = Audio.SpecialChannel;
            if (channel != null)
            {
                channel.OnAudioEnds = () =>
                {
                    Audio.SFXMasterVolume = 1f;
                    Audio.MusicMasterVolume = GAME.MaxMusicVolume;
                };
            }
            OnHintPlayed(index, hint);
        }
    }

    private void OnDestroy()
    {
        Audio.SFXMasterVolume = 1;
        Audio.MusicMasterVolume = GAME.MaxMusicVolume;
    }
}
