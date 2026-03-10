using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.Features;
using Service;

public class AudioLipSyncBridge : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ConvaiNPC convaiNPC;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    [Header("Settings")]
    [Tooltip("Sensitivity of the lip sync to audio volume.")]
    [SerializeField] private float sensitivity = 10f;
    [Tooltip("Smoothing factor for the viseme values.")]
    [SerializeField] private float smoothing = 0.1f;
    [Tooltip("Minimum volume threshold to trigger lip sync.")]
    [SerializeField] private float threshold = 0.01f;
    [SerializeField] private bool debugLogs = true;

    private bool _isPlaying;
    private float _currentVisemeValue;
    private float[] _audioSamples = new float[256];

    private void Awake()
    {
        if (convaiNPC == null) convaiNPC = GetComponent<ConvaiNPC>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    [ContextMenu("Play Audio")]
    public void PlayAudioClip()
    {
        if (audioClip != null)
        {
            PlayAudio(audioClip);
        }
        else
        {
            Debug.LogError("AudioLipSyncBridge: No Audio Clip assigned.");
        }
    }

    /// <summary>
    /// Plays the specified audio clip and drives the lip sync.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    public void PlayAudio(AudioClip clip)
    {
        if (clip == null) return;
        if (convaiNPC == null || audioSource == null)
        {
            Debug.LogError("AudioLipSyncBridge: Missing dependencies!");
            return;
        }

        // Disable ConvaiNPCAudioManager to prevent it from resetting "Talking" state or interfering with AudioSource
        if (convaiNPC.AudioManager != null)
        {
            convaiNPC.AudioManager.enabled = false;
        }

        CheckLipSyncSetup();

        audioSource.clip = clip;
        audioSource.Play();
        _isPlaying = true;
        convaiNPC.SetCharacterTalking(true);
        
        if (debugLogs) Debug.Log($"AudioLipSyncBridge: Started playing {clip.name}. Character Talking set to TRUE.");

        StartCoroutine(WaitForAudioEnd(clip.length));
    }

    private void CheckLipSyncSetup()
    {
        if (convaiNPC.convaiLipSync == null)
        {
            Debug.LogError("AudioLipSyncBridge: ConvaiLipSync component missing on NPC.");
            return;
        }

        if (convaiNPC.convaiLipSync.FacialExpressionData == null)
        {
             Debug.LogWarning("AudioLipSyncBridge: FacialExpressionData is null.");
             return;
        }

        bool hasRenderer = false;
        if (convaiNPC.convaiLipSync.FacialExpressionData.Head != null && convaiNPC.convaiLipSync.FacialExpressionData.Head.Renderer != null) hasRenderer = true;
        if (convaiNPC.convaiLipSync.FacialExpressionData.Teeth != null && convaiNPC.convaiLipSync.FacialExpressionData.Teeth.Renderer != null) hasRenderer = true;
        
        if (!hasRenderer)
        {
             Debug.LogWarning("AudioLipSyncBridge: No SkinnedMeshRenderers assigned in ConvaiLipSync. Lip sync might not be visible.");
        }
    }

    private IEnumerator WaitForAudioEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopAudio();
    }

    public void StopAudio()
    {
        if (audioSource != null) audioSource.Stop();
        _isPlaying = false;
        if (convaiNPC != null) convaiNPC.SetCharacterTalking(false);
        _currentVisemeValue = 0f;
        
        // Reset visemes to silence
        UpdateViseme(0f);

        // Re-enable ConvaiNPCAudioManager
        if (convaiNPC != null && convaiNPC.AudioManager != null)
        {
            convaiNPC.AudioManager.enabled = true;
        }
        
        if (debugLogs) Debug.Log("AudioLipSyncBridge: Stopped audio.");
    }

    private void Update()
    {
        if (!_isPlaying || audioSource == null || convaiNPC == null) return;

        if (!audioSource.isPlaying)
        {
            StopAudio();
            return;
        }

        // Analyze audio volume (RMS)
        audioSource.GetOutputData(_audioSamples, 0);
        float sum = 0;
        foreach (float sample in _audioSamples)
        {
            sum += sample * sample;
        }
        float rms = Mathf.Sqrt(sum / _audioSamples.Length);

        // Map RMS to viseme intensity (0 to 1)
        float targetValue = Mathf.Clamp01(rms * sensitivity);
        
        // Apply threshold
        if (targetValue < threshold) targetValue = 0;

        // Smooth value
        _currentVisemeValue = Mathf.Lerp(_currentVisemeValue, targetValue, 1f - smoothing);

        if (debugLogs && _currentVisemeValue > 0.05f) 
            Debug.Log($"AudioLipSyncBridge: RMS={rms:F4}, Viseme={_currentVisemeValue:F2}");

        UpdateViseme(_currentVisemeValue);
    }

    private void UpdateViseme(float intensity)
    {
        if (convaiNPC.convaiLipSync == null || convaiNPC.convaiLipSync.ConvaiLipSyncApplicationBase == null) return;

        // Create a new Viseme object
        // We mainly drive the 'Aa' (Jaw Open) viseme based on volume
        Viseme viseme = new Viseme();
        viseme.Aa = intensity;
        viseme.Sil = Mathf.Max(0, 1f - intensity);

        VisemesData visemesData = new VisemesData();
        visemesData.Visemes = viseme;

        // Enqueue the frame to the lip sync application
        convaiNPC.convaiLipSync.ConvaiLipSyncApplicationBase.EnqueueFrame(visemesData);
    }
}
