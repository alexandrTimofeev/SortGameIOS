using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DistToPlayerActivate : MonoBehaviour
{
    public enum DistToPlayerActivateType { Dist2D, onlyX, onlyY};
    private Transform player;
    [SerializeField] private bool findOnStart = true;

    [Space]
    [SerializeField] private DistToPlayerActivateType typeDist;
    [SerializeField] private float distToActivate = 10f;
    [SerializeField] private bool onceActivate;

    [Space]
    public UnityEvent<DistToPlayerActivate> OnActivateEv;
    public event Action<DistToPlayerActivate> OnActivate;
    public UnityEvent<DistToPlayerActivate> OnDeactivateEv;
    public event Action<DistToPlayerActivate> OnDeactivate;

    private bool isActive;

    public bool IsActive => isActive;

    private void Start()
    {
        if (findOnStart)
            SetPlayer(GameObject.FindWithTag("Player")?.transform);
    }

    public void SetPlayer (Transform playerTr)
    {
        player = playerTr;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (isActive == false && IsPlayerClose())
            TryActive();
        else if (isActive && onceActivate == false && 
            IsPlayerClose() == false)
            Deactivate();
    }

    private void TryActive()
    {
        if (isActive)
            return;

       Activate();
    }

    private void Activate()
    {
        OnActivateEv?.Invoke(this);
        OnActivate?.Invoke(this);
        isActive = true;
    }

    private void Deactivate()
    {
        isActive = false;
        OnDeactivateEv?.Invoke(this);
        OnDeactivate?.Invoke(this);
    }

    public bool IsPlayerClose()
    {
        switch (typeDist)
        {
            case DistToPlayerActivateType.onlyX:
                return Mathf.Abs(transform.position.x - player.transform.position.x) < distToActivate;
            case DistToPlayerActivateType.onlyY:
                return Mathf.Abs(transform.position.y - player.transform.position.y) < distToActivate;
            default:
                return Vector2.Distance(transform.position, player.transform.position) < distToActivate;
        }
    }
}