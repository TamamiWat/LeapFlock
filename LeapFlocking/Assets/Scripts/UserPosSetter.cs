using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPosSetter : MonoBehaviour
{

    public Material material;
    public GameObject leapmotion;
    public float distanceThreshold = 10;
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
        Vector3 myPos = transform.position;
        if(user.UserPos.HasValue)
        {
            Vector3 userPosition = user.UserPos.Value;
            float distance = Vector3.Distance(myPos, userPosition);
            if(distance < distanceThreshold)
            {
                material.SetVector("_UserPosition", userPosition);
                material.SetInt("_OnUser", 1);
            }
            else
            {
                material.SetInt("_OnUser", 0);
            }
            
        }
        else
        {
            material.SetInt("_OnUser", 0);
            Debug.Log("No hand detected");
        }
    }
}
