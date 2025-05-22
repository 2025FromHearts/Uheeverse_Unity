using UnityEngine;

namespace StylizeForst
{
    public class FlowerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool isActive = true;
        [SerializeField] private float gizmoSize = 0.1f;
        
        public bool IsActive => isActive;
        
        private void OnDrawGizmos()
        {
            if (!isActive) 
            {
                Gizmos.color = Color.gray;
            }
            else 
            {
                Gizmos.color = Color.yellow;
            }
            
            // Draw spawn point
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
            
            // Draw direction
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * gizmoSize * 2);
        }
    }
} 