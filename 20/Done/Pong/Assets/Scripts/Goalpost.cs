using Unity.Netcode;
using UnityEngine;

public class Goalpost : MonoBehaviour
{
    // 골대 주인 플레이어 번호
    public int playerNumber;
    
    // 골대에 공이 들어갔을 때 호출할 콜백
    public void OnGoal()
    {
        // 서버에서만 점수 처리를 실행함
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        
        // 골을 넣은 플레이어의 점수를 1점 증가시킴
        if(playerNumber == 1)
        {
            GameManager.Instance.AddScore(0, 1);
        }
        else
        {
            GameManager.Instance.AddScore(1, 1);
        }
    }
}
