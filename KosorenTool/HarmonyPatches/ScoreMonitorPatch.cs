namespace KosorenTool.HarmonyPatches
{
    public class ScoreMonitorPatch
    {
        public static bool TournamentAssistantActive = false;
        public static void AwakePostfix()
        {
            TournamentAssistantActive = true;
        }
    }
}
