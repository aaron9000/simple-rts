using UnityEngine;
using System.Collections;

public class BarrierBehaviour : MonoBehaviour
{

    public BarrierState State;

    // Use this for initialization
	void Start () {
	    transform.position = Map.ConvertToWorldCoordinates(State.Position);
	}
	
	// Update is called once per frame
	void Update () {

	}
}
