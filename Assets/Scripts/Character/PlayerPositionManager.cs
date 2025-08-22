using UnityEngine;

public class PlayerPositionRestorer : MonoBehaviour
{
    void Start()
    {
        // 복귀 플래그가 설정된 경우에만 위치 복원
        if (PlayerPrefs.GetInt("ShouldRestorePosition", 0) == 1)
        {
            Vector3 restoredPos = new Vector3(
                PlayerPrefs.GetFloat("PlayerPosX"),
                PlayerPrefs.GetFloat("PlayerPosY"),
                PlayerPrefs.GetFloat("PlayerPosZ")
            );

            float yRot = PlayerPrefs.GetFloat("PlayerRotY");
            Quaternion restoredRot = Quaternion.Euler(0f, yRot, 0f);

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.SetPositionAndRotation(restoredPos, restoredRot);
            }

            // 플래그 및 위치정보 초기화
            PlayerPrefs.DeleteKey("PlayerPosX");
            PlayerPrefs.DeleteKey("PlayerPosY");
            PlayerPrefs.DeleteKey("PlayerPosZ");
            PlayerPrefs.DeleteKey("PlayerRotY");
            PlayerPrefs.DeleteKey("ShouldRestorePosition");
        }
    }
}
