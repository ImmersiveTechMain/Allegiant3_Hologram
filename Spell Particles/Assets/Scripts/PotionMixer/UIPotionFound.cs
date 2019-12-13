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

    public KeyPotionLogo originalKeyPotionLogoHolder;

    KeyPotionLogo[] keyPotionLogos;

    [HideInInspector]
    [System.Serializable]
    public struct KeyPotionLogo
    {
        public RectTransform holder;
        public Image background;
        public Image logo;
        public Sprite hiddenLogo;
        public Text number;

        Color originalBackgroundColor;

        bool isShowingNumber { get { return number != null && number.gameObject.activeInHierarchy; } }

        public KeyPotionLogo(RectTransform holder, Sprite hiddenLogo, Transform parent)
        {
            this.holder = holder;
            this.holder.gameObject.SetActive(true);
            this.holder.transform.SetParent(parent, false);
            Image[] images = holder.GetComponentsInChildren<Image>();
            this.background = images[0];
            this.logo = images[1];
            this.number = holder.GetComponentInChildren<Text>();
            this.number.gameObject.SetActive(false);
            this.originalBackgroundColor = background.color;
            this.hiddenLogo = hiddenLogo;
        }

        public void SetNumber(int number)
        {
            if (!isShowingNumber)
            {
                this.logo.StopAllCoroutines();
                this.number.gameObject.SetActive(true);
                this.number.Transparency(0);
                this.number.text = number.ToString();

                RectTransform _holder = this.holder;
                Image _logo = this.logo;
                Image _bg = this.background;
                Text _text = this.number;
                bool swapDone = false;

                this.number.InterpolateCoroutine(2, (n) =>
                {
                    float N = n * n;
                    float Sin_N = Mathf.Sin((N * Mathf.PI) + Mathf.PI) + 1;
                    _holder.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, -90, 0), Quaternion.Euler(0,0,0), Sin_N);
                    if (!swapDone && N >= 0.5) { _logo.Transparency(0); swapDone = true; _bg.color = Color.white; _text.Transparency(1); }
                });

            }
        }

        public void SetLogo(Sprite logo)
        {
            if (this.logo.overrideSprite != logo && logo != null && !isShowingNumber)
            {
                this.logo.StopAllCoroutines();
                Image _logo = this.logo;
                bool swapDone = false;
                this.logo.InterpolateCoroutine(2, (n) =>
                {
                    float N = n * n;
                    float Sin_N = Mathf.Sin((N * Mathf.PI) + Mathf.PI) + 1;
                    Debug.Log(Sin_N);
                    _logo.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, Sin_N);
                    if (!swapDone && N >= 0.5) { _logo.overrideSprite = logo; swapDone = true; }
                });
            }
        }
    }

    public Image[] keyPotionsLogo;

    void Start()
    {
        Hide();
    }

    public void SetKeyPotionsLogo_Numbers(params int[] numbers)
    {
        if (keyPotionLogos != null && numbers != null)
        {
            for (int i = 0; i < Mathf.Min(keyPotionLogos.Length, numbers.Length); i++)
            {
                keyPotionLogos[i].SetNumber(numbers[i]);
            }
        }
    }

    public void SetKeyPotionsLogo(int totalAmountOfKeyPotions, params Sprite[] revealedPotionsLogos)
    {
        originalKeyPotionLogoHolder.holder.gameObject.SetActive(false);

        if (keyPotionLogos != null && keyPotionLogos.Length != totalAmountOfKeyPotions)
        {
            for (int i = 0; i < keyPotionLogos.Length; i++) { Destroy(keyPotionLogos[i].holder); }
            keyPotionLogos = null;
        }
        if (keyPotionLogos == null)
        {
            keyPotionLogos = new KeyPotionLogo[totalAmountOfKeyPotions];
            for (int i = 0; i < keyPotionLogos.Length; i++)
            {
                keyPotionLogos[i] = new KeyPotionLogo(Instantiate(originalKeyPotionLogoHolder.holder), originalKeyPotionLogoHolder.hiddenLogo, originalKeyPotionLogoHolder.holder.transform.parent);
            }
        }

        for (int i = 0; i < keyPotionLogos.Length; i++)
        {
            int _i = i;
            if (keyPotionLogos[_i].holder != null && revealedPotionsLogos != null && _i < revealedPotionsLogos.Length)
            {
                keyPotionLogos[_i].SetLogo(revealedPotionsLogos[_i]);
            }
        }

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
                  icon.Transparency(1 - N);
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
