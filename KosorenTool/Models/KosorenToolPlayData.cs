using KosorenTool.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using Zenject;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
        public float JD = 0;
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

    public class KosorenToolPlayData : IInitializable, IDisposable
    {
        private bool _disposedValue;
        public ConcurrentDictionary<string, IList<Record>> _records { get; set; } = new ConcurrentDictionary<string, IList<Record>>();
        public static SemaphoreSlim RecordsSemaphore = new SemaphoreSlim(1, 1);
        public bool _init;
        public void Initialize()
        {
            _= this.InitPlaydataAsync();
            Plugin.OnPluginExit += BackupPlaydata; //ファイルの書き込み処理はDisposeのときでは間に合わない
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                    Plugin.OnPluginExit -= BackupPlaydata;
                this._disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public List<Record> GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = PluginConfig.Instance;
            var dateSort = config.Sort == "Sort by Date";
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (this._records.TryGetValue(difficulty, out IList<Record> records))
            {
                var filtered = config.ShowFailed ? records : records.Where(s => s.LastNote <= 0);
                var ordered = filtered.OrderByDescending(s => dateSort ? s.Date : s.ModifiedScore);
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
                mods = mods.Take(4).ToList(); // Truncate
                mods.Add("..");
            }
            return string.Join(",", mods);
        }

        public Param ModsToParam(GameplayModifiers mods, bool kosorenModeActive)
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
            param |= kosorenModeActive ? Param.SubmissionDisabled : 0;
            return param;
        }

        public async Task SaveRecordAsync(IDifficultyBeatmap beatmap, LevelCompletionResults result, float jumpDistance, bool kosorenModeActive)
        {
            if (!this._init)
                return;
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
                Param = (int)ModsToParam(result.gameplayModifiers, kosorenModeActive),
                Miss = result.fullCombo ? "FC" : (result.missedCount + result.badCutsCount).ToString(),
                JD = jumpDistance
            };
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (!this._records.ContainsKey(difficulty))
                this._records.TryAdd(difficulty, new List<Record>());
            this._records[difficulty].Add(record);
            await this.SavePlaydataAsync();
            Plugin.Log?.Info($"Saved a new record {difficulty} ({result.modifiedScore}).");
        }

        public async Task InitPlaydataAsync()
        {
            this._init = false;
            this._records = await this.ReadRecordFileAsync(PluginConfig.Instance.PlayDataFile);
            if (this._records == null)
            {
                Plugin.Log?.Info("Restoring playdata backup");
                this._records = await this.ReadRecordFileAsync(Path.ChangeExtension(PluginConfig.Instance.PlayDataFile, ".bak"));
                if (this._records == null)
                    this._records = new ConcurrentDictionary<string, IList<Record>>();
                await this.SavePlaydataAsync();
            }
            this._init = true;
        }
        public async Task SavePlaydataAsync()
        {
            if (this._records.Count == 0)
                return;
            try
            {
                var serialized = await Task.Run(() => JsonConvert.SerializeObject(this._records, Formatting.None)).ConfigureAwait(false);
                if (!await this.WriteAllTextAsync(PluginConfig.Instance.PlayDataFile, serialized))
                    throw new Exception("Failed save songdatabase");
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error(ex.ToString());
            }
        }
        public void SavePlaydata()
        {
            if (this._records.Count == 0)
                return;
            try
            {
                var serialized = JsonConvert.SerializeObject(this._records, Formatting.None);
                File.WriteAllText(PluginConfig.Instance.PlayDataFile, serialized);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error(ex.ToString());
            }
        }
        public void BackupPlaydata()
        {
            if (!this._init)
                return;
            if (!File.Exists(PluginConfig.Instance.PlayDataFile))
                return;
            Plugin.Log?.Info("Play data backup");
            if (!this.CheckPlayDataFile())
            {
                this.SavePlaydata();
                if (!this.CheckPlayDataFile())
                    return;
            }
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
        public bool CheckPlayDataFile()
        {
            try
            {
                var text = File.ReadAllText(PluginConfig.Instance.PlayDataFile);
                var result = JsonConvert.DeserializeObject<ConcurrentDictionary<string, IList<Record>>>(text);
                if (result == null)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e.ToString());
                return false;
            }
        }
        public async Task<ConcurrentDictionary<string, IList<Record>>> ReadRecordFileAsync(string path)
        {
            ConcurrentDictionary<string, IList<Record>> result;
            var json = await this.ReadAllTextAsync(path);
            try
            {
                if (json == null)
                    throw new JsonReaderException($"Json file error {path}");
                result = JsonConvert.DeserializeObject<ConcurrentDictionary<string, IList<Record>>>(json);
                if (result == null)
                    throw new JsonReaderException($"Empty json {path}");
            }
            catch (JsonException ex)
            {
                Plugin.Log?.Error(ex.ToString());
                result = null;
            }
            return result;
        }
        public async Task<string> ReadAllTextAsync(string path)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                Plugin.Log?.Info($"File not found : {path}");
                return null;
            }
            string result;
            await RecordsSemaphore.WaitAsync();
            try
            {
                using (var sr = new StreamReader(path))
                {
                    result = await sr.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e.ToString());
                result = null;
            }
            finally
            {
                RecordsSemaphore.Release();
            }
            return result;
        }
        public async Task<bool> WriteAllTextAsync(string path, string contents)
        {
            bool result;
            await RecordsSemaphore.WaitAsync();
            try
            {
                using (var sw = new StreamWriter(path))
                {
                    await sw.WriteAsync(contents);
                }
                result = true;
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e.ToString());
                result = false;
            }
            finally
            {
                RecordsSemaphore.Release();
            }
            return result;
        }
    }
}
