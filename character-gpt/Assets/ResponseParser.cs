using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class ResponseParser : MonoBehaviour
{
    [SerializeField] private OpenAINPC npc;
    private GameObject[] allGameObjects;
    private void Start()
    {
        
    }
    private void OnEnable()
    {
        OpenAIController_Eddy.OnChatResponseRecieved += HandleChatResponse;
    }

    private void OnDisable()
    {
        OpenAIController_Eddy.OnChatResponseRecieved += HandleChatResponse;
    }

    public void HandleChatResponse(string response)
    {
        var names = ExtractMethodNameAndGameObject(response);
        if (names[0] != null)
        {
            if (names[1] != null)
            {
                CallMethodByName(names[0], names[1]);

            }
            else
            {
                CallMethodByName(names[0]);
            }
        }
    }

    public void CallMethodByName(string methodName, string gameObjectName = null)
    {
        Type type = GetType();
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        MethodInfo methodInfo = null;

        if (!string.IsNullOrEmpty(gameObjectName))
        {
            methodInfo = type.GetMethod(methodName, bindingFlags, null, new Type[] { typeof(string) }, null);
            if (methodInfo != null)
            {
                methodInfo.Invoke(this, new object[] { gameObjectName });
                return;
            }
        }

        methodInfo = type.GetMethod(methodName, bindingFlags);

        if (methodInfo != null)
        {
            methodInfo.Invoke(this, null);
        }
        else
        {
            Debug.LogError($"Method '{methodName}' not found.");
        }
    }
    /// <summary>
    /// Call this method whenever OpenAI gives us back a response 
    /// </summary>
    /// <param name="input"></param>
    public string[] ExtractMethodNameAndGameObject(string input)
    {
        string methodName = null;
        string gameObjectName = null;

        string actionPattern = @"@Action\s+(\w+)(?:\s*\[(\w+)\])?";
        Match actionMatch = Regex.Match(input, actionPattern);

        if (actionMatch.Success)
        {
            methodName = actionMatch.Groups[1].Value;
            gameObjectName = actionMatch.Groups[2].Value;
        }

        string[] output = new string[2];
        output[0] = methodName;
        output[1] = gameObjectName;
        
        Debug.Log($"Method name: {methodName ?? "null"}");
        Debug.Log($"GameObject name: {gameObjectName ?? "null"}");

        return output;
    }
    
    GameObject FindGameObjectByName(string name)
    {
        if (allGameObjects == null)
        {
            allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        }
        foreach (GameObject go in allGameObjects)
        {
            if (go.name == name)
            {
                return go;
            }
        }
        return null;
    }

    
    private void Attack(string gameObjectName)
    {
        var go = FindGameObjectByName(gameObjectName);
        npc.Attack(go);
    }
    
    private void Attack()
    {
        npc.Attack();
    }

    
    private void Wave()
    {
        npc.Wave();

    }

    private void Wave(string gameObjectName)
    {
        var go = FindGameObjectByName(gameObjectName);
        npc.Wave(go);

    }
    
    private void MoveTo (string gameObjectName)
    {
        var go = FindGameObjectByName(gameObjectName);
        npc.MoveTo(go);
    }



    // Add more methods as needed
}