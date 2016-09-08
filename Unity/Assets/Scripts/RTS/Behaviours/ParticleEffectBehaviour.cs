using UnityEngine;
using System.Collections;

public class ParticleEffectBehaviour : MonoBehaviour {


    public ParticleSystem Particle;
    private float _livetime;
        
	#region Unity Lifecycle
	void Start () {
	    Particle = GetComponent<ParticleSystem>();
		_livetime = Particle.startLifetime * 2.0f;
	    transform.localScale = Vector3.one * (Random.value * 0.2f + 0.8f);
	    transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
	}
	void Update () {

		_livetime -= Time.deltaTime;
		if (_livetime < 0.0f) {
			Destroy(gameObject);
		}
	}
	#endregion
}
