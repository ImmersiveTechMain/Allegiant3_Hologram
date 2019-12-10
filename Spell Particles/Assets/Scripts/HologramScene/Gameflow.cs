using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameflow : MonoBehaviour
{
    [Header("UDP")]
    public string UDP_ToReceive_OnScannedFireball = "SPELL_FIREBALL";
    public string UDP_ToReceive_OnScannedLightning = "SPELL_LIGHTNING";
    public string UDP_ToReceive_OnScannedTornado = "SPELL_TORNADO";
    public string UDP_ToReceive_OnScannedCyclone = "SPELL_CYCLONE";
    public string UDP_ToReceive_OnScannedSnowstorm = "SPELL_SNOWSTORM";
    public string UDP_ToReceive_OnScannedConfetti = "SPELL_CONFETTI";
    public string UDP_ToReceive_OnGameStart = "GAME_START";
    public string UDP_ToReceive_OnGameReset = "GAME_RESET";

    [Header("Components")]
    public SpellComboManager SpellManager;


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
            if (command.ToUpper() == UDP_ToReceive_OnScannedFireball.ToUpper()) {
                SpellManager.ToggleFireball();
            }

            if (command.ToUpper() == UDP_ToReceive_OnScannedLightning.ToUpper()) {
                SpellManager.ToggleLightningBall();
            }

            if (command.ToUpper() == UDP_ToReceive_OnScannedTornado.ToUpper()) {
                SpellManager.ToggleTornado();
            }

            if (command.ToUpper() == UDP_ToReceive_OnScannedConfetti.ToUpper()) {
                SpellManager.ToggleConfetti();
            }

            if (command.ToUpper() == UDP_ToReceive_OnScannedCyclone.ToUpper()) {
                SpellManager.ToggleCyclone();
            }

            if (command.ToUpper() == UDP_ToReceive_OnScannedSnowstorm.ToUpper()) {
                SpellManager.ToggleSnowstorm();
            }

            if (command.ToUpper() == UDP_ToReceive_OnGameStart.ToUpper()) {
                
            }

            if (command.ToUpper() == UDP_ToReceive_OnGameReset.ToUpper()) {

            }
        }
    }



}
