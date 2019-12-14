using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public enum SpellShape {
    Ball,
    Tornado,
    Other
}

public class SpellParticleSystem : MonoBehaviour {
    public delegate void CALLBACK();
    public CALLBACK OnEnabled = delegate () { };


    [Header("Sub-Systems")]
    public LightningNetwork[] LightningNetworks;
    public FireballSpawner[] Spawners;
    public ParticleSystem[] ParticleSystems;
    public MeshRenderer[] Meshes;
    public VisualEffect[] VFXGraphs;    
    public Light[] Lights;

    [Header("Settings")]
    public SpellShape SpellShape = SpellShape.Other;
    public bool CanCombine = false;
    public bool SystemsAreEnabled = true;
    public float AutoDisableTime = 0f;
    public float SpellScanCooldown = 8f;
    public KeyCode DebugKey = KeyCode.None;
    public AudioClip SFX_Activation;

    [Header("Prefab Effects")]
    public GameObject[] BirthEffects;
    public GameObject[] DeathEffects;

    private bool _SystemsAreEnabled = true;
    internal bool canBeDisabled = true;
    internal bool SpellScanOnCooldown = false;

  

    private void Awake() {
        EnableSystems(SystemsAreEnabled);
    }

    void SpawnBirthEffect() {
        if (BirthEffects.Length > 0 && BirthEffects != null) {
            for (int i = 0; i < BirthEffects.Length; i++) {
                GameObject newObject = Instantiate(BirthEffects[i], transform);
                newObject.transform.localPosition = Vector3.zero;
                Destroy(newObject, 8f);
            }
        }
    }

    void SpawnDeathEffect() {
        if (DeathEffects.Length > 0 && DeathEffects != null) {
            for (int i = 0; i < DeathEffects.Length; i++) {
                GameObject newObject = Instantiate(DeathEffects[i], transform);
                newObject.transform.localPosition = Vector3.zero;
                Destroy(newObject, 8f);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (SystemsAreEnabled != _SystemsAreEnabled) {
            EnableSystems(SystemsAreEnabled);
        }
    }

    public void EnableSystems(bool state, bool spawnAdditional = true) {                
        SystemsAreEnabled = state;
        if (SystemsAreEnabled != _SystemsAreEnabled && spawnAdditional) {
            if (state) {
                SpawnBirthEffect();
            } else {
                SpawnDeathEffect();
            }
        }

        for (int n = 0; n < LightningNetworks.Length; n++) {
            LightningNetworks[n].DoRepeat = state;
            LightningNetworks[n].InterruptAllCoroutines();
        }
        for (int s = 0; s < Spawners.Length; s++) {
            Spawners[s].DoSpawn = state;
        }
        for (int p = 0; p < ParticleSystems.Length; p++) {
            ParticleSystem ps = ParticleSystems[p];
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = state;
            
            ParticleSystems[p] = ps;
            if (state) {
                ParticleSystems[p].time = 0f;
                ParticleSystems[p].Play(true);
            }
        }
        for (int v = 0; v < VFXGraphs.Length; v++) {
            VFXGraphs[v].resetSeedOnPlay = true;
            if (state == true) {                
                VFXGraphs[v].Play();
            } else {
                VFXGraphs[v].Stop();
            }            
        }
        for (int m = 0; m < Meshes.Length; m++) {
            Meshes[m].enabled = state;
        }
        for (int g = 0; g < Lights.Length; g++) {
            //Lights[g].gameObject.SetActive(state);
        }
       
        _SystemsAreEnabled = state;
        if (_SystemsAreEnabled) {
            OnEnabled();
        }
        if (_SystemsAreEnabled && AutoDisableTime > 0f) {
            if (localAutoResetCoroutine != null) { StopCoroutine(localAutoResetCoroutine); localAutoResetCoroutine = null; }
            localAutoResetCoroutine = StartCoroutine(AutoDisable(AutoDisableTime));
        }
    }

    Coroutine localAutoResetCoroutine = null;
    IEnumerator AutoDisable(float delay) {
        yield return new WaitForSeconds(delay);
        if (canBeDisabled) {
            EnableSystems(false);
        }
        yield return null;
    }

    public void ActionsOnAllParticles(System.Action<ParticleSystem> action) {

        for (int p = 0; p < ParticleSystems.Length; p++) {
            int i = p;
            ParticleSystem ps = ParticleSystems[i];
            if (action != null) { action(ps); }
        }
    }


    public bool WaitForCooldown() {
        if (SpellScanOnCooldown) {
            return true;
        } else {
            if (localCooldownCoroutine != null) { StopCoroutine(localCooldownCoroutine); localCooldownCoroutine = null; }
            localCooldownCoroutine = StartCoroutine(SpellScanCooldownCoroutine());
            return false;
        }
    }

    Coroutine localCooldownCoroutine = null;
    IEnumerator SpellScanCooldownCoroutine() {
        SpellScanOnCooldown = true;
        yield return new WaitForSeconds(SpellScanCooldown);

        SpellScanOnCooldown = false;
    }



}
