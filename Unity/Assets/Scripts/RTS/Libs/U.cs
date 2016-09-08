using UnityEngine;
using System;

public static class U
{

    public static Color ChangeAlpha(Color c, float newAlpha)
    {
        return new Color(c.r, c.g, c.b, Mathf.Clamp01(newAlpha));
    }
}