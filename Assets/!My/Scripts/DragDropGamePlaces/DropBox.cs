using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DropBox : MonoBehaviour
{
    [SerializeField] protected DropPlace[] dropPlaces;

    [Space]
    [SerializeField] protected DropBoxPattern[] patterns;

    public event Action<DropBox> OnPlaceTrue;
    public event Action<DropBox> OnPlaceFalse;

    public virtual void Start()
    {
        foreach (var dropPlace in dropPlaces)
        {
            dropPlace.OnPut += (dragOb) => TryTestAllPlaces();
            //dropPlace.OnGrap += (dragOb) => TryTestAllPlaces();
        }
    }

    public void TryTestAllPlaces()
    {
        List<string> currentTags = new List<string>();
        List<int> emptyPlaces = new List<int>();
        Dictionary<int, string[]> tagsByPlace = new Dictionary<int, string[]>();

        for (int i = 0; i < dropPlaces.Length; i++)
        {
            var dropPlace = dropPlaces[i];
            if (!dropPlace.IsFull)
            {
                emptyPlaces.Add(i);
                continue;
            }

            var dropBoxObject = dropPlace.CurrentDragObject?.GetComponent<DropBoxObject>();
            if (dropBoxObject != null)
            {
                tagsByPlace[i] = dropBoxObject.Tags.ToArray();
                currentTags.AddRange(dropBoxObject.Tags);
            }
        }

        foreach (var pattern in patterns)
        {
            if (pattern.Matches(currentTags, emptyPlaces, tagsByPlace, dropPlaces.Length))
            {
                OnPlaceTrue?.Invoke(this);
                PlacesFull();
                return;
            }
        }

        OnPlaceFalse?.Invoke(this);
        DragDropGameManager.instance.PlaceFalse(this);
    }

    public virtual void PlacesFull() { }
}

[System.Serializable]
public class DropBoxPattern
{
    [Header("=== ќбщие требовани€ ===")]
    [Tooltip(" акие теги должны присутствовать (в любом месте)")]
    public string[] requiredTags;

    [Tooltip(" акие теги запрещены (в любом месте)")]
    public string[] forbiddenTags;

    [Tooltip("—колько мест должно быть пустыми (например, 2)")]
    public int requiredEmptyCount = -1; // -1 = не провер€ть

    [Header("===  онкретные требовани€ по местам ===")]
    [Tooltip("“еги, которые должны находитьс€ на конкретных индексах")]
    public List<SpecificPlaceRule> specificPlaceRules = new List<SpecificPlaceRule>();

    [Serializable]
    public class SpecificPlaceRule
    {
        public int placeIndex;
        public string[] mustHaveTags;
        public bool allowAdditionalTags = true; // если false Ч место должно содержать только эти теги
    }

    public bool Matches(List<string> allTags, List<int> emptyPlaces, Dictionary<int, string[]> tagsByPlace, int totalPlaces)
    {
        // ѕровер€ем количество пустых мест
        if (requiredEmptyCount >= 0 && emptyPlaces.Count != requiredEmptyCount)
            return false;

        // ѕровер€ем запрещЄнные теги
        if (forbiddenTags != null && forbiddenTags.Any(tag => allTags.Contains(tag)))
            return false;

        // ѕровер€ем наличие об€зательных тегов
        if (requiredTags != null && requiredTags.Length > 0)
        {
            var requiredCounts = requiredTags.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
            var currentCounts = allTags.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in requiredCounts)
            {
                if (!currentCounts.TryGetValue(kvp.Key, out int count) || count < kvp.Value)
                    return false;
            }
        }

        // ѕровер€ем конкретные места
        foreach (var rule in specificPlaceRules)
        {
            if (rule.placeIndex < 0 || rule.placeIndex >= totalPlaces)
                continue; // защита от ошибок

            // ≈сли место пустое, то €вно не соответствует
            if (emptyPlaces.Contains(rule.placeIndex))
                return false;

            // ѕолучаем теги на месте
            if (!tagsByPlace.TryGetValue(rule.placeIndex, out var placeTags))
                return false;

            // ѕровер€ем наличие всех нужных тегов
            foreach (var mustTag in rule.mustHaveTags)
            {
                if (!placeTags.Contains(mustTag))
                    return false;
            }

            // ѕровер€ем, что нет лишних тегов (если запрещено)
            if (!rule.allowAdditionalTags && placeTags.Length != rule.mustHaveTags.Length)
                return false;
        }

        return true;
    }
}