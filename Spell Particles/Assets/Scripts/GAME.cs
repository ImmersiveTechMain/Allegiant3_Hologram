using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timer = SkeetoTools.Timer;

public class GAME : MonoBehaviour
{
    public static string MAGIC_MIRROR_IP = "10.199.199.138";
    public static string WELL_IP = "10.199.199.139";
    public static string TIMER_IP = "10.199.199.140";

    public static int MAGIC_MIRROR_PORT = 5000;
    public static int WELL_PORT = 5000;
    public static int TIMER_PORT = 5000;

    public static float MaxMusicVolume = -1;

    public static float VideoDuration_Start = 90;
    public static float VideoDuration_PZ05 = 50;
    public static float VideoDuration_PZ08 = 16;
    public static float VideoDuration_PZ09 = 41;
    public static float VideoDuration_Fail = 41;

    public static Coroutine musicSilenceCoroutine = null;

    public static void MuteMusicVolumeTemporally (MonoBehaviour coroutineHolder, float duration)
    {
        Audio.MusicMasterVolume = 0;
        if (GAME.musicSilenceCoroutine != null) { coroutineHolder.StopCoroutine(GAME.musicSilenceCoroutine); GAME.musicSilenceCoroutine = null; }
        GAME.musicSilenceCoroutine = coroutineHolder.ActionAfterSecondDelay(duration, () => { Audio.MusicMasterVolume = GAME.MaxMusicVolume; });
    }

    [Header("Settings")]
    public float timerDuration = 60 * 30;

    [Header("Components")]
    public Gameflow spellGameflow;
    public PotionMixer_Scene.Gameflow potionMixerGameflow;

    [Header("Audio")]
    public AudioClip MUSIC_GameMusic;

    [Header("UDPs")]
    public string UDP_Receive_GameStart = "GAME_START";
    public string UDP_Receive_GameReset = "GAME_RESET";
    public string UDP_Receive_MagicRealmUnlocked = "MAGIC_REALM_UNLOCKED";
    public string UDP_Receive_WellComplete = "WELL_COMPLETE";
    public string UDP_Receive_PZ05 = "SOLVED_PZ05";
    public string UDP_ToSend_GameFailed = "GAME_FAIL";
    public string UDP_ToSend_GameStart = "GAME_START";
    public string UDP_ToSend_GameReset = "GAME_RESET";

    public static Timer gameTimer { private set; get; }
    public static float gameDuration = 60 * 35;

    public bool isMagicRealmUnlocked { private set; get; }
    public bool isWellCompleted { private set; get; }

    public delegate void CALLBACK();
    public CALLBACK OnWellComplete = delegate () { };
    public CALLBACK OnMagicRealmUnlocked = delegate () { };

    public void Start()
    {
        musicSilenceCoroutine = null;
        MaxMusicVolume = MaxMusicVolume < 0 ? Audio.MusicMasterVolume : MaxMusicVolume;
        Audio.MusicMasterVolume = MaxMusicVolume;
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
        UDP.Write(UDP_ToSend_GameFailed, MAGIC_MIRROR_IP, MAGIC_MIRROR_PORT);
        UDP.Write(UDP_ToSend_GameFailed);
        GAME.MuteMusicVolumeTemporally(this, GAME.VideoDuration_Fail);
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
        Audio.PlayMusic(MUSIC_GameMusic);
        potionMixerGameflow.GameStarts();
        UDP.Write(UDP_ToSend_GameStart, MAGIC_MIRROR_IP, MAGIC_MIRROR_PORT);
        UDP.Write(UDP_ToSend_GameStart, WELL_IP, WELL_PORT);
        UDP.Write(UDP_ToSend_GameStart, TIMER_IP, TIMER_PORT);

        GAME.MuteMusicVolumeTemporally(this, GAME.VideoDuration_Start);

        if (gameTimer != null && !gameTimer.isRunning) { gameTimer.Run(); }
    }

    public void ResetGame()
    {

        UDP.Write(UDP_ToSend_GameReset, MAGIC_MIRROR_IP, MAGIC_MIRROR_PORT);
        UDP.Write(UDP_ToSend_GameReset, WELL_IP, WELL_PORT);
        UDP.Write(UDP_ToSend_GameReset, TIMER_IP, TIMER_PORT);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void UDP_Commands(string command)
    {
        if (command.ToLower() == UDP_Receive_GameStart.ToLower()) { StartGame(); }
        if (command.ToLower() == UDP_Receive_GameReset.ToLower()) { ResetGame(); }
        if (command.ToLower() == UDP_Receive_MagicRealmUnlocked.ToLower()) { UnlockMagicRealm(); }
        if (command.ToLower() == UDP_Receive_WellComplete.ToLower()) { CompleteWell(); }
        if (command.ToLower() == UDP_Receive_PZ05.ToLower())
        {
            UDP.Write(UDP_Receive_PZ05, MAGIC_MIRROR_IP, MAGIC_MIRROR_PORT);
            GAME.MuteMusicVolumeTemporally(this, GAME.VideoDuration_PZ05);
        }
    }
}
