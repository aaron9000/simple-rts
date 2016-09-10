using UnityEngine;
using System.Collections;

public class LightGlowBehaviour : MonoBehaviour {

    public SpriteRenderer SpriteRenderer;
    public LightGlowState State;

    private float _duration = 1.0f;
    private float _livetime = 1.0f;


    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        float scale = 1.0f;
        switch (State.LightGlowType)
        {
            case LightGlowType.Explosion:
                scale = 2.0f;
                _duration = _livetime = 0.2f;
                break;
            case LightGlowType.TurretMuzzle:
                scale = 1.2f;
                _duration = _livetime = 0.1f;
                break;
            case LightGlowType.Muzzle:
                scale = 0.6f;
                _duration = _livetime = 0.05f;
                break;
        }
        scale *= Random.value * 0.2f + 0.8f;
        transform.localScale = Vector3.one * scale;
        transform.position = Map.ConvertToWorldCoordinates(State.Position);
    }

    void Update()
    {
        _livetime -= Time.deltaTime;

        SpriteRenderer.material.color = new Color(1, 1, 1, _duration / _duration);
        if (_livetime < 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
