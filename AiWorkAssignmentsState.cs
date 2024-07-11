using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiWorkAssignmentsState : AiState
{
    ItemContainer targetResourceItemContainer;
    ResourceItem targetResourceItem;
    public AiStateId GetId()
    {
        return AiStateId.DefaultWorkAssignments;
    }

    public void Enter(AiAgent agent)
    {
       //set up target as workAssignments
    }

    public void Update(AiAgent agent)
    {
       
        if (!agent.enabled)
        {
            return;
        }
        else 
        {
            agent.CharacterSituationCheck(true);
        }

      

        if (agent.chD.work.HasWorkOrder()) 
        {
            //Debug.LogWarning("HasWorkOrderPath");
            if (!agent.HasPath() || agent.chD.work.workOrderChanged)
            {
                agent.chD.work.workOrderChanged = false;
                agent.SetTargetDestination(agent.CurrentWorkOrderDestination());
            }
            // else Debug.LogWarning("HasPath");

            //SHIP ENTER WORK ORDER------------------------------------------------------------------------------------------------------------------------------------------
            if (agent.chD.work.shipEnter_WorkOrder)
            {
                if (!agent.chD.work.shipEnter_WorkOrder.WorkOrderCheckPass())
                {
                    return;
                }

                if (agent.IsNearTargetDestination())
                {
                    Debug.LogWarning("1.REACHED DES");
                    agent.chD.CharacterEnteredShip(agent.chD.work.shipEnter_WorkOrder.targetShip.shipData);

                }
            }

            //INVENTORY DELIVERY WORK ORDER------------------------------------------------------------------------------------------------------------------------------------------
            if (agent.chD.work.deliveryToInventory_WorkOrder) 
            {
                if (!agent.chD.work.deliveryToInventory_WorkOrder.WorkOrderCheckPass())
                {
                    return;
                }


                //DELIVER ITEM
                if (agent.chD.work.deliveryToInventory_WorkOrder.workOrderType == WorkOrder.WorkOrderType.DeliverItem) 
                {
                    if (agent.chD.heldItem.currentlyHeldItemContainer && agent.chD.heldItem.currentlyHeldItemContainer != agent.chD.work.deliveryToInventory_WorkOrder.targetItemContainer)
                    {
                        agent.chD.heldItem.DropCurrentItem(agent.chD.localMapManager.GetNearbyFreeTile(agent.chD.transform.position));
                    }

                    agent.SetTargetDestination(agent.CurrentWorkOrderDestination());

                    if (agent.IsNearTargetDestination())
                    {
                        if (agent.chD.heldItem.currentlyHeldItemContainer && agent.chD.heldItem.currentlyHeldItemContainer == agent.chD.work.deliveryToInventory_WorkOrder.targetItemContainer)
                        {
                            agent.chD.work.deliveryToInventory_WorkOrder.targetInventory.AddItemFromItemContainerToInventory(agent.chD.heldItem.currentlyHeldItemContainer);
                        }
                        else //near target container
                        {
                            agent.chD.heldItem.NewHeldItem(agent.chD.work.deliveryToInventory_WorkOrder.targetItemContainer);
                        }




                       


                    }
                }
                
               
                

            }

            //BLOCK MINING WORK ORDER------------------------------------------------------------------------------------------------------------------------------------------
            if (agent.chD.work.blockMining_WorkOrder)
            {
                if (!agent.chD.work.blockMining_WorkOrder.WorkOrderCheckPass())
                {
                    return;
                }


                if (agent.IsNearTargetDestination())
                {
                    Debug.LogWarning("1.REACHED DES");
                    if (agent.chD.work.blockMining_WorkOrder.targetBlockData)
                    {

                        Debug.LogWarning("2.HAS WORK ORDER MINING");
                        agent.RotateCharacterTowards(agent.CurrentWorkOrderDestination());
                        agent.chD.work.blockMining_WorkOrder.targetBlockData.TakeDamage(101f);
                    }

                }
            }

            //ITEM HAULING WORK ORDER------------------------------------------------------------------------------------------------------------------------------------------
            else if (agent.chD.work.itemHauling_WorkOrder) 
            {
                if (!agent.chD.work.itemHauling_WorkOrder.WorkOrderCheckPass())
                {
                    return;
                }


                if (agent.chD.heldItem.currentlyHeldItemContainer && agent.chD.heldItem.currentlyHeldItemContainer != agent.chD.work.itemHauling_WorkOrder.targetItemContainer) 
                {
                    agent.chD.heldItem.DropCurrentItem(agent.chD.localMapManager.GetNearbyFreeTile(agent.chD.transform.position));
                }
                agent.SetTargetDestination(agent.CurrentWorkOrderDestination());
                if (agent.IsNearTargetDestination())
                {
                    if (HaulWorkOrderType_EquipOrPickUp(agent.chD.work.itemHauling_WorkOrder)) 
                    {
                        if (agent.chD.work.itemHauling_WorkOrder.workOrderType == WorkOrder.WorkOrderType.ItemPick) 
                        {
                            agent.chD.characterData.characterInventory.AddItemFromItemContainerToInventory(agent.chD.work.itemHauling_WorkOrder.targetItemContainer);
                        }
                        else if (agent.chD.work.itemHauling_WorkOrder.workOrderType == WorkOrder.WorkOrderType.ItemEquip)
                        {
                            agent.chD.characterData.characterInventory.EquipItemFromItemContainerToInventory(agent.chD.work.itemHauling_WorkOrder.targetItemContainer);
                        }
                    }
                    else 
                    {
                        if (agent.chD.heldItem.currentlyHeldItemContainer && agent.chD.heldItem.currentlyHeldItemContainer == agent.chD.work.itemHauling_WorkOrder.targetItemContainer)
                        {
                            agent.chD.heldItem.DropCurrentItem(agent.chD.work.itemHauling_WorkOrder.targetStorageTile);
                            agent.chD.work.CancelCurrentHaulingOrder();
                        }
                        else //near target container
                        {
                            agent.chD.heldItem.NewHeldItem(agent.chD.work.itemHauling_WorkOrder.targetItemContainer);
                        }
                    }

                    
                }
                
                
            }

            //STRUCTURE BUILDING WORK ORDER------------------------------------------------------------------------------------------------------------------------------------------
            else if (agent.chD.work.structureBuilding_WorkOrder) 
            {
                if (!agent.chD.work.structureBuilding_WorkOrder.WorkOrderCheckPass())
                {
                    return;
                }

                if (agent.chD.heldItem.currentlyHeldItemContainer) //has materials in inventory 
                {
                    if (!agent.chD.work.structureBuilding_WorkOrder.StructureBlueprintHasEnoughMaterials() && agent.chD.heldItem.CurrentlyHeldItemIsRequestedConstructionMaterial(agent.chD.work.structureBuilding_WorkOrder)) //has materials in inventory 
                    {
                        agent.SetTargetDestination(agent.CurrentWorkOrderDestination());
                        if (agent.IsNearTargetDestination())
                        {
                            agent.chD.work.structureBuilding_WorkOrder.structureBlueprint.blueprintInventory.AddItemFromItemContainerToInventory(agent.chD.heldItem.currentlyHeldItemContainer);
                            //place item in inventory
                        }
                    }
                    else //dont need this anyways
                    {
                       // Debug.Log("aidropping why");
                        agent.chD.heldItem.DropCurrentItem(agent.chD.localMapManager.GetNearbyFreeTile(agent.chD.transform.position));
                    }
                }
                else 
                {
                    if (agent.chD.work.structureBuilding_WorkOrder.StructureBlueprintHasEnoughMaterials()) 
                    {
                        agent.SetTargetDestination(agent.CurrentWorkOrderDestination());
                        if (agent.IsNearTargetDestination())
                        {
                            //finish it, reduce work in intervals like attacks
                            agent.chD.localMapManager.FinishConstructionWorkOrder(agent.chD.work.structureBuilding_WorkOrder);
                        }
                    }
                    else 
                    {
                        if (!targetResourceItemContainer)
                        {
                            targetResourceItem = agent.chD.localMapManager.GetResourceItem(agent.chD.work.structureBuilding_WorkOrder.structureBlueprint);
                            if (targetResourceItem) 
                            {
                                targetResourceItemContainer = targetResourceItem.currentItemContainer;
                                targetResourceItemContainer.interactingCharacter = agent.chD;
                            }
                            else 
                            {
                                agent.chD.work.GiveUpCurrentWorkOrder();
                            }
                          
                        }
                       
                        else 
                        {
                            agent.SetTargetDestination(targetResourceItemContainer.transform.position);
                            if (agent.IsNearTargetDestination())
                            {
                                //pick up resources
                                agent.chD.heldItem.NewHeldItem(targetResourceItemContainer);
                                targetResourceItemContainer = null;

                                //if has enough, return to
                            }

                           
                        }
                        //go pick items
                    }
                }
            }

        }
        else //no workorder, idle state:?
        {

        }
        
        


    }


    public void Exit(AiAgent agent)
    {
        if (agent.chD.work.HasWorkOrder())
        {
            agent.chD.work.GiveUpCurrentWorkOrder();
        }

        if (agent.chD.heldItem.currentlyHeldItemContainer)
        {
            agent.chD.heldItem.DropCurrentItem(agent.chD.localMapManager.GetNearbyFreeTile(agent.chD.transform.position));
        }
    }
   
    bool HaulWorkOrderType_EquipOrPickUp(ItemHauling_WorkOrder currentHaulWorkOrder) 
    {
        return currentHaulWorkOrder.workOrderType == WorkOrder.WorkOrderType.ItemEquip || currentHaulWorkOrder.workOrderType == WorkOrder.WorkOrderType.ItemPick;
    }




}