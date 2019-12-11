using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Spells {
    Fireball
}

public class SpellComboManager : MonoBehaviour {

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
    [SerializeField] private SpellParticleSystem LightningFireball;
    [SerializeField] private SpellParticleSystem FireTornado;
    [SerializeField] private SpellParticleSystem LightningTornado;
    [SerializeField] private SpellParticleSystem FireLightningTornado;

    [Header("Other Spells")]
    [SerializeField] private SpellParticleSystem Cyclone;
    [SerializeField] private SpellParticleSystem Snowstorm;
    [SerializeField] private SpellParticleSystem Confetti;


    [Header("All Systems")]
    [SerializeField] private SpellParticleSystem[] AllSpellSystems;




    bool[] pegBooleans;



    public void Reset() {
        foreach (SpellParticleSystem spell in AllSpellSystems) {
            if (spell != null) {
                spell.gameObject.SetActive(true);
                spell.EnableSystems(false, false);
            }
        }

        FireLightningTornado.OnEnabled = CompleteFinalSpell;
    }

    // Update is called once per frame
    void Update() {
        ReceiveInputs();
    }

    void ReceiveInputs() {
        for (int i = 0; i < AllSpellSystems.Length; i++) {
            if (Input.GetKeyDown(AllSpellSystems[i].DebugKey)) {
                ToggleSpell(AllSpellSystems[i]);
            }
        }
    }

    public void ThrowProjectileAtPosition(SpellParticleSystem projectile, Vector3 targetPosition, float duration, bool destroyOnArrive = true) {
        StartCoroutine(ThrowProjectileAtPositionCoroutine(projectile, targetPosition, duration, destroyOnArrive));
    }

    IEnumerator ThrowProjectileAtPositionCoroutine(SpellParticleSystem projectile, Vector3 targetPosition, float duration, bool destroyOnArrive) {
        float startTime = Time.time;
        Vector3 startPos = projectile.transform.position;

        while (Time.time - startTime < duration) {
            projectile.transform.position = Vector3.Lerp(startPos, targetPosition, (Time.time - startTime) / duration);
            yield return null;
        }
        projectile.EnableSystems(!destroyOnArrive);

        yield return null;
    }


    public void ToggleSpell(SpellParticleSystem spell) {
        if (FinalComboSpellIsFormed())
            return;
        if (SpellScanOnCooldown)
            return;
        if (spell.SpellScanOnCooldown)
            return;

        if (spell.SpellShape == SpellShape.Ball) {
            if (LightningFireball.SystemsAreEnabled)
                return;
            spell.transform.position = (ballsActive == 1 && !spell.SystemsAreEnabled ? PrimaryProjectilePosition + SecondaryProjectileOffset : PrimaryProjectilePosition);
        }
        spell.EnableSystems(!spell.SystemsAreEnabled);
        if (spell.SystemsAreEnabled) {
            spell.WaitForCooldown();
            WaitForGlobalCooldown();
        }
        if (spell == Fireball) { spell.ActionsOnAllParticles((p) => { p.Clear(); }); }
        if (spell.CanCombine) { StartCoroutine(MixSpellsCoroutine()); }
        
    }


    private int ballsActive {
        get
        {
            return SumBools(Fireball.SystemsAreEnabled, Lightningball.SystemsAreEnabled, LightningFireball.SystemsAreEnabled);
        }
    }
    private int tornadosActive {
        get
        {
            return SumBools(Tornado.SystemsAreEnabled, FireTornado.SystemsAreEnabled, LightningTornado.SystemsAreEnabled);
        }
    }

    int SumBools(params bool[] bools) {
        int n = 0;
        foreach (bool b in bools) {
            n += b ? 1 : 0;
        }
        return n;
    }

    IEnumerator MixSpellsCoroutine() {
        if (!FinalComboSpellIsFormed()) {
            if (Fireball.SystemsAreEnabled && Lightningball.SystemsAreEnabled) {
                Fireball.canBeDisabled = Lightningball.canBeDisabled = false;                
                //Merge to combo ball
                SpellParticleSystem mixedSpellToActivate = GetMixedParticleSystem();
                if (mixedSpellToActivate != null) {
                    yield return new WaitForSeconds(1.5f);
                    ThrowProjectileAtPosition(Fireball, PrimaryProjectilePosition, ProjectileThrowDuration);
                    ThrowProjectileAtPosition(Lightningball, PrimaryProjectilePosition, ProjectileThrowDuration);
                    yield return new WaitForSeconds(ProjectileThrowDuration);
                    DisableAllSpellsExcept(mixedSpellToActivate);
                    mixedSpellToActivate.transform.position = PrimaryProjectilePosition;
                    mixedSpellToActivate.EnableSystems(true);
                }
            }

            yield return null;
            if (tornadosActive >= 1 && ballsActive >= 1) {
                GetActiveTornadoSpell().canBeDisabled = false;
                GetActiveBallSpell().canBeDisabled = false;
                yield return new WaitForSeconds(1.5f);
                //move ball to secondary position
                ThrowProjectileAtPosition(GetActiveBallSpell(), PrimaryProjectilePosition + SecondaryProjectileOffset, ProjectileThrowDuration, false);
                yield return new WaitForSeconds(ProjectileThrowDuration);
                yield return new WaitForSeconds(0.5f);
                //throw ball at tornado base
                SpellParticleSystem mixedSpellToActivate = GetMixedParticleSystem();
                mixedSpellToActivate.canBeDisabled = false;
                ThrowProjectileAtPosition(GetActiveBallSpell(), Vector3.zero, ProjectileThrowDuration);
                yield return new WaitForSeconds(ProjectileThrowDuration);
                DisableAllSpellsExcept(mixedSpellToActivate);
                mixedSpellToActivate.EnableSystems(true);
            }


        }
        yield return null;
    }

