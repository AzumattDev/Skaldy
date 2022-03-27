using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Skaldy.Extensions
{
    public class SongDropdown : MonoBehaviour
    {
        public Dropdown SongListDropdown;
        internal static string SelectedSongName;
        private static SkaldyBehaviour _skaldyBehaviour;

        private void Start()
        {
            PopulateSongList();
        }

        private void OnGUI()
        {
            SelectedSongName = SongListDropdown.options[SongListDropdown.value].text;
        }

        public void PopulateSongList()
        {
            SongListDropdown.ClearOptions();

            foreach (KeyValuePair<int, string> keyValuePair in SkaldyPlugin.FileDir)
            {
                SkaldyPlugin.SkaldyLogger.LogDebug($"LOGGING: {keyValuePair.Key} VALUE: {keyValuePair.Value}");
                SongListDropdown.options.Add(new Dropdown.OptionData { text = keyValuePair.Value });
            }
        }

        internal static void SetBehaviour(SkaldyBehaviour skaldyBehaviour)
        {
            _skaldyBehaviour = skaldyBehaviour;
        }

        internal SkaldyBehaviour GetBehaviour()
        {
            return _skaldyBehaviour;
        }

        public void SetSong()
        {
            SelectedSongName = SongListDropdown.options[SongListDropdown.value].text;
            SkaldyBehaviour.SetCurrentSong(GetBehaviour(), SelectedSongName);
            try
            {
                AudioController.AudioStart = GetBehaviour().GetComponent<AudioController>()
                    .StartCoroutine(GetBehaviour().GetComponent<AudioController>().LoadAudio());
            }
            catch
            {
                SkaldyPlugin.SkaldyLogger.LogWarning("There was a problem starting the coroutine.");
            }
        }
    }
}