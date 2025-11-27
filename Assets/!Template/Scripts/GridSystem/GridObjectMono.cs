using UnityEngine;
using DG.Tweening;
using System.Collections;

public class GridObjectMono : MonoBehaviour, IGridObject
{
    private Vector2Int gridPosition;
    [SerializeField] private float moveDuration = 0.2f;

    private Tween moveTween;

    public Vector2Int GetPosition() => gridPosition;
    public GameObject GetMono() => gameObject;

    public void Init (GridContainer gridContainer, Vector2Int gridPosition)
    {
        gridContainer.AddGridObject(this);
        this.gridPosition = gridPosition;
    }

    public void SetPosition(Vector3 position, Vector2Int gridPosition, out Coroutine enumerator)
    {
        enumerator = StartCoroutine(SetPositionCoroutine(position, gridPosition));
    }

    private IEnumerator SetPositionCoroutine (Vector3 position, Vector2Int gridPosition)
    {
        //transform.position = position;
        if (moveTween != null && moveTween.IsPlaying())
            moveTween.Kill(false);

        moveTween = transform.DOMove(position, moveDuration);
        this.gridPosition = gridPosition;

        yield return moveTween;
        yield break;
    }
}
