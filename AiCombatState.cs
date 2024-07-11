using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiCombatState : AiState
{
    


    public AiStateId GetId()
    {
        return AiStateId.Combat;
    }

    public void Enter(AiAgent agent)
    {
        Debug.Log("enteredCombatMode");
        if (agent.chD.characterData.PlayersCharacter()) 
        {
            agent.chD.SetCombatModeGFX(true);
        }
       
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

        if (agent.chD.isSwitchingEquipment) 
        {
            agent.ToggleAIAgentStop(true);
            return;
        }

        if (agent.chD.combat.currentTarget) 
        {
            if (!agent.chD.characterData.characterInventory.EquippedForRangedCombat()) 
            {
                if (agent.chD.combat.InDistanceForMeleeAttack())
                {
                    if (agent.chD.MeleeWeapon() && !agent.chD.MeleeWeaponLocal_Loaded())//haslocalweapon 
                    {
                        agent.chD.MW_Local_Load();
                    }

                    agent.chD.TryToggleCombatMode(CharacterCombat.CombatMode.Melee);
                    agent.chD.SetMeleeWeaponAnimation();



                    if (agent.chD.combat.Melee_IsTooCloseToTarget() && agent.chD.combat.CanJumpBack())
                    {
                        agent.SetTargetDestination(agent.chD.combat.IdealMeleeCombatPosition());
                        //Debug.Break();
                        agent.chD.combat.JumpBackToTargetDestination();
                    }

                    else if (agent.chD.combat.CanMeleeAttack())
                    {

                        agent.chD.combat.Attack();
                    }

                }
                else
                {
                    //Debug.Log("distance from cTarget is " + agent.chD.combat.DistanceFromCurrentTarget());
                    agent.chD.TryToggleCombatMode(CharacterCombat.CombatMode.None);

                    if (agent.CanReachDestination(agent.chD.transform.position ,agent.chD.combat.currentTarget.transform.position)) 
                    {
                        agent.ToggleAIAgentStop(false);
                        agent.SetTargetDestination(agent.chD.combat.currentTarget.transform.position);
                    }
                    else 
                    {
                        agent.ToggleAIAgentStop(true);
                    }
                   
                }
            }

            else //is equipped for ranged combat
            {

                if (agent.chD.combat.InDistanceForRangedAttack()) 
                {
                    if (!agent.chD.RangedWeaponLocal_Loaded())//haslocalweapon 
                    {
                        agent.chD.RW_Local_Load();
                    }
                   
                    
                    agent.chD.TryToggleCombatMode(CharacterCombat.CombatMode.Ranged);
                    agent.chD.SetRangedWeaponAnimation();


                    if (agent.chD.combat.CanFireRangedWeapon()) //canfire check states of player, not weapon //stagger, downed....
                    {
                        if (agent.chD.characterData.characterInventory.equippedRangedWeapon.currentAmmoClipCount > 0) 
                        {
                            agent.chD.combat.FireRangedWeapon();
                        }
                        else 
                        {
                            //RELOAD
                        }
                       
                    }
                }
                else
                {
                   // Debug.Log("distance from cTarget is " + agent.chD.combat.DistanceFromCurrentTarget());
                    agent.chD.TryToggleCombatMode(CharacterCombat.CombatMode.None);
                    agent.SetTargetDestination(agent.chD.combat.currentTarget.transform.position);
                }
            }
            
        }
        else if (!agent.chD.characterData.PlayersCharacter()) 
        {
            if (agent.chD.characterData.currentRaidEvent) 
            {
                if (agent.chD.characterData.currentRaidEvent.raidTactic == LME_Raid.RaidTactic.ImmediateAttack) 
                {
                    agent.chD.localMapManager.currentLocalMapBase.localTargetAssigner.TryAddAttacker(agent.chD.characterData);
                  
                }
            }

           
        }
        




    }


    public void Exit(AiAgent agent)
    {
        agent.chD.combat.SetAsCurrentTarget(null);
        agent.chD.localMapManager.currentLocalMapBase.localTargetAssigner.TryRemoveAttacker(agent.chD.characterData);


        agent.chD.TryToggleCombatMode(CharacterCombat.CombatMode.None);
        agent.chD.SetCombatModeGFX(false);

        agent.chD.combat.ResetAllAttackIDs();
    }
   
   




}