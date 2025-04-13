using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Color_Extension
{
    
    public static Color ToGreyScale(this Color color, float saturation = 0)
    {
        float brightness = color.grayscale;
        Color greyedOut = new Color(brightness, brightness, brightness);
        return saturation * color + (1 - saturation) * greyedOut;
    }

    public static Color Copy(this Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }
}
