using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAME : MonoBehaviour
{
    [Header("Components")]
    public Gameflow spellGameflow;
    public PotionMixer_Scene.Gameflow potionMixerGameflow;

    [Header("UDPs")]
    public string UDP_Receive_GameStart = "GAME_START";
    public string UDP_Receive_GameReset = "GAME_RESET";



    public void Start()
    {
        UDP.onMessageReceived = UDP_Commands;
        UDP.onMessageReceived += spellGameflow.UDP_MessageReceived;
        UDP.onMessageReceived += potionMixerGameflow.UDP_Commands;

        Audio.DestroyAllSounds();
        potionMixerGameflow.Setup();
        spellGameflow.Setup();
    }

    public void StartGame()
    {
        potionMixerGameflow.GameStarts();
    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void UDP_Commands(string command)
    {
        if (command.ToLower() == UDP_Receive_GameStart.ToLower()) { StartGame(); }
        if (command.ToLower() == UDP_Receive_GameReset.ToLower()) { ResetGame(); }
    }
}
