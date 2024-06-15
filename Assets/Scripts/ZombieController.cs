using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieState{None, Patrol, Chase, Search, Attack}

public class ZombieController : MonoBehaviour
{
    [SerializeField] private PlayerController target;
    [SerializeField] private ZombieState state;
    [SerializeField] private List<Transform> nodes;
    [SerializeField] private NavMeshAgent navMesh;
    [SerializeField] private Animator _animator;
    [SerializeField] private int currentNode = 0;
    private bool canAttack;
    [SerializeField] private float attackRangeDistance;
    [SerializeField] private float attackRangeAngle;
    [SerializeField] private float delayChasing;
    [SerializeField] private float delaySearching;
    [SerializeField] private Transform hand;
    [SerializeField] private LayerMask characterLayerMask;
    [SerializeField] private Vector3 handSize;
    private bool isAttacking;
    private bool flagAttack;
    public bool IsAttacking
    {
        get => isAttacking;
        set
        {
            isAttacking = value;
            if(!value)
            {
                flagAttack = true;
            }
        }
    }
    private int CurrentNode
    {
        get{ return currentNode; }
        set{
            currentNode = value;
            currentNode = currentNode % nodes.Count;
            navMesh.SetDestination(nodes[currentNode].position);
        }
    }

    [SerializeField] private float distance;
    [SerializeField] private float rangeDistance;
    [SerializeField] private float angle;
    [SerializeField] private float rangeAngle;
    [SerializeField] private bool canSee = false;
    private float timer;

    
    // Start is called before the first frame update
    void Start()
    {
        //CurrentNode = 0;
        State  = ZombieState.Patrol;

    }


    private void Update()
    {
        
        CalculateMeassures();
        var collider = Physics.OverlapBox(hand.position, handSize, hand.rotation, characterLayerMask);
        if (isAttacking && flagAttack && collider.Length > 0)
        {
            
            collider[0].GetComponent<PlayerController>().ApplyDamage(10);
            flagAttack = false;
        }
        if(canAttack)
        {
            State = ZombieState.Attack;
            return;
        }

        if(State == ZombieState.Patrol || State == ZombieState.Search) { 
            if(canSee)
            {
                State = ZombieState.Chase;
                timer = 0;
            } else if(!canSee && State != ZombieState.Patrol) {
                timer += Time.deltaTime;
                if(timer > delaySearching)
                {
                    State = ZombieState.Patrol;
                    timer = 0;
                }
            }
        }else if(State == ZombieState.Chase)
        {
            if(!canSee)
            {
                timer += Time.deltaTime;
                if(timer > delayChasing)
                {
                    State = ZombieState.Search;
                    timer = 0;
                }
            }
            else
            {
                timer = 0;
            }
        }
    }

    private void LateUpdate()
    {
        float velocity = navMesh.velocity.magnitude / navMesh.desiredVelocity.magnitude;
        _animator.SetFloat("Velocity", float.IsNaN(velocity) ? 0 : velocity);
    }
    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Nodo")&& other.transform == nodes[currentNode] && State == ZombieState.Patrol)
        {
            CurrentNode++;
        }
    }
    private void CalculateMeassures()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;
        angle = Vector3.Angle(direction, transform.forward);
        distance = Vector3.Distance(transform.position, target.transform.position);
        canSee = distance < rangeDistance && angle < rangeAngle;
        canAttack = distance < attackRangeDistance && angle < attackRangeAngle;
    }

    // Update is called once per frame
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, rangeAngle, 0) * transform.forward * rangeDistance));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, rangeAngle / 2.0f, 0) * transform.forward * rangeDistance));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, 0) * transform.forward * rangeDistance));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -rangeAngle / 2.0f, 0) * transform.forward * rangeDistance));
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -rangeAngle, 0) * transform.forward * rangeDistance));
        
        Gizmos.color = Color.blue;
        for(int i = 0; i < nodes.Count; i++)
        {
            Gizmos.DrawLine(nodes[i].position, nodes[(i+1)% nodes.Count].position);
        }
        

        Gizmos.color = State switch { 
            ZombieState.None => Color.black,
            ZombieState.Patrol => Color.green,
            ZombieState.Chase => Color.yellow,
            ZombieState.Search => Color.blue,
            ZombieState.Attack => Color.red
        };
        Gizmos.DrawSphere(transform.position + Vector3.up*2,0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hand.position, handSize);
    }
    private IEnumerator AttackCoroutine()
    {
        CancelInvoke(nameof(ChaseTarget));
        navMesh.isStopped = true;
        _animator.SetTrigger("Attack");
        yield return new WaitForSeconds(1);
        State = ZombieState.Chase;
    }
    private void ChaseTarget() => navMesh.SetDestination(target.transform.position);
    public ZombieState State
    {
        get
        {
            return state;
        }
        set
        {
            if (state != value)
            {
                state = value;
                switch (state)
                {
                    case ZombieState.Patrol:
                        CancelInvoke(nameof(ChaseTarget));
                        navMesh.isStopped = false;
                        CurrentNode = CurrentNode;
                        break;
                    case ZombieState.Chase:
                        InvokeRepeating(nameof(ChaseTarget),0,0.5f);
                        navMesh.isStopped = false;
                        break;
                    case ZombieState.Search:
                        CancelInvoke(nameof(ChaseTarget));
                        navMesh.isStopped = true;
                        break;
                    case ZombieState.Attack:
                        StartCoroutine(AttackCoroutine());
                        break;
                }
            }
        }
    }
}
