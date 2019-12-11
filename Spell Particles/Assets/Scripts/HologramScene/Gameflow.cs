using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameflow : MonoBehaviour
{
    

    [Header("Components")]
    public SpellComboManager SpellManager;
    public WellManager WellManager;

    
    

    // Start is called before the first frame update
    public void Setup()
    {
        ResetGame();


        SpellManager.OnFinalSpellSolved = WellManager.ShowParticlesInWell;
        
    }


    public void ResetGame() {
        SpellManager.Reset();
    }

    public void UDP_MessageReceived(string command) {

        SpellManager.UDP_MessageReceived(command);
        if (command != null && command.Length > 0) {
            
        }
    }

    




}
