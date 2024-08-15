using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Monster : MonoBehaviour
{
    public GameObject monsterObject;
    public SHoppingCart sc;
    public Grabbing g;
    public NavMeshAgent agent;
    public Transform player;
    public GameObject playerkill;
    private RaycastHit hit;
    private Ray ray;
    public LayerMask whatIsGround, whatIsPlayer;
    public AudioSource ahh;
    public AudioSource seesyou;
    public Transform[] Spawns;
    public Transform SpawnL;
    public Animator monsterAnim;
    public bool isAttacked;
    private bool isactive;
    public int _health;
    private Vector3 trans;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject hitScreen;



    [SerializeField] private AudioClip killClip;



    public float difficultySpeed, difficultySpeedRun;
    //patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //states
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        hitScreen.gameObject.SetActive(false);
        isAttacked = false;
        player = GameObject.Find("PLAYER//////////////////////////").transform;
        agent = GetComponent<NavMeshAgent>();
        SpawnL = Spawns[Random.Range(0, Spawns.Length)];
        monsterObject.transform.position = SpawnL.transform.position;
    }


    void Update()
    {
        if (agent.velocity == Vector3.zero)
        {
            SearchWalkPoint();
        }
        //check for sight and attackrange
        if (!sc._flashlightON)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange * 0.5f, whatIsPlayer);
        }
        if (sc.rb.velocity.magnitude > 5 && !sc._flashlightON)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        }
        if (sc.rb.velocity.magnitude > 8)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange * 1.5f, whatIsPlayer);
        }
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patrolling()
    {

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);
        agent.speed = difficultySpeed;

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpointreached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;

    }
    private void ChasePlayer()
    {
        walkPointSet = false;
        agent.SetDestination(player.position);
        agent.speed = difficultySpeedRun;
        if (!isactive)
        {
            StartCoroutine("seeU");
        }
    }

    void AttackPlayer()
    {
        if (!alreadyAttacked)
        {
            isAttacked = true;
            ahh.PlayOneShot(killClip);
            StartCoroutine(Diepe());
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            Debug.Log("g");
            sc.punch.Play();
            sc.cartAudio.volume = 2;
            
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    IEnumerator Diepe()
    {
        _health--;
        if (_health < 1)
        {
            g.heart1.gameObject.SetActive(false);
            agent.SetDestination(monsterObject.transform.position);
            agent.transform.LookAt(player);
            agent.velocity = Vector3.zero;
            ahh.PlayOneShot(killClip);
            yield return new WaitForSeconds(3f);
            isAttacked = false;
            SceneManager.LoadScene("Start");
            yield return new WaitForSeconds(0.5f);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Start"));
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            g.heart2.gameObject.SetActive(false);
            hitScreen.gameObject.SetActive(true);
            isAttacked = false;
            SpawnL = Spawns[Random.Range(0, Spawns.Length -2 + 1)];
            monsterObject.transform.position = SpawnL.transform.position;
            sc.maxSpeed = 11;
            sc.sprintSpeed = 11;
            sc.sprintTimer = 10;
            cam.fieldOfView = 65;
            yield return new WaitForSeconds(10f);
            hitScreen.gameObject.SetActive(false);
            sc.maxSpeed = 9;
            sc.sprintSpeed = 9;
            sc.sprintTimer = sc.sprintTime;
            cam.fieldOfView = 55;
    
       }


   }
    IEnumerator seeU()
    {
        isactive = true;
        if (!seesyou.isPlaying)
        seesyou.Play();
        yield return new WaitForSeconds(5f);
        isactive = false;
    }
}
