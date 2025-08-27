using UnityEngine;
using FishNet.Object;

public class RemotePlayerController : NetworkBehaviour
{
    private Vector3 moveDirection = Vector3.zero;

    [ServerRpc]
    public void SetMoveCommandServerRpc(string command)
    {
        moveDirection = command.ToUpper() switch
        {
            "UP" => Vector3.forward,
            "DOWN" => Vector3.back,
            "LEFT" => Vector3.left,
            "RIGHT" => Vector3.right,
            "STOP" => Vector3.zero,
            _ => moveDirection
        };
    }

    void Update()
    {
        if (IsServerInitialized && moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection * Time.deltaTime * 5f);
        }
    }
}