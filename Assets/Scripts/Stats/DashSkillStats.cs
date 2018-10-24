/*-------------------------------------------------------------------------*
  # INTR Group 2
  # Student's Name: Kevin Ho, Myles Hagen, Shane Weerasuriya, 
  #					Tianqi Xiao, Yan Zhang, Yunzheng Zhou
  # CMPT 498 Final Project
  # DashSkillStats.cs
*-----------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Range - stat representing range of the skill
 * RageGeneration - stat represeingting the rage generation of the skill (similar to mana regen)
 * 
 * Creator: Myles Hagen, Kevin Ho
 */

public class DashSkillStats : SkillStats
{
    public Stat Range;
    public Stat RageGeneration;
    
	// subscribe to ongemchanged event in equipmentmanager
    void Start()
    {
        EquipmentManager.instance.onGemChanged_SkillDash += OnGemChanged;
    }

	/*
	 * Function: OnGemChanged
	 * Parameters: newItem, oldItem
	 * Description: when a new gem is equiped, the stats for the given skill are updated
	 * and the stats for the old gem are removed if one was equiped.
	 */
    public void OnGemChanged(Equipment newItem, Equipment oldItem)
    {
        if (newItem != null)
        {

            Damage.AddModifier(newItem.damageModifier);
            //armor.AddModifier(newItem.armorModifier);
            RageCost.AddModifier(newItem.rageModifier);
            //AttackSpeed.AddModifier(newItem.attackspeedModifier);
            Cooldown.AddModifier(newItem.cooldownModifier);
            Lifesteal.AddModifier(newItem.lifestealModifier);
            Range.AddModifier(newItem.rangeModifier);
            RageGeneration.AddModifier(newItem.rageGenerationModifer);

        }

        if (oldItem != null)
        {
            //armor.RemoveModifier(oldItem.armorModifier);
            Damage.RemoveModifier(oldItem.damageModifier);
            RageCost.RemoveModifier(oldItem.rageModifier);
            //AttackSpeed.RemoveModifier(oldItem.attackspeedModifier);
            Cooldown.RemoveModifier(oldItem.cooldownModifier);
            Lifesteal.RemoveModifier(oldItem.lifestealModifier);
            Range.RemoveModifier(oldItem.rangeModifier);
            RageGeneration.RemoveModifier(oldItem.rageModifier);

            
        }
    }

	/*
	 * Funtion: ClearModifiers (overriden from SkillStats)
	 * Description: Clear the stat modifiers for the skill
	 */
    public override void ClearModifiers()
    {
        base.ClearModifiers();
        Range.modifiers.Clear();
        RageGeneration.modifiers.Clear();

    }
}
