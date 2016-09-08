using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    void Start()
    {
        Game.StartNewGame();
        Game.PushEvents(Map.GetMapSpawnEvents());
        Game.PushEvent(new Events.GameStartEvent());
    }

    void Update()
    {
        Game.ProcessEventsAndSync();
    }
}