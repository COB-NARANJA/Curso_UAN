using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Transform tTarget;
    private Vector3 refVelocity;
    [SerializeField] private float angle = 0;
    [SerializeField] private float smoth;
    [SerializeField] private float smothRotation;
    [SerializeField] private float distance = 2;
    [SerializeField] private float height = 2;
    // Start is called before the first frame update
    public float Angle {  
        get =>   angle;  
        private set =>  angle = value; 
    } 
    void Start()
    {
        tTarget = GameObject.FindWithTag("Player").transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        Angle += Input.GetAxis("Mouse X");
        distance += Input.mouseScrollDelta.y / 10f;
        distance = Mathf.Clamp(distance, 1, 4);
        Vector3 pos = new Vector3(Mathf.Cos(angle*Mathf.Deg2Rad),0,Mathf.Sin(angle*Mathf.Deg2Rad));
        Vector3 targetPosition = tTarget.position + pos*distance+Vector3.up*height;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref refVelocity, smoth * Time.deltaTime);
        
        
        Vector3 dir = (tTarget.position - transform.position).normalized;
        Quaternion targetRotarion = Quaternion.Euler(25, Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg + 90, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRotarion, Time.deltaTime*smothRotation);
    }
}
