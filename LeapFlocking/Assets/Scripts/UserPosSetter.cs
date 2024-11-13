using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPosSetter : MonoBehaviour
{

    public Material material;
    public GameObject leapmotion;
    private HandDataGetter user;

    void Start()
    {
        leapmotion = GameObject.Find ("LeapMotionManager");
        user = leapmotion.GetComponent<HandDataGetter>();
        
    }


    void Update()
    {
        UpdateUser();
    }

    void UpdateUser()
    {
        if(user.UserPos.HasValue)
        {
            Vector3 position = user.UserPos.Value;
            material.SetVector("_UserPosition", position);
            material.SetInt("_OnUser", 1);
        }
        else
        {
            material.SetInt("_OnUser", 0);
            Debug.Log("No hand detected");
        }
    }
}
