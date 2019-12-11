using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SparkEffect_UI : MonoBehaviour
{
    [Header("Settings")]
    public int maxSparkCount = 20;
    public float minSparkSize = 10;
    public float maxSparkSize = 60;
    public float particleLifetime = 1;

    [Header("Components")]
    public RectTransform folder;
    public Image originalSpark;

    Image[] sparks = null;

    public void Setup()
    {
        originalSpark.gameObject.SetActive(false);
        if (sparks == null)
        {
            sparks = new Image[maxSparkCount];
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i] = Instantiate(originalSpark);
                sparks[i].gameObject.SetActive(true);
                sparks[i].transform.SetParent(folder, false);
            }
            Hide();
        }
    }

    public void Hide()
    {
        StopAllCoroutines();
        Setup();
        for (int i = 0; i < sparks.Length; i++)
        {
            sparks[i].Transparency(0);
            sparks[i].gameObject.SetActive(false);
        }
    }

    public void Show(float entryDuration, float duration)
    {
        StopAllCoroutines();
        //Hide();
        Setup();

        float[] initialSpawnDelay = new float[maxSparkCount];
        float[] lastTimeSpawened = new float[maxSparkCount];
        for (int i = 0; i < initialSpawnDelay.Length; initialSpawnDelay[i++] = Random.Range(0, entryDuration));
        float time = Time.time;
        this.InterpolateCoroutine(duration, (n) => 
        {
            float timePassed = Time.time - time;
            for (int a = 0; a < initialSpawnDelay.Length; a++)
            {
                int i = a;
                bool hasSpawned = sparks[i].gameObject.activeInHierarchy;
                bool needsToSpawn = !hasSpawned && timePassed >= initialSpawnDelay[i];
                bool hasExpired = hasSpawned && sparks[i].color.a <= 0;
                bool hasTimeToBeReborn = timePassed < duration && duration - timePassed > particleLifetime;
                if (hasTimeToBeReborn && (needsToSpawn || hasExpired))
                {
                    sparks[i].gameObject.SetActive(true);
                    sparks[i].rectTransform.anchoredPosition = new Vector2(Random.Range(-folder.rect.width * 0.5f, folder.rect.width * 0.5f), Random.Range(-folder.rect.height * 0.5f, folder.rect.height * 0.5f));
                    sparks[i].Transparency(1);
                    sparks[i].rectTransform.sizeDelta = Vector2.one * Random.Range(minSparkSize, maxSparkSize);
                    lastTimeSpawened[i] = Time.time;
                }
                else if (hasSpawned)
                {
                    float life = Mathf.Clamp(Time.time - lastTimeSpawened[i],0, particleLifetime) / particleLifetime;
                    sparks[i].Transparency(1 - life);
                }
            }
        }, Hide);


    }

}
