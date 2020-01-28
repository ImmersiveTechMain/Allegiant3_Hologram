using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpellComboManager : MonoBehaviour
{

    public delegate void CALLBACK();
    public CALLBACK OnFinalSpellSolved = delegate () { };

    [Header("UDP")]
    //public string UDP_ToReceive_OnScannedFireball = "SPELL_FIREBALL";
    //public string UDP_ToReceive_OnScannedLightning = "SPELL_LIGHTNING";
    //public string UDP_ToReceive_OnScannedTornado = "SPELL_TORNADO";
    //public string UDP_ToReceive_OnScannedCyclone = "SPELL_CYCLONE";
    //public string UDP_ToReceive_OnScannedSnowstorm = "SPELL_SNOWSTORM";
    //public string UDP_ToReceive_OnScannedConfetti = "SPELL_CONFETTI";
    public string UDP_ToReceive_SpellPegs = "SPELL_";
    public string UDP_ToSend_CompleteFinalSpell = "FINAL_SPELL_COMPLETE";
    public string UDP_ToSend_CompleteFinalCyclone = "SOLVED_PZ09";

    [Header("Sound Effects")]
    public AudioClip SFX_SpellCombined;
    public AudioClip SFX_FinalSpellCombined;
    public AudioClip SFX_WinSpellPuzzle;
    public AudioClip SFX_ThrowProjectile;
    public AudioClip SFX_ProjectileImpact;

    [Header("Settings")]
    public Vector3 PrimaryProjectilePosition = new Vector3(0f, 11f, -20f);
    public Vector3 SecondaryProjectileOffset = new Vector3(4f, 0f, 0f);
    public float ProjectileThrowDuration = 1f;

    public bool SpellScanOnCooldown = false;
    public float GlobalSpellCooldown = 1.5f;

    [Header("Base Spells")]
    [SerializeField] private SpellParticleSystem Fireball;
    [SerializeField] private SpellParticleSystem Lightningball;
    [SerializeField] private SpellParticleSystem Tornado;

    [Header("Combo Spells")]
    //[SerializeField] private SpellParticleSystem LightningFireball;
    [SerializeField] private SpellParticleSystem FireTornado;
    //[SerializeField] private SpellParticleSystem LightningTornado;
    //[SerializeField] private SpellParticleSystem FireLightningTornado;

    [Header("Other Spells")]
    [SerializeField] private SpellParticleSystem Cyclone;
    [SerializeField] private SpellParticleSystem Snowstorm;
    [SerializeField] private SpellParticleSystem Confetti;


    [Header("All Systems")]
    [SerializeField] private SpellParticleSystem[] AllSpellSystems;




    bool[] pegBooleans = new bool[21]; // any number so it is not null



    public void Reset()
    {
        foreach (SpellParticleSystem spell in AllSpellSystems)
        {
            if (spell != null)
            {
                spell.gameObject.SetActive(true);
                spell.EnableSystems(false, false);
            }
        }

        FireTornado.OnEnabled = CompleteFinalSpell;
        Cyclone.OnEnabled = AttemptCompleteCyclonePuzzle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 5 == 0 && pegBooleans != null) { CheckSpellPegs(); }
        ReceiveInputs();
    }

    void ReceiveInputs()
    {
        for (int i = 0; i < AllSpellSystems.Length; i++)
        {
            if (Input.GetKeyDown(AllSpellSystems[i].DebugKey))
            {
                ToggleSpell(AllSpellSystems[i]);
            }
        }
    }

    public void ThrowProjectileAtPosition(SpellParticleSystem projectile, Vector3 targetPosition, float duration, bool destroyOnArrive = true)
    {
        StartCoroutine(ThrowProjectileAtPositionCoroutine(projectile, targetPosition, duration, destroyOnArrive));
    }

    IEnumerator ThrowProjectileAtPositionCoroutine(SpellParticleSystem projectile, Vector3 targetPosition, float duration, bool destroyOnArrive)
    {
        float startTime = Time.time;
        Vector3 startPos = projectile.transform.position;

        while (Time.time - startTime < duration)
        {
            projectile.transform.position = Vector3.Lerp(startPos, targetPosition, (Time.time - startTime) / duration);
            yield return null;
        }
        projectile.EnableSystems(!destroyOnArrive);

        yield return null;
    }


    public void ToggleSpell(SpellParticleSystem spell)
    {
        if (FinalComboSpellIsFormed())
            return;
        if (SpellScanOnCooldown)
            return;
        if (spell.SpellScanOnCooldown)
            return;

        if (spell == Fireball && (FireTornado.SystemsAreEnabled))// || LightningFireball.SystemsAreEnabled))
            return;
        //if (spell == Lightningball && (LightningTornado.SystemsAreEnabled || LightningFireball.SystemsAreEnabled))
        //    return;
        if (spell == Tornado && (FireTornado.SystemsAreEnabled))// || LightningTornado.SystemsAreEnabled))
            return;

        if (spell.SpellShape == SpellShape.Ball)
        {
            //if (LightningFireball.SystemsAreEnabled)
            //    return;
            spell.transform.position = (ballsActive == 1 && !spell.SystemsAreEnabled ? PrimaryProjectilePosition + SecondaryProjectileOffset : PrimaryProjectilePosition);
        }
        spell.EnableSystems(!spell.SystemsAreEnabled);
        Audio.PlaySFX(spell.SFX_Activation);
        if (spell.SystemsAreEnabled)
        {
            spell.WaitForCooldown();
            WaitForGlobalCooldown();
        }
        if (spell == Fireball) { spell.ActionsOnAllParticles((p) => { p.Clear(); }); }
        if (spell.CanCombine)
        {
            StartCoroutine(MixSpellsCoroutine());
        }

    }


    private int ballsActive
    {
        get
        {
            return SumBools(Fireball.SystemsAreEnabled, Lightningball.SystemsAreEnabled);//, LightningFireball.SystemsAreEnabled);
        }
    }
    private int tornadosActive
    {
        get
        {
            return SumBools(Tornado.SystemsAreEnabled, FireTornado.SystemsAreEnabled);//, LightningTornado.SystemsAreEnabled);
        }
    }

    int SumBools(params bool[] bools)
    {
        int n = 0;
        foreach (bool b in bools)
        {
            n += b ? 1 : 0;
        }
        return n;
    }

    IEnumerator MixSpellsCoroutine()
    {
        if (!FinalComboSpellIsFormed())
        {
            //if (Fireball.SystemsAreEnabled && Lightningball.SystemsAreEnabled)
            //{
            //    Fireball.canBeDisabled = Lightningball.canBeDisabled = false;
            //    //Merge to combo ball
            //    SpellParticleSystem mixedSpellToActivate = GetMixedParticleSystem();
            //    if (mixedSpellToActivate != null)
            //    {
            //        yield return new WaitForSeconds(1.5f);
            //        ThrowProjectileAtPosition(Fireball, PrimaryProjectilePosition, ProjectileThrowDuration);
            //        ThrowProjectileAtPosition(Lightningball, PrimaryProjectilePosition, ProjectileThrowDuration);
            //        Audio.PlaySFX(SFX_ThrowProjectile);
            //        yield return new WaitForSeconds(ProjectileThrowDuration);
            //        Audio.PlaySFX(SFX_SpellCombined);
            //        Audio.PlaySFX(SFX_ProjectileImpact);
            //        DisableAllSpellsExcept(mixedSpellToActivate);
            //        mixedSpellToActivate.transform.position = PrimaryProjectilePosition;
            //        mixedSpellToActivate.EnableSystems(true);
            //    }
            //}

            yield return null;
            if (Tornado.SystemsAreEnabled && Fireball.SystemsAreEnabled)
            {
                Tornado.canBeDisabled = false;
                Fireball.canBeDisabled = false;
                yield return new WaitForSeconds(1.5f);
                //move ball to secondary position
                ThrowProjectileAtPosition(Fireball, PrimaryProjectilePosition + SecondaryProjectileOffset, ProjectileThrowDuration, false);
                yield return new WaitForSeconds(ProjectileThrowDuration);
                yield return new WaitForSeconds(0.5f);
                //throw ball at tornado base
                SpellParticleSystem mixedSpellToActivate = FireTornado;
                mixedSpellToActivate.canBeDisabled = false;
                ThrowProjectileAtPosition(GetActiveBallSpell(), Vector3.zero, ProjectileThrowDuration);
                Audio.PlaySFX(SFX_ThrowProjectile);
                yield return new WaitForSeconds(ProjectileThrowDuration);
                Audio.PlaySFX(SFX_SpellCombined);
                Audio.PlaySFX(SFX_ProjectileImpact);
                DisableAllSpellsExcept(mixedSpellToActivate);
                mixedSpellToActivate.EnableSystems(true);
            }


        }
        yield return null;
    }

    SpellParticleSystem GetMixedParticleSystem()
    {
        //if (Fireball.SystemsAreEnabled && Lightningball.SystemsAreEnabled)
        //{
        //    return LightningFireball;
        //}

        if (GetActiveTornadoSpell() == Tornado)
        {
            if (GetActiveBallSpell() == Fireball)
            {
                return FireTornado;
            }
            //else if (GetActiveBallSpell() == Lightningball)
            //{
            //    return LightningTornado;
            //}
            //else if (GetActiveBallSpell() == LightningFireball)
            //{
            //    return FireLightningTornado;
            //}
        }
        //else if (GetActiveTornadoSpell() == FireTornado && GetActiveBallSpell() == Lightningball)
        //{
        //    return FireLightningTornado;
        //}
        //else if (GetActiveTornadoSpell() == LightningTornado && GetActiveBallSpell() == Fireball)
        //{
        //    return FireLightningTornado;
        //}


        return null;
    }

    void DisableAllSpellsExcept(SpellParticleSystem spellToExcept)
    {
        foreach (SpellParticleSystem spell in AllSpellSystems)
        {
            if (spell != spellToExcept)
            {
                spell.EnableSystems(false, false);
            }
        }
    }

    SpellParticleSystem GetActiveBallSpell()
    {
        return Fireball.SystemsAreEnabled ? Fireball : Lightningball.SystemsAreEnabled ? Lightningball : null;// : LightningFireball.SystemsAreEnabled ? LightningFireball : null;
    }

    SpellParticleSystem GetActiveTornadoSpell()
    {
        return Tornado.SystemsAreEnabled ? Tornado : FireTornado.SystemsAreEnabled ? FireTornado : null;// FireLightningTornado.SystemsAreEnabled ? FireLightningTornado : LightningTornado.SystemsAreEnabled ? LightningTornado : null;
    }

    bool FinalComboSpellIsFormed()
    {
        return FireTornado.SystemsAreEnabled;
    }


    bool WaitForGlobalCooldown(bool goToCooldownOnCheck = true)
    {
        if (SpellScanOnCooldown)
        {
            return true;
        }
        else if (goToCooldownOnCheck)
        {
            StartCoroutine(SpellScanCooldownCoroutine());
            return false;
        }
        else
        {
            return false;
        }
    }


    IEnumerator SpellScanCooldownCoroutine()
    {
        if (!SpellScanOnCooldown)
        {
            SpellScanOnCooldown = true;
            yield return new WaitForSeconds(GlobalSpellCooldown);
            SpellScanOnCooldown = false;
            yield return null;
        }
    }

    void CompleteFinalSpell()
    {
        OnFinalSpellSolved();
        this.ActionAfterSecondDelay(0.5f, () =>
        {
            Audio.PlaySFX(SFX_FinalSpellCombined);
        });
        this.ActionAfterSecondDelay(3.60f, () =>
        {
            Audio.PlaySFX(SFX_WinSpellPuzzle);
            UDP.Write(UDP_ToSend_CompleteFinalSpell);
        });
        this.ActionAfterSecondDelay(11.00f, () =>
        {
            PutAllSpellsOnCooldown();
        });
    }

    public void PutAllSpellsOnCooldown()
    {
        foreach (SpellParticleSystem spell in AllSpellSystems)
        {
            spell.SpellScanOnCooldown = true;
        }
    }

    bool CanCompleteSuperCyclone = false;
    bool SuperCycloneHasBeenCompleted = false;
    public void AllowCycloneAsFinalSpell()
    {
        FireTornado.EnableSystems(false, false);
        Audio.PlaySFX(SFX_WinSpellPuzzle);
        Cyclone.SpellScanOnCooldown = false;
        CanCompleteSuperCyclone = true;
    }

    public void AttemptCompleteCyclonePuzzle()
    {
        if (CanCompleteSuperCyclone && !SuperCycloneHasBeenCompleted)
        {
            SuperCycloneHasBeenCompleted = true;
            Audio.PlaySFX(SFX_FinalSpellCombined);
            this.ActionAfterSecondDelay(3.1f, () =>
            {
                Audio.PlaySFX(SFX_WinSpellPuzzle);
            });
            UDP.Write(UDP_ToSend_CompleteFinalCyclone, GAME.MAGIC_MIRROR_IP, GAME.MAGIC_MIRROR_PORT);
            UDP.Write(UDP_ToSend_CompleteFinalCyclone);
            GAME.MuteMusicVolumeTemporally(this, GAME.VideoDuration_PZ09);
        }
    }


    public void UDP_MessageReceived(string command)
    {
        if (command != null && command.Length > 0)
        {
            bool matchesSpellPeg = command.Length >= UDP_ToReceive_SpellPegs.Length && command.Substring(0, UDP_ToReceive_SpellPegs.Length).ToUpper() == UDP_ToReceive_SpellPegs.ToUpper();
            if (matchesSpellPeg)
            {
                string toParse = command.Substring(UDP_ToReceive_SpellPegs.Length);
                string[] pieces = toParse.Split('_');
                pegBooleans = ConvertBinaryStringArrayToBoolArray(pieces);
                CheckSpellPegs();
            }

            if (command.ToLower() == UDP_ToSend_CompleteFinalCyclone.ToLower())
            {
                AttemptCompleteCyclonePuzzle();
            }
        }
    }

    bool overflow1 = false;
    bool overflow2 = false;
    bool overflow3 = false;

    public enum PegSector
    {
        One,
        Two,
        Three
    }

    void CheckSpellPegs()
    {
        if (SuperCycloneHasBeenCompleted) { return; }
        overflow1 = CountActivePegsInSector(PegSector.One) > 4;
        overflow2 = CountActivePegsInSector(PegSector.Two) > 4;
        overflow3 = CountActivePegsInSector(PegSector.Three) > 4;



        if (PegsAreActive(PegSector.One, 1, 5, 3, 4) || PegsAreActive(PegSector.Two, 8, 12, 10, 11) || PegsAreActive(PegSector.Three, 15, 19, 17, 18))
        {
            //CONFETTI
            ToggleSpell(Confetti);
        }
        if (PegsAreActive(PegSector.One, 7, 6, 5, 2) || PegsAreActive(PegSector.Two, 13, 9, 12, 14) || PegsAreActive(PegSector.Three, 20, 16, 19, 21))
        {
            //CYCLONE
            if (CanCompleteSuperCyclone)
            {
                ToggleSpell(Cyclone);
            }
        }
        if (PegsAreActive(PegSector.One, 7, 5, 4, 2) || PegsAreActive(PegSector.Two, 9, 12, 11, 14) || PegsAreActive(PegSector.Three, 16, 19, 18, 21))
        {
            //SNOWSTORM
            ToggleSpell(Snowstorm);
        }
        if (PegsAreActive(PegSector.One, 2, 7, 5, 3) || PegsAreActive(PegSector.Two, 9, 14, 12, 10) || PegsAreActive(PegSector.Three, 20, 21, 19, 17))
        {
            //TORNADO
            ToggleSpell(Tornado);
        }
        if (PegsAreActive(PegSector.One, 1, 7, 3, 4) || PegsAreActive(PegSector.Two, 8, 14, 10, 11) || PegsAreActive(PegSector.Three, 15, 21, 17, 18))
        {
            //LIGHTNING
            ToggleSpell(Lightningball);
        }
        if (PegsAreActive(PegSector.One, 1, 7, 4, 5) || PegsAreActive(PegSector.Two, 8, 14, 11, 12) || PegsAreActive(PegSector.Three, 15, 21, 18, 19))
        {
            //FIREBALL
            ToggleSpell(Fireball);
        }
    }

    int[] SortedInts(params int[] ints)
    {
        int[] intsToSort = ints;
        Array.Sort(intsToSort);
        return intsToSort;
    }

    int CountActivePegsInSector(PegSector sector)
    {
        int count = 0;
        int sectorLength = 7;
        for (int i = 0; i < sectorLength; i++)
        {
            if (pegBooleans[((int)sector) * sectorLength + i])
                count++;
        }

        return count;
    }



    bool PegsAreActive(PegSector sector, int a, int b, int c, int d)
    {
        //string numbers = a + ", " + b + ", " + c + ", " + d;
        bool pegsActive = PegIsActive(a) && PegIsActive(b) && PegIsActive(c) && PegIsActive(d);
        bool sectorOverflow;
        switch (sector)
        {
            case PegSector.One:
                sectorOverflow = overflow1;
                break;
            case PegSector.Two:
                sectorOverflow = overflow2;
                break;
            case PegSector.Three:
                sectorOverflow = overflow3;
                break;
            default:
                sectorOverflow = false;
                break;
        }


        if (sectorOverflow && pegsActive)
        {
            return false;
        }
        else
        {
            return pegsActive;
        }
    }

    bool PegIsActive(int index)
    {
        bool[] array = pegBooleans;
        if (index <= 0 || index > array.Length)
        {
            return false;
        }
        return array[index - 1];
    }

    bool[] ConvertBinaryStringArrayToBoolArray(string[] stringArray)
    {
        bool[] boolArray = new bool[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            int n;
            boolArray[i] = int.TryParse(stringArray[i], out n) && n == 1;
        }
        return boolArray;
    }

}
