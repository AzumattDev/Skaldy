﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skaldy;
using UnityEngine;
using static Skaldy.SkaldyPlugin;

public class AudioController : MonoBehaviour
{
    //public const string audioName = "t.mp3";

    [Header("Audio Stuff")] public static AudioSource audioSource;
    public static AudioClip audioClip;
    public static string soundPath;
    public static Coroutine? AudioStart;

    private void Awake()
    {
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
        soundPath = BepInEx.Paths.PluginPath + Path.DirectorySeparatorChar + "Sound" + Path.DirectorySeparatorChar;
        AudioStart = StartCoroutine(LoadAudio());
    }

    public IEnumerator LoadAudio()
    {
        WWW request = GetAudioFromFile(soundPath,
            gameObject.GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value));
        yield return request;

        audioClip = request.GetAudioClip();
        audioClip.name = GetComponent<ZNetView>().m_zdo.GetString("CurrentSong", audioFileName.Value);

        PlayAudioFile();
    }

    public static void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        audioSource.enabled = true;
        audioSource.Play();
        audioSource.loop = true;
    }

    public static WWW GetAudioFromFile(string path, string filename)
    {
        string audioToLoad = string.Format(path + "{0}", filename);
        WWW request = new(audioToLoad);
        return request;
    }
}