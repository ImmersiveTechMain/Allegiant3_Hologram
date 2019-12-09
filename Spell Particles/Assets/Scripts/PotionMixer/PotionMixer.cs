using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.UI;

[System.Serializable]
public class Ingredient
{
    public string name;
    public string UDP_ID;
}

[System.Serializable]
public class Extract : Ingredient
{
    public Color identityColor;
    public Gradient waterColor;
    public Color inkColor;
    public Sprite icon;
}

[System.Serializable]
public class Potion
{
    public string name;
    public string description;
    public Color identityColor;
    public Sprite image;
    public Ingredient[] ingredients;

    public Potion()
    {
    }

    public Potion(Potion p)
    {
        if (p != null)
        {
            this.name = p.name;
            this.description = p.description;
            this.identityColor = p.identityColor;
            this.image = p.image;
            this.ingredients = (Ingredient[])p.ingredients.Clone();
        }
    }
}

public class PotionMixer : MonoBehaviour
{
    public delegate void Extract_CALLBACK(Extract extract);
    public delegate void POTION_CALLBACK(Potion extract);
    public Extract_CALLBACK OnExtractAdded = delegate (Extract extract) { };
    public POTION_CALLBACK OnPotionMatchFound = delegate (Potion potion) { };

    [Header("Components")]
    public VisualEffect cauldron;

    [Header("Settings")]
    public bool mergeColors = true;
    public Potion[] potions;
    public Extract[] extracts;

    Extract lastExtractUsed = null;
    List<Extract> extractsUsed = new List<Extract>();

    public bool isClear { private set; get; }

    private void Start()
    {
        Clear();
        OnPotionMatchFound += (potion) => { Debug.Log("Potion Match Found: " + potion.name); };
    }

    public void AddExtract(Extract extract)
    {
        if (extract != null)
        {
            bool usingEntryAnimation = cauldron.GetBool("UsingEntryAnimation");
            cauldron.SendEvent("Drop"); // the entry effect call
            cauldron.SetVector4("LiquidInkColor", extract.inkColor); // the ink color of the drop

            Gradient waterColor = !mergeColors || isClear ? extract.waterColor : cauldron.GetGradient("WaterColor").MixWith(extract.waterColor);

            if (usingEntryAnimation)
            {
                this.ActionAfterSecondDelay(cauldron.GetFloat("LiquidToCenter_Duration"), () =>
                {
                    cauldron.SetGradient("WaterColor", waterColor);
                    cauldron.SetVector4("CenterColor", extract.identityColor);
                    cauldron.SetTexture("Icon", extract.icon.texture);
                });
            }
            else
            {
                cauldron.SetGradient("WaterColor", waterColor);
                cauldron.SetVector4("CenterColor", extract.identityColor);
                cauldron.SetTexture("Icon", extract.icon.texture);
            }
            lastExtractUsed = extract;
            extractsUsed.Add(extract);
            isClear = false; // to mark the cauldron as "inpure" after adding the first extract
            OnExtractAdded(extract);
            IdentifyMix();
        }
    }

    public Ingredient GetIngredientlByUdpID(string UdpID)
    {
        if (!string.IsNullOrEmpty(UdpID) && extracts != null)
        {
            for (int i = 0; i < extracts.Length; i++)
            {
                if (extracts[i].UDP_ID.ToLower()== UdpID.ToLower()) { return extracts[i]; }
            }
        }
        return null;
    }

    bool IdentifyMix()
    {
        if (extractsUsed != null && extractsUsed.Count > 0)
        {
            Extract[] currentMix = extractsUsed.ToArray();
            if (potions != null)
            {
                for (int i = 0; i < potions.Length; i++)
                {
                    bool potionMatch = true;
                    if (potions[i] != null)
                    {
                        for (int x = 0; x < potions[i].ingredients.Length; x++)
                        {
                            int currentMix_Index = currentMix.Length - potions[i].ingredients.Length + x ;
                            potionMatch &= potions[i].ingredients[x] != null && currentMix_Index >= 0 && potions[i].ingredients[x].name == currentMix[currentMix_Index].name;
                        }
                    }
                    else { potionMatch = false; }

                    if (potionMatch) { int _i = i; OnPotionMatchFound(potions[_i]); return true; }
                }
            }
        }
        return false;
    }

    public void Clear()
    {
        isClear = true;
        lastExtractUsed = null;
        extractsUsed.Clear();
        cauldron.SetGradient("WaterColor", cauldron.GetGradient("DefaultWaterGradient"));
        cauldron.SetVector4("CenterColor", cauldron.GetVector4("DefaultCenterColor"));
        cauldron.SetTexture("Icon", cauldron.GetTexture("DefaultIcon"));
    }

    public void Update()
    {
        KeyboardControlls();
    }

    public void KeyboardControlls()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { Clear(); }
        if (Input.anyKeyDown)
        {
            char? key = string.IsNullOrEmpty(Input.inputString) ? null : (char?)Input.inputString[0];
            int index = -1;
            if (key != null && int.TryParse(((char)key).ToString(), out index) && --index < extracts.Length && index >= 0)
            {
                AddExtract(extracts[index]);
            }
        }
    }

}
