using UnityEngine;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.Events;

public class IQBanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpIQ;
    [SerializeField] private string prefix = "Your IQ: <color=yellow>";

    [Header("Animation")]
    [SerializeField] private float scaleForAnim = 1.5f;
    [SerializeField] private float animDuration = 1.2f;
    [SerializeField] private float punchStrength = 0.25f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float delayPlayTickMin = 0.2f;

    [Space]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickClip;
    [SerializeField] private AudioClip winClip;

    [Space]
    [SerializeField] private UnityEvent onFinish;

    private Vector3 scaleStart;

    void Start()
    {
        if (PlayerPrefs.GetInt("IsSeeIQ", 0) != 1)
        {
            gameObject.SetActive(false);
            return;
        }

        scaleStart = transform.localScale;

        if (IQSystem.IsIntcraceAnim || PlayerPrefs.GetInt("IsSeeIQ", 0) == 1)
        {
            SetIQAnim(IQSystem.GetIQ());
            IQSystem.IsIntcraceAnim = false;
            PlayerPrefs.SetInt("IsSeeIQ", 0);
        }
        else
        {
            SetIQWithoutAnim(IQSystem.GetIQ());
        }
    }

    private void SetIQAnim(int iq)
    {
        transform.localScale = scaleStart * scaleForAnim;

        int currentIQ = 0;
        float prevTimePlayTick = Time.time - delayPlayTickMin;

        DOTween.To(() => currentIQ, x => {
            if (x != currentIQ)
            {
                currentIQ = x;
                tmpIQ.text = $"{prefix}{currentIQ}";

                if (audioSource && tickClip && Time.time - prevTimePlayTick > delayPlayTickMin)
                {
                    audioSource.PlayOneShot(tickClip);
                    prevTimePlayTick = Time.time;
                }
            }
        },
        iq,
        animDuration)
        .SetEase(Ease.OutExpo)
        .OnComplete(() =>
        {
            if (audioSource && winClip)
                audioSource.PlayOneShot(winClip);

            transform.DOPunchScale(Vector3.one * punchStrength, punchDuration, 2, 1f)
            .OnComplete(() => 
            transform.DOScale(scaleStart, 0.8f));        

            onFinish?.Invoke();
        });
    }

    private void SetIQWithoutAnim(int iq)
    {
        tmpIQ.text = $"{prefix}{iq}";
    }
}