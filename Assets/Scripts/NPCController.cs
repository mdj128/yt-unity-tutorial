
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public Transform player;
    public float chaseDistance = 10f;
    public float stealDistance = 2f;
    public float stealCooldown = 2f;
    public int stealAmount = 1;
    public AudioClip stealSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private float lastStealTime;

    private enum State
    {
        Idle,
        Chasing,
        Stealing
    }

    private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            player = PlayerController.instance.transform;
        }

        currentState = State.Idle;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer < chaseDistance)
                {
                    currentState = State.Chasing;
                }
                break;

            case State.Chasing:
                if (distanceToPlayer <= stealDistance)
                {
                    currentState = State.Stealing;
                    agent.isStopped = true;
                }
                else if (distanceToPlayer > chaseDistance)
                {
                    currentState = State.Idle;
                    agent.isStopped = true;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
                break;

            case State.Stealing:
                if (distanceToPlayer > stealDistance)
                {
                    currentState = State.Chasing;
                }
                else
                {
                    if (Time.time - lastStealTime > stealCooldown)
                    {
                        Steal();
                        lastStealTime = Time.time;
                    }
                }
                break;
        }

        if (animator != null)
        {
            animator.SetFloat("Vel", agent.velocity.magnitude);
        }
    }

    void Steal()
    {
        if (CollectableCounter.instance != null && CollectableCounter.instance.currentCount > 0)
        {
            CollectableCounter.instance.RemoveFromCount(stealAmount);
            if (audioSource != null && stealSound != null)
            {
                audioSource.PlayOneShot(stealSound);
            }
        }
    }
}
