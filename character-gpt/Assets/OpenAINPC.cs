using System;
using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;
using UnityEditor;
using UnityEngine;

public class OpenAINPC : MonoBehaviour
{
    [SerializeField] MemoryDataBase memoryDB;
    [SerializeField] public string myName = "Obenayeye";
    [SerializeField] private List<string> Actions; // a list of actions the character can take 

    [SerializeField] private Collider attackCollider;
    
    private RPGCharacterController rpgCharacterController;
    private RPGCharacterNavigationController rpgNavigationController;

    private int health = 5;

    [SerializeField] private GameObject deadBody;

    bool death = false;
    bool hitOnce = false;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        rpgCharacterController = GetComponent<RPGCharacterController>();
        rpgNavigationController = GetComponent<RPGCharacterNavigationController>();

        health = 5;

    }

    public void Attack()
    {
        StartCoroutine(PlayAttack());
    }
    
    public void Attack(GameObject gameObject)
    {
        StartCoroutine(PlayAttack(gameObject.transform));
    }

    // Add an attack collider to the character and then call the attack command 
    IEnumerator PlayAttack(Transform transform = null)
    {
        if (transform != null)
        {
            rpgCharacterController.StartAction(HandlerTypes.Navigation, transform.position);
        }
        
        while (Vector3.Distance(this.transform.position, transform.position) > 0.2f)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        attackCollider.enabled = true;
        rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.Right));
        
        yield return new WaitForSeconds(2f);

        attackCollider.enabled = false;

        memoryDB.AddNewMemory($"{myName} threw a punch.");
    }

    public void Wave()
    {
        StartCoroutine(PlayWave());
    }
    
    public void Wave(GameObject gameObject)
    {
        StartCoroutine(PlayWave(gameObject.transform));
    }

    // Play the wave animation 
    IEnumerator PlayWave()
    {
        rpgCharacterController.StartAction(HandlerTypes.EmoteCombat, EmoteType.Boost);
        memoryDB.AddNewMemory($"{myName} has waved to {transform.gameObject.name}.");

        yield return new WaitForSeconds(1f);
    }

    // Play the wave animation 
    IEnumerator PlayWave(Transform transform)
    {
        rpgCharacterController.target = transform;
        rpgCharacterController.StartAction(HandlerTypes.EmoteCombat, EmoteType.Boost);
        memoryDB.AddNewMemory($"{myName} has waved to {transform.gameObject.name}.");

        yield return new WaitForSeconds(2.5f);
        
        rpgCharacterController.target = null;
    }

    public void MoveTo(GameObject gameObject)
    {
        StartCoroutine(WalkTo(gameObject.transform));
    }

    // Play the walk to animation
    public IEnumerator WalkTo(Transform transform)
    {
        rpgCharacterController.StartAction(HandlerTypes.Navigation, transform.position);
        while (Vector3.Distance(this.transform.position, transform.position) > 0.2f)
        {
            yield return new WaitForSeconds(0.5f);
        }

        memoryDB.AddNewMemory($"{myName} has walked to {transform.gameObject.name}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attacker"))
        {
            Debug.Log("was attacked");
            rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext());


            health--;
            if (health <= 0 && !death)
            {
                Invoke("Revive", 3);
                CharacterDied(other.gameObject);
            }
            else if(!hitOnce)
			{
                hitOnce = true;
                Invoke("HitOnce", 3);
                memoryDB.AddNewMemory($"{myName} was pushed by {other.gameObject.name}!");
            }
        }
    }

	private void HitOnce()
	{
        hitOnce = false;
	}

    private void Revive()
	{
        death = false;
    }

	private void CharacterDied(GameObject killer)
    {
        death = true;
        rpgCharacterController.StartAction(HandlerTypes.Knockdown,
            new HitContext((int)KnockdownType.Knockdown1, Vector3.back));
        
        Debug.Log(myName + " has died");
        memoryDB.AddNewMemory($"{myName} was Stunned by {killer.name}... damn");

        //Instantiate(deadBody, this.transform.position, Quaternion.identity);
        //Destroy(this);
    }
}
