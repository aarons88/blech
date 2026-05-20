using NUnit.Framework;
using UnityEngine;
using Blech.Player;
using Blech.World;

namespace Blech.Tests
{
    public class WorldCoreTests
    {
        [SetUp]
        public void Reset()
        {
            CheckpointManager.ResetForTests();
            RunStatsTracker.ResetForTests();
        }

        [Test]
        public void KillCause_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "Pit"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "Acid"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "OutOfBounds"));
        }

        [Test]
        public void CheckpointManager_StartsNull_AndUpdates()
        {
            Assert.IsNull(CheckpointManager.CurrentSpawn);
            var t = new GameObject("s").transform;
            CheckpointManager.SetSpawn(t);
            Assert.AreEqual(t, CheckpointManager.CurrentSpawn);
            Object.DestroyImmediate(t.gameObject);
        }

        [Test]
        public void Checkpoint_IsIdempotent()
        {
            var go = new GameObject(); go.AddComponent<BoxCollider>().isTrigger = true;
            var cp = go.AddComponent<Checkpoint>();
            int n = 0; cp.OnRegistered += _ => n++;
            cp.RegisterFromTest(); cp.RegisterFromTest();
            Assert.AreEqual(1, n);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerRespawn_TeleportsToCheckpoint()
        {
            var spawn = new GameObject("spawn").transform;
            spawn.position = new Vector3(10, 5, 7);
            CheckpointManager.SetSpawn(spawn);
            var go = new GameObject();
            go.AddComponent<CharacterController>();
            var pr = go.AddComponent<PlayerRespawn>();
            pr.Kill(KillCause.Pit);
            Assert.AreEqual(new Vector3(10, 5, 7), go.transform.position);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(spawn.gameObject);
        }

        [Test]
        public void RunStats_TrackKills_FallHeight_Time()
        {
            RunStatsTracker.RecordKill(KillCause.Acid);
            RunStatsTracker.RecordKill(KillCause.Acid);
            RunStatsTracker.RecordKill(KillCause.Pit);
            RunStatsTracker.RecordFallHeight(3f);
            RunStatsTracker.RecordFallHeight(7f);
            RunStatsTracker.TickTime(2.5f);
            Assert.AreEqual(3, RunStatsTracker.FallCount);
            Assert.AreEqual(2, RunStatsTracker.CountForCause(KillCause.Acid));
            Assert.AreEqual(7f, RunStatsTracker.MaxFallHeight);
            Assert.AreEqual(2.5f, RunStatsTracker.ElapsedSeconds, 0.001f);
        }

        [Test]
        public void FinishTrigger_FiresOnce()
        {
            var go = new GameObject(); go.AddComponent<BoxCollider>().isTrigger = true;
            var ft = go.AddComponent<FinishTrigger>();
            int n = 0; ft.OnRouteComplete += () => n++;
            ft.RaiseFromTest(); ft.RaiseFromTest();
            Assert.AreEqual(1, n);
            Object.DestroyImmediate(go);
        }
    }
}
