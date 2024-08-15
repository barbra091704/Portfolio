using UnityEngine;

public class ShadowmanDecisionMakerBehaviour : IDecisionMaker
{
    private IMovement movementBehavior;
    private ICombat combatBehavior;
    private ISight perceptionBehavior;
    public Shadowman main;


    public void Initialize(Transform transform, IMovement movementBehavior, ICombat combatBehavior, ISight perceptionBehavior)
    {
        this.movementBehavior = movementBehavior;
        this.combatBehavior = combatBehavior;
        this.perceptionBehavior = perceptionBehavior;
        main = transform.GetComponent<Shadowman>();
    }

    public void EvaluateActions(Transform target)
    {
        // Implement the logic to evaluate the current situation and determine the appropriate actions for the Shadowman
        if (target != null && perceptionBehavior.CanSeeTarget(target))
        {
            MonoBehaviour.print("SHADOWMAN: can see u");
            if (!perceptionBehavior.CanTargetSeeMe(main.renderer))
            {
                // If the target is not looking at the Shadowman, it can chase and attack
                if (perceptionBehavior.IsTargetInAttackRange(target))
                {
                    if (main.attackAmount < main.entityData.AttackAmount)
                    {
                        MonoBehaviour.print("SHADOWMAN: in Attacc fr");
                        combatBehavior.Attack(target);
                    }
                }
                else
                {
                    MonoBehaviour.print("SHADOWMAN: Chasing and shi");
                    movementBehavior.Chase(target);
                    main.agent.speed = main.entityData.AttackSpeed;
                }
            }
            else
            {
                MonoBehaviour.print("SHADOWMAN: is Fleein");
                movementBehavior.Patrol();
                main.agent.speed = main.entityData.FleeSpeed;
                
            }
        }
        else
        {
            main.attackAmount = 0;
            MonoBehaviour.print("SHADOWMAN: Wanderin and stuff");
            movementBehavior.Patrol();
            main.agent.speed = main.entityData.WanderSpeed;
        }
    }
}
