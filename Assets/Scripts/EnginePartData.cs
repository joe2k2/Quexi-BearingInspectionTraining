using UnityEngine;

[CreateAssetMenu(fileName = "EnginePartData", menuName = "Engine/Part Data")]
public class EnginePartData : ScriptableObject
{
    [Header("Part Information (English)")]
    public string partName;
    [TextArea(3, 10)]
    public string description;

    [Header("Part Information (Tamil)")]
    public string partNameTamil;
    [TextArea(3, 10)]
    public string descriptionTamil;

    [Header("Part Information (Hindi)")]
    public string partNameHindi;
    [TextArea(3, 10)]
    public string descriptionHindi;

    [Header("Audio (English)")]
    public AudioClip explanationAudio;

    [Header("Audio (Tamil)")]
    public AudioClip explanationAudioTamil;

    [Header("Audio (Hindi)")]
    public AudioClip explanationAudioHindi;

    public string GetName(Language language)
    {
        switch (language)
        {
            case Language.Tamil: return !string.IsNullOrEmpty(partNameTamil) ? partNameTamil : partName;
            case Language.Hindi: return !string.IsNullOrEmpty(partNameHindi) ? partNameHindi : partName;
            default: return partName;
        }
    }

    public string GetDescription(Language language)
    {
        switch (language)
        {
            case Language.Tamil: return !string.IsNullOrEmpty(descriptionTamil) ? descriptionTamil : description;
            case Language.Hindi: return !string.IsNullOrEmpty(descriptionHindi) ? descriptionHindi : description;
            default: return description;
        }
    }

    public AudioClip GetAudio(Language language)
    {
        switch (language)
        {
            case Language.Tamil: return explanationAudioTamil != null ? explanationAudioTamil : explanationAudio;
            case Language.Hindi: return explanationAudioHindi != null ? explanationAudioHindi : explanationAudio;
            default: return explanationAudio;
        }
    }
}
