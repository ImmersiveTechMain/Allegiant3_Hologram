using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace PotionMixer_Scene
{
    public class Gameflow : MonoBehaviour
    {
        public string UDP_IngredientScanned = "INGREDIENT_";
        public string UDP_ClearCauldron = "CLEAR";
        public string UDP_KeyPotionsScanned = "SOLVED_PZ02";

        public string[] keyPotions;

        public AudioClip MUSIC_Bubbles;
        public AudioClip[] SFXs_Drops;

        public UIPotionFound UIPotionFound;
        public PotionMixer potionMixer;
        public VideoPlayer videoPlayer_magicMirror;

        [Header("Videos")]
        public VideoClip safetyVideo;
        public VideoClip introVideo;
        public VideoClip magicRealmUnlockedVideo;
        public VideoClip magicSongVideo;
        public VideoClip loseVideo;
        public VideoClip winVideo;

        public void Setup()
        {
            Audio.MusicMasterVolume = 0.2f;
            Audio.PlayMusic(MUSIC_Bubbles, true, true, 5);
            Audio.MusicChannel.SetPitch(0.4f);
            potionMixer.OnPotionMatchFound = UIPotionFound.Show;
            potionMixer.OnPotionMatchFound += CheckPotion;
            potionMixer.OnExtractAdded = (extract) => { if (SFXs_Drops != null && SFXs_Drops.Length > 0) { Audio.PlaySFX(SFXs_Drops[Random.Range(0, SFXs_Drops.Length)]); } };
            potionMixer.Clear();
        }

        bool[] potionsCompleted = null;
        public void CheckPotion (Potion potion)
        {
            if (keyPotions == null || keyPotions.Length <= 0) return;
            potionsCompleted = potionsCompleted ?? new bool[keyPotions.Length];
            bool allCompleted = true;
            for (int i = 0; i < potionsCompleted.Length; i++)
            {
                if (keyPotions[i].ToLower() == potion.name.ToLower()) { potionsCompleted[i] = true; }
                allCompleted &= potionsCompleted[i];
            }

            if (allCompleted)
            {
                UDP.Write(UDP_KeyPotionsScanned);
            }
        }

        public void Reset()
        {
            potionMixer.Clear();
        }

        public void GameStarts()
        {
            videoPlayer_magicMirror.PlayVideo(safetyVideo, false,null, false, ()=>this.ActionAfterFrameDelay(1, ()=> { videoPlayer_magicMirror.PlayVideo(introVideo); }));
        }
        
        public void OnMagicRealUnlocked()
        {
            PlayVideo(magicRealmUnlockedVideo);
        }

        public void OnWellCompleted()
        {
            PlayVideo(magicSongVideo);
        }

        public void PlayVideo(VideoClip video)
        {
            videoPlayer_magicMirror.PlayVideo(video);
        }

        public void GameLost()
        {
            PlayVideo(loseVideo);
        }

        public void GameCompleted()
        {
            PlayVideo(winVideo);
        }

        public void UDP_Commands(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                if (command.Length > UDP_IngredientScanned.Length && command.ToLower().Substring(0, UDP_IngredientScanned.Length) == UDP_IngredientScanned.ToLower())
                {
                    string ingredientID = command.ToLower().Substring(UDP_IngredientScanned.Length);
                    potionMixer.AddExtract(potionMixer.GetIngredientlByUdpID(ingredientID) as Extract);
                }

                if (command.ToLower() == UDP_ClearCauldron.ToLower()) { potionMixer.Clear(); }
            }
        }
    }
}
