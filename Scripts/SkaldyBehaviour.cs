using System;
using System.Collections.Generic;
using System.Text;
using Skaldy;
using UnityEngine;

public class SkaldyBehaviour : MonoBehaviour, Hoverable, Interactable
{
    public string m_name;
    public float m_standRange;
    public float m_greetRange;
    public float m_byeRange;
    public float m_hideDialogDelay;
    public float m_randomTalkInterval;
    public List<string> m_randomTalk;
    public List<string> m_randomGreets;
    public List<string> m_randomGoodbye;
    public bool m_didGreet;
    public bool m_didGoodbye;
    public LookAt m_lookAt;
    public static AudioController instance;

    public void Start()
    {
        InvokeRepeating("RandomTalk", m_randomTalkInterval, m_randomTalkInterval);
        instance = gameObject.GetComponent<AudioController>();
    }

    public void Update()
    {
        Player closestPlayer = Player.GetClosestPlayer(transform.position, m_standRange);
        if (closestPlayer)
        {
            float num = Vector3.Distance(closestPlayer.transform.position, transform.position);
            if (!m_didGreet && num < (double)m_greetRange)
            {
                m_didGreet = true;
                Say(m_randomGreets, "Greet");
            }

            if (!m_didGreet || m_didGoodbye || num <= (double)m_byeRange)
                return;
            m_didGoodbye = true;
            Say(m_randomGoodbye, "Greet");
            StopCoroutine(AudioController.AudioStart);
            gameObject.GetComponent<AudioSource>().Stop();
        }
    }

    public void RandomTalk()
    {
        if (!Player.IsPlayerInRange(transform.position, m_greetRange))
            return;
        Say(m_randomTalk, "Talk");
    }

    public string GetHoverText()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(
            Localization.instance.Localize(m_name +
                                           $" [<color=green>Playing:</color> {GetCurrentSong()}]" +
                                           "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact"));
        return stringBuilder.ToString();
    }

    public string GetHoverName() => Localization.instance.Localize(m_name);

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold)
            return false;
        //if (Input.GetKey(KeyCode.LeftShift))
        try
        {
            CycleAccessMode();
            return true;
        }
        catch (Exception ex)
        {
            SkaldyPlugin.SkaldyLogger.LogError($"Interact Patch LeftShift : {ex}");
            return true;
        }
    }

    public void Say(List<string> texts, string trigger) =>
        Say(texts[UnityEngine.Random.Range(0, texts.Count)], trigger);

    public void Say(string text, string trigger)
    {
        Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 20f, m_hideDialogDelay, "", text, false);
        if (trigger.Length <= 0)
            return;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

    public string GetCurrentSong()
    {
        if (!GetComponent<ZNetView>().IsValid() || !GetComponent<ZNetView>().IsOwner())
            return "";
        return GetComponent<ZNetView>().GetZDO().GetString("CurrentSong", SkaldyPlugin.audioFileName.Value);
    }

    private void CycleAccessMode()
    {
        if (!GetComponent<ZNetView>().IsValid() || !GetComponent<ZNetView>().IsOwner())
            return;
        string currentSongName = GetCurrentSong();

        for (int i = 0; i < SkaldyPlugin.fileDir.Count; i++)
            if (SkaldyPlugin.fileDir[i] == currentSongName)
            {
                i++;
                if (i >= SkaldyPlugin.fileDir.Count)
                    i = 0;
                SetCurrentSong(this, SkaldyPlugin.fileDir[i]);
                AudioController.AudioStart = instance.StartCoroutine(instance.LoadAudio());
                return;
            }


        SetCurrentSong(this, GetCurrentSong());
    }

    public static void SetCurrentSong(SkaldyBehaviour skaldy,
        string songName)
    {
        if (skaldy.GetComponent<ZNetView>() && skaldy.GetComponent<ZNetView>().m_zdo != null)
        {
            skaldy.GetComponent<ZNetView>().m_zdo.Set("CurrentSong", songName);
        }
    }
}