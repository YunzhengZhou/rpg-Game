/*-------------------------------------------------------------------------*
  # INTR Group 2
  # Student's Name: Kevin Ho, Myles Hangen, Shane Weerasuriya, 
  #					Tianqi Xiao, Yan Zhang, Yunzheng Zhou
  # CMPT 498 Final Project
  # PlayerController2.cs
  # Controller and FSM of player character
*-----------------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/*
Player controller. Contains controls that call the motor functions for the player
such as moving, interacting, and attacking. Also contains player focus to move to
Creator: Kevin Ho, Myles Hangen, Shane Weerasuriya
*/

/*
 * onFocusChangedCallback - Previous focus
 * focus - Interacable to focus to
 * movementMask - Moveable ground
 * interactionMask - Interacable objects
 * interactableTag - Tag of interactable object 
 * WeaponState - Weapon currently equiped (0 - Fist, 1 - Sword)
 * motor - Reference to playermotor
 * cam - Reference to cam
 * TPC_State - Enum of the player states
 * state - Player states
 * agent - Player nav mesh agent
 * defaultAttackWait - Default skill attack delay
 * projectileAttackWait - Projectile skill attack delay
 * AOEAttackWait - AOE skill attack delay
 * dashAttackWait - Dash skill attack delay
 * TERRAIN_MASK - Terrain mask for raycast
 * punchAttack - Deafult punch attack SFX clip
 * weaponEquip - Weapon switch SFX clip
 * nextInteractable - Time of next interactable SFX call
 * interactableRate - Rate of interactable SFX call
 * equipStatus - Equipment equiped check
 */


[RequireComponent(typeof(PlayerMotor2))]
public class PlayerController2: MonoBehaviour {

	public delegate void OnFocusChanged(Interactable newFocus);
	public OnFocusChanged onFocusChangedCallback;

	public Interactable focus;	// Our current focus: Item, Enemy etc.

	public LayerMask movementMask;		// The ground
	public LayerMask interactionMask;	// Everything we can interact with
	
	private string interactableTag = null;

	[HideInInspector]
	public int WeaponState = 0;
    public GameObject[] spellSlots;

	PlayerMotor2 motor;		// Reference to our motor
	Camera cam;				// Reference to our camera
	
	//FSM Variables
	private enum TPC_State {IDLE, MOVING, INTERACTING, ATTACKING, WAITING};
	private TPC_State state;
	
	public NavMeshAgent agent;
	
	public float defaultAttackWait = 0.5f, projectileAttackWait = 0.5f, AOEAttackWait = 4.0f;
	public float dashAttackWait = 0.5f, buffAttackWait = 0.5f, debuffAttackWait = 0.5f;

	private const int TERRAIN_MASK = 1 << 10;
	
	//Audio
	public AudioClip punchAttack;
	//Change later when it's no longer 1 and 2 buttons
	public AudioClip weaponEquip;
	//public AudioClip weaponDequip;
	
	private float nextInteractable;
	private float interactableRate = 1.0f;
    public UIWindow inventory, gemslots;
	private bool equipStatus = false;

