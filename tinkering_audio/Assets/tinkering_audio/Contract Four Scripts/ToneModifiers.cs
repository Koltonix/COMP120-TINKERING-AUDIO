﻿using System.Collections.Generic;
using UnityEngine;

public class ToneModifiers : MonoBehaviour
{
    #region Singleton Instance
    public static ToneModifiers Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
    #endregion

    #region Volume
    /// <summary>
    /// Uses a list of float samples that change the volume of the wave
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="amplitude"></param>
    /// <returns>
    /// A list of floats that represent the samples in an AudioClip
    /// </returns>
    public float[] ChangeVolume(float[] samples, float amplitude)
    {
        for (int i = 0; i < samples.Length - 1; i++)
        {
            samples[i] *= amplitude;
        }

        return samples;
    }

    /// <summary>
    /// Uses an audio clip to get the samples if the samples are not available elsewhere
    /// </summary>
    /// <param name="audioClip"></param>
    /// <remarks>
    /// https://docs.unity3d.com/ScriptReference/AudioClip.GetData.html
    /// Documentation used to be able to access the data from the AudioClip
    /// </remarks>
    /// <returns>
    /// An updated AudioClip with the changed volume
    /// </returns>
    public AudioClip ChangeVolume(AudioClip audioClip, float amplitude)
    {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        for (int i = 0; i < samples.Length - 1; i++)
        {
            samples[i] = samples[i] * amplitude;
        }

        audioClip.SetData(samples, 0);
        return audioClip;
    }

    #endregion

    #region Multiplying Audio
    public Sound MultiplyAudioClips(Sound[] sounds)
    {
        Sound combinedSoundSettings = new Sound();
        GetLargestSoundValues(combinedSoundSettings, sounds);
        combinedSoundSettings.samples = new float[50000];
        for (int i = 0; i < sounds.Length; i++)
        {        
            for (int j = 0; j < sounds[i].samples.Length - 1; j++)
            {
                combinedSoundSettings.samples[i] += sounds[i].samples[j];
            }
        }

        combinedSoundSettings.audioClip = ToneGenerator.Instance.CreateToneAudioClip(combinedSoundSettings);
        return combinedSoundSettings;
    }

    private void GetLargestSoundValues(Sound soundSettings, Sound[] sounds)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (soundSettings.frequency < sounds[i].frequency)
            {
                soundSettings.frequency = sounds[i].frequency;
            }

            //Currently not working and providees a null exception error
            //if (soundSettings.samples.Length < sounds[i].samples.Length)
            //{
            //    soundSettings.samples = new float[sounds[i].samples.Length];
            //}

            if (soundSettings.sampleRate < sounds[i].sampleRate)
            {
                soundSettings.sampleRate = sounds[i].sampleRate;
            }

            if (soundSettings.sampleDurationSecs < sounds[i].samples.Length)
            {
                soundSettings.sampleDurationSecs = sounds[i].sampleDurationSecs;
            }
        }
    }

    #endregion

    #region Pitch
    /// <summary>
    /// Increases the pitch of an audioclip where it uses a modifier
    /// to duplicate each element a certain amount of times
    /// </summary>
    /// <param name="soundSettings"></param>
    /// <param name="tempoModifier"></param>
    /// <remarks>
    /// This will only increase, decreasing will be a separate function.
    /// Note that this will currently keep the maximum amount of samples
    /// and will therefore remove any extras on the end that do not fit
    /// </remarks>
    public void IncreasePitch(Sound soundSettings, int tempoModifier)
    {
        float[] alteredSamples = new float[Mathf.FloorToInt(soundSettings.samples.Length * tempoModifier)];

        int samplesIndex = 0;
        int alteredSamplesIndex = 0;
        while (samplesIndex < alteredSamples.Length - 1)
        {
            if (samplesIndex < soundSettings.samples.Length - 1)
            {
                alteredSamples[alteredSamplesIndex] = soundSettings.samples[samplesIndex];
            }

            if (alteredSamplesIndex % tempoModifier == 0)
            {
                samplesIndex++;
            }

            alteredSamplesIndex++;
        }

        soundSettings.samples = alteredSamples;
    }
    #endregion
}