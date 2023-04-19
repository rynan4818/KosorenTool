using System.Collections;
using UnityEngine;
using Zenject;

namespace KosorenTool.Models
{
    public class KosorenToolPlayerController : IInitializable
    {
        public float _noteJumpValue;
        public float _noteJumpMovementSpeed;

        private BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private AudioTimeSyncController _audioTimeSyncController;
        private KosorenToolController _kosorenToolController;
        KosorenToolPlayerController(BeatmapObjectSpawnController beatmapObjectSpawnController, AudioTimeSyncController audioTimeSyncController, KosorenToolController kosorenToolController)
        {
            this._beatmapObjectSpawnController = beatmapObjectSpawnController;
            this._audioTimeSyncController = audioTimeSyncController;
            this._kosorenToolController = kosorenToolController;
        }
        private IEnumerator SongStartWait()
        {
            yield return new WaitWhile(() => this._audioTimeSyncController.songTime > this._audioTimeSyncController.songTime);
            this._kosorenToolController._jumpDistance = this._beatmapObjectSpawnController.jumpDistance;
        }

        public void Initialize()
        {
            this._kosorenToolController._jumpDistance = 0;
            HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait());
        }
    }
}
