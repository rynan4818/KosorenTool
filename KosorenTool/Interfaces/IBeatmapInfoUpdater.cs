namespace KosorenTool.Interfaces
{
    public interface IBeatmapInfoUpdater
    {
        void BeatmapInfoUpdated(IDifficultyBeatmap beatmap);
        void RefreshResult();
    }
}
