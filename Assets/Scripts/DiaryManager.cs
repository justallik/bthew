using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Синглтон. Хранит страницы дневника и управляет состоянием разблокировки.
/// Размести на отдельном GameObject (например, "DiarySystem") в сцене.
/// </summary>
public class DiaryManager : MonoBehaviour
{
    public static DiaryManager instance;

    public const int MaxEntries = 9;

    /// <summary>Формат даты для страниц дневника (например "17 апр 2026").</summary>
    public const string DateFormat = "d MMM yyyy";

    private bool isUnlocked = false;
    private readonly List<DiaryEntry> entries = new List<DiaryEntry>();

    /// <summary>Вызывается при любом изменении (разблокировка, новая запись).</summary>
    public event Action onDiaryChanged;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ──────────────────────────────────────────────────────────────
    // Публичный API
    // ──────────────────────────────────────────────────────────────

    public bool IsUnlocked() => isUnlocked;

    public int EntryCount => entries.Count;

    public DiaryEntry GetEntry(int index) =>
        (index >= 0 && index < entries.Count) ? entries[index] : null;

    /// <summary>Разблокировать вкладку дневника (вызывается при подборе 3D-дневника).</summary>
    public void UnlockDiary()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        Debug.Log("📖 Дневник разблокирован!");
        onDiaryChanged?.Invoke();
    }

    /// <summary>Добавить новую страницу. Максимум 9.</summary>
    public void AddEntry(string title, string content, string date)
    {
        if (entries.Count >= MaxEntries)
        {
            Debug.LogWarning("DiaryManager: достигнут максимум страниц (9).");
            return;
        }
        entries.Add(new DiaryEntry(title, content, date));
        Debug.Log($"📝 Добавлена запись в дневник: «{title}»");
        onDiaryChanged?.Invoke();
    }

    /// <summary>Пометить страницу как прочитанную (убрать метку NEW).</summary>
    public void MarkAsRead(int index)
    {
        if (index >= 0 && index < entries.Count)
            entries[index].isNew = false;
    }

    /// <summary>Есть ли хоть одна непрочитанная запись.</summary>
    public bool HasNewEntries()
    {
        foreach (DiaryEntry e in entries)
            if (e.isNew) return true;
        return false;
    }
}
