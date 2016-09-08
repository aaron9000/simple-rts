using UnityEngine;
using System.Collections;

public class NoiseBehaviour : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public float Speed;

    private float _x;

    void Start()
    {
        _x = Random.value * 0.5f;
    }

    void Update()
    {
        _x += Time.deltaTime * Speed;
        if (_x >= 0.5f)
            _x -= 0.5f;

        MeshRenderer.material.SetTextureOffset("_MainTex", new Vector2(_x, 0));
        MeshRenderer.material.SetTextureScale("_MainTex", new Vector2(0.5f, 1.0f));
    }
}