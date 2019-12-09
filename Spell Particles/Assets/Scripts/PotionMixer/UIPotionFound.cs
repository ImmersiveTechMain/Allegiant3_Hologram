using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPotionFound : MonoBehaviour
{
    public float entryDuration = 2;
    public float duration = 6;
    public float sizeModifier = 0.8f;
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI potionNameLabel;
    public Image icon;
    
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
    }

    public void Show(Potion potion)
    {
        StopAllCoroutines();
        potionNameLabel.text = potion.name;
        potionNameLabel.color = potion.identityColor;
        icon.overrideSprite = potion.image;

        this.InterpolateCoroutine(entryDuration, (n) => 
        {
            float N = n * n;
            potionNameLabel.Transparency(N);
            icon.Transparency(N);
            titleLabel.Transparency(N);
            potionNameLabel.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
            icon.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
            titleLabel.transform.localScale = Vector3.Lerp(Vector3.one * sizeModifier, Vector3.one, N);
        },()=> { this.ActionAfterSecondDelay(duration, Hide); });
    }
}
