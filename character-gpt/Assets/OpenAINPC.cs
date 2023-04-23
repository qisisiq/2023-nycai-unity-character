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

    void Attack()
    {
        StartCoroutine(PlayAttack());
    }
    
    // Add an attack collider to the character and then call the attack command 
    IEnumerator PlayAttack()
    {
        attackCollider.enabled = true;
        rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.Right));
        
        yield return new WaitForSeconds(2f);

        attackCollider.enabled = false;

        memoryDB.AddNewMemory($"{myName} threw a punch.");
    }

    void Wave()
    {
        StartCoroutine(PlayAttack());
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
    
    // Play the walk to animation
    void WalkTo(Transform transform)
    {
        rpgCharacterController.StartAction(HandlerTypes.Navigation, transform.position);

        memoryDB.AddNewMemory($"{myName} has waved to {transform.gameObject.name}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attacker"))
        {
            Debug.Log("was attacked");
            rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext());

            health--;
            if (health <= 0)
            {
            CharacterDied(other.gameObject);
            }

            memoryDB.AddNewMemory($"{myName} was hit by {other.gameObject.name}!");
        }
    }

    private void CharacterDied(GameObject killer)
    {
        rpgCharacterController.StartAction(HandlerTypes.Knockdown,
            new HitContext((int)KnockdownType.Knockdown1, Vector3.back));
        
        Debug.Log(myName + " has died");
        memoryDB.AddNewMemory($"{myName} was KILLED by {killer.name}... damn");
        
    }
}
