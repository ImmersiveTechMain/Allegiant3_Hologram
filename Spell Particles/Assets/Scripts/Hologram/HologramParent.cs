using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramParent : MonoBehaviour
{

    [Range(-500f, 500f)]
    public float DistanceFromCenter = 0f;

    private HologramChild[] children;

    private float lastDistance;

    // Start is called before the first frame update
    void Start()
    {
        lastDistance = 99999999f;
        children = GetComponentsInChildren<HologramChild>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastDistance != DistanceFromCenter) {
            //Update children
            UpdateChildren();
        }
        lastDistance = DistanceFromCenter;
    }

    void UpdateChildren() {
        for (int i = 0; i < children.Length; i++) {
            children[i].distanceFromCenter = DistanceFromCenter;
        }
    }

}
