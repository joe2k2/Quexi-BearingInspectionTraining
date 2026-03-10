using Lean.Localization;
using TMPro; // Added for TMP_InputField
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIFlowManager : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI loginStatusText;

    [Header("AI Welcome Panel")]
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
        // First, show the login screen
        InitializeLoginScreen();
        SetupButtonListeners();
    }

    void InitializeLoginScreen()
    {
        // Hide all other panels first
        if (aiWelcomePanel != null) aiWelcomePanel.SetActive(false);
        if (languageSelectionPanel != null) languageSelectionPanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            if (loginStatusText != null) loginStatusText.text = "";
        }
        else
        {
            // If no login panel assigned, fail-safe to welcome screen
            InitializeAIWelcomeScreen();
        }
    }

    void InitializeAIWelcomeScreen()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
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
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginButtonClicked);
        nextButton.onClick.AddListener(() => { aiWelcomePanel.SetActive(false); aiInstructor.SetActive(false); InitializeLanguageSelectionScreen(); });
        selectButton.onClick.AddListener(ShowWelcomeScreen);
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void RemoveButtonListeners()
    {
        if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        nextButton.onClick.RemoveListener(() => { aiWelcomePanel.SetActive(false); aiInstructor.SetActive(false); InitializeLanguageSelectionScreen(); });
        selectButton.onClick.RemoveListener(ShowWelcomeScreen);
        startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    private void OnLoginButtonClicked()
    {
        if (emailInputField == null || passwordInputField == null) return;

        string email = emailInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetLoginStatus("Email and Password required", Color.red);
            return;
        }

        SetLoginStatus("Logging in...", Color.white);
        loginButton.interactable = false;

        APIManager.Instance.Login(email, password, (success, message) => {
            loginButton.interactable = true;
            if (success)
            {
                SetLoginStatus("Login Successful!", Color.green);
                InitializeAIWelcomeScreen();
            }
            else
            {
                SetLoginStatus($"Failed: {message}", Color.red);
            }
        });
    }

    private void SetLoginStatus(string message, Color color)
    {
        if (loginStatusText != null)
        {
            loginStatusText.text = message;
            loginStatusText.color = color;
        }
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
