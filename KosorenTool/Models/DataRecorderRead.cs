/*
using KosorenTool.Configuration;
using System;
using System.IO;
using System.Data.SQLite;
using System.Data.Common;

namespace KosorenTool.Models
{
    internal class DataRecorderRead
    {
        public string ScoreRead(string levelID)
        {
            if (!File.Exists(PluginConfig.Instance.DBFilePath))
                return "";
            using (var connection = new SQLiteConnection($"Data Source={PluginConfig.Instance.DBFilePath};Version=3;"))
            {
                connection.Open();
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"SELECT menuTime, cleared, difficulty, scorePercentage, rawScore, score, missedNotes, notesCount, passedNotes, softFailed, obstacles, instaFail, noFail, batteryEnergy, disappearingArrows, noBombs, songSpeed, songSpeedMultiplier, noArrows, ghostNotes, failOnSaberClash, strictAngles, fastNotes, smallNotes, proMode, zenMode, staticLights, leftHanded, playerHeight FROM MovieCutRecord WHERE levelId = \"{levelID}\";";
                        using (SQLiteDataReader db_reader = command.ExecuteReader())
                        {
                            while (db_reader.Read())
                            {
                                (int)db_reader["menuTime"]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
                connection.Close();
            }
        }
    }
}
*/