using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameStateData : ScriptableObject
{    
    public string timeCreated;
    public List<string> gameData = new List<string>();

    private static GameStateData gameStateData;
    
    public static GameStateData Instance
    {
        get
        {
            if (gameStateData == null)
            {
                gameStateData = CreateInstance<GameStateData>();
                gameStateData.timeCreated = System.DateTime.Now.ToString();

                // Save the new instance as an asset in the project (Editor only)
                #if UNITY_EDITOR
                string assetPath = "Assets/GameStateData" + gameStateData.timeCreated + ".asset";
                AssetDatabase.CreateAsset(gameStateData, assetPath);
                AssetDatabase.SaveAssets();
                #endif
            }
            return gameStateData;
        }
    }
    
    public static void AddToGameState(string data)
    {
        Instance.gameData.Add(data);
    }

    private void OnDestroy()
    {
    #if UNITY_EDITOR
        AssetDatabase.SaveAssets();
    #endif
    }


}
