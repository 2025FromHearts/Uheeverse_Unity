using System.Collections.Generic;
using UnityEngine;

public class RiverToTerrain : MonoBehaviour
{
    public Terrain terrain;                         // 대상 Terrain
    public LineRenderer riverLine;                  // 연결할 LineRenderer
    public string riverObjectName = "River";        // 자동으로 찾을 오브젝트 이름
    public float riverWidth = 5f;                   // 강의 반지름
    public float depth = -1f;                     // 강 파임 깊이 (음수)

    void Start()
    {
        // 자동 연결
        if (riverLine == null)
        {
            GameObject found = GameObject.Find(riverObjectName);
            if (found != null && found.TryGetComponent(out LineRenderer lr))
            {
                riverLine = lr;
                Debug.Log($"River LineRenderer 자동 연결 완료! ({found.name})");
            }
            else
            {
                Debug.LogWarning("River LineRenderer를 찾지 못했습니다.");
                return;
            }
        }

        LowerTerrainAlongRiver();
    }

    void LowerTerrainAlongRiver()
    {
        if (terrain == null || riverLine == null)
        {
            Debug.LogWarning("Terrain 또는 RiverLine이 연결되지 않았습니다.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        int hmWidth = terrainData.heightmapResolution;
        int hmHeight = terrainData.heightmapResolution;

        float[,] heights = terrainData.GetHeights(0, 0, hmWidth, hmHeight);
        Vector3 terrainPos = terrain.GetPosition();
        Vector3 terrainSize = terrainData.size;

        int numPositions = riverLine.positionCount;
        for (int i = 0; i < numPositions; i++)
        {
            Vector3 worldPos = riverLine.GetPosition(i);
            float normX = (worldPos.x - terrainPos.x) / terrainSize.x;
            float normZ = (worldPos.z - terrainPos.z) / terrainSize.z;

            int centerX = Mathf.RoundToInt(normX * (hmWidth - 1));
            int centerZ = Mathf.RoundToInt(normZ * (hmHeight - 1));
            int radius = Mathf.RoundToInt(riverWidth / terrainSize.x * hmWidth);

            for (int z = -radius; z <= radius; z++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int hx = centerX + x;
                    int hz = centerZ + z;

                    if (hx >= 0 && hx < hmWidth && hz >= 0 && hz < hmHeight)
                    {
                        float distance = Mathf.Sqrt(x * x + z * z);
                        if (distance <= radius)
                        {
                            float depthAmount = Mathf.Lerp(depth, 0, distance / radius);
                            heights[hz, hx] += depthAmount;
                        }
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
        Debug.Log("River가 Terrain에 성공적으로 적용되었습니다.");
    }
}
