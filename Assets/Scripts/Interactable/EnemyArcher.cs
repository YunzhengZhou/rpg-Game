/*-------------------------------------------------------------------------*
  # INTR Group 2
  # Student's Name: Kevin Ho, Myles Hagen, Shane Weerasuriya, 
  #					Tianqi Xiao, Yan Zhang, Yunzheng Zhou
  # CMPT 498 Final Project
  # EnemyArcher.cs
  # Enemy archer base stats
*-----------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/*
 * playerManager - Instance of enemy
 * myStats - Enemy stats
 * col - Collider of enemy
 * agent - NavMeshAgent of enemey
 * con - Enemy ControllerColliderHit
 * table - Droppable items
 * isDead - is enemy archer dead
 * enemyDeathClips - list of enemy death SFXs
*/

/*
Creator: Myles Hagen, ,Tianqi Xiao, Shane Weerasuriya
*/

[RequireComponent(typeof(CharacterStats))]
public class EnemyArcher : Interactable {

	PlayerManager playerManager;
    EnemyStats myStats;
	CapsuleCollider col;
	NavMeshAgent agent;
	EnemyArcherController con;
	private DropTable table;
	bool isDead = false;
	
	//AudioSource audioSource;
	public AudioClip[] enemyDeathClips;
	
	//Initialize components
	void Start ()
	{
		con = GetComponent<EnemyArcherController> ();
		col = GetComponent<CapsuleCollider> ();
		agent = GetComponent<NavMeshAgent> ();
		table = GetComponent<DropTable>();
		myStats = GetComponent<EnemyStats>();
		myStats.OnHealthReachedZero += Die;
	}
	
	/*
	Player Interaction
	Creator: Myles Hagen,Tianqi Xiao, Shane Weerasuriya
	*/
	public override void Interact ()
	{

		base.Interact ();
        //CharacterCombat playerCombat = playerManager.player.GetComponent<CharacterCombat> ();
        CharacterCombat playerCombat = Player.instance.playerCombatManager;
        playerCombat.Attack(myStats);
		
		
	}

	/*
	Trigger death of enemy. Drops loot and destorys game object
	Creator: Myles Hagen,Tianqi Xiao, Shane Weerasuriya
	*/
	public void Die()
    {
		isDead = true;
        con.enabled = false;
        agent.enabled = false;
        col.enabled = false;
        table.DropItem();
		
		//Play a randomization of clips in list if muiltple exist
		if (enemyDeathClips.Length > 0)	{
			int x = (int)Random.Range(0.0f, enemyDeathClips.Length);
			AudioSource.PlayClipAtPoint(enemyDeathClips[x], transform.position, 1.0f);
		}
		
		TimeOfDeath();	
        Destroy(gameObject, 10f);
    }

	/*
	 * Function: TimeOfDeath
	 * Description: track time of enemy death using EnemyDeathTime object
	 * add the EnemyDeathTime object to the global dictionary for that scene
	 * 
	 * Creator: Myles Hagen,Tianqi Xiao, Shane Weerasuriya
	 */
    public void TimeOfDeath()
    {
        int sceneID = SceneManager.GetActiveScene().buildIndex;
        EnemyDeathTime et = new EnemyDeathTime();
        et.deathTime = Time.time;
        et.name = name;
        GlobalControl.Instance.SceneDeathLists[sceneID].Add(et);

    }
	
	/*
	Returns death status of enemy
	Return: Returns enemy death status
	Creator: Kevin Ho
	*/
	public bool getIsDead() {
		return isDead;
	}

    /*
	Returns the CharacterStats of the enemy. Called in HitDetection
	Return: Returns enemy stats
	Creator: Kevin Ho
	*/
    public EnemyStats getMyStats(){
		return myStats;
	}
}
