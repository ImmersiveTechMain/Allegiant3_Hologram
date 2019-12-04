using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorStrobeLight : MonoBehaviour
{
    
    public Gradient ColorGradient;

    public float StrobeInterval = 11f;
    public float StrobeDuration = 10f;
    [Range(0.025f, 16f)]
    public float StrobeSpeed = 0.5f;


    private Light lightSource;
    private float startIntensity;

    // Start is called before the first frame update
    void Start()
    {
        lightSource = GetComponent<Light>();
        if (lightSource == null) {
            Debug.LogError("Flicker script must have a Light Component on the same GameObject.");
            return;
        }
        startIntensity = lightSource.intensity;
        StartCoroutine(StrobeCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        ChangeColor();
        StrobeDuration = Mathf.Min(StrobeInterval, StrobeDuration);
    }

    void ChangeColor() {
        float speed = StrobeSpeed;
        Color color = ColorGradient.Evaluate(Time.time * speed - Mathf.Floor(Time.time * speed));
        lightSource.color = color;
    }


    IEnumerator StrobeCoroutine() {        
        yield return null;
        lightSource.intensity = 0f;
        yield return new WaitForSeconds(StrobeInterval);
        while (true) {            
            lightSource.intensity = startIntensity;
            yield return new WaitForSeconds(StrobeDuration);
            lightSource.intensity = 0f;
            yield return new WaitForSeconds(StrobeInterval - StrobeDuration);
        }
    }

}
