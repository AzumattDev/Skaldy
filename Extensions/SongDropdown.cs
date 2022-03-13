using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Skaldy.Extensions
{
    public class SongDropdown : MonoBehaviour
    {
        public static Dropdown SongListDropdown;
        internal static string SelectedSongName;

        private void Start()
        {
            PopulateSongList();
        }

        private void OnGUI()
        {
           // SelectedSongName = SongListDropdown.options[SongListDropdown.value].text;
        }

        internal static void PopulateSongList()
        {
            SongListDropdown.ClearOptions();

            foreach (KeyValuePair<int, string> keyValuePair in SkaldyPlugin.FileDir)
            {
                SkaldyPlugin.SkaldyLogger.LogWarning($"LOGGING: {keyValuePair.Key} VALUE: {keyValuePair.Value}");
                SongListDropdown.options.Add(new Dropdown.OptionData { text = keyValuePair.Value });
            }
        }
    }
}