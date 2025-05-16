using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace StylizeForst
{
    public class FlowerBush : MonoBehaviour
    {
        [SerializeField] private GameObject flowerPrefab;
        [SerializeField] private int flowerCount = 4;
        
        [Header("Flower Placement Settings")]
        [SerializeField, Range(0.01f, 0.2f)] private float surfaceOffset = 0.05f;
        [SerializeField, Range(0f, 1f)] private float upwardInfluence = 0.5f;
        
        [Header("Distribution Settings")]
        [SerializeField, Range(4, 12)] private int angleSegments = 4;
        [SerializeField, Range(0.1f, 1f)] private float minFlowerDistance = 0.3f;
        [SerializeField, Range(0f, 1f)] private float heightWeight = 0.6f;
        [SerializeField, Range(0f, 1f)] private float edgeWeight = 0.4f;
        [SerializeField, Range(0f, 5f)] private float minDistanceFromCenter = 0.5f;

        private List<GameObject> spawnedFlowers = new List<GameObject>();
        private MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        private struct VertexData
        {
            public int index;
            public float weight;
            public float angle;

            public VertexData(int index, float weight, float angle)
            {
                this.index = index;
                this.weight = weight;
                this.angle = angle;
            }
        }

        private float GetHorizontalAngle(Vector3 position)
        {
            Vector3 directionToVertex = position - transform.position;
            directionToVertex.y = 0; // Project onto XZ plane
            float angle = Vector3.SignedAngle(Vector3.forward, directionToVertex, Vector3.up);
            return (angle + 360f) % 360f; // Normalize to 0-360
        }

        private bool IsTooCloseToExistingFlowers(Vector3 position)
        {
            foreach (var flower in spawnedFlowers)
            {
                if (flower != null && Vector3.Distance(position, flower.transform.position) < minFlowerDistance)
                {
                    return true;
                }
            }
            return false;
        }

        private List<VertexData> GetVerticesInAngleRange(Vector3[] vertices, Vector3[] normals, float startAngle, float endAngle)
        {
            List<VertexData> segmentVertices = new List<VertexData>();
            Vector3 center = transform.position;
            float maxDistance = 0f;

            // First pass: find maximum distance
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                Vector3 toCenter = center - worldPos;
                float horizontalDistance = new Vector3(toCenter.x, 0, toCenter.z).magnitude;
                maxDistance = Mathf.Max(maxDistance, horizontalDistance);
            }

            // Adjust minimum distance
            float adjustedMinDistance = Mathf.Min(minDistanceFromCenter, maxDistance * 0.5f);

            // Second pass: collect vertices in angle range
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                float angle = GetHorizontalAngle(worldPos);

                // Check if angle is in range (handle wrap-around)
                bool inRange = false;
                if (startAngle <= endAngle)
                {
                    inRange = angle >= startAngle && angle <= endAngle;
                }
                else
                {
                    inRange = angle >= startAngle || angle <= endAngle;
                }

                if (inRange)
                {
                    Vector3 toCenter = center - worldPos;
                    float horizontalDistance = new Vector3(toCenter.x, 0, toCenter.z).magnitude;

                    if (horizontalDistance >= adjustedMinDistance)
                    {
                        float heightFactor = Mathf.InverseLerp(center.y - 1f, center.y + 1f, worldPos.y);
                        float edgeFactor = Mathf.InverseLerp(adjustedMinDistance, maxDistance, horizontalDistance);
                        float weight = (heightFactor * heightWeight + edgeFactor * edgeWeight);
                        weight *= Random.Range(0.8f, 1.2f);

                        segmentVertices.Add(new VertexData(i, weight, angle));
                    }
                }
            }

            return segmentVertices;
        }

        public void GenerateFlowers()
        {
            ClearFlowers();

            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (flowerPrefab == null)
            {
                Debug.LogError("Flower Prefab is not assigned!");
                return;
            }

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("MeshFilter or Mesh is missing!");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            float anglePerSegment = 360f / angleSegments;
            int flowersPerSegment = flowerCount / angleSegments;
            int remainingFlowers = flowerCount % angleSegments;

            for (int segment = 0; segment < angleSegments; segment++)
            {
                float startAngle = segment * anglePerSegment;
                float endAngle = startAngle + anglePerSegment;

                // Get vertices in this angle range
                List<VertexData> segmentVertices = GetVerticesInAngleRange(vertices, normals, startAngle, endAngle);

                // Calculate how many flowers to generate in this segment
                int flowersToGenerate = flowersPerSegment + (segment < remainingFlowers ? 1 : 0);

                // Sort vertices by weight
                segmentVertices.Sort((a, b) => b.weight.CompareTo(a.weight));

                // Try to generate flowers in this segment
                int attempts = 0;
                int maxAttempts = segmentVertices.Count * 2;
                int flowersGenerated = 0;

                while (flowersGenerated < flowersToGenerate && attempts < maxAttempts && segmentVertices.Count > 0)
                {
                    // Select from top 20% of weighted vertices
                    int selectionRange = Mathf.Max(1, segmentVertices.Count / 5);
                    int randomIndex = Random.Range(0, selectionRange);
                    
                    if (randomIndex >= segmentVertices.Count) continue;

                    VertexData selectedVertex = segmentVertices[randomIndex];
                    Vector3 position = transform.TransformPoint(vertices[selectedVertex.index]);

                    if (!IsTooCloseToExistingFlowers(position))
                    {
                        Vector3 normal = normals[selectedVertex.index];
                        Vector3 worldNormal = transform.TransformDirection(normal);
                        
                        // Offset position along normal direction
                        position += worldNormal * surfaceOffset;
                        
                        // Spawn flower
                        GameObject flower = Instantiate(flowerPrefab, position, Quaternion.identity);
                        flower.transform.SetParent(transform);

                        // Adjust flower rotation with upward influence
                        Vector3 adjustedNormal = (worldNormal + Vector3.up * upwardInfluence).normalized;
                        flower.transform.rotation = Quaternion.FromToRotation(Vector3.up, adjustedNormal);

                        spawnedFlowers.Add(flower);
                        flowersGenerated++;
                    }

                    // Remove used vertex to avoid selecting it again
                    segmentVertices.RemoveAt(randomIndex);
                    attempts++;
                }
            }
        }

        private void ClearFlowers()
        {
            foreach (var flower in spawnedFlowers)
            {
                if (flower != null)
                {
                    DestroyImmediate(flower);
                }
            }
            spawnedFlowers.Clear();
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(FlowerBush))]
    public class FlowerBushEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FlowerBush flowerBush = (FlowerBush)target;
            if (GUILayout.Button("Generate Flowers"))
            {
                flowerBush.GenerateFlowers();
            }
        }
    }
    #endif 
} 