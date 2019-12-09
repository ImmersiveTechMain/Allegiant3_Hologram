using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleDisplaySetup_Custom : MonoBehaviour
{
    [Header("Settings")]
    public int[] additionalDisplaysIndex;
    
    void Awake()
    {
        Setup();
    }
    
    public void Setup()
    {
        if (additionalDisplaysIndex != null && additionalDisplaysIndex.Length > 0)
        {
            for (int i = 0; i < additionalDisplaysIndex.Length; i++)
            {
                if (additionalDisplaysIndex[i] >= 0 && additionalDisplaysIndex[i] < Display.displays.Length)
                {
                    Display.displays[additionalDisplaysIndex[i]].Activate();
                }
            }
        }
    }

}
