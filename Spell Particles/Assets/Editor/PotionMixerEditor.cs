using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PotionMixer))]
public class PotionMixerEditor : Editor
{
    public Sprite potionIcon;
    public Material potionIconMaterial;
    SerializedProperty visualEffect;
    SerializedProperty mergeColors;
    SerializedProperty potions;
    SerializedProperty extracts;

    bool potionsFoldout = false;
    bool[,] individualPotionsFoldout;
    int? potionArraySize = null;

    private void OnEnable()
    {
        mergeColors = serializedObject.FindProperty("mergeColors");
        visualEffect = serializedObject.FindProperty("cauldron");
        potions = serializedObject.FindProperty("potions");
        extracts = serializedObject.FindProperty("extracts");
        potionArraySize = null;
    }

    public override void OnInspectorGUI()
    {
        PotionMixer potionMixer = (PotionMixer)target;
        EditorGUILayout.PropertyField(visualEffect, false, GUILayout.ExpandHeight(true));
        EditorGUILayout.PropertyField(mergeColors, false, GUILayout.ExpandHeight(true));

        //EditorGUILayout.PropertyField(potions, true, GUILayout.ExpandHeight(true));
        potionsFoldout = EditorGUILayout.Foldout(potionsFoldout, "Potions", true);

        if (potionsFoldout)
        {
            EditorGUI.indentLevel++;
            if (potionMixer.potions == null) { potionMixer.potions = new Potion[0]; }
            potionArraySize = EditorGUILayout.IntField("Amount", potionArraySize == null || ((int)potionArraySize) < 0 ? potionMixer.potions.Length : (int)potionArraySize);

            if (potionMixer.potions.Length != potionArraySize && GUILayout.Button("Apply new amount"))
            {
                if (potionArraySize == 0) { potionMixer.potions = new Potion[0]; }
                if (potionArraySize != potionMixer.potions.Length)
                {
                    Potion[] newPotions = new Potion[(int)potionArraySize];
                    for (int i = 0; i < newPotions.Length; i++) { newPotions[i] = potionMixer.potions == null || potionMixer.potions.Length == 0 ? new Potion() : new Potion(potionMixer.potions[Mathf.Min(i, potionMixer.potions.Length - 1)]); }
                    potionMixer.potions = newPotions;
                }
            }

            if (potionMixer.potions != null && potionMixer.potions.Length > 0)
            {
                if (individualPotionsFoldout == null || individualPotionsFoldout.GetLength(0) != potionMixer.potions.Length) { individualPotionsFoldout = new bool[potionMixer.potions.Length, 2]; }
                for (int i = 0; i < potionMixer.potions.Length; i++)
                {
                    individualPotionsFoldout[i, 0] = EditorGUILayout.Foldout(individualPotionsFoldout[i, 0], potionMixer.potions[i].name, true);
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    Color bgColor = potionMixer.potions[i].identityColor;
                    bgColor.a = 0.05f;
                    if (individualPotionsFoldout[i, 0])
                    {
                        EditorGUI.indentLevel++;
                        potionMixer.potions[i].name = EditorGUILayout.TextField("Name", potionMixer.potions[i].name, GUILayout.ExpandHeight(true));
                        potionMixer.potions[i].description = EditorGUILayout.TextField("Description", potionMixer.potions[i].description, GUILayout.ExpandHeight(true));
                        potionMixer.potions[i].identityColor = EditorGUILayout.ColorField("Identity Color", potionMixer.potions[i].identityColor, GUILayout.ExpandHeight(true));
                        EditorGUILayout.BeginHorizontal();
                        potionMixer.potions[i].image = EditorGUILayout.ObjectField("Image", potionMixer.potions[i].image ?? potionIcon, typeof(Sprite), false) as Sprite;
                        EditorGUILayout.EndHorizontal();
                        individualPotionsFoldout[i, 1] = EditorGUILayout.Foldout(individualPotionsFoldout[i, 1], "Ingredients", true);
                        if (individualPotionsFoldout[i, 1])
                        {
                            EditorGUI.indentLevel++;
                            if (potionMixer.potions[i].ingredients == null) { potionMixer.potions[i].ingredients = new Ingredient[0]; }
                            int ingredientCount = EditorGUILayout.IntField("Amount", potionMixer.potions[i].ingredients.Length);
                            if (ingredientCount >= 0 && ingredientCount != potionMixer.potions[i].ingredients.Length)
                            {
                                Ingredient[] newIngredients = new Ingredient[ingredientCount];
                                for (int x = 0; x < newIngredients.Length; x++)
                                {
                                    newIngredients[x] = potionMixer.potions[i].ingredients.Length <= 0 ? null : potionMixer.potions[i].ingredients[Mathf.Min(x, potionMixer.potions[i].ingredients.Length - 1)];
                                }
                                potionMixer.potions[i].ingredients = newIngredients;
                            }

                            for (int x = 0; x < potionMixer.potions[i].ingredients.Length; x++)
                            {
                                string[] options = new string[(potionMixer.extracts == null ? 0 : potionMixer.extracts.Length) + 1];
                                int initialIndex = 0;
                                options[0] = "None";
                                if (options.Length > 1)
                                {
                                    for (int y = 0; y < potionMixer.extracts.Length; y++)
                                    {
                                        options[y + 1] = potionMixer.extracts[y].name;
                                        if (potionMixer.potions[i].ingredients[x] != null && potionMixer.potions[i].ingredients[x].name == options[y + 1]) { initialIndex = y + 1; }
                                    }
                                }

                                int index = EditorGUILayout.Popup(initialIndex, options);
                                if (index != 0 && potionMixer.extracts != null && index - 1 < potionMixer.extracts.Length && (potionMixer.potions[i].ingredients[x] == null || potionMixer.potions[i].ingredients[x].name != potionMixer.extracts[index - 1].name))
                                {
                                    potionMixer.potions[i].ingredients[x] = potionMixer.extracts[index - 1];
                                }
                                else if (index <= 0) { potionMixer.potions[i].ingredients[x] = null; }
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                        Rect lastRect2 = GUILayoutUtility.GetLastRect();
                        EditorGUI.DrawRect(new Rect(EditorGUI.indentLevel * 20, lastRect.y + lastRect.height, EditorGUIUtility.currentViewWidth, (lastRect2.y + lastRect2.height) - (lastRect.y + lastRect.height)), bgColor);
                    }
                    bgColor.a = 0.25f;
                    EditorGUI.DrawRect(new Rect(8, lastRect.y, EditorGUIUtility.currentViewWidth, lastRect.height), bgColor);
                    bgColor = Color.Lerp(bgColor, Color.black, 0.5f);
                    bgColor.a = 1f;
                    EditorGUI.DrawRect(new Rect(0, lastRect.y, lastRect.height, lastRect.height), bgColor);
                    if (potionIcon != null && potionIconMaterial != null) { EditorGUI.DrawPreviewTexture(new Rect(0, lastRect.y, lastRect.height, lastRect.height), potionMixer.potions[i].image != null ? potionMixer.potions[i].image.texture : potionIcon.texture, potionIconMaterial, ScaleMode.ScaleToFit); }
                }
            }
            EditorGUI.indentLevel--;
        }



        EditorGUILayout.PropertyField(extracts, true, GUILayout.ExpandHeight(true));


        serializedObject.ApplyModifiedProperties();
    }

}
