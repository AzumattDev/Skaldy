using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skaldy;
using UnityEngine;
using static Skaldy.SkaldyPlugin;

public class AudioController : MonoBehaviour
{
    //public const string audioName = "t.mp3";

    [Header("Audio Stuff")] public AudioSource audioSource;
    public AudioClip audioClip;
    public string soundPath;
    public static Coroutine? AudioStart;

    private void Awake()
    {
        if (!GetComponent<ZNetView>().IsValid() || !GetComponent<ZNetView>().IsOwner())
            return;
        audioSource = gameObject.GetComponent<AudioSource>();

        try
        {
            audioSource.outputAudioMixerGroup =
                AudioMan.instance.m_ambientMixer;
        }
        catch
        {
            SkaldyLogger.LogError(
                $"AudioMan.instance.m_ambientMixer could not be assigned on outputAudioMixerGroup of {gameObject.name}'s AudioSource");
        }

        //soundPath = "file://" + Application.streamingAssetsPath + "/Sound/";
        soundPath = BepInEx.Paths.PluginPath + Path.DirectorySeparatorChar + "BardSounds" + Path.DirectorySeparatorChar;
        AudioStart = StartCoroutine(LoadAudio());
    }

    public IEnumerator LoadAudio()
    {
        WWW request = GetAudioFromFile(soundPath,
            gameObject.GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value));
        yield return request;

        audioClip = request.GetAudioClip();
        audioClip.name = gameObject.GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value);

        PlayAudioFile();
    }

    public void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        audioSource.enabled = true;
        audioSource.Play();
        audioSource.loop = true;
    }

    private WWW GetAudioFromFile(string path, string filename)
    {
        string audioToLoad = string.Format(path + "{0}", filename);
        WWW request = new(audioToLoad);
        return request;
    }
}