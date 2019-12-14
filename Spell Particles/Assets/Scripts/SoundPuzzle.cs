using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPuzzle : MonoBehaviour
{
    public delegate void CALLBACK();
    public CALLBACK OnPuzzleComplete = delegate () { };


    bool[] startPuzzleRequiredNotes = null;
    public string[] UDP_ToReceive_StartSoundPuzzle;
    public string UDP_ToReceive_NoteScan = "NOTE_";
    public string UDP_ToSend_CompleteSoundPuzzle = "COMPLETED_SOUND_PUZZLE";
    public string UDP_ToSend_CompleteWell = "SOLVED_PZ06";

    public AudioClip[] Notes;
    public AudioClip WellSensorSound;
    public AudioClip IncorrectNoteSound;

    public float TimeBetweenNotes = 0.5f;

    public int[] correctNoteSequence;
    private int sequenceIndex = 0;

    private bool gameFinished = false;
    private bool canPlayNotes = false;
    private bool hasBegun = false;


    // Start is called before the first frame update
    void Start()
    {
        ClampSequence();
    }

    void BeginPuzzle()
    {
        if (!hasBegun)
        {
            hasBegun = true;
            Audio.PlaySFX(WellSensorSound);
            UDP.Write(UDP_ToSend_CompleteWell);
            this.ActionAfterSecondDelay(3f, () =>
            {
                RestartSequence();
                StartCoroutine(PlaySampleSongCoroutine());
            });
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BeginPuzzle();
        }
    }

    void GenerateRandomSequence()
    {
        correctNoteSequence = new int[8];
        for (int i = 0; i < correctNoteSequence.Length; i++)
        {
            correctNoteSequence[i] = Random.Range(0, Notes.Length);
        }
    }


    void ClampSequence()
    {
        for (int i = 0; i < correctNoteSequence.Length; i++)
        {
            correctNoteSequence[i] = Mathf.Clamp(correctNoteSequence[i], 0, Notes.Length - 1);
        }
    }

    void RestartSequence()
    {
        sequenceIndex = 0;
    }

    void PlayNote(int index)
    {
        if (!canPlayNotes)
            return;

        if (index >= 0 && index < correctNoteSequence.Length)
        {
            PlayNoteAudioClip(index);
        }

        if (gameFinished)
            return;


        if (index == correctNoteSequence[sequenceIndex])
        {
            sequenceIndex++;
            if (sequenceIndex >= correctNoteSequence.Length)
            {
                gameFinished = true;
                UDP.Write(UDP_ToSend_CompleteSoundPuzzle);
                Debug.Log("Sound puzzle completed.");
                OnPuzzleComplete();
            }
        }
        else
        {
            StartCoroutine(IncorrectNoteCoroutine());
        }
    }

    IEnumerator IncorrectNoteCoroutine()
    {
        canPlayNotes = false;
        RestartSequence();
        Audio.PlaySFX(IncorrectNoteSound);
        yield return new WaitForSeconds(0.1f);
        Audio.PlaySFX(IncorrectNoteSound);
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(PlaySampleSongCoroutine());
    }

    void PlayNoteAudioClip(int index)
    {
        if (index >= 0 && index < Notes.Length)
        {
            Audio.PlaySFX(Notes[index], false);
            Audio.PlaySFX(Notes[index], false);
            Audio.PlaySFX(Notes[index], false);
            Audio.PlaySFX(Notes[index], false);
            Audio.PlaySFX(Notes[index], false);
        }
    }

    IEnumerator PlaySampleSongCoroutine()
    {
        canPlayNotes = false;
        int n = 0;
        while (n < correctNoteSequence.Length)
        {
            PlayNoteAudioClip(correctNoteSequence[n]);
            n++;
            yield return new WaitForSeconds(TimeBetweenNotes);
        }
        canPlayNotes = true;
        yield return null;
    }

    public void UDP_MessageReceived(string command)
    {
        if (command != null && command.Length > 0)
        {

            if (UDP_ToReceive_StartSoundPuzzle != null && UDP_ToReceive_StartSoundPuzzle.Length > 0)
            {
                startPuzzleRequiredNotes = startPuzzleRequiredNotes ?? new bool[UDP_ToReceive_StartSoundPuzzle.Length];
                bool allTrue = true;
                for (int i = 0; i < UDP_ToReceive_StartSoundPuzzle.Length; i++)
                {
                    if (!hasBegun && UDP_ToReceive_StartSoundPuzzle[i] != null && command.ToUpper() == UDP_ToReceive_StartSoundPuzzle[i].ToUpper())
                    {
                        startPuzzleRequiredNotes[i] = true;
                    }
                    allTrue &= startPuzzleRequiredNotes[i];
                }

                if (!hasBegun && allTrue)
                {
                    BeginPuzzle();
                }
            }



            bool matchesNoteScan = command.Length >= UDP_ToReceive_NoteScan.Length && command.Substring(0, UDP_ToReceive_NoteScan.Length).ToUpper() == UDP_ToReceive_NoteScan.ToUpper();
            if (matchesNoteScan)
            {
                string toParse = command.Substring(UDP_ToReceive_NoteScan.Length);
                string[] pieces = toParse.Split('_');
                int n;
                if (int.TryParse(pieces[0], out n))
                {
                    PlayNote(n);
                }
            }
        }
    }


}
