using UnityEngine;
using UnityEngine.AI;

public class ShadowmanMovementBehaviour : IMovement
{
    private NavMeshAgent agent;
    private Vector3 wanderPosition;
    private EntityData entityData;

    public void Initialize(Transform transform, NavMeshAgent agent, EntityData entityData)
    {
        this.agent = agent;
        this.entityData = entityData;
        wanderPosition = GetRandomPatrolPoint();
    }
    public void Patrol()
    {
        if (agent.velocity == Vector3.zero || Vector3.Distance(agent.transform.position, wanderPosition) < 2)
        {
            wanderPosition = GetRandomPatrolPoint();
        }
        agent.SetDestination(wanderPosition);
    }

    public void Chase(Transform target)
    {
        // Implement the chasing behavior for the Shadowman
        agent.SetDestination(target.position);
    }

    public void Flee(Transform target)
    {
        // Implement the fleeing behavior for the Shadowman
        agent.SetDestination(GetFleePosition(target.position));
    }

    Vector3 GetRandomPatrolPoint()
    {
        int attempts = 0;
        do
        {
            attempts++;

            // Generate a random direction
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            
            // Calculate the target position
            Vector3 nextPosition = agent.transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * entityData.WanderRadius;

            // Sample the position on the NavMesh
            NavMesh.SamplePosition(nextPosition, out NavMeshHit hit, entityData.WanderRadius, NavMesh.AllAreas);

            // Check if the sampled position has a valid path
            if (NavMesh.Raycast(agent.transform.position, hit.position, out NavMeshHit pathHit, NavMesh.AllAreas))
            {
                // If there is a clear path, return the position
                return hit.position;
            }

        } while (attempts < 20);

        // If max attempts reached, set agent's destination to a random point within _walkpointDistance
        Vector2 randomDirectionFallback = Random.insideUnitCircle.normalized * entityData.WanderRadius;
        Vector3 fallbackPosition = agent.transform.position + new Vector3(randomDirectionFallback.x, 0f, randomDirectionFallback.y);
        NavMesh.SamplePosition(fallbackPosition, out NavMeshHit fallbackHit, entityData.WanderRadius, NavMesh.AllAreas);

        Debug.LogWarning("Failed to find a suitable walk point after " + attempts + " attempts. Using fallback point.");

        return fallbackHit.position;
    }


    private Vector3 GetFleePosition(Vector3 targetPosition)
    {
        // Implement the logic to get a flee position based on the target's position and the Shadowman's flee distance
        return agent.transform.position - ((targetPosition - agent.transform.position).normalized * entityData.FleeDistance);
    }
}