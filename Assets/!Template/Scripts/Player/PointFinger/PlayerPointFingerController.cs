using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerPointFingerController : Controller2DAction
{
    [SerializeField] private LayerMask layersSolid;
    [SerializeField] private float radius = 1f;

    public event Action<Vector2> OnMove;

    public override void Init(IInput input)
    {
        base.Init(input);
    }

    protected override void AnyPositionOn(Vector2 point)
    {
        base.BeganOn(point);

        Vector3 pos = Camera.main.ScreenToWorldPoint(point) + Vector3.forward;
        if (Physics2D.OverlapCircle((Vector2)pos, radius, layersSolid))
        {
            return;
        }

        OnMove?.Invoke(pos - transform.position);
        transform.position = pos;
    }
}
