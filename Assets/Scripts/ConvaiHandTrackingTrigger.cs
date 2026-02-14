using UnityEngine;
using Convai.Scripts.Runtime.Core;

/// <summary>
/// This script allows triggering ConvAI voice recognition via UI buttons.
/// It uses the official ConvaiInputManager event system to ensure all 
/// internal states (like active NPC checks) are handled correctly.
/// </summary>
public class ConvaiHandTrackingTrigger : MonoBehaviour
{
    private bool _isListening = false;

    /// <summary>
    /// Call this from a UI Button Click event (Toggle mode).
    /// </summary>
    public void ToggleListening()
    {
        SetListening(!_isListening);
    }

    /// <summary>
    /// Call this from a PointerDown event (Push-to-Talk mode).
    /// </summary>
    public void StartListening()
    {
        SetListening(true);
    }

    /// <summary>
    /// Call this from a PointerUp event (Push-to-Talk mode).
    /// </summary>
    public void StopListening()
    {
        SetListening(false);
    }

    /// <summary>
    /// Core logic to talk to ConvAI system.
    /// </summary>
    private void SetListening(bool state)
    {
        if (ConvaiInputManager.Instance == null)
        {
            Debug.LogError("ConvaiInputManager not found in scene! Make sure the Convai prefab is present.");
            return;
        }

        _isListening = state;
        
        // This invokes the official ConvAI 'Talk' event
        // This will automatically find the active NPC and start/stop recording
        ConvaiInputManager.Instance.talkKeyInteract?.Invoke(state);
        
        Debug.Log($"ConvAI UI Trigger: {(state ? "START" : "STOP")} listening.");
    }

    private void OnDisable()
    {
        if (_isListening)
        {
            SetListening(false);
        }
    }
}
