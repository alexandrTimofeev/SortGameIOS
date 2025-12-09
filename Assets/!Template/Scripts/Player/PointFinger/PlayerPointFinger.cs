using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerPointFinger : Player2DAction<PlayerPointFingerController>
{
    [SerializeField] private Transform target;
    [SerializeField] private SimpleMover simpleMover;
    [SerializeField] private float speedKofKof = 1f;
    [SerializeField] private GameObject vfxHit;
    [SerializeField] private float kofVFXHit = 1f;
    [SerializeField] private float kofVFXHitVolume = 1f;

    [Space]
    [SerializeField] private float forceMemoryDuration = 1f;
    [SerializeField] private float forceKof = 1f;
    private float forceMemory = 0f;

    [Space]
    [SerializeField] private float forcePunch = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = target.position;
        playerController.OnMove += SetForceMemory;
    }

    private void Update()
    {
        DistUpdate(startPos, target.position);
        forceMemory = Mathf.MoveTowards(forceMemory, 0f, (1f / forceMemoryDuration) * Time.deltaTime);
    }

    public override void SetSpeedKof(float speed)
    {
        simpleMover.direction = new Vector3(0, 1f + (speed * speedKofKof), 0);
    }

    protected override void AddSize(float addSize)
    {
        playerController.transform.DOScale(playerController.transform.localScale.x + addSize, 1f);
    }

    protected override void RemoveSize(float removeSize)
    {
        playerController.transform.DOKill(false);
        playerController.transform.DOScale(1f, 1f);
    }

    public override Transform GetTarget() => target;

    public override void Dead()
    {
        base.Dead();
        target.gameObject.SetActive(false);
    }

    private void SetForceMemory(Vector2 delta)
    {
        float newForce= delta.magnitude;

        if (newForce > forceMemory)
            forceMemory = newForce;
        else        
            forceMemory = Mathf.Lerp(forceMemory, newForce, 0.5f);        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (vfxHit)
        {
            float force = (forceMemory * forceKof) + collision.relativeVelocity.magnitude;

            GameObject vfxGO = Instantiate(vfxHit, collision.contacts[0].point, Quaternion.identity);
            vfxGO.transform.localScale *= (force * kofVFXHit);

            AudioSource source = vfxGO.transform.GetComponentInChildren<AudioSource>();
            if (source)            
                source.volume *= Mathf.Clamp(force * kofVFXHitVolume, 0.3f, 5f);

            collision.rigidbody.AddForce(-collision.relativeVelocity.normalized * forceMemory * forceKof * forcePunch);
        }
    }
}