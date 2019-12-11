using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Timer : MonoBehaviour
{
    public TextMeshProUGUI timerLabel;
    
    // Update is called once per frame
    void Update()
    {
        timerLabel.text = SkeetoTools.Timer.FormatToDisplay((GAME.gameTimer != null && GAME.gameTimer.isRunning) ? GAME.gameTimer.timeRemaining : GAME.gameDuration, false, false, true);
    }
}
