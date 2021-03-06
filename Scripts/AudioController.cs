using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skaldy;
using UnityEngine;
using static Skaldy.SkaldyPlugin;

public class AudioController : MonoBehaviour
{
    [Header("Audio Stuff")] public AudioSource audioSource;
    public AudioClip audioClip;
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
        AudioStart = StartCoroutine(LoadAudio());
    }

    public IEnumerator LoadAudio()
    {
        if (audioFileName.Value.Length <= 1) yield break;
        WWW request = GetAudioFromFile(FilesFullPath,
            gameObject.GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value));
        yield return request;

        audioClip = request.GetAudioClip();
        audioClip.name = gameObject.GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value);

        PlayAudioFile();
    }

    public void OnDestroy()
    {
        audioSource.Stop();
    }

    public void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        audioSource.enabled = true;
        audioSource.Play();
        audioSource.loop = true;
        audioSource.volume = audioFileVolume.Value;
    }

    private WWW GetAudioFromFile(string path, string filename)
    {
        string audioToLoad = string.Format(path + "{0}", filename);
        WWW request = new(audioToLoad);
        return request;
    }
}