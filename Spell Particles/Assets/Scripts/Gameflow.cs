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

    [Header("Components")]
    public SpellComboManager SpellManager;


    // Start is called before the first frame update
    void Start()
    {
        UDP.onMessageReceived = UDP_MessageReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void UDP_MessageReceived(string command) {
        if (command != null && command.Length > 0) {
            if (command == UDP_ToReceive_OnScannedFireball) {
                
            }

        }
    }



}
