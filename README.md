#Simple RTS
A lane-based strategy game for Unity3D.

![alt text](https://s3.amazonaws.com/aaron-cdn/simple-rts/thumb-half.png)

##Playing the Game
#####Links & Downloads
- Mac Standalone (540 x 960): [Download Zip](https://s3.amazonaws.com/aaron-cdn/simple-rts/simple-rts-mac-full.zip)
- HTML5 (540 x 960): [Play in Browser](	
https://s3.amazonaws.com/aaron-cdn/simple-rts/simple-rts-web-full/index.html)
- HTML5 LOW-RES (405 x 720): [Play in Browser](	
https://s3.amazonaws.com/aaron-cdn/simple-rts/simple-rts-web-reduced/index.html)


#####Instructions
- Destroy all 3 enemy bases to win
- Capture and hold control points to earn resources (they glow green when captured)
- Purchase soldiers by clicking the bottom buttons

#####Pro Tips
- Make sure to spend your resources quickly
- Deprive the enemy of control points
- Always attack in groups

##The Insides
Internally, the game uses an event system to reduce interactions between game objects and simplify state management. This has a few advantages over the traditional pattern of letting game objects communicate with each other directly.

#####Main Game Loop
```c#
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
```

#####Testing Game Code
```c#
[Test]
public void ProcessEventsTest()
{

    // Start with two soldiers at full health
    var gameState = new GameState(Difficulty.Easy);
    gameState.Soldiers.AddRange(new[]
    {
        new SoldierState {Side = Side.Enemy, Id = "0", Health = 10f},
        new SoldierState {Side = Side.Player, Id = "1",  Health = 10f}
    });
    Game.LoadFromState(gameState);

    // Push attack event & process
    Game.PushEvent(new Events.SoldierAttackEvent("0", "1", 5f));
    Game.ProcessEvents();

    // Assert soldier 0 was damaged
    Assert.AreEqual(Game.State.Soldiers[1].Health, 5.0f);
}
```

#####Advantages:
- Game code is testable (even without a scene)
- You can inspect the application's state without traversing the scene graph
- Side effects, state mutations, and scene changes happen in one place
- It's easy to schedule events in the future
- Separation of concerns. Game objects only modify their own state and emit events


##Running the Tests
The tests are written for Unity's testing framework. Open the project in the Unity3D editor to run them.

`Window -> Editor Tests Runner -> Run All`


##Full Screenshot
![alt text](https://s3.amazonaws.com/aaron-cdn/simple-rts/screen-1.png)