    SpellParticleSystem GetMixedParticleSystem() {
        if (Fireball.SystemsAreEnabled && Lightningball.SystemsAreEnabled) {
            return LightningFireball;
        }

        if (GetActiveTornadoSpell() == Tornado) {
            if (GetActiveBallSpell() == Fireball) {
                return FireTornado;
            } else if (GetActiveBallSpell() == Lightningball) {
                return LightningTornado;
            } else if (GetActiveBallSpell() == LightningFireball) {
                return FireLightningTornado;
            }
        } else if (GetActiveTornadoSpell() == FireTornado && GetActiveBallSpell() == Lightningball) {
            return FireLightningTornado;
        } else if (GetActiveTornadoSpell() == LightningTornado && GetActiveBallSpell() == Fireball) {
            return FireLightningTornado;
        }


        return null;
    }

    void DisableAllSpellsExcept(SpellParticleSystem spellToExcept) {
        foreach (SpellParticleSystem spell in AllSpellSystems) {
            if (spell != spellToExcept) {
                spell.EnableSystems(false, false);
            }
        }
    }

    SpellParticleSystem GetActiveBallSpell() {
        return Fireball.SystemsAreEnabled ? Fireball : Lightningball.SystemsAreEnabled ? Lightningball : LightningFireball.SystemsAreEnabled ? LightningFireball : null;
    }

    SpellParticleSystem GetActiveTornadoSpell() {
        return Tornado.SystemsAreEnabled ? Tornado : LightningTornado.SystemsAreEnabled ? LightningTornado : FireTornado.SystemsAreEnabled ? FireTornado : FireLightningTornado.SystemsAreEnabled ? FireLightningTornado : null;
    }

    bool FinalComboSpellIsFormed() {
        return FireLightningTornado.SystemsAreEnabled;
    }


    bool WaitForGlobalCooldown(bool goToCooldownOnCheck = true) {        
        if (SpellScanOnCooldown) {
            return true;
        } else if (goToCooldownOnCheck) {
            StartCoroutine(SpellScanCooldownCoroutine());
            return false;
        } else {
            return false;
        }
    }


    IEnumerator SpellScanCooldownCoroutine() {
        if (!SpellScanOnCooldown) {
            SpellScanOnCooldown = true;
            yield return new WaitForSeconds(GlobalSpellCooldown);
            SpellScanOnCooldown = false;
            yield return null;
        }
    }

    void CompleteFinalSpell() {
        OnFinalSpellSolved();
        UDP.Write(UDP_ToSend_CompleteFinalSpell);
    }


    public void UDP_MessageReceived(string command) {
        if (command != null && command.Length > 0) {
            bool matchesSpellPeg = command.Substring(0, UDP_ToReceive_SpellPegs.Length).ToUpper() == UDP_ToReceive_SpellPegs.ToUpper();
            if (matchesSpellPeg) {
                string toParse = command.Substring(UDP_ToReceive_SpellPegs.Length);
                string[] pieces = toParse.Split('_');
                pegBooleans = ConvertBinaryStringArrayToBoolArray(pieces);
                CheckSpellPegs();
            }
        }
    }

    void CheckSpellPegs() {
        if (PegsAreActive(1, 5, 3, 4) || PegsAreActive(8, 12, 10, 11) || PegsAreActive(15, 19, 17, 18)) {
            //CONFETTI
            ToggleSpell(Confetti);
        }
        if (PegsAreActive(6, 4, 5, 2) || PegsAreActive(13, 9, 12, 14) || PegsAreActive(20, 16, 19, 21)) {
            //CYCLONE
            ToggleSpell(Cyclone);
        }
        if (PegsAreActive(6, 5, 3, 2) || PegsAreActive(13, 12, 10, 14) || PegsAreActive(20, 19, 17, 21)) {
            //SNOWSTORM
            ToggleSpell(Snowstorm);
        }

        
        
        if (PegsAreActive(6, 7, 5, 4) || PegsAreActive(13, 14, 12, 11) || PegsAreActive(20, 21, 19, 18)) {
            //TORNADO
            ToggleSpell(Tornado);
        }
        if (PegsAreActive(1, 6, 3, 4) || PegsAreActive(8, 13, 10, 11) || PegsAreActive(15, 20, 17, 18)) {
            //LIGHTNING
            ToggleSpell(Lightningball);
        }
        if (PegsAreActive(1, 6, 3, 5) || PegsAreActive(8, 13, 10, 12) || PegsAreActive(15, 20, 17, 19)) {
            //FIREBALL
            ToggleSpell(Fireball);
        }
    }

    bool PegsAreActive(int a, int b, int c, int d) {
        return PegIsActive(a) && PegIsActive(b) && PegIsActive(c) && PegIsActive(d);
    }

    bool PegIsActive(int index) {
        bool[] array = pegBooleans;
        if (index <= 0 || index > array.Length) {
            return false;
        }
        return array[index - 1];
    }

    bool[] ConvertBinaryStringArrayToBoolArray(string[] stringArray) {
        bool[] boolArray = new bool[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++) {
            int n;
            boolArray[i] = int.TryParse(stringArray[i], out n) && n == 1;
        }
        return boolArray;
    }

}
