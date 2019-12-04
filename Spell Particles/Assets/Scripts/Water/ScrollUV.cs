using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollUV : MonoBehaviour
{
    // Scroll main texture based on time

    public Vector2 scrollSpeed = new Vector2(0.1f, 0.15f);
    public bool InvertX = false;
    public bool InvertY = false;
    
    //public float NormalScale = 1.2f;

    Renderer rend;

    Vector2 offset;

    void Start() {
        rend = GetComponent<Renderer>();
    }

    void Update() {
        offset.x += Time.deltaTime * scrollSpeed.x * (InvertX ? -1f : 1f);
        offset.y += Time.deltaTime * scrollSpeed.y * (InvertY ? -1f : 1f);
        rend.material.SetTextureOffset("_BaseColorMap", offset);
        //rend.material.SetFloat("_NormalScale", NormalScale);
    }
}
