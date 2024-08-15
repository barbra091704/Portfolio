using UnityEngine;

public class WaterEntityFleeState : WaterEntityBaseState
{
    private Vector3 fleeDirection;

    public override void EnterState(WaterEntityAI main)
    {
        // Pick a random direction within a 45-degree angle away from the player
        float angle = Random.Range(-45f, 45f);
        fleeDirection = Quaternion.Euler(0f, angle, 0f) * (main.transform.position - main.Target.position).normalized;
        main.ChangeAnimationState(main.FleeAnimationName); 
    }

    public override void FixedUpdateState(WaterEntityAI main)
    {
        if (Vector3.Distance(main.Rigidbody.position, main.Target.position) <= main.entityData.FleeDistance)
        {
            Debug.DrawLine(main.transform.position, main.transform.position + fleeDirection, Color.yellow);

            Quaternion targetRotation = Quaternion.LookRotation(fleeDirection, Vector3.up);
            main.Rigidbody.MoveRotation(Quaternion.Slerp(main.Rigidbody.rotation, targetRotation, Time.deltaTime * 5));

            main.Rigidbody.AddForce(fleeDirection * main.entityData.FleeSpeed, ForceMode.Acceleration);

        }
        else
        {
            main.ChangeState(main.WanderState);
            main.ChangeAnimationState("Shark_Swim");
        }
    }
}
