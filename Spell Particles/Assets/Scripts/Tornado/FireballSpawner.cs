using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballSpawner : MonoBehaviour
{
    public GameObject FireballPrefab;

    [Header("Spawn Settings")]
    public float SpawnInterval = 1f;
    public float FireballLifetime = 9f;
    public bool SetSpawnerAsParent = false;

    [Header("Double Spawn")]
    public bool DoubleSpawn = false;
    public bool InvertSecond = false;
    public float DoubleSpawnOffset = 0.25f;


    [Header("Tornado Curve")]
    public float TornadoCurveMagnitude = 1f;
    public float TornadoCurveFrequency = 1f;
    public bool AssignChildrenToCurve = true;

    [Header("Other Settings")]
    public KeyCode SpawnKey = KeyCode.Space;
    public bool DoSpawn = true;

    float defaultMagnitude = 1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnFireballsCoroutine());
        defaultMagnitude = TornadoCurveMagnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(SpawnKey)) {
            SpawnFireball();
        }

        TornadoCurveMagnitude = defaultMagnitude * Mathf.Sin(Time.time);
    }

    void SpawnFireball(bool invertSpiral = false) {
        GameObject newFireball = Instantiate(FireballPrefab, transform.position, Quaternion.identity);
        
        if (SetSpawnerAsParent) newFireball.transform.SetParent(transform);

        if (newFireball.GetComponent<MoveInSpiral>() != null) {
            MoveInSpiral spiral = newFireball.GetComponent<MoveInSpiral>();            
            spiral.Invert180 = invertSpiral;
            if (AssignChildrenToCurve) spiral.tornado = this;
        }

        if (FireballLifetime > 0f) {
            StartCoroutine(StopEmissionAfterLifetimeCoroutine(newFireball, FireballLifetime));
        }
    }

    IEnumerator SpawnFireballsCoroutine() {
        while (true) {
            while (DoSpawn) {
                SpawnFireball();
                if (DoubleSpawn) {
                    yield return new WaitForSeconds(DoubleSpawnOffset);
                    SpawnFireball(InvertSecond);
                    yield return new WaitForSeconds(Mathf.Max(SpawnInterval - DoubleSpawnOffset, 0f));
                } else {
                    yield return new WaitForSeconds(SpawnInterval);
                }

                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator StopEmissionAfterLifetimeCoroutine(GameObject fireball, float delay) {
        ParticleSystem ps = (fireball.GetComponent<ParticleSystem>() != null ? fireball.GetComponent<ParticleSystem>() : null);
        if (ps != null) {            
            yield return new WaitForSeconds(delay);
            ps.Stop(true);        
            yield return new WaitForSeconds(10f);
            Destroy(fireball);
        } else {
            Destroy(fireball, delay);
        }

        yield return null;
    }

}
