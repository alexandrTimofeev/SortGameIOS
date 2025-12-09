using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropObjectImager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private List<TagDropObjectInImage> tagDropObjectIns;

    public void Init(string[] tags)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            string tag = tags[i];
            spriteRenderers[i].sprite = GetSpriteToTag(tag);
        }
    }

    public Sprite GetSpriteToTag(string tag) => tagDropObjectIns.FirstOrDefault(tdoi => tdoi.tag == tag)?.sprite;
}

[System.Serializable]
public class TagDropObjectInImage
{
    public string tag;
    public Sprite sprite;
}