    //Initialize components
    void Start()
    {
        motor = GetComponent<PlayerMotor2>();
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }
	//Update once per frame
	void Update (){
        //Debug.Log(agent.speed + " " +  agent.velocity);
		//FSM Code checks
		bool isMoving = Input.GetButton("Moving");
		bool isInteractable = Input.GetButton("Interactable");
		bool isDefaultAttack = Input.GetButton("Default Attack"); //Rename this attack later to dash attack
		bool isProjectileAttack = Input.GetButton("Projectile Attack");
		bool isAOEAttack = Input.GetButton("AOE Attack");
		bool isBuffAttack = Input.GetButton("Buff Attack");
		bool isDebuffAttack = Input.GetButton("Debuff Attack");

		if (EventSystem.current.IsPointerOverGameObject ())
			return;

        /*
		Hot key weapon switching. Remove later
		*/
		/*
        switch (Input.inputString) {
		case "0":
			WeaponState = 0;//unarmed
			equipStatus = false;
			
			break;
		case "1":
			WeaponState = 1;//longsword
			if (!equipStatus) {
				playSFX(weaponEquip);
				equipStatus = true;
			}
			break;
		}
		*/
		
		
		/*
		State machine of the player. Will handle idle, moving, interacting and attacking states of the player
		Waiting is not empty for future use
		Creator: Kevin Ho
		*/
		switch(state) {
		case TPC_State.IDLE:
			//Debug.Log("State: IDLE");
			//Attack was found
			if (isDefaultAttack | isProjectileAttack | isAOEAttack | isBuffAttack | isDebuffAttack) {
				state = TPC_State.ATTACKING;
				return;
			}
			//Destination mouse click was found
			if (isMoving){
				state = TPC_State.MOVING;
				return;
			}
			//Interactable mouse click was found
			if (isInteractable){
				state = TPC_State.INTERACTING;
				return;
			}
			break;
		case TPC_State.MOVING:
			//New location clicked
			if ((!isDefaultAttack | !isProjectileAttack | !isAOEAttack | !isBuffAttack | !isDebuffAttack) & isMoving) {
				//Debug.Log("State: NEW MOVING");
				MousePoint();
				if (agent.remainingDistance < 0.1f){
					state = TPC_State.IDLE;		
					return;
				}
			}
			//Called attack. 
			if (isDefaultAttack | isProjectileAttack | isAOEAttack | isBuffAttack | isDebuffAttack){
				motor.MoveToPoint(this.transform.position);
				state = TPC_State.IDLE;
			}
			//Interactable mouse click was found
			if (isInteractable){
				state = TPC_State.INTERACTING;
				return;
			}
			//Still currently moving
			else {
				//Debug.Log("State: CURRENTLY MOVING");
				if (agent.remainingDistance < 0.1f) {
					state = TPC_State.IDLE;		
					return;
				}
			}
			break;
		case TPC_State.INTERACTING:
			//Debug.Log("State: INTERACTING");
			//New location clicked. Override interactable
			if ((!isDefaultAttack | !isProjectileAttack | !isAOEAttack | !isBuffAttack | !isDebuffAttack) & isMoving){
				state = TPC_State.MOVING;		
				return;
			}
			//New interactable clicked
			if ((!isDefaultAttack | !isProjectileAttack | !isAOEAttack | !isBuffAttack | !isDebuffAttack) & isInteractable){
				//Debug.Log("State: NEW INTERACTABLE");
				MousePoint();
				if (agent.remainingDistance < 1.5f){ //Change value later when interactables range is less
					if (interactableTag == "Item" && Time.time > nextInteractable) {
						nextInteractable = Time.time + interactableRate;
						interactableTag = null;
					}
					else if (WeaponState == 0 && ((interactableTag == "Enemy") || (interactableTag == "EnemyArcher")) && Time.time > nextInteractable) {
						nextInteractable = Time.time + interactableRate;
						playSFX(punchAttack);
						interactableTag = null;
					}
					state = TPC_State.IDLE;		
					return;
				}
			}
			//Still currently moving  to interactable
			else {
				//Debug.Log("State: CURRENT INTERACTABLE");
				//Debug.Log(agent.remainingDistance.ToString("F4"));
				if (agent.remainingDistance < 1.5f){
					//Play pick up soiund
					if (interactableTag == "Item" && Time.time > nextInteractable) {
						nextInteractable = Time.time + interactableRate;
						interactableTag = null;
					}
					else if (WeaponState == 0 && ((interactableTag == "Enemy") || (interactableTag == "EnemyArcher")) && Time.time > nextInteractable) {
						nextInteractable = Time.time + interactableRate;
						playSFX(punchAttack);
						interactableTag = null;
					}
					state = TPC_State.IDLE;		
					return;
				}
			}	
			break;
		case TPC_State.ATTACKING:
			//Player is using default attack
			//Debug.Log("State: ATTACKING");
			if (isDefaultAttack){
				RaycastHit hit = getMousePosition();
				Quaternion targetRotation = rotationAngle(hit);
				motor.rotatePlayer(targetRotation);
				motor.fireDashAttack();
				StartCoroutine(AttackWait(dashAttackWait));
                if (spellSlots != null)
                    spellSlots[2].GetComponent<UISlotCooldown>().StartCooldown(3, 5f);
                //motor.fireDefaultAttack();
                //StartCoroutine(AttackWait(defaultAttackWait));
                //state = TPC_State.IDLE;
                return;
			}
			//Player is using projectile attack
			if (isProjectileAttack){
				RaycastHit hit = getMousePosition();
				Quaternion targetRotation = rotationAngle(hit);
				motor.rotatePlayer(targetRotation);
				motor.fireProjectileAttack();
				StartCoroutine(AttackWait(projectileAttackWait));
                if (spellSlots != null)
                    spellSlots[1].GetComponent<UISlotCooldown>().StartCooldown(2, 1.5f);
				//state = TPC_State.IDLE;
				return;
			}
			//Player is using AOE attack
			if (isAOEAttack){
				RaycastHit hit = getMousePosition();
				Quaternion targetRotation = rotationAngle(hit);
				motor.rotatePlayer(targetRotation);
				motor.fireAOEAttack();
				if (spellSlots != null)
					spellSlots[0].GetComponent<UISlotCooldown>().StartCooldown(1, 4f);
				//StartCoroutine(AttackWait(AOEAttackWait));	//Add back to lock player down
				//state = TPC_State.IDLE;
                return;
			}
			//Player is using buff attack
			if (isBuffAttack){
				RaycastHit hit = getMousePosition();
				Quaternion targetRotation = rotationAngle(hit);
				motor.rotatePlayer(targetRotation);
				motor.fireBuffAttack();
				StartCoroutine(AttackWait(buffAttackWait));
                return;
			}
			if (isDebuffAttack){
				RaycastHit hit = getMousePosition();
				Quaternion targetRotation = rotationAngle(hit);
				motor.rotatePlayer(targetRotation);
				motor.fireDebuffAOEAttack(hit);
				StartCoroutine(AttackWait(debuffAttackWait));
                return;
			}
			state = TPC_State.IDLE;
			break;
		//Waiting state does nothing	
		case TPC_State.WAITING:
			//Debug.Log("State: WAITING");
			break;
		}
	}
	

