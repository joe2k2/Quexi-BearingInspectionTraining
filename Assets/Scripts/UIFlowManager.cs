using Lean.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIFlowManager : MonoBehaviour
{

    [Header("LanguageSelection Panel")]
    [SerializeField] private GameObject aiWelcomePanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject aiInstructor;

    [Header("LanguageSelection Panel")]
    [SerializeField] private GameObject languageSelectionPanel;
    [SerializeField] private Button selectButton;

    [Header("Welcome Screen")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private Button startButton;
    [SerializeField] private AudioClip welcomeAudioClip;

    [Header("Audio Settings")]
    [SerializeField] private bool autoPlayWelcomeAudio = true;
    [SerializeField] private bool stopAudioOnStart = true;
    [SerializeField] private string audioPhrase;


    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Start()
    {
        //InitializeLanguageSelectionScreen();
        InitializeAIWelcomScreen();
        SetupButtonListeners();
    }
    void InitializeAIWelcomScreen()
    {
        aiWelcomePanel.SetActive(true);
    }
    void OnDestroy()
    {
        RemoveButtonListeners();
    }
    private AudioClip GetLocalizedAudio(string audioPhraseName)
    {
        if (string.IsNullOrEmpty(audioPhraseName))
            return null;

        LeanTranslation translation = LeanLocalization.GetTranslation(audioPhraseName);

        if (translation != null && translation.Data is AudioClip audioClip)
        {
            return audioClip;
        }

        return null;
    }
    private void InitializeLanguageSelectionScreen()
    {
        languageSelectionPanel.SetActive(true);
    }

    private void SetupButtonListeners()
    {
        nextButton.onClick.AddListener(() => { aiWelcomePanel.SetActive(false); aiInstructor.SetActive(false); InitializeLanguageSelectionScreen(); });
        selectButton.onClick.AddListener(ShowWelcomeScreen);
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void RemoveButtonListeners()
    {
        nextButton.onClick.RemoveListener(() => { aiWelcomePanel.SetActive(false); aiInstructor.SetActive(false); InitializeLanguageSelectionScreen(); });
        selectButton.onClick.RemoveListener(ShowWelcomeScreen);
        startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    private void PlayWelcomeAudio()
    {
        if (welcomeAudioClip != null && audioSource != null)
        {
            audioSource.clip = GetLocalizedAudio(audioPhrase);//welcomeAudioClip;
            audioSource.Play();
        }
    }

    public void StopWelcomeAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnStartButtonClicked()
    {
        if (stopAudioOnStart)
        {
            StopWelcomeAudio();
        }
        welcomePanel.SetActive(false);
        SceneManager.LoadScene("Demo");
    }
    public void ShowWelcomeScreen()
    {
        languageSelectionPanel.SetActive(false); 
        welcomePanel.SetActive(true);

        if (autoPlayWelcomeAudio)
        {
            PlayWelcomeAudio();
        }
    }
    public bool IsWelcomeAudioPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public AudioClip GetWelcomeAudioClip()
    {
        return welcomeAudioClip;
    }
}
