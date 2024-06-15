using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : Character
{

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private float speedTarget;
    [SerializeField] private float speedRunningTarget;
    [SerializeField] private float speedCurrent;
    private Vector2 axis;
    private float refSpeed;
    [SerializeField] private float speedSmooth;
    private float angle;
    [SerializeField] private float rotationSmooth;
    private  Quaternion targetRotation;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxDistance;
    [SerializeField] private bool IsOnGround;
    private int extraJump = 1;
    [SerializeField] private bool isRunning;
    private CameraManager _camera;
    [SerializeField] private int healt = 100;
    [SerializeField] private bool isDead;

    // Start is called before the first frame update
    private void Awake()
    {
        _camera = Camera.main.GetComponent<CameraManager>();
    }
    private void Start()
    {
        //transform.rotation = Quaternion.Euler(90, 0, 0 );
    }

    // Update is called once per frame
    void Update()
    {

        if (IsOnGround)
        {
            extraJump = 1;
            axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            isRunning = Input.GetKey(KeyCode.LeftShift);

            if(Input.GetKey(KeyCode.LeftAlt))
            {
                _animator.SetTrigger("Dodge");
            }
        }
        

        //speedCurrent = Mathf.SmoothDamp(speedCurrent, speedTarget * axis.magnitude, ref refSpeed, speedSmooth);
        //transform.position += new Vector3 (axis.x, 0, axis.y) * speedTarget * Time.deltaTime;
       
        speedCurrent = Mathf.SmoothDamp(speedCurrent, (isRunning ? speedRunningTarget : speedTarget) * axis.magnitude, ref refSpeed, speedSmooth);
        if (axis.magnitude > 0.1f)
        {
            angle = Mathf.Atan2(-axis.y, axis.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.Euler(0,angle+180 - _camera.Angle, 0);
        }
        if(Input.GetKeyDown(KeyCode.Space) && (IsOnGround || extraJump >=1))
        {
           _rigidbody.AddForce(Vector3.up * jumpForce * _rigidbody.mass);
           extraJump --;
        }
        
        IsOnGround = Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, maxDistance, groundLayer);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);        


    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(transform.forward.x * speedCurrent, _rigidbody.velocity.y, transform.forward.z * speedCurrent);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.down * maxDistance);
    }
    private void LateUpdate()
    {
        _animator.SetFloat("Velocity", !IsOnGround ? 0 : (speedCurrent / (isRunning ? speedRunningTarget : speedTarget)));
        _animator.SetBool("IsRunning", isRunning);
    }
    public void ApplyDamage(int  damage)
    {
        if (isDead)
        {
            return;
        }
        healt -= damage;
        Debug.Log("ApplyDamage");
        if(healt <=0)
        {
            isDead = true;
            _animator.SetTrigger("IsDead");
        }
    }
}
