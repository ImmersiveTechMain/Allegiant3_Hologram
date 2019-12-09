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

        public AudioClip MUSIC_Bubbles;
        public AudioClip[] SFXs_Drops;

        public UIPotionFound UIPotionFound;
        public PotionMixer potionMixer;
        public VideoPlayer videoPlayer_magicMirror;

        [Header("Videos")]
        public VideoClip introVideo;
        
        public void Setup()
        {
            Audio.MusicMasterVolume = 0.2f;
            Audio.PlayMusic(MUSIC_Bubbles, true, true, 5);
            Audio.MusicChannel.SetPitch(0.4f);
            potionMixer.OnPotionMatchFound = UIPotionFound.Show;
            potionMixer.OnExtractAdded = (extract) => { if (SFXs_Drops != null && SFXs_Drops.Length > 0) { Audio.PlaySFX(SFXs_Drops[Random.Range(0, SFXs_Drops.Length)]); } };
            potionMixer.Clear();
        }

        public void Reset()
        {
            potionMixer.Clear();
        }

        public void GameStarts()
        {
            videoPlayer_magicMirror.PlayVideo(introVideo);
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
