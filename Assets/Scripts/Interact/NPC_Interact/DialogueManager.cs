using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;

// Cấu trúc mới để sếp ghép Tên nhân vật <-> Giọng nói
[System.Serializable]
public struct CharacterVoice
{
    [Tooltip("Tên nhân vật (Phải nhập đúng y hệt trong Dialogue Data)")]
    public string characterName;
    [Tooltip("File âm thanh lải nhải của nhân vật này")]
    public AudioClip voiceClip;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // === EVENTS ===
    public static event Action DialogueStarted;
    public static event Action DialogueEnded;
    public static event Action DialogueLineTypingStarted;
    public static event Action DialogueLineTypingEnded;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Audio System")]
    public AudioSource typingSource;

    [Tooltip("'bla bla'")]
    public AudioClip defaultVoiceClip;

    [Tooltip("Danh sách các giọng nói đặc biệt")]
    public CharacterVoice[] customVoices;

    [Header("Settings")]
    public float typeSpeed = 0.03f;

    public bool IsDialogueActive { get; private set; }

    private DialogueData currentData;
    private int currentLine;
    private bool isTyping;
    private Coroutine typeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.enterKey.wasPressedThisFrame)
        {
            ContinueDialogue();
        }
    }

    public void ContinueDialogue()
    {
        if (!IsDialogueActive) return;

        if (isTyping)
            SkipTyping();
        else
            NextLine();
    }

    public void ForceEndDialogue()
    {
        if (IsDialogueActive)
            EndDialogue();
    }

    public void StartDialogue(DialogueData data)
    {
        currentData = data;
        currentLine = 0;
        IsDialogueActive = true;

        dialoguePanel.SetActive(true);
        ShowLine();
        DialogueStarted?.Invoke();
    }

    void ShowLine()
    {
        DialogueLine line = currentData.lines[currentLine];
        nameText.text = line.speakerName;


        AudioClip selectedClip = defaultVoiceClip; // Mặc định xài tiếng bla bla

        // Quét xem tên người đang nói có nằm trong danh sách giọng đặc biệt không
        if (customVoices != null && customVoices.Length > 0)
        {
            foreach (var cv in customVoices)
            {
                // So sánh tên (Không phân biệt chữ hoa/thường)
                if (string.Equals(cv.characterName.Trim(), line.speakerName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    selectedClip = cv.voiceClip;
                    break;
                }
            }
        }

        if (typingSource != null)
        {
            typingSource.clip = selectedClip;
        }

        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        typeCoroutine = StartCoroutine(TypeText(line.text));
        RestartTypingSfx();
        DialogueLineTypingStarted?.Invoke();
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        DialogueLineTypingEnded?.Invoke();
        StopTypingSfx();
    }

    void SkipTyping()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        dialogueText.text = currentData.lines[currentLine].text;
        isTyping = false;
        DialogueLineTypingEnded?.Invoke();
        StopTypingSfx();
    }

    void NextLine()
    {
        StopTypingSfx();
        currentLine++;

        if (currentLine < currentData.lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        IsDialogueActive = false;
        dialoguePanel.SetActive(false);
        nameText.text = string.Empty;
        dialogueText.text = string.Empty;

        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        isTyping = false;

        StopTypingSfx();
        if (typingSource != null)
        {
            typingSource.loop = false;
        }

        DialogueLineTypingEnded?.Invoke();

        DialogueEnded?.Invoke();
    }

    private void PlayTypingSfx()
    {
        if (typingSource != null && typingSource.clip != null) typingSource.Play();
    }

    private void StopTypingSfx()
    {
        if (typingSource != null) typingSource.Stop();
    }

    private void RestartTypingSfx()
    {
        if (typingSource != null && typingSource.clip != null)
        {
            typingSource.time = 0;
            typingSource.loop = true;
            typingSource.Play();
        }
    }
}
