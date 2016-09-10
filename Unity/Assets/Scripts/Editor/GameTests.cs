using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityTest
{
    [TestFixture]
    [Category("Game Tests")]
    internal class GameTests
    {
        [Test]
        public void GameStateCloneTest()
        {
            var a = new GameState(Difficulty.Easy);
            a.Soldiers.Add(new SoldierState {Side = Side.Enemy});
            var b = a.Clone();
            a.Soldiers[0].Side = Side.Neutral;
            Assert.AreEqual(b.Soldiers[0].Side, Side.Enemy);
        }

        [Test]
        public void ProcessEventsTest()
        {

            // Start with two soldiers at full health
            var gameState = new GameState(Difficulty.Easy);
            gameState.Soldiers.AddRange(new[]
            {
                new SoldierState {Side = Side.Enemy, Id = "0", Health = 10},
                new SoldierState {Side = Side.Player, Id = "1",  Health = 10}
            });
            Game.LoadFromState(gameState);

            // Push attack event & process
            Game.PushEvent(new Events.SoldierAttackEvent("0", "1", 5));
            Game.ProcessEvents();

            // Assert soldier 0 was damaged
            var updatedGameState = Game.State;
            Assert.AreEqual(updatedGameState.Soldiers[1].Health, 5.0f);
        }

        [Test]
        public void LookupsTest()
        {
            var x = 50.0f;
            var r = PhysicsConsts.SoldierRadius * 2;
            var posA = new Vector2(x, 0);
            var posB = new Vector2(x, r);
            var posC = new Vector2(x, r * 2.0f);
            var unitA = new SoldierState {Side = Side.Player, Id = "0", LaneKey = "0", Position = posA};
            var unitB = new SoldierState {Side = Side.Enemy, Id = "1", LaneKey = "0", Position = posB};
            var unitC = new SoldierState {Side = Side.Enemy, Id = "2", LaneKey = "0", Position = posC};
            var a = new GameState(Difficulty.Easy);
            a.Soldiers = new List<SoldierState>(new[] {unitA, unitB, unitC});
            Game.LoadFromState(a);
            var lookups = Game.Queries;
            Func<BaseUnitState, bool> enemyFilter = v => v.Id != unitA.Id && v.Side != unitA.Side;
            Func<BaseUnitState, bool> anyFilter = v => true;
            var closestEnemyQuery = new Queries.NearestUnit
            {
                LaneKey = "0",
                MaxDistance = 200,
                PreferredId = null,
                SearchFrom = unitA.Position
            };
            var preferredEnemyQuery = new Queries.NearestUnit
            {
                LaneKey = "0",
                MaxDistance = 200,
                PreferredId = "2",
                SearchFrom = unitA.Position
            };
            var nonexistentEnemyQuery = new Queries.NearestUnit
            {
                LaneKey = "0",
                MaxDistance = 0,
                PreferredId = null,
                SearchFrom = unitA.Position
            };
            var closesetEnemy = lookups.GetNearestUnit(closestEnemyQuery, enemyFilter);
            var preferredEnemy = lookups.GetNearestUnit(preferredEnemyQuery, enemyFilter);
            var nonexistentEnemy = lookups.GetNearestUnit(nonexistentEnemyQuery, enemyFilter);
            var self = lookups.GetNearestUnit(closestEnemyQuery, anyFilter);
            Assert.AreEqual(closesetEnemy.Id, "1");
            Assert.AreEqual(preferredEnemy.Id, "2");
            Assert.AreEqual(self.Id, "0");
            Assert.AreEqual(nonexistentEnemy, null);
        }

        [Test]
        public void CollisionTest()
        {
            var a = new GameState(Difficulty.Easy);
            var posA = new Vector2(60, 0);
            var posB = new Vector2(60, 20);
            var posC = new Vector2(60, 320);
            var posD = new Vector2(60, 320);
            a.Soldiers = new List<SoldierState>(new[]
            {
                new SoldierState {Side = Side.Player, Id = "0", LaneKey = "0", Position = posA},
                new SoldierState {Side = Side.Enemy, Id = "1", LaneKey = "0", Position = posB},
                new SoldierState {Side = Side.Player, Id = "2", LaneKey = "0", Position = posC},
                new SoldierState {Side = Side.Enemy, Id = "3", LaneKey = "0", Position = posD}
            });
            Game.LoadFromState(a);
            var b = Game.State;
            var unitA = b.Soldiers[0];
            var unitB = b.Soldiers[1];
            var unitC = b.Soldiers[2];
            var unitD = b.Soldiers[3];
            Assert.AreEqual(PhysicsConsts.SoldierRadius * 2f, Vector2.Distance(unitA.Position, unitB.Position));
            Assert.AreEqual(PhysicsConsts.SoldierRadius * 2f, Vector2.Distance(unitC.Position, unitD.Position));
        }


        [Test]
        public void BoundaryCollsionTest()
        {
            var a = new GameState(Difficulty.Easy);
            a.Soldiers = new List<SoldierState>(new[]
            {
                new SoldierState {Side = Side.Player, Id = "0", LaneKey = "0", Position = Vector2.zero}
            });
            Game.LoadFromState(a);
            var b = Game.State;
            var unitA = b.Soldiers[0];
            Assert.AreNotEqual(unitA.Position, Vector2.zero);
        }

        private void _spawnSoldier(int laneIndex, Side side)
        {
            var pos = Map.GetSoldierSpawnPosition(laneIndex, side);
            var e = new Events.SpawnEvent(pos, 0, side, laneIndex.ToString(), ObjectType.UnitSoldier);
            Game.PushEvent(e);
        }

        private void _setupMap()
        {
            var a = new GameState(Difficulty.Hard);

            // Add default map objects
            Game.LoadFromState(a);
            Game.PushEvents(Map.GetMapSpawnEvents());
            Game.ProcessEvents();

            // Lane 0
            // 1:4 enemy to player
            _spawnSoldier(0, Side.Player);
            _spawnSoldier(0, Side.Player);
            _spawnSoldier(0, Side.Player);
            _spawnSoldier(0, Side.Player);
            _spawnSoldier(0, Side.Enemy);

            // Lane 1
            // 4:1 enemy to player
            _spawnSoldier(1, Side.Enemy);
            _spawnSoldier(1, Side.Enemy);
            _spawnSoldier(1, Side.Enemy);
            _spawnSoldier(1, Side.Enemy);
            _spawnSoldier(1, Side.Player);
            Game.ProcessEvents();

            // Damage player turret in lane 0
            var b = Game.State;
            var t = b.Turrets[0];
            var s = b.Soldiers[0];
            var e = new Events.SoldierAttackEvent(s.Id, t.Id, BalanceConsts.TurretHealth * 0.25f);
            Game.PushEvent(e);
            Game.ProcessEvents();

        }

        [Test]
        public void EnemyAITest()
        {

            // Default test map
            _setupMap();

            var a = Game.State;
            var q = Game.Queries;
            var lane0 = q.GetLaneMetrics("0");
            var lane1 = q.GetLaneMetrics("1");

            Assert.AreEqual(a.Soldiers.Count, 10);

            Assert.AreEqual(lane0.PlayerBaseHealthPercentage, 0.75f);
            Assert.AreEqual(lane0.EnemyBaseHealthPercentage, 1f);
            Assert.AreEqual(lane0.PlayerControlledPoints, 0);
            Assert.AreEqual(lane0.EnemyControlledPoints, 0);
            Assert.AreEqual(lane0.PlayerUnits, 4);
            Assert.AreEqual(lane0.EnemyUnits, 1);

            var objectiveValue0 = lane0.GetObjectiveValue();
            Assert.AreEqual(objectiveValue0, 0.625f);

            var militaryValue0 = lane0.GetMilitaryValue();
            Assert.AreEqual(militaryValue0, 0.772727251f);

            var eceonomyValue0 = lane0.GetEconomyValue();
            Assert.AreEqual(eceonomyValue0, 0.5f);

            var militaryValue1 = lane1.GetMilitaryValue();
            Assert.AreEqual(militaryValue1, 0.0);

            var spawnLane = EnemyAI.GetSpawnLaneKey(a, q);
            Assert.AreEqual("0", spawnLane);
        }

		[Test]
		public void ProbabilitiesTest()
		{
			var pA = new [] {1f, 0f, 0f};
			var pB = new [] {0f, 1f, 0f};
			var pC = new [] {1f, 1f, 0f};

			for (var i = 0; i < 10; i++){
				var a = M.SampleFromProbabilities(pA);
				Assert.AreEqual(a, 0);

				var b = M.SampleFromProbabilities(pB);
				Assert.AreEqual(b, 1);

				var c = M.SampleFromProbabilities(pC);
				Assert.AreEqual(c == 0 || c == 1, true);
			}				
		}
    }
}