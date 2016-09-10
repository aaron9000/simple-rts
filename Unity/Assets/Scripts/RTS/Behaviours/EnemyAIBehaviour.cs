using System;
using UnityEngine;
using System.Collections;

public class EnemyAIBehaviour : MonoBehaviour
{

    public EnemyAIState State;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    var state = Game.State;
	    var difficulty = state.Difficulty;
	    var money = state.EnemyResources;

	    if (state.Winner != Side.Neutral)
	    {
	        return;
	    }
	    if (State.SpawnCooldown > 0 || money <= 0)
	    {
	        State.SpawnCooldown -= Time.deltaTime;
	    }
	    else
	    {
	        var laneKey = EnemyAI.GetSpawnLaneKey(state, Game.Queries);
	        var e = new Events.PurchaseEvent(Side.Enemy, Int32.Parse(laneKey));
	        Game.PushEvent(e);
	        State.SpawnCooldown = EnemyAI.GetCooldownForDifficulty(difficulty);
	    }
	}
}
