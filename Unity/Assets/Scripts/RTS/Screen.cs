using UnityEngine;
using System.Collections;

public static class Screen {


    public static void DumpScreenInfo()
    {
        Debug.Log("screen width = " + UnityEngine.Screen.width);
        Debug.Log("screen height = " + UnityEngine.Screen.height);
        Debug.Log("width = " + GetWidth());
        Debug.Log("height = " + GetHeight());
        Debug.Log("left = " + GetLeftEdge());
        Debug.Log("right = " + GetRightEdge());
        Debug.Log("top = " + GetTopEdge());
        Debug.Log("bottom = " + GetBottomEdge());
        Debug.Log("screenRatio = " + GetScreenRatio());
        Debug.Log("pixelToWorldRatio = " + GetPixelToWorldRatio());

    }

    public static float GetPixelToWorldRatio()
    {
        return GetHeight() / ScreenConsts.HeightInPixels;
    }

    public static float GetLeftEdge()
    {
        return GetWidth() * -0.5f;
    }

    public static float GetRightEdge()
    {
        return GetWidth() * 0.5f;
    }

    public static float GetBottomEdge()
    {
        return GetHeight() * -0.5f;
    }

    public static float GetTopEdge()
    {
        return GetHeight() * 0.5f;
    }

    public static float GetHeight()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, UnityEngine.Screen.height, 0)).y * 2.0f;
    }

    public static float GetWidth()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(UnityEngine.Screen.width, 0, 0)).x * 2.0f;
    }

    public static float GetMiddleX()
    {
        return (GetLeftEdge() + GetWidth() * 0.5f);
    }

    public static float GetMiddleY()
    {
        return (GetTopEdge() + GetHeight() * 0.5f);
    }

    public static float GetSpriteHeight(SpriteRenderer sprite)
    {
        float scaleRatio = sprite.sprite.texture.height / ScreenConsts.HeightInPixels;
        return scaleRatio * GetHeight();
    }

    public static float GetSpriteWidth(SpriteRenderer sprite)
    {
        float scaleRatio = sprite.sprite.texture.width / ScreenConsts.HeightInPixels;
        return scaleRatio * GetWidth();
    }

    public static float GetScreenRatio()
    {
        return UnityEngine.Screen.height / ScreenConsts.HeightInPixels;
    }
}
