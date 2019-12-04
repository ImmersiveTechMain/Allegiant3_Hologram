using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramChild : MonoBehaviour
{
    public bool UpIsHorizontal;
    public bool InvertUp;

    internal float distanceFromCenter;

    RectTransform rt;

    Vector2 defaultPos;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        defaultPos = rt.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float invert = (InvertUp ? -1f : 1f);
        float xOffset = (UpIsHorizontal ? distanceFromCenter * invert : 0f);
        float yOffset = (!UpIsHorizontal ? distanceFromCenter * invert : 0f);
        Vector2 pos;
        pos.x = defaultPos.x + xOffset;
        pos.y = defaultPos.y + yOffset;
        rt.anchoredPosition = pos;
    }
}
