using UnityEngine;
using System.Collections;

public class LightningBall : MonoBehaviour {

    [Range(1,10)]
    public int lightningCount = 3;

    [Range(0.1f, 10)]
    public float intensity = 1;
    [Range(1, 40)]
    public int lightningFrameLife = 10;
    public float range = 10f;

    [Range(2, 15)]
    public int lightningVertexCount = 5;

    public float maxPointOffset = 2f;

    Light luz;
    LineRenderer[] lr;
    LineRenderer originalLineRenderer;
    MeshRenderer rend;

    float energy = 100;
    int frameCounter = 0;
    int currentVertexCount = 0;

    Transform[] rotationRef;

	// Use this for initialization
	void Start () {
        luz = GetComponentInChildren<Light>();

        rotationRef = new Transform[lightningCount];
        lr = new LineRenderer[lightningCount];
        originalLineRenderer = GetComponent<LineRenderer>();
        for (int i = 0; i < lightningCount; i++)
        {
            rotationRef[i] = new GameObject("Direction").transform;
            lr[i] = rotationRef[i].gameObject.AddComponent<LineRenderer>();
            lr[i].material = originalLineRenderer.material;
            lr[i].startWidth = 0.2f;
            lr[i].endWidth = 0.0f;
            //lr[i].SetWidth(0.2f,0f);
        }

        rend = GetComponent<MeshRenderer>();
        ChangeLightningTargetPoint();

    }
	
    void ConnectLightWithEmmision()
    {
        float normalizedValue = energy / 100f;

        rend.material.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, normalizedValue));
        luz.intensity = intensity * normalizedValue;
    }

    void SpreadLightning()
    {

        for (int a = 0; a < lightningCount; a++)
        {
            for (int i = 1; i < currentVertexCount - 1; i++)
            {

                Vector3 nextPoint = rotationRef[a].transform.forward * range;
                nextPoint *= i;
                nextPoint /= (currentVertexCount);

                Vector3 offset = rotationRef[a].right * Random.Range(-1f, 1f);
                offset += rotationRef[a].up * Random.Range(-1f, 1f);

                offset.Normalize();
                offset *= Random.Range(0, maxPointOffset);

                nextPoint += offset;
                nextPoint += transform.position;

                nextPoint.y *= Mathf.Sign(nextPoint.y);


                lr[a].SetPosition(i, nextPoint);
            }
        }


        frameCounter++;

        if (frameCounter > lightningFrameLife)
        {
            ChangeLightningTargetPoint();
            frameCounter = 0;
        }

    }

    void ChangeLightningTargetPoint()
    {

        for (int i = 0; i < lightningCount; i++)
        {
            Vector3 nextPosition = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            rotationRef[i].Rotate(nextPosition);
            lr[i].SetVertexCount(lightningVertexCount);


            nextPosition = transform.position + rotationRef[i].transform.forward * range;
            nextPosition.y = Mathf.Clamp(nextPosition.y, 0, float.MaxValue);

            lr[i].SetPosition(lightningVertexCount - 1, nextPosition);
            lr[i].SetPosition(0, transform.position);
            currentVertexCount = lightningVertexCount;
        }
    }

	// Update is called once per frame
	void Update () {

        energy = Random.Range(0, 100f);
        SpreadLightning();
        ConnectLightWithEmmision();

    }
}
