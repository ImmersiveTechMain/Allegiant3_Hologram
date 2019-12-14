using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timer = SkeetoTools.Timer;

public class GAME : MonoBehaviour
{
    [Header("Settings")]
    public float timerDuration = 60 * 30;

    [Header("Components")]
    public Gameflow spellGameflow;    
    public PotionMixer_Scene.Gameflow potionMixerGameflow;

    [Header("UDPs")]
    public string UDP_Receive_GameStart = "GAME_START";
    public string UDP_Receive_GameReset = "GAME_RESET";
    public string UDP_Receive_MagicRealmUnlocked = "MAGIC_REALM_UNLOCKED";
    public string UDP_Receive_WellComplete = "WELL_COMPLETE";

    public static Timer gameTimer { private set; get; }
    public static float gameDuration = 60 * 30;

    public bool isMagicRealmUnlocked { private set; get; }
    public bool isWellCompleted { private set; get; }

    public delegate void CALLBACK();
    public CALLBACK OnWellComplete = delegate () { };
    public CALLBACK OnMagicRealmUnlocked = delegate () { };

    public void Start()
    {
        DefineTimer();
        Audio.DestroyAllSounds();

        isMagicRealmUnlocked = false;

        

        OnMagicRealmUnlocked = potionMixerGameflow.OnMagicRealUnlocked;
        OnWellComplete = potionMixerGameflow.OnWellCompleted;
        gameTimer.OnTimerEnds = LoseGame;
        UDP.onMessageReceived = UDP_Commands;
        UDP.onMessageReceived += spellGameflow.UDP_MessageReceived;
        UDP.onMessageReceived += potionMixerGameflow.UDP_Commands;


        potionMixerGameflow.Setup();
        spellGameflow.Setup();
    }

    public void UnlockMagicRealm()
    {
        if (!isMagicRealmUnlocked)
        {
            isMagicRealmUnlocked = true;
            OnMagicRealmUnlocked();
        }
    }

    public void CompleteWell()
    {
        if (!isWellCompleted)
        {
            isWellCompleted = true;
            OnWellComplete();
        }
    }

    public void LoseGame()
    {
        Debug.Log("Game Lost - Timer ran out");
        potionMixerGameflow.GameLost();
    }

    public void GameCompleted()
    {
        Debug.Log("Game Completed");
        potionMixerGameflow.GameCompleted();
        if (gameTimer != null) { gameTimer.Pause(); }
    }

    public void DefineTimer()
    {
        gameDuration = timerDuration;
        if (gameTimer != null && gameTimer.isRunning) { gameTimer.OnTimerEnds = delegate () { }; gameTimer.OnTick = delegate () { }; gameTimer.Stop(); }
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
        if (command.ToLower() == UDP_Receive_MagicRealmUnlocked.ToLower()) { UnlockMagicRealm(); }
        if (command.ToLower() == UDP_Receive_WellComplete.ToLower()) { CompleteWell(); }
    }
}
