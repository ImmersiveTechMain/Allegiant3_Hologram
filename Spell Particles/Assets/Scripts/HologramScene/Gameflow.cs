using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameflow : MonoBehaviour
{
    [Header("UDP")]
    //public string UDP_ToReceive_OnScannedFireball = "SPELL_FIREBALL";
    //public string UDP_ToReceive_OnScannedLightning = "SPELL_LIGHTNING";
    //public string UDP_ToReceive_OnScannedTornado = "SPELL_TORNADO";
    //public string UDP_ToReceive_OnScannedCyclone = "SPELL_CYCLONE";
    //public string UDP_ToReceive_OnScannedSnowstorm = "SPELL_SNOWSTORM";
    //public string UDP_ToReceive_OnScannedConfetti = "SPELL_CONFETTI";
    public string UDP_ToReceive_SpellPegs = "SPELL_";

    [Header("Components")]
    public SpellComboManager SpellManager;

    bool[] pegBooleans;


    // Start is called before the first frame update
    public void Setup()
    {
        ResetGame();
    }


    public void ResetGame() {
        UDP.onMessageReceived = UDP_MessageReceived;
        SpellManager.Reset();
    }

    public void UDP_MessageReceived(string command) {
        if (command != null && command.Length > 0) {
            //if (command.ToUpper() == UDP_ToReceive_OnScannedFireball.ToUpper()) {
            //    SpellManager.ToggleFireball();
            //}

            //if (command.ToUpper() == UDP_ToReceive_OnScannedLightning.ToUpper()) {
            //    SpellManager.ToggleLightningBall();
            //}

            //if (command.ToUpper() == UDP_ToReceive_OnScannedTornado.ToUpper()) {
            //    SpellManager.ToggleTornado();
            //}

            //if (command.ToUpper() == UDP_ToReceive_OnScannedConfetti.ToUpper()) {
            //    SpellManager.ToggleConfetti();
            //}

            //if (command.ToUpper() == UDP_ToReceive_OnScannedCyclone.ToUpper()) {
            //    SpellManager.ToggleCyclone();
            //}

            //if (command.ToUpper() == UDP_ToReceive_OnScannedSnowstorm.ToUpper()) {
            //    SpellManager.ToggleSnowstorm();
            //}

            bool matchesSpellPeg = command.Substring(0, UDP_ToReceive_SpellPegs.Length).ToUpper() == UDP_ToReceive_SpellPegs.ToUpper();
            if (matchesSpellPeg) {
                string toParse = command.Substring(UDP_ToReceive_SpellPegs.Length);
                string[] pieces = toParse.Split('_');
                pegBooleans = ConvertBinaryStringArrayToBoolArray(pieces);
                CheckSpellPegs();
            }
        }
    }




    /// <summary>
    /// LANCE USES THESE "x inputs":
    /// Fireball:   [13 18 15 17] - 20 9 6 8 - 11 3 0 2
    /// Lighting:   13 18 15 16 - [20 9 6 7] - 11 3 0 1
    /// tornado:    18 19 17 16 - 9 10 8 7 - [3 4 2 1]
    ///
    /// Confetti:   13 17 15 16 - 20 8 6 7 - 11 2 0 1
    /// cyclone:    18 14  17 19 - 9 5 8 10 - 3 * 2 4
    /// snowstorm:  18 17 15 19 - 9 8 6 10 - 3 2 0 4
    /// 
    /// 
    /// WE WILL RECEIVE THESE:
    /// Fireball: 1 6 3 5 - 8 13 10 12 - 15 20 17 19
    /// Lighting: 1 6 3 4 - 8 13 10 11 - 15 20 17 18
    /// tornado: 6 7 5 4 - 13 14 12 11 - 20 21 19 18
    /// 
    /// Confetti: 1 5 3 4 - 8 12 10 11 - 15 19 17 18
    /// cyclone: 6 4 5 2 - 13 9 12 14 - 20 16 19 21
    /// snowstorm: 6 5 3 2 - 13 12 10 14 - 20 19 17 21
    /// </summary>   

    void CheckSpellPegs() {
        if (PegsAreActive(1, 6, 3, 5) || PegsAreActive(8, 13, 10, 12) || PegsAreActive(15, 20, 17, 19)) {
            //FIREBALL
            SpellManager.ToggleFireball();
        }
        if (PegsAreActive(1, 6, 3, 4) || PegsAreActive(8, 13, 10, 11) || PegsAreActive(15, 20, 17, 18)) {
            //LIGHTNING
            SpellManager.ToggleLightningBall();
        }
        if (PegsAreActive(6, 7, 5, 4) || PegsAreActive(13, 14, 12, 11) || PegsAreActive(20, 21, 19, 18)) {
            //TORNADO
            SpellManager.ToggleTornado();
        }

        if (PegsAreActive(1, 5, 3, 4) || PegsAreActive(8, 12, 10, 11) || PegsAreActive(15, 19, 17, 18)) {
            //CONFETTI
            SpellManager.ToggleConfetti();
        }
        if (PegsAreActive(6, 4, 5, 2) || PegsAreActive(13, 9, 12, 14) || PegsAreActive(20, 16, 19, 21)) {
            //CYCLONE
            SpellManager.ToggleCyclone();
        }
        if (PegsAreActive(6, 5, 3, 2) || PegsAreActive(13, 12, 10, 14) || PegsAreActive(20, 19, 17, 21)) {
            //SNOWSTORM
            SpellManager.ToggleSnowstorm();
        }
    }

    bool PegsAreActive(int a, int b, int c, int d) {        
        return PegIsActive(a) && PegIsActive(b) && PegIsActive(c) && PegIsActive(d);
    }

    bool PegIsActive(int index) {
        bool[] array = pegBooleans;
        if (index <= 0 || index > array.Length) {
            return false;
        }
        return array[index - 1];
    }

    bool[] ConvertBinaryStringArrayToBoolArray(string[] stringArray) {
        bool[] boolArray = new bool[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++) {
            int n;
            boolArray[i] = int.TryParse(stringArray[i], out n) && n == 1;
        }
        return boolArray;
    }


}
