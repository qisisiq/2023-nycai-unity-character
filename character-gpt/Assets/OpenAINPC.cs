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
    [SerializeField] public string myName = "Obenayeye";
    [SerializeField] private List<string> Actions; // a list of actions the character can take 

    [SerializeField] private Collider attackCollider;
    
    private RPGCharacterController rpgCharacterController;
    private RPGCharacterNavigationController rpgNavigationController;
    


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
        
        GameStateData.AddToGameState($"{myName} threw a punch.");
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
        GameStateData.AddToGameState($"{myName} has waved to {transform.gameObject.name}.");

        yield return new WaitForSeconds(2.5f);
        
        rpgCharacterController.target = null;
    }
    
    // Play the walk to animation
    void WalkTo(Transform transform)
    {
        rpgCharacterController.StartAction(HandlerTypes.Navigation, transform.position);
        
        GameStateData.AddToGameState($"{myName} has waved to {transform.gameObject.name}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attacker"))
        {
            Debug.Log("was attacked");
            rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext());
            
            GameStateData.AddToGameState($"{myName} was hit by {other.gameObject.name}!");
        }
    }
}
