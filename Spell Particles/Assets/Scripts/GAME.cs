using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timer = SkeetoTools.Timer;

public class GAME : MonoBehaviour
{
    [Header("Settings")]
    public float timerDuration = 60*30;

    [Header("Components")]
    public Gameflow spellGameflow;
    public PotionMixer_Scene.Gameflow potionMixerGameflow;

    [Header("UDPs")]
    public string UDP_Receive_GameStart = "GAME_START";
    public string UDP_Receive_GameReset = "GAME_RESET";

    public static Timer gameTimer { private set; get; }
    public static float gameDuration = 60 * 30;

    public void Start()
    {
        DefineTimer();
        gameTimer.OnTimerEnds = LoseGame;

        UDP.onMessageReceived = UDP_Commands;
        UDP.onMessageReceived += spellGameflow.UDP_MessageReceived;
        UDP.onMessageReceived += potionMixerGameflow.UDP_Commands;
        Audio.DestroyAllSounds();

        potionMixerGameflow.Setup();
        spellGameflow.Setup();
    }

   

    public void LoseGame()
    {
        Debug.Log("Game Lost - Timer ran out");
    }

    public void DefineTimer()
    {
        gameDuration = timerDuration;
        if (gameTimer != null && gameTimer.isRunning) { gameTimer.OnTimerEnds = delegate () { }; gameTimer.OnTick = delegate() { }; gameTimer.Stop(); }
        gameTimer = new Timer(timerDuration);
    }

    public void StartGame()
    {
        potionMixerGameflow.GameStarts();
        if (gameTimer != null && !gameTimer.isRunning) { gameTimer.Run(); }
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
