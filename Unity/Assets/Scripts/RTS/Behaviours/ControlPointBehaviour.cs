using UnityEngine;
using System.Collections;
using System.Linq;

public class ControlPointBehaviour : MonoBehaviour
{

    public ControlPointState State;

    public SpriteRenderer SpriteRenderer;
    public ControlPointGlowBehaviour ControlPointGlow;

	// Use this for initialization
	void Start () {
	    transform.localScale = Vector3.one * 2f;
	    transform.position = Map.ConvertToWorldCoordinates(State.Position);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    var laneWidth = Map.GetLaneWidth();

	    // Change side?
	    var q = new Queries.NearestUnit
	    {
	        SearchFrom = State.Position,
	        MaxDistance = laneWidth,
	        LaneKey = State.LaneKey,
	    };
	    var capDist = BalanceConsts.ControlPointCaptureDistance;
	    var ul = new Vector2(laneWidth * -0.5f, -capDist) + State.Position;
	    var rect = new Rect(ul, new Vector2(laneWidth, capDist * 2f));
	    var tuples = Game.Queries().GetNearestUnitTuples(q, s => s.UnitType == UnitType.Soldier);
	    var soldiers = F.Map(t => t.Second, tuples);
	    var hasEnemy = soldiers.Any(s => s.Side == Side.Enemy && rect.Contains(s.Position));
	    var hasPlayer = soldiers.Any(s => s.Side == Side.Player && rect.Contains(s.Position));
	    if (hasPlayer ^ hasEnemy)
	    {
	        var newSide = hasEnemy ? Side.Enemy : Side.Player;
	        if (State.Side != newSide)
	        {
	            var sound = newSide == Side.Player ? SoundType.Capture : SoundType.LoseCapture;
	            Game.PushEvent(new Events.PlaySoundEvent(sound));
	            State.Side = newSide;
	            ControlPointGlow.ChangeSide(newSide);
	        }
	    }

	    // Produce resources
	    State.ProductionCooldown -= Time.deltaTime;
	    if (State.ProductionCooldown < 0 && State.Side != Side.Neutral)
	    {
	        State.ProductionCooldown = BalanceConsts.ControlPointProductionCooldown;
	        Game.PushEvent(new Events.ProduceResourceEvent(State.Id));
	    }
	}
}
