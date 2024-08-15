using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public Transform upstairsTarget;
    public Transform downstairsTarget;
    
    [SerializeField] private float goUpstairsChance = 0.5f;
    [SerializeField] private float goDownstairsChance = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Civilian"))
        {
            Transform target = GetCloserTarget(other.transform);

            if (target != null)
            {
                if (target == upstairsTarget && Random.value < goUpstairsChance)
                {
                    other.transform.position = new(upstairsTarget.position.x, upstairsTarget.position.y, 0f);
                }
                else if (target == downstairsTarget && Random.value < goDownstairsChance)
                {
                    other.transform.position = new(downstairsTarget.position.x, downstairsTarget.position.y, 0f);
                }
            }
        }
    }

    private Transform GetCloserTarget(Transform civilian)
    {
        float distanceToUpstairs = Vector3.Distance(civilian.position, upstairsTarget.position);
        float distanceToDownstairs = Vector3.Distance(civilian.position, downstairsTarget.position);

        if (distanceToUpstairs < distanceToDownstairs)
        {
            return downstairsTarget;
        }
        else
        {
            return upstairsTarget;
        }
    }
}
