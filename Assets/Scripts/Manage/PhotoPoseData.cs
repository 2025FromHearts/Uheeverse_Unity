using UnityEngine;

public class PhotoPoseData : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPoseSet
    {
        public string characterName;

        public GameObject pose1Prefab;
        public GameObject pose2Prefab;
        public GameObject pose3Prefab;

        public Vector3 pose1PositionOffset;
        public Vector3 pose1RotationOffset;

        public Vector3 pose2PositionOffset;
        public Vector3 pose2RotationOffset;

        public Vector3 pose3PositionOffset;
        public Vector3 pose3RotationOffset;
    }
}
