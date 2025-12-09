using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private List<DropBoxObject> dropBoxObjectsPrefabs;
    [SerializeField] private List<int> probabilityPrefabs = new () { 50, 50, 0 };

    [SerializeField] private bool infiniteMode = false;
    [SerializeField] private int linesPerBox = 2;
    [SerializeField] private int emptySlots = 2;

    // Список заранее подготовленных линий для каждого ящика
    [SerializeField] private List<DropBoxGamePattern[]> predefinedLinesPerBox;
    private static DropBoxGame[] boxes;

    [Space]
    [SerializeField] private bool isTest = true;

    public static LevelGenerator Instance { get; private set; }

    public static DropBoxGame[] AllBoxes => boxes;

    public static bool IsNeedEmptySlots => (GetEmptySlotsCount() - 3) < 
       Mathf.Clamp(Instance.emptySlots * ((AllBoxes.Length - AvailableBoxes.Length) / AllBoxes.Length), 1, AllBoxes.Length / 3);

    public static DropBoxGame[] AvailableBoxes;
    public static void UpdateAvailableBoxes() { AvailableBoxes = AllBoxes.Where(b => b.gameObject.activeInHierarchy).ToArray(); }

    public Action<DropBoxGame> OnReroll;

    private void Start()
    {
        Instance = this;
        boxes = FindObjectsByType<DropBoxGame>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < boxes.Length; i++)
        {
            var box = boxes[i];
            box.OnLineCreated += HandleLineCreated;
        }

        //GenerateLevel(AllBoxes, linesPerBox, Mathf.Max(1, AllBoxes.Length / 4), infiniteMode);
    }

    // Возвращает случайный паттерн из predefinedLinesPerBox
    public DropBoxGamePattern GetRandomPatternForBox()
    {
        if (predefinedLinesPerBox == null || predefinedLinesPerBox.Count == 0)
            return null;

        var randomIndex = Random.Range(0, predefinedLinesPerBox.Count);
        var patterns = predefinedLinesPerBox[randomIndex];
        if (patterns == null || patterns.Length == 0)
            return null;

        return patterns[Random.Range(0, patterns.Length)];
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (isTest == false)
            return;

        // Генерация нового уровня
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateLevel(AllBoxes, linesPerBox, Mathf.Max(1, AllBoxes.Length / 4), infiniteMode);
            Debug.Log("LevelGenerator: сгенерирован новый уровень");
        }

        // Переброс объектов (Refill) по цифрам 1–4
        for (int i = 0; i < 4; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                if (i < AvailableBoxes.Length)
                {
                    RefillBox(AvailableBoxes[i], true);
                    Debug.Log($"LevelGenerator: RefillBox вызван для ящика {i + 1}");
                }
            }
        }

        // Блокировка ящика по Shift+цифра 1–4
        for (int i = 0; i < 4; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(key))
            {
                if (i < AvailableBoxes.Length)
                {
                    AvailableBoxes[i].SetInteractible(false, probabilityPrefabs.ToArray());
                    Debug.Log($"LevelGenerator: ящик {i + 1} заблокирован");
                }
            }
        }

        // Разблокировка ящика по Ctrl+цифра 1–4
        for (int i = 0; i < 4; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(key))
            {
                if (i < AvailableBoxes.Length)
                {
                    AvailableBoxes[i].SetInteractible(true, probabilityPrefabs.ToArray());
                    Debug.Log($"LevelGenerator: ящик {i + 1} разблокирован");
                }
            }
        }
