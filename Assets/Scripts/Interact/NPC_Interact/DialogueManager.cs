using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // === EVENTS: để các hệ thống khác lắng nghe mà không cần tham chiếu trực tiếp ===
    public static event Action DialogueStarted;
    public static event Action DialogueEnded;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

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

        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        typeCoroutine = StartCoroutine(TypeText(line.text));
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
    }

    void SkipTyping()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        dialogueText.text = currentData.lines[currentLine].text;
        isTyping = false;
    }

    void NextLine()
    {
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
        DialogueEnded?.Invoke();
    }
}
