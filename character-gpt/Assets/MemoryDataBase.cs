using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "MemoryDataBase", menuName = "ScriptableObject/MemoryDataBase", order = 1)]
public class MemoryDataBase : ScriptableObject
{    
    [System.Serializable]
    public class Memory
    {
        public string action;
    }
    public Dictionary<DateTime, Memory> memoryStream = new Dictionary<DateTime, Memory>();
    public List<Memory> memoryList = new List<Memory>();
    
    public void AddNewMemory(string data)
    {
        //Dictionary<DateTime, string> dict = new Dictionary<DateTime, string>();
        //dict.Add(DateTime.Now, "Value 1");
        //dict.Add(DateTime.Now.AddDays(-1), "Value 2");
        //dict.Add(DateTime.Now.AddDays(-2), "Value 3");
        //// sort the dictionary by timestamp in descending order
        //var sortedDict = dict.OrderByDescending(x => x.Key);
        //// get the top 5 key-value pairs
        //var top5 = sortedDict.Take(5);
        //// print the top 5 key-value pairs
        //foreach (var kvp in top5)
        //{
        //    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
        //}
        Memory memory = new Memory();
        memory.action = data;
        memoryStream.Add(DateTime.Now, memory);

        
        memoryList.Add(memory);

        foreach (var kvp in memoryStream)
		{

			Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
		}
	}


}
