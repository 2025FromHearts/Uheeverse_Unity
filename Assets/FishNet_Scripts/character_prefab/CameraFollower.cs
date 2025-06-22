using UnityEngine;
using Unity.Cinemachine;
using FishNet.Object;

public class CameraFollower : NetworkBehaviour
{
    private bool _cameraSet = false;

    void Update()
    {
        if (!_cameraSet && IsOwner)
        {
            var cam = GameObject.FindAnyObjectByType<CinemachineCamera>();
            if (cam != null)
            {
                cam.Follow = this.transform;
                _cameraSet = true;
            }
        }
    }
}
