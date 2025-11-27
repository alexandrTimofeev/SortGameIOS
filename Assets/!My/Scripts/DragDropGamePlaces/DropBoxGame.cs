using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class DropBoxGame : DropBox
{
    [SerializeField] private List<DropBoxGamePattern> linesPatterns = new List<DropBoxGamePattern>();
    [SerializeField] private bool generateOnStart = true;

    [SerializeField] private bool isInteractible = true;
    public bool IsInteractible => isInteractible;

    [Space]
    [SerializeField] private DropObjectImager imgsLock;
    [SerializeField] private DropBoxGamePatternInteractible patternToInteractible;

    [Space]
    [SerializeField] private AudioSource sourceComplite;
    [SerializeField] private AudioSource sourceComplite2;
    [SerializeField] private AudioClip clipGrap;
    [SerializeField] private AudioClip clipPut;

    private DropBoxGamePattern lastDropBoxGamePattern;
    int[] probability = new int[0];

    public event Action<DropBoxGame> OnEnd; 
    public event Action<DropBoxGame, DropBoxGamePattern> OnLineCreated;


    public void Init()
    {
        //SetInteractible(isInteractible);
        DragDropGameManager.instance.OnClear += InvokeClear;
        
        foreach (var place in dropPlaces)
        {
            place.OnGrap += (dob) => sourceComplite.PlayOneShot(clipGrap);
            place.OnPut += (dob) => sourceComplite.PlayOneShot(clipPut);
        }
        
        //if (generateOnStart && isInteractible)
        //    TryCreateNextLine();
    }

    public void SetInteractible(bool interact)
    {
        //if (probability.Length == 0)
       // {
            probability = new int[patterns.Length];
            for (int i = 0; i < probability.Length; i++)
            {
                int prob = probability[i];
                probability[i] = 100 / patterns.Length;
            }
        //}
        SetInteractible(interact, probability);
    }

    public void SetInteractible(bool interact, int[] probability)
    {
        //this.probability = probability;
        if (isInteractible == interact) return;

        isInteractible = interact;

        foreach (var place in dropPlaces)
            place.SetInteractible(interact);

        if (interact)
        {
            TryCreateNextLine();
        }
        else
        {
            // Убираем все объекты
            foreach (var place in dropPlaces)
                place.DeliteCurrentDragObject();

            // Выбираем один случайный паттерн по обратной вероятности
            if (patterns != null && patterns.Length > 0)
            {
                // Генерируем инверсированные веса
                List<int> weights = new List<int>();
                for (int i = 0; i < patterns.Length; i++)
                {
                    if (probability != null && i < probability.Length && probability[i] > 0)
                    {
                        // Чем меньше вероятность, тем больше вес
                        weights.Add(1000 / probability[i]);
                    }
                    else
                    {
                        weights.Add(0); // вероятность 0 → не выбираем
                    }
                }

                int totalWeight = weights.Sum();
                if (totalWeight > 0)
                {
                    int roll = UnityEngine.Random.Range(0, totalWeight);
                    int sum = 0;
                    int selectedIndex = 0;

                    for (int i = 0; i < weights.Count; i++)
                    {
                        sum += weights[i];
                        if (roll < sum)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    var randomPattern = patterns[selectedIndex];

                    patternToInteractible = new DropBoxGamePatternInteractible
                    {
                        tagsGroops = new List<StringArray>()
                    };

                    if (randomPattern.requiredTags != null && randomPattern.requiredTags.Length > 0)
                    {
                        patternToInteractible.tagsGroops.Add(new StringArray
                        {
                            tags = randomPattern.requiredTags
                        });
                    }
                }
            }
        }

        imgsLock.gameObject.SetActive(!interact);
        if (patternToInteractible?.tagsGroops != null && patternToInteractible.tagsGroops.Count > 0)
            imgsLock.Init(patternToInteractible.tagsGroops[0].tags);
    }

    public override void PlacesFull()
    {
        base.PlacesFull();

        DropBoxObject[] dropBoxObjects = dropPlaces.Select(p => p.CurrentDragObject?.GetComponent<DropBoxObject>()).ToArray();
        DragDropGameManager.instance.Clear(new DropBoxClearEvent(this, dropBoxObjects));

        for (int i = 0; i < dropPlaces.Length; i++)
        {
            DropPlace dropPlace = dropPlaces[i];
            var dragObject = dropPlace.CurrentDragObject;
            if (dragObject == null) continue;
            dropPlace.CurrentDragObject.Drop(null);
            RemoveDragObject(dragObject, i * 0.25f);
        }

        DOVirtual.DelayedCall(0.5f, () => TryCreateNextLine());
        DragDropGameManager.ChangeNotGetSteps(true);
    }

    private void RemoveDragObject(DragObject dragObject, float delaySound = 0.1f)
    {
        DOVirtual.DelayedCall(delaySound, () => sourceComplite2.PlayOneShot(dragObject.ClipUse, 2.5f));
        dragObject.ScalePunchAnimation()
            .OnComplete(() => Destroy(dragObject.gameObject));
    }
    public void TryCreateNextLine()
    {
        if (!isInteractible) return; // ничего не делаем, если ящик заблокирован
        if (linesPatterns.Count == 0)
        {
            LevelGenerator.RefillBox(this);
            return;
        }

        var line = linesPatterns[0];
        CreatePattern(line);
        linesPatterns.RemoveAt(0);

        // Сообщаем LevelGenerator, что линия создана
        OnLineCreated?.Invoke(this, line);

        foreach(var dropPlace  in dropPlaces)
        {
            dropPlace.SetInteractible(true);
        }
    }


    private void CreatePattern(DropBoxGamePattern boxGamePattern)
    {
        int emptySlots = LevelGenerator.IsNeedEmptySlots ? UnityEngine.Random.Range(0, boxGamePattern.PlacePrefs.Length) : -1;
        for (int i = 0; i < boxGamePattern.PlacePrefs.Length; i++)
        {
            var prefab = boxGamePattern.PlacePrefs[i];
            var dropPlace = dropPlaces[i];

            if (prefab == null || i == emptySlots) continue;

            StartCoroutine(CreatePatternEn(prefab, dropPlace, i * 0.1f));
        }

        lastDropBoxGamePattern = boxGamePattern;
    }

    private IEnumerator CreatePatternEn(DropBoxObject prefab, DropPlace dropPlace, float wait)
    {
        yield return new WaitForSeconds(wait);
        var obj = Instantiate(prefab, dropPlace.transform.position, dropPlace.transform.rotation);
        dropPlace.PutObject(obj.GetComponent<DragObject>());
        obj.GetComponent<DragObject>().ScaleCreateAnimation();
        obj.Init();
    }

    private void InvokeClear(DropBoxClearEvent clearEvent)
    {
        if (isInteractible) return;

        foreach (var dropBox in clearEvent.dropBoxObjects)
        {
            if (dropBox != null && patternToInteractible != null && !patternToInteractible.TestDropBoxObject(dropBox))
                return;
        }

        SetInteractible(true);
        //TryCreateNextLine();
    }

    public void ClearLinesPatterns() => linesPatterns.Clear();

    public void SetPatternToInteractible(DropBoxGamePatternInteractible interact)
    {
        patternToInteractible = interact;
        SetInteractible(interact == null);
    }

    public void AddLinePatterns(DropBoxGamePattern[] patterns) => linesPatterns.AddRange(patterns);

    public bool TestEmpty() => dropPlaces.All(p => !p.IsFull);
    public bool TestEmptyAny() => dropPlaces.Any(p => !p.IsFull);

    public DropPlace[] GetDropPlaces() => dropPlaces;

    public void SetDropPlaces(DragObject[] objects)
    {
        if (objects.Length != dropPlaces.Length)
            throw new ArgumentException("Неверное количество объектов для ящиков.");

        for (int i = 0; i < dropPlaces.Length; i++)
        {
            dropPlaces[i].ThrowCurrentDragObject();
            if (objects[i] != null)
                dropPlaces[i].PutObject(objects[i]);
        }
    }

    public DropBoxGamePattern[] GetLinesPatterns() => linesPatterns.ToArray();

    public void SetLinesPatterns(DropBoxGamePattern[] patterns)
    {
        linesPatterns.Clear();
        linesPatterns.AddRange(patterns);
    }
}

[System.Serializable]
public class DropBoxGamePattern
{
    public DropBoxObject[] PlacePrefs;
    public AudioClip Clip;
}

[System.Serializable]
public class StringArray
{
    public string[] tags;
}

[System.Serializable]
public class DropBoxGamePatternInteractible
{
    [SerializeField] public List<StringArray> tagsGroops = new List<StringArray>();

    public bool TestDropBoxObject(DropBoxObject dropBoxObject)
    {
        foreach (var group in tagsGroops)
        {
            /*string tstLog = "TestDropBoxObject:\n";
            tstLog += "\ngroup.tags:\n";

            for (int i = 0; i < group.tags.Length; i++)
            {
                tstLog += ", " + group.tags[i];
            }

            tstLog += "\ndropBoxObject.Tag:\n";
            for (int i = 0; i < dropBoxObject.Tags.Count; i++)
            {
                tstLog += ", " + dropBoxObject.Tags[i];
            }*/

            if (dropBoxObject.Tags.All(tg => group.tags.Contains(tg)))
                return true;
        }
        return false;
    }
}