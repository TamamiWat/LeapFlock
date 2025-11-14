using UnityEngine;
using extOSC;

public class ReceiveOSC : MonoBehaviour
{
    [SerializeField] int port = 8000;
    [SerializeField] string IPaddress = "127.0.0.1";
    [SerializeField] string address = "/leap/index_f";
    private OSCReceiver receiver;
    public Vector3? UserPos { get; private set; }
    public int? IsUserExist { get; private set; }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;   
        Application.targetFrameRate = 60; 
    }

    void Start()
    {
        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalHost = IPaddress;
        receiver.LocalPort = port;
        receiver.Bind(address, MessageReceived);
    }

    void MessageReceived(OSCMessage message)
    {
        UserPos = new Vector3(
            message.Values[0].FloatValue,
            message.Values[1].FloatValue,
            message.Values[2].FloatValue
        );

        IsUserExist = message.Values[3].IntValue;

        Debug.Log($"extOSC receive: {message.Values[0].FloatValue} {message.Values[1].FloatValue} {message.Values[2].FloatValue}{message.Values[3].IntValue}");
    }
}