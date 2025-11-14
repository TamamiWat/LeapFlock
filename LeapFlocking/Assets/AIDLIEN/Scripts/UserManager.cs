using UnityEngine;

public class UserManager : MonoBehaviour
{
    public bool debugMode = false;
    [SerializeField] private GameObject _testPrefab;
    [SerializeField] private GameObject _OSCobject;
    private ReceiveOSC user;

    void Start()
    {
        user = _OSCobject.GetComponent<ReceiveOSC>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUser();
    }
    
    void UpdateUser()
    {
        if(user.UserPos.HasValue)
        {
            Vector3 position = user.UserPos.Value;
            if (debugMode && user.IsUserExist.Value != 0)
            {
                GameObject obj = Instantiate(_testPrefab, position, Quaternion.identity);
                Debug.Log("instantiate");
                Destroy(obj, 5.0f);
            }            
        }
    }
}
