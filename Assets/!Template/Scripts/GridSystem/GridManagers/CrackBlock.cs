using System;
using UnityEngine;
public enum SignBlock { Sign1, Sign2, Sign3, Sign4 };

[RequireComponent(typeof(GridObjectMono))]
public class CrackBlock : MonoBehaviour
{
    private GridObjectMono gridObject;
    [SerializeField] private SignBlock sign;
    [SerializeField] private SpriteRenderer spriteRendererIcon;
    [SerializeField] private Sprite[] spritesIcons;

    public SignBlock Sign => sign;

    public GridObjectMono GridObject { get
        {
            if (gridObject == null)
                gridObject = GetComponent<GridObjectMono>();
            return gridObject;
        }
    }

    public void Broke()
    {
        gameObject.SetActive(false);
    }

    public void Init()
    {
        sign = (SignBlock)(UnityEngine.Random.Range(0, 4));
        spriteRendererIcon.sprite = spritesIcons[(int)sign];
    }
}
