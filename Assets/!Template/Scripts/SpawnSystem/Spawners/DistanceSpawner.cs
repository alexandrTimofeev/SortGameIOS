using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DistanceSpawner : Spawner
{
    [Header("Distance Spawn Settings")]
    [SerializeField] private bool useX = true;
    [SerializeField] private bool useY = false;
    [SerializeField] private Transform player;
    [SerializeField] private DistanceSpawnerTiming[] distanceToSpawns;

    private Dictionary<string, Vector3> lastSpawnPositions = new Dictionary<string, Vector3>();

    private void Start()
    {
        if (player == null)
        {
            Debug.Log("DistanceSpawner: Player Transform не установлен.");
            enabled = false;
        }
    }

    public override IEnumerator WaitCondition(SpawnInstruction instr)
    {
        while (true)
        {
            if (GamePause.IsPause)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }


            Vector3 spawnPosition = lastSpawnPositions.ContainsKey(instr.id) ? lastSpawnPositions[instr.id] : InitSpawnPosition(instr.id);

            float dx = useX ? Mathf.Abs(player.position.x - spawnPosition.x) : -1f;
            float dy = useY ? Mathf.Abs(player.position.y - spawnPosition.y) : -1f;

            if (dx >= GetTiming(instr.id).x || dy >= GetTiming(instr.id).y)
            {
                lastSpawnPositions[instr.id] = player.position;
                yield break; // Условие выполнено — выходим из ожидания
            }

            yield return null; // Ждём следующего кадра
        }
    }

    private Vector3 InitSpawnPosition (string id)
    {
        lastSpawnPositions.Add(id, transform.position - (Vector3)GetTiming(id));
        return lastSpawnPositions[id];
    }

    private Vector2 GetTiming (string id)
    {
        if (distanceToSpawns == null)
            return Vector2.zero;

        return distanceToSpawns.FirstOrDefault((s) => s.id == id).distanceToSpawn;
    }

    [System.Serializable]
    public class DistanceSpawnerTiming
    {
        public string id;
        public Vector2 distanceToSpawn;
    }
}