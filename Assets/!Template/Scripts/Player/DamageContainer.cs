using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class DamageContainer : MonoBehaviour
{
    [SerializeField] private List<string> colliderTargetTags = new List<string>();
    [SerializeField] private List<string> destoryMeTags = new List<string>();
    [SerializeField] private bool deliteIfCollisionTarget = true;

    [Space]
    [SerializeField] private bool destroyGOForDelite =   true;

    [Space]
    [SerializeField] private float punchForce = 0f;

    [Space]
    [SerializeField] private GameObject hitVFXPref;
    [SerializeField] private GameObject deliteVFXPref;

    public float PunchForce => punchForce;

    public event Action<DamageCollider> OnCollisionDamageCollider;
    public event Action<DamageCollider> OnHitDamageCollider;
    public event Action<DamageContainer> OnDelite;

    public UnityEvent OnHitDamageColliderEv;

    public void CollisionDamageCollider(DamageCollider damageCollider)
    {
        OnCollisionDamageCollider?.Invoke(damageCollider);

        if (destoryMeTags.Any((s) => damageCollider.ColliderTags.Contains(s)))
            Delite();
    }

    public void HitDamageCollider(DamageCollider damageCollider)
    {
        OnHitDamageCollider?.Invoke(damageCollider);
        OnHitDamageColliderEv?.Invoke();

        if (hitVFXPref)
            Destroy(Instantiate(hitVFXPref, damageCollider.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.rotation), 10f);

        if (deliteIfCollisionTarget)        
            Delite();
    }

    private void Delite()
    {
        OnDelite?.Invoke(this);

        if(deliteVFXPref)
            Destroy(Instantiate(deliteVFXPref, transform.position, transform.rotation), 10f);

        if (destroyGOForDelite)
         Destroy(gameObject);
    }

    public bool ContainsAnyTags(params string[] tags)
    {
        return colliderTargetTags.Any((s) => tags.Contains(s));
    }
}