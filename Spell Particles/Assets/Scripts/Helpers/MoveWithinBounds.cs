using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithinBounds : MonoBehaviour
{
    public Vector3 MaxBounds = new Vector3(2f, 2f, 0f);
    public Vector3 MinBounds = new Vector3(-2f, -2f, 0f);    
    public float MoveDuration = 0.5f;
    public bool RandomMoveDuration = false;
    public float RestDuration = 0.0f;
    public bool DoMove = true;
    
    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (RandomMoveDuration) {
            RandomizeMoveDuration();
        }
        startPosition = transform.position;
        StartCoroutine(MoveWithinBoundsCoroutine());
    }

    private void Update() {
        ClampBounds();
    }

    void RandomizeMoveDuration() {
        float n = MoveDuration;
        MoveDuration = Random.Range(n * 0.9f, n * 1.1f);
    }

    void ClampBounds() {
        MaxBounds.x = Mathf.Max(MinBounds.x, MaxBounds.x);
        MaxBounds.y = Mathf.Max(MinBounds.y, MaxBounds.y);
        MaxBounds.z = Mathf.Max(MinBounds.z, MaxBounds.z);
    }

    Vector3 GetNewTargetPosition() {
        float xRand = Random.Range(MinBounds.x, MaxBounds.x) + startPosition.x;
        float yRand = Random.Range(MinBounds.y, MaxBounds.y) + startPosition.y;
        float zRand = Random.Range(MinBounds.z, MaxBounds.z) + startPosition.z;

        return new Vector3(xRand, yRand, zRand);
    }

    IEnumerator MoveWithinBoundsCoroutine() {
        while (true) {
            while (DoMove) {
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = GetNewTargetPosition();
                float startTime = Time.time;
                float duration = MoveDuration;

                while (Time.time - startTime < duration) {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, (Time.time - startTime) / duration));
                    yield return null;
                }
                yield return new WaitForSeconds(RestDuration);
                yield return null;
            }
            yield return null;
        }
    }
}
