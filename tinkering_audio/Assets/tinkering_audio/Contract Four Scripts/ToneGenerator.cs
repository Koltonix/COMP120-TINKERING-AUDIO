﻿using System;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------
// <copyright file="ToneGenerator.cs">
// MIT License Copyright (c) 2019.
// </copyright>
// <author>Christopher Philip Robertson</author>
// <summary>
// Handles the sound clip generation and the respective methods associated
// with updating the sound and some primitive debugging tools for it too.
// </summary>
//----

public enum WaveType
{
    SINE = 0,
    SQUARE = 1,
    PERLIN_NOISE = 2,
    DYNAMIC
};

public enum PianoKey
{
    C4,
    D4,
    E4,
    F4,
    G4,
    A4,
    B4
};

[Serializable]
public struct PianoNotes
{
    public PianoKey key;
    public int frequency;
}

[Serializable]
public class Sound
{
    public AudioClip audioClip;
    public float frequency;
    public WaveType waveType;

    public float[] samples;
    public int sampleRate;
    public float sampleDurationSecs;
    [HideInInspector]
    public int sampleLength;
}

[RequireComponent(typeof(AudioSource))]
public class ToneGenerator : MonoBehaviour
{
    [Header("Singleton Referencing")]
    public static ToneGenerator Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    [Header("Debug Sound Data")]
    [SerializeField]
    [Range(0, 1)]
    private float globalSound = .25f;
    [SerializeField]
    private Sound primarySound;
    [SerializeField]
    private Sound secondarySound;
    public GameObject squarePrefab;

    [Header("Piano Keys")]
    [SerializeField]
    private PianoNotes[] pianoNotes;
    [SerializeField]
    private PianoKey[] pianoKeys;
    [SerializeField]
    private Sound pianoSound;

    [Header("Caching")]
    [SerializeField]
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    #region Generating Audioclip
    /// <summary>
    /// A function that uses a variety of sound settings and 
    /// the frequency to generate a clip of sound
    /// </summary>
    /// <param name="soundSettings"></param>
    /// <param name="frequency"></param>
    /// <remarks>
    /// Sourced from https://github.com/yaustar/ACM-COMP120-Tinker-Audio-Template/blob/master/Assets/Game/Scripts/AudioTinker.cs
    /// </remarks>
    /// <returns>
    /// A Unity AudioClip data type
    /// </returns>
    public AudioClip CreateToneAudioClip(Sound soundSettings)
    {
        if (soundSettings.sampleDurationSecs <= 0)
        {
            throw new Exception("Audioclip must be longer than 0 seconds");
        }

        //Needs to be rounded to the Ceiling otherwise it might round down and lose sample data
        soundSettings.sampleLength = Mathf.CeilToInt(soundSettings.sampleRate * soundSettings.sampleDurationSecs);
        float maxValue = 1f / 4f;

        soundSettings.audioClip = AudioClip.Create("new_tone", soundSettings.sampleLength, 1, soundSettings.sampleRate, false);

        soundSettings.samples = new float[soundSettings.sampleLength];
        for (int i = 0; i < soundSettings.sampleLength; i++)
        {
            float s = ToneWaves.Instance.GetSinValue(soundSettings.frequency, i, soundSettings.sampleRate);
            float v = s * maxValue;
            soundSettings.samples[i] = v;
        }

        soundSettings.audioClip.SetData(soundSettings.samples, 0);
        ToneWaves.Instance.RefactorAudioClipWave(soundSettings);
        return soundSettings.audioClip;
    }

    #endregion

    #region Audio Key Generation

    public AudioClip GenerateAudioFromKey(Sound soundSettings, PianoKey[] pianoKeys)
    {
        soundSettings.audioClip = CreateToneAudioClip(soundSettings);
        float maxValue = 1f / 4f;

        int maxSampleIncrease = Mathf.CeilToInt(soundSettings.samples.Length / pianoKeys.Length);
        int maxSampleLimit = maxSampleIncrease;
        int startingPosition = 0;

        for (int i = 0; i < pianoKeys.Length; i++)
        {
            for (int j = startingPosition; j < maxSampleLimit; j++)
            {
                if (j >= soundSettings.samples.Length) break;

                float s = ToneWaves.Instance.GetSinValue(GetNoteFrequency(pianoKeys[i]), j, soundSettings.sampleRate);
                float v = s * maxValue;
                soundSettings.samples[j] = v;

                if (j == maxSampleLimit - 1 && maxSampleLimit <= soundSettings.sampleRate)
                {
                    startingPosition = j;
                    maxSampleLimit += maxSampleIncrease;
                    i++;
                }
            }

           
        }

        soundSettings.audioClip.SetData(soundSettings.samples, 0);
        return soundSettings.audioClip;
    }

