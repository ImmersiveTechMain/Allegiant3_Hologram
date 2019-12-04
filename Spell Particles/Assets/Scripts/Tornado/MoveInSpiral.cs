using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInSpiral : MonoBehaviour
{

    [Header("Spiral")]
    public float circleSpeedMin = 1f;
    public float circleSpeedMax = 3f;

    public float verticalSpeed = 1f;
    public float circleStartSize = 0.1f;
    public float circleEndSize = 2f;
    public float circleGrowDuration = 3f;

    [Header("Spiral Bend")]
    public float CurveMagnitude = 1f;
    public float CurveFrequency = 1f;

    [Header("Other Settings")]
    public bool Counterclockwise = false;
    public bool UseLocalPosition = false;

    internal bool Invert180 = false;

    float circleSpeed = 1f;
    float startTime = 0f;
    float circleSize = 0f;

    internal FireballSpawner tornado;
    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        startPosition = transform.position;
        circleSpeed = Mathf.Max(Random.Range(circleSpeedMin, circleSpeedMax), 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        circleSize = Mathf.Lerp(circleStartSize, circleEndSize, (Time.time - startTime) / circleGrowDuration);
        Vector3 pos = transform.position;
        pos.y += verticalSpeed * Time.deltaTime;
        float lifetimeElapsed = Time.time - startTime;

        float speedMultiplier = lifetimeElapsed * circleSpeed;
        float posMultiplier = circleSize * (Invert180 ? -1f : 1f);
        float freq = (tornado != null ? tornado.TornadoCurveFrequency : CurveFrequency);
        float mag = (tornado != null ? tornado.TornadoCurveMagnitude : CurveMagnitude);
        float x = (lifetimeElapsed / circleGrowDuration);
        float curveXOffset = Mathf.Sin(x * 2f * Mathf.PI * freq) * mag;

        pos.x = startPosition.x + curveXOffset + Mathf.Sin(speedMultiplier) * posMultiplier * (Counterclockwise ? -1f : 1f);
        pos.z = startPosition.z + Mathf.Cos(speedMultiplier) * posMultiplier;

        if (UseLocalPosition) {
            transform.localPosition = pos;
        } else {
            transform.position = pos;
        }
    }
}
