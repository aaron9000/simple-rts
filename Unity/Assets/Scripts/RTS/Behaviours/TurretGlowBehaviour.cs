using UnityEngine;
using System.Collections;

public class TurretGlowBehaviour : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    private float _angle;
    private float _velocity;
    private Side _side = Side.Neutral;

    public void ChangeSide(Side side)
    {
        _side = side;
    }

    void Start()
    {
        _angle = Random.value * Mathf.PI;
        _velocity = Random.value * 0.8f + 2.4f;
    }

    private Color _getColorForSide()
    {
        var alpha = Mathf.Sin(_angle) * 0.3f + 0.7f;
        switch (_side)
        {
            case Side.Enemy:
                return new Color(1, 0, 0, alpha);
            default:
                return new Color(0, 1, 0, alpha);
        }
    }

    void Update()
    {
        _angle += _velocity * Time.deltaTime;
        SpriteRenderer.color = _getColorForSide();
    }
}