    public void setDash()
    {
        //RaycastHit hit = getMousePosition();
        //Quaternion targetRotation = rotationAngle(hit);
        //motor.rotatePlayer(targetRotation);
        motor.fireDashAttack();
        StartCoroutine(AttackWait(dashAttackWait));
    }

    public void setAOE()
    {
        RaycastHit hit = getMousePosition();
        Quaternion targetRotation = rotationAngle(hit);
        motor.rotatePlayer(targetRotation);
        motor.fireAOEAttack();
    }

    public void setProjectile()
    {
        //RaycastHit hit = getMousePosition();
        //Quaternion targetRotation = rotationAngle(hit);
        //motor.rotatePlayer(targetRotation);
        motor.fireProjectileAttack();
        StartCoroutine(AttackWait(projectileAttackWait));
    }
	/*
	Get raycast location of mouse
	Creator: Kevin Ho, Shane Weerasuriya
	Returns: RaycastHit of mouse position	
	*/
	RaycastHit getMousePosition(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100f, movementMask)) {
			//Debug.Log("Hit " + mask.LayerToName(1));
			//Debug.Log("Hit " + TERRAIN_MASK);
		}
		return hit;
	}

	/*
	Get rotation angle of a hit point and player position
	Parameters: hit - Raycast of mouse position
	Returns: Quaterion of rotation position	
	Creator: Kevin Ho, Shane Weerasuriya
	*/
	Quaternion rotationAngle(RaycastHit hit){
		//Debug.Log("Player" +this.transform.position);
		//Debug.Log("Mouse" + hit.point);
		Quaternion targetRotation = Quaternion.LookRotation(hit.point - this.transform.position);
		targetRotation.x = 0;
		targetRotation.z = 0;
		//Debug.Log("Angle " + targetRotation);
		return targetRotation;
	}

	/*
	 * Function: SetFocus
	 * Parameters: Interactable newFocus (interactable object to focus on)
	 * Description: call onfocusedcallback delegate calling playermotor function to onfocuschanged
	 * defocus old focus, set focus to new focus, focus transform of new focus
	 * Creator: Myles Hagen
	 */
	void SetFocus (Interactable newFocus)
	{
		if (onFocusChangedCallback != null)
			onFocusChangedCallback.Invoke(newFocus);

		// If our focus has changed
		if (focus != newFocus && focus != null)
		{
			// Let our previous focus know that it's no longer being focused
			focus.OnDefocused();
		}

		// Set our focus to what we hit
		// If it's not an interactable, simply set it to null
		focus = newFocus;

		if (focus != null)
		{
			// Let our focus know that it's being focused
			focus.OnFocused(transform);
		}
	}
	
	/*
	Add comments here for whoever created it!!!
	*/
	//Get mouse point and move or interactable depending on click
	//Creator: Myles Hangen
	private void MousePoint(){
		// Shoot out a ray
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		//Movement. Left Click
		if (Input.GetMouseButton(0)){
            if (!EventSystem.current.IsPointerOverGameObject())
            {
				if (Physics.Raycast(ray, out hit, 100f, movementMask)) {
					if (inventory != null || gemslots != null) {											
						if (inventory.IsOpen || gemslots.IsOpen) {
							return;
						}
					}
                    motor.MoveToPoint(hit.point);
                    SetFocus(null);
                }
            }
		}
		//Interactable. Right Click
		else if (Input.GetMouseButton(1)){
			if (Physics.Raycast(ray, out hit, 100f, interactionMask)){
				SetFocus(hit.collider.GetComponent<Interactable>());
				interactableTag = hit.transform.tag;
			}
		}
	}
	
	/*
	Coroutine to wait after an attack
	Parameters: delay - float of time delay of attack
	Creator: Kevin Ho, Myles Hagen
	*/
	IEnumerator AttackWait(float delay) {
		state = TPC_State.WAITING;
		//Debug.Log("WAITING");
		yield return new WaitForSeconds(delay);
		//Debug.Log("DONE");
		state = TPC_State.IDLE;
	}
	
	/*
	Function: Plays SFX for player actions
	Parameters: clip - Audioclip to play
	*/
	public void playSFX(AudioClip clip) {
		//Play audio
		if (clip != null) {
			AudioSource.PlayClipAtPoint(clip, Player.instance.transform.position, 1.0f);
		}
		return;
	}
	
	/*
	Returns weapon state of the player to play appropriate hitbox/animation
	Creator: Kevin Ho, Myles Hagen
	*/
	public int GetWeaponState() {
		return WeaponState;
	}
}
