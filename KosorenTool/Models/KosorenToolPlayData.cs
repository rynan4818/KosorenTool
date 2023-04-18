using KosorenTool.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using Zenject;
using Newtonsoft.Json;
using System.Linq;

namespace KosorenTool.Models
{
    public class Record
    {
        public long Date = 0L;
        public int ModifiedScore = 0;
        public int RawScore = 0;
        public int LastNote = 0;
        public int Param = 0;
        public string Miss = "?";
    }

    [Flags]
    public enum Param
    {
        None = 0x0000,
        BatteryEnergy = 0x0001,
        NoFail = 0x0002,
        InstaFail = 0x0004,
        NoObstacles = 0x0008,
        NoBombs = 0x0010,
        FastNotes = 0x0020,
        StrictAngles = 0x0040,
        DisappearingArrows = 0x0080,
        FasterSong = 0x0100,
        SlowerSong = 0x0200,
        NoArrows = 0x0400,
        GhostNotes = 0x0800,
        SuperFastSong = 0x1000,
        SmallCubes = 0x2000,
        ProMode = 0x4000,
        SubmissionDisabled = 0x10000,
    }

    public class KosorenToolPlayData : IInitializable
    {
        public Dictionary<string, IList<Record>> Records { get; set; } = new Dictionary<string, IList<Record>>();
        public void Initialize()
        {
            InitPlaydata();
        }

