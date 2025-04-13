using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hideable : MonoBehaviour
{
    Renderer[] renderers;
    Image[] images;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        images = GetComponentsInChildren<Image>();

        //FogOfWar.InitializePlayerOwnedTransforms();
        if (!FogOfWar.InSight(transform.position))
            SetVisibility(false);
    }


    void Update()
    {
        if (FogOfWar.InSight(transform.position))
            SetVisibility(true);
        else
            SetVisibility(false);
    }

    void SetVisibility(bool isVisible)
    {
        foreach (var renderer_ in renderers)
            renderer_.enabled = isVisible;
        foreach (var image in images)
            image.enabled = isVisible;
    }


}
