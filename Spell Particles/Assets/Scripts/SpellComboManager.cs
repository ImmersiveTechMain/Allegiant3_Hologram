using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellComboManager : MonoBehaviour {
    [Header("Settings")]
    public Vector3 PrimaryProjectilePosition = new Vector3(0f, 11f, -20f);
    public Vector3 SecondaryProjectileOffset = new Vector3(4f, 0f, 0f);
    public float ProjectileThrowDuration = 1f;

    public static bool SpellScanOnCooldown = false;
    public float SpellScanCooldown = 3f;

    [Header("Base Spells")]
    public SpellParticleSystem Fireball;
    public SpellParticleSystem Lightningball;
    public SpellParticleSystem Tornado;

    [Header("Combo Spells")]
    public SpellParticleSystem LightningFireball;
    public SpellParticleSystem FireTornado;
    public SpellParticleSystem LightningTornado;
    public SpellParticleSystem FireLightningTornado;

    [Header("Other Spells")]
    public SpellParticleSystem Cyclone;
    public SpellParticleSystem Snowstorm;
    public SpellParticleSystem Confetti;

    [Header("All Systems")]
    public SpellParticleSystem[] AllSpellSystems;

    [Header("Components")]
    public CanvasGroup WellSpellCover;




    // Start is called before the first frame update
    void Start() {
        foreach (SpellParticleSystem spell in AllSpellSystems) {
            if (spell != null) {
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
        if (Input.GetKeyDown(KeyCode.F)) {
            ToggleFireball();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            ToggleLightningBall();
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            ToggleTornado();
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
            ToggleCyclone();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            ToggleSnowstorm();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            ToggleConfetti();
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

    public void ToggleFireball() {
        if (Fireball.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Fireball.transform.position = (ballsActive == 1 && !Fireball.SystemsAreEnabled ? PrimaryProjectilePosition + SecondaryProjectileOffset : PrimaryProjectilePosition);
        Fireball.EnableSystems(!Fireball.SystemsAreEnabled);
        Fireball.ActionsOnAllParticles((p) => { p.Clear(); });
        StartCoroutine(MixSpellsCoroutine());
    }

    public void ToggleLightningBall() {
        if (Lightningball.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Lightningball.transform.position = (ballsActive == 1 && !Lightningball.SystemsAreEnabled ? PrimaryProjectilePosition + SecondaryProjectileOffset : PrimaryProjectilePosition);
        Lightningball.EnableSystems(!Lightningball.SystemsAreEnabled);
        StartCoroutine(MixSpellsCoroutine());
    }

    public void ToggleTornado() {
        if (Tornado.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Tornado.EnableSystems(!Tornado.SystemsAreEnabled);
        StartCoroutine(MixSpellsCoroutine());
    }


    public void ToggleCyclone() {
        if (Cyclone.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Cyclone.EnableSystems(!Cyclone.SystemsAreEnabled);
    }

    public void ToggleSnowstorm() {
        if (Snowstorm.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Snowstorm.EnableSystems(!Snowstorm.SystemsAreEnabled);
    }

    public void ToggleConfetti() {
        if (Confetti.WaitForCooldown() || FinalComboSpellIsFormed())
            return;

        Confetti.EnableSystems(!Confetti.SystemsAreEnabled);
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
                yield return new WaitForSeconds(1.5f);
                //Merge to combo ball
                SpellParticleSystem mixedSpellToActivate = GetMixedParticleSystem();
                ThrowProjectileAtPosition(Fireball, PrimaryProjectilePosition, ProjectileThrowDuration);
                ThrowProjectileAtPosition(Lightningball, PrimaryProjectilePosition, ProjectileThrowDuration);
                yield return new WaitForSeconds(ProjectileThrowDuration);
                DisableAllSpellsExcept(mixedSpellToActivate);
                mixedSpellToActivate.transform.position = PrimaryProjectilePosition;
                mixedSpellToActivate.EnableSystems(true);
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


    bool WaitForGlobalCooldown() {
        if (SpellScanOnCooldown) {
            return true;
        } else {
            StartCoroutine(SpellScanCooldownCoroutine());
            return false;
        }
    }


    IEnumerator SpellScanCooldownCoroutine() {
        SpellScanOnCooldown = true;
        yield return new WaitForSeconds(SpellScanCooldown);

        SpellScanOnCooldown = false;
    }

    void CompleteFinalSpell() {
        StartCoroutine(CompleteFinalSpellCoroutine());
    }

    IEnumerator CompleteFinalSpellCoroutine() {
        float startTime = Time.time;
        float duration = 4.0f;

        while (Time.time - startTime < duration) {
            WellSpellCover.alpha = Mathf.Lerp(1.0f, 0.0f, (Time.time - startTime) / duration);
            yield return null;
        }

        yield return null;
    }

}