    private float GetNoteFrequency(PianoKey pianoKey)
    {
        foreach (PianoNotes key in pianoNotes)
        {
            if (key.key == pianoKey)
            {
                return key.frequency;
            }
        }

        throw new Exception("No key of that type exists");
    }

    #endregion

    #region Refactor Samples
    /// <summary>
    /// Uses the AudioClp in 
    /// </summary>
    /// <param name="soundSettings"></param>
    public void RefactorSamplesInClip(Sound soundSettings)
    {
        if (soundSettings.audioClip != null)
        {
            soundSettings.audioClip.SetData(soundSettings.samples, 0);
        }

        else
        {
            throw new Exception("No audioclip is present to alter the samples of...");
        }
        
    }
    #endregion

    #region Debug Functions

    public void DebugInitialisaion()
    {
        ToneGenerator.Instance = this;
        ToneModifiers.Instance = FindObjectOfType<ToneModifiers>();
        ToneWaves.Instance = FindObjectOfType<ToneWaves>();
    }

    public void CombineAudioClips()
    {
        primarySound.audioClip = CreateToneAudioClip(primarySound);
        secondarySound.audioClip = CreateToneAudioClip(secondarySound);

        Sound[] combinedSettings = new Sound[2];
        combinedSettings[0] = primarySound;
        combinedSettings[1] = secondarySound;

        Sound combinedSound = ToneModifiers.Instance.MultiplyAudioClips(combinedSettings);
        combinedSound.audioClip = ToneModifiers.Instance.ChangeVolume(combinedSound.audioClip, globalSound);
        audioSource.PlayOneShot(combinedSound.audioClip);
        SaveWav.Save("combined_sound_clip", combinedSound.audioClip);
    }

    public void InsertAudioClips()
    {
        Sound insertedSound = ToneModifiers.Instance.InsertAudioClip(primarySound, secondarySound, primarySound.sampleLength);
        insertedSound.audioClip = ToneModifiers.Instance.ChangeVolume(insertedSound.audioClip, globalSound);

        audioSource.PlayOneShot(insertedSound.audioClip);
        SaveWav.Save("inserted_audio_clip", insertedSound.audioClip);
    }

    public void PlayKeyboardKeys()
    {
        if (pianoKeys.Length > 0)
        {
            pianoSound.audioClip = GenerateAudioFromKey(pianoSound, pianoKeys);
            pianoSound.audioClip = ToneModifiers.Instance.ChangeVolume(pianoSound.audioClip, globalSound);

            audioSource.PlayOneShot(pianoSound.audioClip);
            SaveWav.Save("piano_audio_clip", pianoSound.audioClip);
        }    
    }

    public void PlayAudioClipOne()
    {
        primarySound.audioClip = CreateToneAudioClip(primarySound);

        primarySound.audioClip = ToneModifiers.Instance.ChangeVolume(primarySound.audioClip, globalSound);
        audioSource.PlayOneShot(primarySound.audioClip);
        SaveWav.Save("primary_sound_clip", primarySound.audioClip);
    }

    public void PlayAudioClipTwo()
    {
        secondarySound.audioClip = CreateToneAudioClip(secondarySound);

        secondarySound.audioClip = ToneModifiers.Instance.ChangeVolume(secondarySound.audioClip, globalSound);
        audioSource.PlayOneShot(secondarySound.audioClip);
        SaveWav.Save("secondary_sound_clip", primarySound.audioClip);
    }

    /// <summary>
    /// Uses the samples from the audioclip to generate a visual intepretation of the wave using squares.
    /// </summary>
    /// <remarks>
    /// It can and should be limited otherwise it will cause peformance issues.
    /// </remarks>
    /// <param name="samples"></param>
    /// <param name="amountToSpawn"</param>
    private void SpawnSampleSquare(float[] samples, int amountToSpawn)
    {
        if (amountToSpawn > samples.Length - 1) amountToSpawn = samples.Length - 1;

        Vector3 spawnPosition = Vector3.zero;
        float squareIntervals = 2.5f;

        for (int i = 0; i < amountToSpawn; i++)
        {
            spawnPosition = new Vector3(spawnPosition.x + squareIntervals, samples[i] * 100 , 0);
            Instantiate(squarePrefab, spawnPosition, Quaternion.identity);
        }
    }
    #endregion

}
