using UnityEngine;
using UnityEngine.Splines;

public class WaterEntityWanderState : WaterEntityBaseState
{
    private Spline currentSpline;
    private float timeSincePathChange = 0f;
    private float pathChangeInterval = 60f;
    private Vector3 targetPoint;
    private int index = 0;

    public override void EnterState(WaterEntityAI main)
    {
        currentSpline = FindNewSpline(main);
        index = Random.Range(0, currentSpline.Count);
        targetPoint = GetNearestKnot(main);
        main.ChangeAnimationState(main.SwimAnimationName);
    }

    public override void FixedUpdateState(WaterEntityAI main)
    {
        if (currentSpline == null) return;
        if (Vector3.Distance(main.Rigidbody.position, targetPoint) <= 3f)
        {
            targetPoint = GetNearestKnot(main);
        }

        Vector3 direction = (targetPoint - main.Rigidbody.position).normalized;
        Debug.DrawLine(main.transform.position, targetPoint, Color.green);

        // Calculate the rotation towards the target using Slerp
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        main.Rigidbody.MoveRotation(Quaternion.Slerp(main.Rigidbody.rotation, targetRotation, Time.deltaTime * 5));

        // Add force in the direction of movement
        main.Rigidbody.AddForce(direction * main.entityData.WanderSpeed, ForceMode.Acceleration);

        timeSincePathChange += Time.deltaTime;

        if (timeSincePathChange >= pathChangeInterval)
        {
            Debug.Log("Changing spline path.");
            currentSpline = FindNewSpline(main);
            timeSincePathChange = 0f;
        }
    }

    public Spline FindNewSpline(WaterEntityAI main)
    {
        foreach (var splineDepth in main.entityManager.splinesByDepths)
        {
            if (splineDepth.SplineContainer != null && splineDepth.DepthRange.x <= main.entityData.DepthRange.x && splineDepth.DepthRange.y >= main.entityData.DepthRange.y)
            {
                if (splineDepth.SplineContainer.Splines.Count > 0)
                {
                    int randomSplineIndex = Random.Range(0, splineDepth.SplineContainer.Splines.Count);
                    if (splineDepth.SplineContainer.Splines[randomSplineIndex] != null)
                    {
                        return splineDepth.SplineContainer.Splines[randomSplineIndex];
                    }
                }
            }
            else Debug.LogWarning("Spline container empty");
        }
        return null;
    }
    public Vector3 GetNearestKnot(WaterEntityAI main)
    {
        Vector3 pos = currentSpline.Next(index).Position;
        index = currentSpline.NextIndex(index);

        // Add random difference to the position
        pos += new Vector3(Random.Range(-main.entityData.WanderRadius, main.entityData.WanderRadius),
                           Random.Range(-main.entityData.WanderRadius, main.entityData.WanderRadius),
                           Random.Range(-main.entityData.WanderRadius, main.entityData.WanderRadius));

        if (pos.y > main.entityData.DepthRange.y)
        {
            pos.y = main.entityData.DepthRange.y - Random.Range(1,5);
        }

        return main.entityManager.transform.TransformPoint(pos);
    }
}
