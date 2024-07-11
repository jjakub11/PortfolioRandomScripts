using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AiAgent : MonoBehaviour
{

    public AiStateId initialState; //setUpDelay
    [Header("CurrentState")]
    public AiStateId currentState;
    //public AiAgentConfig config;
    public CharacterLocalMap chD;
    [SerializeField] AIPath pathfindingAI;
    public IAstarAI ai;
    [HideInInspector] public AiStateMachine stateMachine;

    [Space]
    public bool setUpDelayDone;
    [Space]
    [Header("Navigation")]
    [SerializeField] public Vector3 targetDestination;



    //debug

    Vector3 closestWalkablePos = Vector3.zero;
    Vector3 targetClosestPos = Vector3.zero;

    

    private void OnEnable()
    {
        ai = GetComponent<IAstarAI>();
        // Update the destination right before searching for a path as well.
        // This is enough in theory, but this script will also update the destination every
        // frame as the destination is used for debugging and may be used for other things by other
        // scripts as well. So it makes sense that it is up to date every frame.
        if (ai != null) ai.onSearchPath += Update;
    }

    void OnDisable()
    {
        if (ai != null) ai.onSearchPath -= Update;
    }


    void Start()
    {
        stateMachine = new AiStateMachine(this);
      
        stateMachine.RegisterState(new AiWanderAroundState());
        stateMachine.RegisterState(new AiSetUpDelayState());
        stateMachine.RegisterState(new AiWorkAssignmentsState());
        stateMachine.RegisterState(new AiCombatState());

        stateMachine.ChangeState(AiStateId.SetUpDelay);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = stateMachine.currentState;
        stateMachine.Update();

        if (targetDestination != Vector3.zero && ai != null) 
        {
            ai.destination = targetDestination;
        }

      



    }

    private void OnDrawGizmosSelected()
    {
        
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(closestWalkablePos, .15f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetClosestPos, .16f);

    }

    public float GetAgentMaxSpeed() //calculate, default is 10f for now
    {
        if (chD.CanMove()) 
        {
            return 10f * TimeManager.instance.PureGDTMultiplier();
        }
        else 
        {
            return 0f;
        }
        
    }

    public void ToggleAIAgentStop(bool _state) 
    {

            ai.isStopped = _state;

    }

    public void RotateCharacterTowards(Vector3 targetPos) 
    {
        targetPos.y = 0f;
        transform.LookAt(targetPos);
    }

    public void CharacterSituationCheck(bool checkForAttack)
    {
        CheckCurrentBehaviour();
      


        

    }

    public void CheckCurrentBehaviour() 
    {
        if (chD.currentBehaviourMode == CharacterLocalMap.CurrentBehaviourMode.PlayerWorkMode && currentState != AiStateId.DefaultWorkAssignments) 
        {
            stateMachine.ChangeState(AiStateId.DefaultWorkAssignments);
        }
        else if (chD.currentBehaviourMode == CharacterLocalMap.CurrentBehaviourMode.CombatMode && currentState != AiStateId.Combat) 
        {
            //CURRENT WO IS CANCELED IN EXIT METHOD OF AIWORKASSIGNMENTSTATE
            SetTargetDestination(transform.position);
            stateMachine.ChangeState(AiStateId.Combat);
        }
    }


    public bool IsNearTargetDestination() 
    {
        if (Vector3.Distance(transform.position, targetDestination) < 1.2f) 
        {
            return true;
        }
        else return false;
    }
   
   


    public bool ReachedDestination() 
    {
         if (ai.reachedDestination) 
         {
             return true;
         }
         else return false;
    }

    public bool HasPath() 
    {
        if (ai.hasPath || ai.pathPending) 
        {
            return true;
        }
        else return false;
    }

    public void SetTargetDestination(Vector3 _targetDestination) 
    {
        targetDestination = _targetDestination;
    }

    public Vector3 CurrentWorkOrderDestination() 
    {
        if (chD.work.blockMining_WorkOrder && chD.work.blockMining_WorkOrder.targetBlockData)
        {
            return chD.work.blockMining_WorkOrder.targetBlockData.transform.position;
        }

        else if(chD.work.itemHauling_WorkOrder && chD.work.itemHauling_WorkOrder.targetItemContainer) //workorder data
        {
            if (chD.heldItem.currentlyHeldItemContainer == chD.work.itemHauling_WorkOrder.targetItemContainer) //return place position
            {
                return chD.work.itemHauling_WorkOrder.targetStorageTile.transform.position;
            }
            else //return itemC position
            {
                return chD.work.itemHauling_WorkOrder.targetItemContainer.transform.position;
            }
        }

        else if (chD.work.structureBuilding_WorkOrder) 
        {
            return chD.work.structureBuilding_WorkOrder.targetTileData.transform.position;
        }

        else if (chD.work.shipEnter_WorkOrder)
        {
            return chD.work.shipEnter_WorkOrder.targetShip.transform.position;
        }

        else if (chD.work.deliveryToInventory_WorkOrder && chD.work.deliveryToInventory_WorkOrder.workOrderType == WorkOrder.WorkOrderType.DeliverItem) //workorder data
        {
            //DELIVER ITEM

            if (chD.work.deliveryToInventory_WorkOrder.targetItemContainer)
            {
                if (chD.heldItem.currentlyHeldItemContainer == chD.work.deliveryToInventory_WorkOrder.targetItemContainer) //return place position
                {
                    return chD.work.deliveryToInventory_WorkOrder.targetInventoryLocalTransform.transform.position;
                }
                else //return itemC position
                {
                    return chD.work.deliveryToInventory_WorkOrder.targetItemContainer.transform.position;
                }
            }

            else return transform.position;

        }
        else return transform.position;
    }

    public bool CanReachDestination(Vector3 starterPos, Vector3 targetPos) 
    {
       
       //NORTH

        var constraint = NNConstraint.Default;
        constraint.constrainWalkability = true;
        constraint.walkable = true;


        GraphNode startPositionNode = AstarPath.active.GetNearest(starterPos, constraint).node;

        GraphNode targetPositionNode = AstarPath.active.GetNearest(targetPos).node;
        targetClosestPos = (Vector3)targetPositionNode.position;

        Vector3 newTargetPos;
        GraphNode nearestWalkable;



        //NORTH
        newTargetPos = targetPos;
        newTargetPos.z += .1f;
        nearestWalkable = AstarPath.active.GetNearest(newTargetPos, constraint).node;
        closestWalkablePos = (Vector3)nearestWalkable.position;
        closestWalkablePos.y += .5f;
        if (PathUtilities.IsPathPossible(startPositionNode, nearestWalkable) && (Vector3.Distance((Vector3)nearestWalkable.position, targetPos) < 1.1f))
        {
            return true;
        }

        //SOUTH
        newTargetPos = targetPos;
        newTargetPos.z -= .1f;
        nearestWalkable = AstarPath.active.GetNearest(newTargetPos, constraint).node;
        closestWalkablePos = (Vector3)nearestWalkable.position;
        closestWalkablePos.y += .5f;
        if (PathUtilities.IsPathPossible(startPositionNode, nearestWalkable) && (Vector3.Distance((Vector3)nearestWalkable.position, targetPos) < 1.1f))
        {
            return true;
        }

        //WEST
        newTargetPos = targetPos;
        newTargetPos.x += .1f;
        nearestWalkable = AstarPath.active.GetNearest(newTargetPos, constraint).node;
        closestWalkablePos = (Vector3)nearestWalkable.position;
        closestWalkablePos.y += .5f;
        if (PathUtilities.IsPathPossible(startPositionNode, nearestWalkable) && (Vector3.Distance((Vector3)nearestWalkable.position, targetPos) < 1.1f))
        {
            return true;
        }

        //EAST
        newTargetPos = targetPos;
        newTargetPos.x -= .1f;
        nearestWalkable = AstarPath.active.GetNearest(newTargetPos, constraint).node;
        closestWalkablePos = (Vector3)nearestWalkable.position;
        closestWalkablePos.y += .5f;
        if (PathUtilities.IsPathPossible(startPositionNode, nearestWalkable) && (Vector3.Distance((Vector3)nearestWalkable.position, targetPos) < 1.1f))
        {
            return true;
        }

        else 
        {
            //chD.work.GiveUpCurrentWorkOrder();
            return false;
           
        }
    }

    public float DistanceFromTargetDestination() 
    {
        return Vector3.Distance(transform.position, targetDestination);
    }

    

    bool IsAnyConnectedNodeWalkable(GraphNode tPN) 
    {
        bool walkable = false;
        tPN.GetConnections(neighbour => walkable |= neighbour.Walkable);
        return walkable;

    }

    public bool NodeIsNeighbor(GraphNode closestWalkable, GraphNode targetNode) 
    {
        if (closestWalkable.position.x == targetNode.position.x)
        {
            if (Mathf.Abs(closestWalkable.position.z - targetNode.position.z) < 1.01f)
            {
                return true;
            }
            else return false;
        }
        else if (closestWalkable.position.z == targetNode.position.z)
        {
            if (Mathf.Abs(closestWalkable.position.x - targetNode.position.x) < 1.01f)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    public bool CanReachDestinationOld(Vector3 starterPos, Vector3 targetPos) 
    {
        var constraint = NNConstraint.None;
        constraint.constrainWalkability = true;
        constraint.walkable = true;

        GraphNode node1 = AstarPath.active.GetNearest(starterPos, constraint).node;
        GraphNode node2 = AstarPath.active.GetNearest(targetPos, NNConstraint.Default).node;

        
        GraphNode nearestWalkable = AstarPath.active.GetNearest(targetPos, constraint).node;

        if (PathUtilities.IsPathPossible(node1, node2) )
        {
            return true;
        }
        else 
        {
            //chD.work.GiveUpCurrentWorkOrder();
            return false;
           
        }
    }

    //WORK

    public void EnterWorkAssignmentsState() 
    {
        if (stateMachine.currentState != AiStateId.DefaultWorkAssignments && chD.work.HasWorkOrder()) 
        {
            stateMachine.ChangeState(AiStateId.DefaultWorkAssignments);
        }
    }

    //AI

    public void AI_GetSuitableState() 
    {
        if (chD.characterData.currentRaidEvent) 
        {
            if (chD.characterData.currentRaidEvent.raidTactic == LME_Raid.RaidTactic.ImmediateAttack) 
            {
                chD.ChangeBehaviourMode(CharacterLocalMap.CurrentBehaviourMode.CombatMode);
            }
        }
    }

    //COMBAT

   

    /*
    public void EnterCombat_General(CharacterLocalMap characterLocalMap) //decide which via inventory and preferences
    {
        EnterMeleeCombat(characterLocalMap);
    }

    public void LostCurrentTarget() 
    {
        stateMachine.ChangeState(AiStateId.WanderAround);
    }

    public bool CanEnter_MeleeCombatState() 
    {
        return true;
        if (!chD.characterData.isDead && stateMachine.currentState != AiStateId.MeleeCombat  && stateMachine.currentState != AiStateId.Death)
        {
            return true;
        }
        else return false;
}
    public void EnterMeleeCombat(CharacterLocalMap _characterLocalMap) 
    {
        if (CanEnter_MeleeCombatState() && _characterLocalMap)
        {
            //chD.combat.SetAsCurrentTarget_AI(_characterLocalMap);
            stateMachine.ChangeState(AiStateId.MeleeCombat);
        }
    }

    public bool IsInMeleeRange()
    {
        return true;

        /*if (Vector3.Distance(navMeshAgent.destination, transform.position) < navMeshAgent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    
    //public bool CheckForNewTargetAndLoadIfAvalible()

   public Vector3 PositionOfCurrentCombatTarget() 
    {
        return Vector3.zero;//chD.combat.currentTarget.transform.position;
    }
*/




}
