using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WellManager : MonoBehaviour
{

    public CanvasGroup WellSpellCover;


    public void ShowParticlesInWell() {
        StartCoroutine(ShowParticlesInWellCoroutine());
    }

    IEnumerator ShowParticlesInWellCoroutine() {
        float startTime = Time.time;
        float duration = 4.0f;

        while (Time.time - startTime < duration) {
            WellSpellCover.alpha = Mathf.Lerp(1.0f, 0.0f, (Time.time - startTime) / duration);
            yield return null;
        }

        yield return null;
    }

}
