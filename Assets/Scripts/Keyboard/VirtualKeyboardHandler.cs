using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualKeyboardHandler : MonoBehaviour
{
    [SerializeField] private NonNativeKeyboard keyboard;
    [SerializeField] private TMP_InputField mailIDInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    private TMP_InputField activeInputField;

    private void Start()
    {
        EventManager.closeKeyboard += () => { keyboard.Close(); };

        if (mailIDInputField != null)
        {
            mailIDInputField.onSelect.AddListener((x) => OpenKeyboard(mailIDInputField));
        }

        if (passwordInputField != null)
        {
            passwordInputField.onSelect.AddListener((x) => OpenKeyboard(passwordInputField));
        }

        keyboard.OnTextUpdated += HandleTextUpdated;
        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnClosed += HandleKeyboardClosed;
    }

    private void OnDestroy()
    {
        EventManager.closeKeyboard -= () => { keyboard.Close(); };

        if (mailIDInputField != null) mailIDInputField.onSelect.RemoveAllListeners();
        if (passwordInputField != null) passwordInputField.onSelect.RemoveAllListeners();

        if (keyboard != null)
        {
            keyboard.OnTextUpdated -= HandleTextUpdated;
            keyboard.OnTextSubmitted -= HandleTextSubmitted;
            keyboard.OnClosed -= HandleKeyboardClosed;
        }
    }

    private void OpenKeyboard(TMP_InputField inputField)
    {
        activeInputField = inputField;
        keyboard.InputField = inputField;
        keyboard.PresentKeyboard(inputField.text);
    }

    private void HandleTextUpdated(string text)
    {
        if (activeInputField != null)
        {
            activeInputField.text = text;
        }
    }
    private void HandleTextSubmitted(object sender, EventArgs e)
    {
        string capturedText = keyboard.InputField.text;
        activeInputField.text = capturedText;
    }
    private void HandleKeyboardClosed(object sender, EventArgs e)
    {
        activeInputField = null;
    }
}
