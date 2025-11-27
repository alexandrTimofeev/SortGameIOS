using UnityEngine;

public class SizeRandom : MonoBehaviour
{
    [SerializeField] private Vector2 minMaxCoef = new Vector2(1f, 2f);

    void Start()
    {
        transform.localScale *= Random.Range(minMaxCoef.x, minMaxCoef.y);
    }
}
