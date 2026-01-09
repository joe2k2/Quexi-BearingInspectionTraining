using UnityEngine;
using System;
public enum Language
{
    English,
    Tamil,
    Hindi
}
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public event Action<Language> OnLanguageChanged;
    [SerializeField] private Language currentLanguage = Language.English;
    public Language CurrentLanguage
    {
        get => currentLanguage;
        private set
        {
            if (currentLanguage != value)
            {
                currentLanguage = value;
                OnLanguageChanged?.Invoke(currentLanguage);
                Debug.Log($"Language changed to: {currentLanguage}");
            }
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetLanguage(Language language)
    {
        CurrentLanguage = language;
    }

    public void CycleLanguage()
    {
        int nextLang = (int)currentLanguage + 1;
        if (nextLang > (int)Language.Hindi) nextLang = 0;
        SetLanguage((Language)nextLang);
    }
}