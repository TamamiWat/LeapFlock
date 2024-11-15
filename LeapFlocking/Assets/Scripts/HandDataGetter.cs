
using UnityEngine;
using Leap.Unity;
using Leap;

public class HandDataGetter : MonoBehaviour
{
    public LeapProvider provider;
    private Vector3[] fingerPosition = new Vector3[5];
    public GameObject prefab;
    public Vector3 offset = Vector3.zero;
    public Vector3 scale = Vector3.one;
    
    private Vector3? userPos;
    public Vector3? UserPos
    {
        get { return userPos; }
    }
    // private void Awake()
    // {
    //     // Vsync Count を 0にすることにより、FPS を固定できるようになる
    //     QualitySettings.vSyncCount = 0;
    //     Application.targetFrameRate = 60;
    // }

    void Start()
    {
        userPos = null;
    }

    void Update()
    {
        Frame frame = provider.CurrentFrame;
        if(frame.Hands.Count != 0)
        {
            foreach (Hand hand in frame.Hands)
            {

                if (hand.IsLeft || hand.IsRight) 
                {
                    //人差し指のみ取得
                    Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                    userPos = new Vector3(indexFinger.TipPosition.x, indexFinger.TipPosition.y, indexFinger.TipPosition.z);
                    userPos = Vector3.Scale(userPos.Value, scale);
                    userPos += offset;

                    // Debug.Log(userPos);
                    // if (userPos.HasValue && prefab != null)
                    // {
                    //     // Instantiateでオブジェクトを生成
                    //     Instantiate(prefab, userPos.Value, Quaternion.identity);
                    // }
                }
            }
        }
        else
        {
            userPos = null;
        } 
    }
}