#endif
    }

    public void SetAvailableBoxes(int avilibleCount)
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].gameObject.SetActive(avilibleCount > i);
        }

        UpdateAvailableBoxes();
    }

    public void GenerateLevel(int avilibleCount, int lockedCount)
    {
        ClearLevel();
        SetAvailableBoxes(avilibleCount);
        GenerateLevel(AvailableBoxes, linesPerBox, lockedCount, true);
    }

    public void GenerateLevel(DropBoxGame[] boxes, int linesPerBox, int lockedCount, bool infiniteMode)
    {
        if (boxes == null || boxes.Length == 0)
            throw new Exception("Нет ящиков для генерации уровня.");

        int totalSlots = boxes.Length * linesPerBox * 3;
        int emptySlotsLeft = emptySlots; // общее количество пустых мест, которое нужно распределить

        for (int i = 0; i < boxes.Length; i++)
        {
            var box = boxes[i];
            bool isLocked = i >= boxes.Length - lockedCount;

            box.SetInteractible(!isLocked, probabilityPrefabs.ToArray());

            DropBoxGamePattern[] lines = new DropBoxGamePattern[linesPerBox];

            for (int l = 0; l < linesPerBox; l++)
            {
                DropBoxGamePattern line = new DropBoxGamePattern { PlacePrefs = new DropBoxObject[3] };

                // Сначала выбираем индекс пустого места, если нужно
                int emptyIndex = -1;
                if (!isLocked && emptySlotsLeft > 0)
                {
                    emptyIndex = UnityEngine.Random.Range(0, 3);
                    emptySlotsLeft--;
                }

                // Заполняем линию объектами с учётом вероятности
                for (int j = 0; j < 3; j++)
                {
                    if (j == emptyIndex)
                    {
                        line.PlacePrefs[j] = null;
                    }
                    else
                    {
                        line.PlacePrefs[j] = GetRandomPrefabByProbability();
                    }
                }

                lines[l] = line;
            }

            box.SetLinesPatterns(lines);

            // Только открытые ящики создают объекты сразу
            if (!isLocked || infiniteMode)
                box.TryCreateNextLine();
        }

        foreach (var boxGame in boxes)
        {
            boxGame.Init();
        }
    }

    public void ClearLevel()
    {
        foreach (var box in boxes)
        {
            box.SetInteractible(true);
            foreach (var place in box.GetDropPlaces())
            {
                place.DeliteCurrentDragObject();
            }
        }
    }

    private static void RefilPreStart()
    {
        foreach (var boxGame in AvailableBoxes)
        {
            RefillBox(boxGame);
        }
    }

    public static void RefillBox(DropBoxGame box, bool isInvokeEvent = false)
    {
        if (box == null) return;

        // Уничтожаем все текущие объекты в этом боксе
        foreach (var place in box.GetDropPlaces())
        {
            place.DeliteCurrentDragObject();
        }

        int totalPlaces = box.GetDropPlaces().Length;

        // Проверяем, есть ли пустой слот в других открытых ящиках
        int emptySlotExistsElsewhere = GetEmptySlotsCount(box);

        // Собираем все объекты из текущих линий (для повторного использования)
        var lines = box.GetLinesPatterns();
        var shufflePool = new List<DropBoxObject>();
        foreach (var line in lines)
        {
            for (int i = 0; i < line.PlacePrefs.Length; i++)
            {
                line.PlacePrefs[i] = null; // очищаем
            }
        }

        // Если пустого слота нет нигде (кроме заблокированных и текущего) — оставляем один слот пустым
        /*int emptySlotIndex = -1;
        if (emptySlotExistsElsewhere < Instance.emptySlots)
        {
            emptySlotIndex = UnityEngine.Random.Range(0, totalPlaces);
        }*/

        // Заполняем линии заново с учётом вероятности
        int lineIndex = 0;
        int placeIndex = 0;

        for (int i = 0; i < totalPlaces; i++)
        {
            /*if (i == emptySlotIndex)
            {
                // оставляем пустое место
                lines[lineIndex].PlacePrefs[placeIndex] = null;
            }
            else
            {*/
            Debug.Log($"lines => {lines.Length}; lineIndex => {lineIndex}; placeIndex => placeIndex");
                lines[lineIndex].PlacePrefs[placeIndex] = Instance.GetRandomPrefabByProbability();
            //}

            placeIndex++;
            if (placeIndex >= lines[lineIndex].PlacePrefs.Length)
            {
                placeIndex = 0;
                lineIndex++;
                if (lineIndex >= lines.Length) break;
            }
        }

        box.SetLinesPatterns(lines);
        box.TryCreateNextLine();

        if (isInvokeEvent)
            Instance.OnReroll?.Invoke(box);
    }

    private static int GetEmptySlotsCount(DropBoxGame box = null)
    {
        int emptySlotExistsElsewhere = 0;
        foreach (var b in AvailableBoxes)
        {
            if (b == box) continue;
            if (!b.IsInteractible) continue; // игнорируем заблокированные
            if (b.TestEmptyAny())
            {
                emptySlotExistsElsewhere++;
            }
        }

        return emptySlotExistsElsewhere;
    }

    private void HandleLineCreated(DropBoxGame box, DropBoxGamePattern line)
    {
        if (infiniteMode)
        {
            // Добавляем новый паттерн в конец очереди линий
            var newLine = GenerateRandomLine();
            box.AddLinePatterns(new DropBoxGamePattern[] { newLine });
        }
    }

    // Исправление метода GenerateRandomLine для выбора по вероятности
    private DropBoxGamePattern GenerateRandomLine()
    {
        DropBoxGamePattern line = new DropBoxGamePattern { PlacePrefs = new DropBoxObject[3] };

        for (int j = 0; j < 3; j++)
            line.PlacePrefs[j] = GetRandomPrefabByProbability();

        if (LevelGenerator.GetEmptySlotsCount() < emptySlots)
            line.PlacePrefs[UnityEngine.Random.Range(0, 3)] = null;

        return line;
    }

    public static int GetProbability(string targetTag, bool isProcent = false)
    {
        int total = 0;
        int targetValue = 0;

        for (int i = 0; i < Instance.dropBoxObjectsPrefabs.Count; i++)
        {
            var prefab = Instance.dropBoxObjectsPrefabs[i];
            int prob = Instance.probabilityPrefabs[i];
            total += prob;

            if (prefab.AnyTag(targetTag))
            {
                targetValue = prob;
                break;
            }
        }

        if (isProcent)
        {
            if (total == 0) return 0;
            return Mathf.RoundToInt((float)targetValue / total * 100f);
        }

        return targetValue;
    }

    public static void AddProbabilityForTag(string targetTag, int add, bool isProcent = false)
    {
        for (int i = 0; i < Instance.dropBoxObjectsPrefabs.Count; i++)
        {
            var prefab = Instance.dropBoxObjectsPrefabs[i];
            if (prefab.AnyTag(targetTag))
            {
                if (isProcent)
                    add *= Instance.probabilityPrefabs.Sum() / 100;

                Instance.probabilityPrefabs[i] += add;
                return;
            }
        }
    }
    
    // Метод для выбора префаба с учётом вероятностей
    private DropBoxObject GetRandomPrefabByProbability()
    {
        int total = probabilityPrefabs.Where(p => p > 0).Sum();
        if (total <= 0)
            return dropBoxObjectsPrefabs[UnityEngine.Random.Range(0, dropBoxObjectsPrefabs.Count)];

        int roll = UnityEngine.Random.Range(0, total);
        int sum = 0;
        for (int i = 0; i < dropBoxObjectsPrefabs.Count; i++)
        {
            if (probabilityPrefabs[i] <= 0) continue; // пропускаем объекты с вероятностью 0
            sum += probabilityPrefabs[i];
            if (roll < sum)
                return dropBoxObjectsPrefabs[i];
        }

        return dropBoxObjectsPrefabs.Last();
    }

    public void LockRandomBox()
    {
       List<DropBoxGame> unlockBoxes = new List<DropBoxGame>();
        foreach (var boxGame in AvailableBoxes)
        {
            if (boxGame.IsInteractible)
                unlockBoxes.Add(boxGame);
        }

        if (unlockBoxes.Count <= 3)
            return;

        unlockBoxes[Random.Range(0, unlockBoxes.Count)].SetInteractible(false, probabilityPrefabs.ToArray());
    }

    public static void RefillRandomBox()
    {
        for (int i = 0; i < 100; i++)
        {
            var box = AvailableBoxes[Random.Range(0, AvailableBoxes.Length)];
            if (box.IsInteractible)
            {
                RefillBox(box);
                return;
            }
        }
    }
}
