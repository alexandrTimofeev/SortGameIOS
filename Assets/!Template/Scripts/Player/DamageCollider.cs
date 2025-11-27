using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DamageCollider : MonoBehaviour
{
    [SerializeField] private List<string> colliderTags = new List<string>();

    [Space]
    [SerializeField] private GameObject hitVFX;

    [Space]
    [SerializeField] private UnityEvent DamageEvent;

    public event Action<DamageContainer> OnDamage;
    public event Action<Vector2> OnPush;

    public List<string> ColliderTags => colliderTags;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out DamageContainer damageContriner))
        {
            damageContriner.CollisionDamageCollider(this);
            if (damageContriner.ContainsAnyTags(colliderTags.ToArray()))
            {
                Hit(damageContriner, collision.ClosestPoint(transform.position));
            }
        }
    }

    private void Hit(DamageContainer damageContriner, Vector3 point)
    {
        damageContriner.HitDamageCollider(this);
        OnDamage?.Invoke(damageContriner);
        DamageEvent?.Invoke();

        if (hitVFX)
            Destroy(Instantiate(hitVFX, point, transform.rotation), 10f);

        if (damageContriner.PunchForce != 0)
            OnPush?.Invoke((transform.position - damageContriner.transform.position).normalized * damageContriner.PunchForce);
    }
}
