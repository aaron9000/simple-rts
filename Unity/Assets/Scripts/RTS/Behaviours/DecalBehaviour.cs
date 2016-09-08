using UnityEngine;
using System.Collections;

public class DecalBehaviour : MonoBehaviour
{

    public SpriteRenderer SpriteRenderer;
    public Sprite BulletScorch;
    public Sprite ExplosionScorch;
    public Sprite BloodSplatter;

    public DecalState State;

    private float _livetime = 3.5f;

    private Sprite _getSpriteForType()
    {
        switch (State.DecalType)
        {
            case DecalType.Blood:
                return BloodSplatter;
            case DecalType.Scorch:
                return ExplosionScorch;
            default:
                return BulletScorch;
        }
    }
	void Start ()
	{
	    transform.position = Map.ConvertToWorldCoordinates(State.Position);
	    transform.rotation = Quaternion.Euler(0, 0, State.Angle);
	    transform.localScale = Vector3.one * (Random.value + 1.0f);
	    SpriteRenderer.sprite = _getSpriteForType();
	}

	void Update ()
	{
	    _livetime -= Time.deltaTime;
	    if (_livetime <= 1)
	        SpriteRenderer.material.color = new Color(1, 1, 1, _livetime / 1.0f);
        if (_livetime <= 0)
	        Destroy(gameObject);
	}
}
