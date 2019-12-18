using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace PotionMixer_Scene
{
    public class Gameflow : MonoBehaviour
    {
        public string UDP_IngredientScanned = "INGREDIENT_";
        public string UDP_IngredientScanned2 = "RFID_0";
        public string UDP_ClearCauldron = "CLEAR";
        public string UDP_KeyPotionsScanned = "SOLVED_PZ02";

        public string[] keyPotions;

        [Header("Audio")]
        public AudioClip SFX_PotionFound;
        public AudioClip SFX_PuzzleCompleted;
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

        public bool potionMixerPuzzleCompleted { private set; get; }

        public void Setup()
        {
            Audio.PlaySpecial(MUSIC_Bubbles, true, true, 5);
            Audio.AudioChannel channel = Audio.SpecialChannel;
            channel.source.loop = true;
            channel.SetVolume(0.2f);
            channel.SetPitch(0.4f);

            potionMixer.OnPotionMatchFound = (potion) => { if (!potionMixerPuzzleCompleted) { Audio.PlaySFX(SFX_PotionFound); UIPotionFound.Show(potion); } };
            potionMixer.OnPotionMatchFound += (potion) => { if (!potionMixerPuzzleCompleted) { CheckPotion(potion); } };
            potionMixer.OnExtractAdded = (extract) => { if (SFXs_Drops != null && SFXs_Drops.Length > 0) { Audio.PlaySFX(SFXs_Drops[Random.Range(0, SFXs_Drops.Length)]); } };
            UIPotionFound.SetKeyPotionsLogo(keyPotions.Length, new Sprite[keyPotions.Length]);
            potionMixerPuzzleCompleted = false;
            potionMixer.Clear();
        }

        bool[] potionsCompleted = null;
        public void CheckPotion(Potion potion)
        {
            if (keyPotions == null || keyPotions.Length <= 0) return;
            potionsCompleted = potionsCompleted ?? new bool[keyPotions.Length];
            bool allCompleted = true;
            Sprite[] keyPotionsScanned = new Sprite[keyPotions.Length];
            int keyPotionsScannedIndex = 0;
            for (int i = 0; i < potionsCompleted.Length; i++)
            {
                if (keyPotions[i].ToLower() == potion.name.ToLower()) { potionsCompleted[i] = true; }
                allCompleted &= potionsCompleted[i];
                Potion _potion = potionMixer.GetPotionByName(keyPotions[i]);
                if (potionsCompleted[i]) { keyPotionsScanned[keyPotionsScannedIndex++] = _potion == null ? null : _potion.image; }
            }

            UIPotionFound.SetKeyPotionsLogo(keyPotions.Length, keyPotionsScanned);

            if (allCompleted)
            {
                potionMixerPuzzleCompleted = true;
                this.ActionAfterSecondDelay(UIPotionFound.duration, () =>
                {
                    UIPotionFound.MoveKeyPotionSectionToCenter(() =>
                    {
                        Audio.PlaySFX(SFX_PuzzleCompleted);
                        this.ActionAfterSecondDelay(0.6f, () =>
                        {
                            UIPotionFound.SetKeyPotionsLogo_Numbers(7);
                            this.ActionAfterSecondDelay(0.6f, () =>
                            {
                                UIPotionFound.SetKeyPotionsLogo_Numbers(7, 3);
                                this.ActionAfterSecondDelay(0.6f, () => { UIPotionFound.SetKeyPotionsLogo_Numbers(7, 3, 5); });
                            });
                        });
                    });
                });
                UDP.Write(UDP_KeyPotionsScanned);
            }
            
        }


        public void Reset()
        {
            potionMixer.Clear();
        }

        public void GameStarts()
        {
            //videoPlayer_magicMirror.PlayVideo(safetyVideo, false,null, false, ()=>this.ActionAfterFrameDelay(1, ()=> { videoPlayer_magicMirror.PlayVideo(introVideo); }));
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
            //videoPlayer_magicMirror.PlayVideo(video);
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

                if (command.Length > UDP_IngredientScanned2.Length && command.ToLower().Substring(0, UDP_IngredientScanned2.Length) == UDP_IngredientScanned2.ToLower())
                {
                    string ingredientIndex = command.ToLower().Substring(UDP_IngredientScanned2.Length);
                    int index = -1;
                    if (int.TryParse(ingredientIndex, out index) && potionMixer.extracts != null && --index >= 0 && index < potionMixer.extracts.Length)
                    {
                        potionMixer.AddExtract(potionMixer.extracts[index]);
                    }
                }

                if (command.ToLower() == UDP_ClearCauldron.ToLower()) { potionMixer.Clear(); }
            }
        }
    }
}
