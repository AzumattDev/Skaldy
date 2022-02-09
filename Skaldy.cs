﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PieceManager;
using ServerSync;
using UnityEngine;

namespace Skaldy
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class SkaldyPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Skaldy";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static Dictionary<int, string> fileDir = new();

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource SkaldyLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public void Awake()
        {
            _serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            audioFileName = config("General", "Audio File Name", "",
                "The audio file you wish to load from the Sound folder.");

            BuildPiece buildPiece = new("skaldy", "Skaldy");
            buildPiece.Name.English("Skaldy The Bard");
            buildPiece.Description.English("Skald!");
            buildPiece.RequiredItems.Add("Wood", 1, false);

            /* Load all of the sounds in the folder of the client */
            DirSearch(Paths.PluginPath + Path.DirectorySeparatorChar + "Sound" + Path.DirectorySeparatorChar);

            _harmony.PatchAll();
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;


            FileSystemWatcher folderWatcher =
                new(Paths.PluginPath + Path.DirectorySeparatorChar + "Sound" + Path.DirectorySeparatorChar);
            folderWatcher.Changed += UpdateAudioFiles;
            folderWatcher.Created += UpdateAudioFiles;
            folderWatcher.Deleted += UpdateAudioFiles;
            folderWatcher.Renamed += UpdateAudioFiles;
            folderWatcher.Error += OnError;
            folderWatcher.IncludeSubdirectories = true;
            folderWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            folderWatcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                SkaldyLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                SkaldyLogger.LogError($"There was an issue loading your {ConfigFileName}");
                SkaldyLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        static void DirSearch(string sDir)
        {
            try
            {
                int i = 0;
                fileDir.Clear();
                foreach (string f in Directory.GetFiles(sDir))
                {
                    string? justFileName = Path.GetFileName(f);
                    fileDir.Add(i, justFileName);
                    SkaldyLogger.LogWarning(justFileName);
                    i++;
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearch(d);
                }
            }
            catch (Exception excpt)
            {
                SkaldyLogger.LogError(excpt.Message);
            }
        }

        static void UpdateAudioFiles(object sender, FileSystemEventArgs e)
        {
            DirSearch(Paths.PluginPath + Path.DirectorySeparatorChar + "BardSounds" + Path.DirectorySeparatorChar);
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                SkaldyLogger.LogError($"Message: {ex.Message}");
                SkaldyLogger.LogError("Stacktrace:");
                SkaldyLogger.LogError(ex.StackTrace);
                PrintException(ex.InnerException);
            }
        }


        #region ConfigOptions

        private static ConfigEntry<bool>? _serverConfigLocked;
        internal static ConfigEntry<string>? audioFileName;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion
    }
}