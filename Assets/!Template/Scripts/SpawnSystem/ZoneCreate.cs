using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ZoneCreate : MonoBehaviour
{
    [SerializeField] private Vector2 area;
    [SerializeField] private float destroyWait = 12f;

    [Space]
    [SerializeField] private float dealy = 0.5f;
    [SerializeField] private LayerMask layerMask;

    [Space]
    [SerializeField] private int count = 4;
    [SerializeField] private SpawnInstruction instr;

    private List<SpawnbleObject> spawnedObjects = new List<SpawnbleObject>();

    public void SpawnRandom()
    {
        DOVirtual.DelayedCall(dealy, () => SpawnRandomProcess());
    }

    private void SpawnRandomProcess()
    {
        int specalNextSpawn = instr.spawnSpecialCount;
        SpawnableItem specalSpawn = instr.spawnSpecial;

        int countLocal = this.count;

        while (countLocal > 0)
        {
            var item = instr.GetRandomItem();
            if (specalNextSpawn == 0 && specalSpawn.Prefab != null)
            {
                item = specalSpawn;
            }

            GameObject prefab = item.Prefab;
            GameObject ngo = Spawn(instr.id, prefab);

            if (ngo != null)
            {
                switch (instr.spawnAnimation)
                {
                    case SpawnAnimationType.None:
                        break;
                    case SpawnAnimationType.Scale:
                        ngo.transform.localScale = Vector3.zero;
                        ngo.transform.DOScale(1f, instr.durarion);
                        break;
                    default:
                        break;
                }
            }

            countLocal--;
        }
    }

    public virtual GameObject Spawn(string id, GameObject prefab)
    {
        float x, y;
        RandomXY(out x, out y);
        while (Physics2D.OverlapCircle(new Vector2(x, y), 0.5f, layerMask) != null)
        {
            RandomXY(out x, out y);
        }

        GameObject ngo = SpawnPocess(id, prefab, transform.position + new Vector3(x, y, 0f));

        return ngo;

        void RandomXY(out float x, out float y)
        {
            x = Random.Range(-area.x / 2f, area.x / 2f);
            y = Random.Range(-area.y / 2f, area.y / 2f);
        }
    }

    protected GameObject SpawnPocess(string id, GameObject prefab, Vector3 position)
    {
        GameObject ngo = Instantiate(prefab, position, Quaternion.identity);
        Destroy(ngo, destroyWait);

        if (spawnedObjects.Contains(null))
            spawnedObjects.RemoveAll(null);

        SpawnbleObject spawnbleObject = ngo.GetComponent<SpawnbleObject>();
        if (spawnbleObject == null)
            spawnbleObject = ngo.AddComponent<SpawnbleObject>();

        spawnbleObject.Init(id);

        spawnedObjects.Add(spawnbleObject);

        return ngo;
    }
    public virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, area);
    }
}
