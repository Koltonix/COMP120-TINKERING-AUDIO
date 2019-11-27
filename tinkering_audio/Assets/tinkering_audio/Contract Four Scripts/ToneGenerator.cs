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
    STATIC
};

[Serializable]
public class Sound
{
    public AudioClip audioClip;
    public float frequency;

    // Using a getter and setter to automatically convert waves that
    // have set in the inspector rather than doing it through code manually
    public float[] samples;
    public float[] Samples
    {
        get { return samples; }
        set
        {
            samples = value;
            if (this.audioClip != null) ToneWaves.Instance.ChangeAudioClipToSquare(this);

        }
    }


    public int sampleRate;
    public float sampleDurationSecs;
    [HideInInspector]
    public int sampleLength;

    public WaveType waveType;
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

    [Header("Wave Attributes")]
    [SerializeField]
    private Sound generatedSound;
    [SerializeField]
    private Sound secondarySound;
    [SerializeField]
    private Sound placeHolder;
    private AudioSource audioSource;

    public List<float> samplesList = new List<float>();
    public GameObject squarePrefab;

    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();

        //generatedSound.audioClip = CreateToneAudioClip(generatedSound);
        //secondarySound.audioClip = CreateToneAudioClip(secondarySound);

        //Sound[] combinedSettings = new Sound[2];
        //combinedSettings[0] = generatedSound;
        //combinedSettings[1] = secondarySound;

        //Sound newSound = ToneModifiers.Instance.MultiplyAudioClips(combinedSettings);
        //newSound.audioClip = ToneModifiers.Instance.ChangeVolume(newSound.audioClip, 0.1f);
        //placeHolder = newSound;
        //audioSource.PlayOneShot(newSound.audioClip);

        //SpawnSampleSquare(newSound.Samples, 200);

        generatedSound.audioClip = CreateToneAudioClip(generatedSound);
        
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

        soundSettings.sampleLength = Mathf.RoundToInt(soundSettings.sampleRate * soundSettings.sampleDurationSecs);
        float maxValue = 1f / 4f;

        AudioClip audioClip = AudioClip.Create("new_tone", soundSettings.sampleLength, 1, soundSettings.sampleRate, false);

        soundSettings.Samples = new float[soundSettings.sampleLength];
        for (int i = 0; i < soundSettings.sampleLength; i++)
        {
            float s = ToneWaves.Instance.GetSinValue(soundSettings.frequency, i, soundSettings.sampleRate);
            float v = s * maxValue;
            soundSettings.Samples[i] = v;
        }

        audioClip.SetData(soundSettings.Samples, 0);
        return audioClip;
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
            soundSettings.audioClip.SetData(soundSettings.Samples, 0);
        }

        else
        {
            throw new Exception("No audioclip is present to alter the samples of...");
        }
        
    }
    #endregion

    #region Saving Sound
    private void SaveAudioClip(AudioClip clip)
    {
        SaveWavUtil.Save(Application.dataPath, clip);
    }
    #endregion

    #region Debug Functions
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
