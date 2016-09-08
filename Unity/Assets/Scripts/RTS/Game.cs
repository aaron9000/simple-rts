using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public static class Game
{
    private static Difficulty _difficulty;

    private static GameState _publicState;
    private static Queries _publicQueries = new Queries();

    private static GameState _privateState;
    private static Queries _privateQueries = new Queries();

    private static List<BaseEvent> _events = new List<BaseEvent>();

    #region Private Helpers

    private static void _updateStateWithGameObjects<TBehaviour, TState>(string gameStateKey, string objectTag)
        where TBehaviour : MonoBehaviour where TState : BaseState
    {
        var a = GameObject.FindGameObjectsWithTag(objectTag);
        var b = F.Map(g => (object) g.GetComponent<TBehaviour>(), a);
        var c = F.PluckFromObjects<TState>("State", b);
        var d = c.ToList();
        F.SetValue(gameStateKey, d, _privateState);
    }

    private static void _syncStateWithGameObjects()
    {
        _updateStateWithGameObjects<SoldierBehaviour, SoldierState>("Soldiers", ObjectTag.Soldier);
        _updateStateWithGameObjects<ControlPointBehaviour, ControlPointState>("ControlPoints", ObjectTag.ControlPoint);
        _updateStateWithGameObjects<TurretBehaviour, TurretState>("Turrets", ObjectTag.Turret);
    }

    private static void _processEvents(bool syncStateWithGameObjects)
    {
        // Process events & modify private state
        var unhandledEvents = new List<BaseEvent>();
        foreach (var e in _events)
        {
            if (e.EventFireDelay <= 0f)
            {
                e.Fire(_privateState, _privateQueries);
            }
            else
            {
                e.EventFireDelay -= Time.deltaTime;
                unhandledEvents.Add(e);
            }
        }
        _events = unhandledEvents;

        // Refresh private state with GameObjects (objects may have updated their own state)
        if (syncStateWithGameObjects)
        {
            _syncStateWithGameObjects();
        }
        _privateQueries.RebuildLookups(_privateState);

        // Run physics
        Physics.Update(_privateQueries);

        // Make a deep copy of the state for consumption by GameObject
        _publicState = _privateState.Clone();
        _publicQueries.RebuildLookups(_publicState);
    }

    #endregion

    #region Public methods

    public static void SetDifficulty(Difficulty difficulty)
    {
        _difficulty = difficulty;
    }

    public static void StartNewGame()
    {
        LoadFromStateAndSync(new GameState(_difficulty));
    }


    public static void LoadFromState(GameState state)
    {
        _privateState = state;
        _events.Clear();

        ProcessEvents();
    }

    public static void LoadFromStateAndSync(GameState state)
    {
        _privateState = state;
        _events.Clear();

        ProcessEventsAndSync();
    }

    public static GameState State()
    {
        return _publicState;
    }

    public static Queries Queries()
    {
        return _publicQueries;
    }

    public static void PushEvent(BaseEvent e)
    {
        _events.Add(e);
    }

    public static void PushEvents(IEnumerable<BaseEvent> events)
    {
        _events.AddRange(events);
    }

    public static void ProcessEvents()
    {
        _processEvents(false);
    }

    public static void ProcessEventsAndSync()
    {
        _processEvents(true);
    }

    #endregion
}