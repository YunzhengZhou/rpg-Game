/*-------------------------------------------------------------------------*
  # INTR Group 2
  # Student's Name: Kevin Ho, Myles Hagen, Shane Weerasuriya, 
  #					Tianqi Xiao, Yan Zhang, Yunzheng Zhou
  # CMPT 498 Final Project
  # Enemy.cs
  # Enemy base stats
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
 * cube - refernece to enemy health bar
 * td - 
 * isDead - is enemy dead
 * enemyDeathClips - list of enemy death SFXs
*/

/*
Creator: Myles Hagen, Kevin Ho
*/

public class Enemy : Interactable {

	PlayerManager playerManager;
	EnemyStats myStats;
	CapsuleCollider col;
	NavMeshAgent agent;
	EnemyController con;
	private DropTable table;
    //Rigidbody rb;
    GameObject cube;
    bool td = true;
    bool isDead = false;
	
	//Audio
	//AudioSource audioSource;
	public AudioClip[] enemyDeathClips;
   
    //Initialize components
    void Start ()
	{
		con = GetComponent<EnemyController> ();
		col = GetComponent<CapsuleCollider> ();
		agent = GetComponent<NavMeshAgent> ();
		table = GetComponent<DropTable>();
        myStats = GetComponent<EnemyStats>();
		myStats.OnHealthReachedZero += Die;
        
        
    }
	
	/*
	Player Interaction
	Creator: Myles Hangen
	*/
	public override void Interact ()
	{
		base.Interact ();
        //Debug.Log("INTERACTING");
        CharacterCombat playerCombat = Player.instance.playerCombatManager;
        playerCombat.Attack(myStats);
	}

	/*
	Trigger death of enemy. Drops loot and destorys game object
	Creator: Myles Hangen
	*/
	public virtual void Die()
    {
		//Debug.Log("die in enemy");
		//for village scene initial event
		int scenenum = SceneManager.GetActiveScene().buildIndex;
		if (scenenum == 0 && AllEventList.returnStatus("villageInitial", 0) == false) {
			Debug.Log("Increasing count.");
			ManageScene.instance.increaseCount ();
		}
        //
        if (aiGroundSwarm.instance != null)
            aiGroundSwarm.instance.deathCounter++;
        isDead = true;
        if (con != null)
            con.enabled = false;
        agent.enabled = false;
        col.enabled = false;
        if (table != null)
            table.DropItem();
		
		//Play a randomization of clips in list if muiltple exist
		if (enemyDeathClips.Length > 0)	{
			int x = (int)Random.Range(0.0f, enemyDeathClips.Length);
			AudioSource.PlayClipAtPoint(enemyDeathClips[x], transform.position, 1.0f);
		}
		
        TimeOfDeath();		
        Destroy(gameObject, 10f);
    }

    

    public void TimeOfDeath()
    {
        int sceneID = SceneManager.GetActiveScene().buildIndex;
        EnemyDeathTime et = new EnemyDeathTime();
        et.deathTime = Time.time;
        et.name = name;
        GlobalControl.Instance.SceneDeathLists[sceneID].Add(et);
        //Debug.Log("list count after death " + GlobalControl.Instance.SceneDeathLists[sceneID].Count);
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

    public bool SpawnOnDeath()
    {
        return true;
    }
}
