using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace StylizeForst
{
    public class FlowerBushManager : MonoBehaviour
    {
        [SerializeField] private GameObject flowerPrefab;
        private List<GameObject> spawnedFlowers = new List<GameObject>();

        public void GenerateFlowers()
        {
            ClearFlowers();
            
            if (flowerPrefab == null)
            {
                Debug.LogError("Flower Prefab is not assigned!");
                return;
            }

            FlowerSpawnPoint[] spawnPoints = GetComponentsInChildren<FlowerSpawnPoint>();
            
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.IsActive)
                {
                    GameObject flower = Instantiate(flowerPrefab, 
                        spawnPoint.transform.position, 
                        spawnPoint.transform.rotation);
                        
                    flower.transform.SetParent(transform);
                    spawnedFlowers.Add(flower);
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
    [CustomEditor(typeof(FlowerBushManager))]
    public class FlowerBushManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FlowerBushManager manager = (FlowerBushManager)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Point Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Spawn Point"))
            {
                // Create spawn point
                GameObject spawnPoint = new GameObject("FlowerSpawnPoint");
                spawnPoint.transform.SetParent(manager.transform);
                spawnPoint.transform.localPosition = Vector3.zero;
                spawnPoint.AddComponent<FlowerSpawnPoint>();
                
                // Select the new spawn point
                Selection.activeGameObject = spawnPoint;
            }

            if (GUILayout.Button("Generate Flowers"))
            {
                manager.GenerateFlowers();
            }
        }
    }
    #endif 
} 