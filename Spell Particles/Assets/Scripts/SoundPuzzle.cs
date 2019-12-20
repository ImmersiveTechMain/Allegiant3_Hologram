using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPuzzle : MonoBehaviour
{
    public delegate void CALLBACK();
    public CALLBACK OnPuzzleComplete = delegate () { };
    public CALLBACK OnWellNoteScanned = delegate () { };



    bool[] startPuzzleRequiredNotes = null;
    public string[] UDP_ToReceive_StartSoundPuzzle;
    public string UDP_ToReceive_NoteScan = "NOTE_";
    public string UDP_ToSend_CompleteSoundPuzzle = "SOLVED_PZ08";
    public string UDP_ToReceive_Skip_CompleteSoundPuzzle = "SKIP_PZ08";
    public string UDP_ToSend_CompleteWell = "SOLVED_PZ06";

    [Header("4 Well Notes")]
    public AudioClip[] ActivationNotes;
    [Header("Other Sounds")]
    public AudioClip[] Notes;
    public AudioClip WellSensorSound;
    public AudioClip IncorrectNoteSound;

    public float cooldownBetweenUserInputedNotes = 0.9f;
    public float TimeBetweenNotes = 0.5f;
    public int howManyInstancesOfTheNoteToPlayAtOnce = 1;

    public int[] correctNoteSequence;
    private int sequenceIndex = 0;

    private bool gameFinished = false;
    private bool canPlayNotes = false;
    private bool gameStarted = false;


    // Start is called before the first frame update
    void Start()
    {
        ClampSequence();
        OnWellNoteScanned = () =>
        {
            if (gameStarted && !gameFinished)
            {
                RestartSequence();
                StartCoroutine(PlaySampleSongCoroutine());
            }
        };
    }

    void BeginPuzzle()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            Audio.PlaySFX(WellSensorSound);
            UDP.Write(UDP_ToSend_CompleteWell);
            this.ActionAfterSecondDelay(1f, () =>
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

    float[] lastTimeUserInputNoteWasPressed = null;
    void PlayNote(int index, bool isUserInput = false)
    {
        if (!canPlayNotes)
            return;

        if (index >= 0 && index < Notes.Length)
        {
            if (isUserInput)
            {
                lastTimeUserInputNoteWasPressed = lastTimeUserInputNoteWasPressed ?? new float[Notes.Length]; 
                float timePassedSinceLAstNotePlayedByUser = Time.time - lastTimeUserInputNoteWasPressed[index];
                if (timePassedSinceLAstNotePlayedByUser < cooldownBetweenUserInputedNotes) { return; }
                else { lastTimeUserInputNoteWasPressed[index] = Time.time; }
            }
            PlayNoteAudioClip(index);
        }

        if (gameFinished)
            return;


        if (index == correctNoteSequence[sequenceIndex])
        {
            sequenceIndex++;
            if (sequenceIndex >= correctNoteSequence.Length)
            {
                Win();
            }
        }
        else
        {
            StartCoroutine(IncorrectNoteCoroutine());
        }
    }

    public void Win()
    {
        //WIN GAME
        gameFinished = true;
        UDP.Write(UDP_ToSend_CompleteSoundPuzzle);
        UDP.Write(UDP_ToSend_CompleteSoundPuzzle, GAME.MAGIC_MIRROR_IP, GAME.MAGIC_MIRROR_PORT);
        UDP.Write(UDP_ToSend_CompleteSoundPuzzle, GAME.WELL_IP, GAME.WELL_PORT);
        GAME.MuteMusicVolumeTemporally(this, GAME.VideoDuration_PZ08);
        Debug.Log("Sound puzzle completed.");
        OnPuzzleComplete();
    }

    IEnumerator IncorrectNoteCoroutine()
    {
        canPlayNotes = false;
        RestartSequence();
        Audio.PlaySFX(IncorrectNoteSound);
        yield return new WaitForSeconds(0.1f); // you can make this function a void function and use this.ActionAfterSecondDelay( 0.1f, ()+>{ here what to do after 0.1 seconds; } ); ( so you dont have to use StartCoroutine )
        Audio.PlaySFX(IncorrectNoteSound);
        canPlayNotes = true;
        //StartCoroutine(PlaySampleSongCoroutine()); // commented so no sequence repeated on fail. only the well shows you the sequence
    }

    void PlayNoteAudioClip(int index)
    {
        if (index >= 0 && index < Notes.Length)
        {
            howManyInstancesOfTheNoteToPlayAtOnce = Mathf.Clamp(howManyInstancesOfTheNoteToPlayAtOnce, 1, int.MaxValue);
            for (int i = 0; i < howManyInstancesOfTheNoteToPlayAtOnce; i++) { Audio.PlaySFX(Notes[index], howManyInstancesOfTheNoteToPlayAtOnce == 1); }
        }
    }

    bool playingSampleSong = false;
    IEnumerator PlaySampleSongCoroutine()
    {
        if (!playingSampleSong)
        {
            playingSampleSong = true;
            canPlayNotes = false;
            yield return new WaitForSeconds(1.5f);
            int n = 0;
            while (n < correctNoteSequence.Length)
            {
                PlayNoteAudioClip(correctNoteSequence[n]);
                n++;
                yield return new WaitForSeconds(TimeBetweenNotes);
            }
            canPlayNotes = true;
            yield return null;
            playingSampleSong = false;
        }
    }

    int NumberOfStartNotesAlreadyTrue()
    {
        int count = 0;
        for (int i = 0; i < UDP_ToReceive_StartSoundPuzzle.Length; i++)
        {
            if (startPuzzleRequiredNotes[i]) count++;
        }

        return count;
    }

    public void UDP_MessageReceived(string command)
    {
        if (command != null && command.Length > 0)
        {

            if (UDP_ToReceive_StartSoundPuzzle != null && UDP_ToReceive_StartSoundPuzzle.Length > 0)
            {
                startPuzzleRequiredNotes = startPuzzleRequiredNotes ?? new bool[UDP_ToReceive_StartSoundPuzzle.Length];

                for (int i = 0; i < UDP_ToReceive_StartSoundPuzzle.Length; i++)
                {
                    if (UDP_ToReceive_StartSoundPuzzle[i] != null && command.ToUpper() == UDP_ToReceive_StartSoundPuzzle[i].ToUpper())
                    {
                        startPuzzleRequiredNotes[i] = true;
                        OnWellNoteScanned();
                    }
                }


                if (NumberOfStartNotesAlreadyTrue() > 0 && !gameStarted) { for (int i = 0; i < howManyInstancesOfTheNoteToPlayAtOnce; i++) { Audio.PlaySFX(ActivationNotes[NumberOfStartNotesAlreadyTrue() - 1], howManyInstancesOfTheNoteToPlayAtOnce == 1); } }
                bool allTrue = NumberOfStartNotesAlreadyTrue() == startPuzzleRequiredNotes.Length;
                if (!gameStarted && allTrue)
                {
                    BeginPuzzle();
                }

            }

            if (command.ToLower() == UDP_ToReceive_Skip_CompleteSoundPuzzle.ToLower())
            {
                Win();
            }

            bool matchesNoteScan = command.Length >= UDP_ToReceive_NoteScan.Length && command.Substring(0, UDP_ToReceive_NoteScan.Length).ToUpper() == UDP_ToReceive_NoteScan.ToUpper();
            if (matchesNoteScan)
            {
                string toParse = command.Substring(UDP_ToReceive_NoteScan.Length);
                string[] pieces = toParse.Split('_');
                int n;
                if (int.TryParse(pieces[0], out n) && --n >= 0)
                {
                    PlayNote(n, true);
                }
            }
        }
    }


}
