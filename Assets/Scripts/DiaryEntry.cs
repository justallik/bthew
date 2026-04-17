using System;

/// <summary>
/// Данные одной страницы (записи) в дневнике.
/// </summary>
[Serializable]
public class DiaryEntry
{
    public string title;
    public string content;
    public string date;
    public bool isNew;

    public DiaryEntry(string title, string content, string date)
    {
        this.title   = title;
        this.content = content;
        this.date    = date;
        this.isNew   = true;
    }
}
