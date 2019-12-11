using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPotionFound : MonoBehaviour
{
    public float entryDuration = 2;
    public float exitDuration = 1;
    public float duration = 6;
    public float sizeModifier = 0.8f;
    public float maxLightTransparency = 0.18f;
    public float maxBlackBorderTransparency = 0.58f;
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI potionNameLabel;
    public Image icon;
    public new Image light;
    public Image blackBorder;
    public SparkEffect_UI sparks;

    void Start()
    {
        Hide();
    }

    public void Hide()
    {
        StopAllCoroutines();
        titleLabel.Transparency(0);
        potionNameLabel.Transparency(0);
        icon.Transparency(0);
        light.Transparency(0);
        blackBorder.Transparency(0);
        sparks.Hide();
    }

    public void Show(Potion potion)
    {
        StopAllCoroutines();
        potionNameLabel.text = potion.name;
        potionNameLabel.color = potion.identityColor;
        icon.overrideSprite = potion.image;
        sparks.Show(entryDuration, duration);

        this.InterpolateCoroutine(entryDuration, (n) =>
        {
            float N = n * n;
            potionNameLabel.Transparency(N);
            icon.Transparency(N);
            light.Transparency(N * maxLightTransparency);
            blackBorder.Transparency(N * maxBlackBorderTransparency);
            titleLabel.Transparency(N);
            potionNameLabel.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
            icon.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
            titleLabel.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
            light.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);

        }, () =>
        {
            this.ActionAfterSecondDelay(duration, () =>
              this.InterpolateCoroutine(exitDuration, (n) =>
              {
                  // Exit Animation
                  float N = n * n;

                  potionNameLabel.Transparency(1 - N);
                  titleLabel.Transparency(1 - N);
                  icon.Transparency(1-N);
                  light.Transparency(maxLightTransparency * (1 - N));
                  blackBorder.Transparency(maxBlackBorderTransparency * (1 - N));

                  Vector3 titlesSizes = new Vector3(1 + 0.5f * N, 1 - 0.1f * N, 1);

                  potionNameLabel.transform.localScale = titlesSizes;
                  titleLabel.transform.localScale = titlesSizes;
                  //light.transform.localScale = Vector3.one + Vector3.one * 3 * N;
                  icon.transform.localScale = Vector3.one + Vector3.one * 0.2f * N;

              }, Hide));
        });
    }
}
