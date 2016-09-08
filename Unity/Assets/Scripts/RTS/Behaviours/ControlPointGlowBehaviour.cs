using UnityEngine;
using System.Collections;

public class ControlPointGlowBehaviour : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public Sprite WhiteGlowSprite;
    public Sprite RedGlowSprite;
    public Sprite GreenGlowSprite;

    private float _angle;
    private float _velocity;
    private Side _currentSide = Side.Neutral;


    private Sprite _getSpriteForSide(Side side)
    {
        switch (side)
        {
            case Side.Enemy:
                return RedGlowSprite;
            case Side.Player:
                return GreenGlowSprite;
            default:
                return WhiteGlowSprite;
        }
    }

    public void ChangeSide(Side side)
    {
        if (side == _currentSide)
            return;

        SpriteRenderer.sprite = _getSpriteForSide(side);
        _currentSide = side;
    }

    void Start()
    {
        _angle = Random.value * Mathf.PI;
        _velocity = Random.value * 0.4f + 1.0f;
    }

    void Update()
    {
        var alpha = Mathf.Sin(_angle) * 0.2f + 0.7f;
        _angle += _velocity * Time.deltaTime;
        SpriteRenderer.color = new Color(1f, 1f, 1f, alpha);
    }
}