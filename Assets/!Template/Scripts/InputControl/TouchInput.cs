using UnityEngine;
using System;
using UnityEngine.EventSystems;

/// <summary>
/// Мобильный ввод через сенсорный экран.
/// </summary>
public class TouchInput : MonoBehaviour, IInput
{
    public event Action<Vector2> OnBegan;
    public event Action<Vector2> OnMoved;
    public event Action<Vector2> OnEnded;
    public event Action<Vector2> OnAnyPosition;

    private Vector3 lastPos;

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            // Игнорировать, если палец над UI
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                continue;

            lastPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnBegan?.Invoke(touch.position);
                    break;

                case TouchPhase.Moved:
                    // touch.deltaPosition — это смещение с прошлого кадра
                    if (touch.deltaPosition != Vector2.zero)
                        OnMoved?.Invoke(touch.deltaPosition);
                    break;

                case TouchPhase.Ended:
                    OnEnded?.Invoke(touch.position);
                    break;

                // Canceled зафиксируем тоже как окончание
                case TouchPhase.Canceled:
                    OnEnded?.Invoke(touch.position);
                    break;
            }

            OnAnyPosition?.Invoke(touch.position);
        }

    }

    public Vector2 GetOverPosition() => Input.touchCount > 0 ? Input.GetTouch(0).position : lastPos;
}