        public List<Record> GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = PluginConfig.Instance;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (Records.TryGetValue(difficulty, out IList<Record> records))
            {
                var filtered = config.ShowFailed ? records : records.Where(s => s.LastNote <= 0);
                var ordered = filtered.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore);
                return ordered.ToList();
            }
            return new List<Record>();
        }

        public string ConcatParam(Param param)
        {
            if (param == Param.None)
                return "";
            var mods = new List<string>();
            if (param.HasFlag(Param.BatteryEnergy)) mods.Add("BE");
            if (param.HasFlag(Param.NoFail)) mods.Add("NF");
            if (param.HasFlag(Param.InstaFail)) mods.Add("IF");
            if (param.HasFlag(Param.NoObstacles)) mods.Add("NO");
            if (param.HasFlag(Param.NoBombs)) mods.Add("NB");
            if (param.HasFlag(Param.FastNotes)) mods.Add("FN");
            if (param.HasFlag(Param.StrictAngles)) mods.Add("SA");
            if (param.HasFlag(Param.DisappearingArrows)) mods.Add("DA");
            if (param.HasFlag(Param.SuperFastSong)) mods.Add("SF");
            if (param.HasFlag(Param.FasterSong)) mods.Add("FS");
            if (param.HasFlag(Param.SlowerSong)) mods.Add("SS");
            if (param.HasFlag(Param.NoArrows)) mods.Add("NA");
            if (param.HasFlag(Param.GhostNotes)) mods.Add("GN");
            if (param.HasFlag(Param.SmallCubes)) mods.Add("SC");
            if (param.HasFlag(Param.ProMode)) mods.Add("PM");
            if (param.HasFlag(Param.SubmissionDisabled)) mods.Add("KR");
            if (mods.Count > 4)
            {
                mods = mods.Take(3).ToList(); // Truncate
                mods.Add("..");
            }
            return string.Join(",", mods);
        }

        public Param ModsToParam(GameplayModifiers mods)
        {
            Param param = Param.None;
            param |= mods.energyType == GameplayModifiers.EnergyType.Battery ? Param.BatteryEnergy : 0;
            param |= mods.noFailOn0Energy ? Param.NoFail : 0;
            param |= mods.instaFail ? Param.InstaFail : 0;
            param |= mods.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles ? Param.NoObstacles : 0;
            param |= mods.noBombs ? Param.NoBombs : 0;
            param |= mods.fastNotes ? Param.FastNotes : 0;
            param |= mods.strictAngles ? Param.StrictAngles : 0;
            param |= mods.disappearingArrows ? Param.DisappearingArrows : 0;
            param |= mods.songSpeed == GameplayModifiers.SongSpeed.SuperFast ? Param.SuperFastSong : 0;
            param |= mods.songSpeed == GameplayModifiers.SongSpeed.Faster ? Param.FasterSong : 0;
            param |= mods.songSpeed == GameplayModifiers.SongSpeed.Slower ? Param.SlowerSong : 0;
            param |= mods.noArrows ? Param.NoArrows : 0;
            param |= mods.ghostNotes ? Param.GhostNotes : 0;
            param |= mods.smallCubes ? Param.SmallCubes : 0;
            param |= mods.proMode ? Param.ProMode : 0;
            param |= PluginConfig.Instance.DisableSubmission ? Param.SubmissionDisabled : 0;
            return param;
        }

        public void SaveRecord(IDifficultyBeatmap beatmap, LevelCompletionResults result)
        {
            if (beatmap == null || result == null)
                return;
            if (result.levelEndStateType == LevelCompletionResults.LevelEndStateType.Incomplete)
                return;
            var cleared = result.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared;
            var record = new Record
            {
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ModifiedScore = result.modifiedScore,
                RawScore = result.multipliedScore < 0 ? -result.multipliedScore : result.multipliedScore,
                LastNote = cleared ? -1 : result.goodCutsCount + result.badCutsCount + result.missedCount,
                Param = (int)ModsToParam(result.gameplayModifiers),
                Miss = result.fullCombo ? "FC" : (result.missedCount + result.badCutsCount).ToString()
            };
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (!Records.ContainsKey(difficulty))
                Records.Add(difficulty, new List<Record>());
            Records[difficulty].Add(record);
            SavePlaydata();
            Plugin.Log?.Info($"Saved a new record {difficulty} ({result.modifiedScore}).");
        }

        public void InitPlaydata()
        {
            if (!File.Exists(PluginConfig.Instance.PlayDataFile))
                return;
            var json = File.ReadAllText(PluginConfig.Instance.PlayDataFile);
            try
            {
                Records = JsonConvert.DeserializeObject<Dictionary<string, IList<Record>>>(json);
                if (Records == null)
                    throw new JsonReaderException("Empty json playdata");
            }
            catch (JsonException ex)
            {
                Plugin.Log?.Error(ex.ToString());
                var backup = new FileInfo(Path.ChangeExtension(PluginConfig.Instance.PlayDataFile, ".bak"));
                if (backup.Exists && backup.Length > 0)
                {
                    Plugin.Log?.Info("Restoring playdata backup");
                    json = File.ReadAllText(backup.FullName);
                    Records = JsonConvert.DeserializeObject<Dictionary<string, IList<Record>>>(json);
                    if (Records == null)
                        throw new Exception("Failed restore playdata");
                }
                else
                    Records = new Dictionary<string, IList<Record>>();
                SavePlaydata();
            }
        }
        public void SavePlaydata()
        {
            try
            {
                if (Records.Count > 0)
                {
                    var serialized = JsonConvert.SerializeObject(Records, Formatting.Indented);
                    File.WriteAllText(PluginConfig.Instance.PlayDataFile, serialized);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error(ex.ToString());
            }
        }
        public void BackupPlaydata()
        {
            if (!File.Exists(PluginConfig.Instance.PlayDataFile))
                return;
            var backupFile = Path.ChangeExtension(PluginConfig.Instance.PlayDataFile, ".bak");
            try
            {
                if (File.Exists(backupFile))
                {
                    if (new FileInfo(PluginConfig.Instance.PlayDataFile).Length > new FileInfo(backupFile).Length)
                        File.Copy(PluginConfig.Instance.PlayDataFile, backupFile, true);
                    else
                        Plugin.Log?.Info("Nothing backup");
                }
                else
                {
                    File.Copy(PluginConfig.Instance.PlayDataFile, backupFile);
                }
            }
            catch (IOException ex)
            {
                Plugin.Log?.Error(ex.ToString());
            }
        }
    }
}
