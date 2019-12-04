using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlickerFire : MonoBehaviour {

    [Range(0f,1f)]
    public float MinIntensity = 0.25f;
    [Range(0f, 1f)]
    public float MaxIntensity = 1f;
    [Range(0.001f, 1.5f)]
    public float UpdatePeriod = 0.1f;

    private Light lightSource;
    private float baseIntensity;
    private bool flickering;
    private float targetIntensity;

    public void Start() {
        lightSource = GetComponent<Light>();
        if (lightSource == null) {
            Debug.LogError("Flicker script must have a Light Component on the same GameObject.");
            return;
        }
        baseIntensity = lightSource.intensity;
        StartCoroutine(DoFlicker());
    }


    private IEnumerator DoFlicker() {
        flickering = true;

        while (true) {
            targetIntensity = Random.Range(MinIntensity, MaxIntensity) * baseIntensity;
            float currentIntensity = lightSource.intensity;
            float startTime = Time.time;
            float duration = UpdatePeriod;
            while (Time.time - startTime < duration) {
                lightSource.intensity = Mathf.Lerp(currentIntensity, targetIntensity, (Time.time - startTime) / duration);
                yield return null;
            }
            lightSource.intensity = targetIntensity;
            yield return null;
        }

        
    }
}


