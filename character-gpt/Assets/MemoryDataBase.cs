using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

[CreateAssetMenu(fileName = "MemoryDataBase", menuName = "ScriptableObject/MemoryDataBase", order = 1)]
public class MemoryDataBase : ScriptableObject
{
    private OpenAIAPI api;
    [TextArea(4, 100)]
    public string initialPrompt_Memory;
    [System.Serializable]
    public class Memory
    {
        public string timeStamp;
        public string action;
    }
    //public Dictionary<DateTime, Memory> memoryStream = new Dictionary<DateTime, Memory>();
    public List<Memory> memoryList = new List<Memory>();

    public List<Memory> shortermMemory = new List<Memory>();

    public List<Memory> compressedMemory = new List<Memory>();
    
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
        memory.timeStamp = DateTime.Now.ToString();
        //memoryStream.Add(DateTime.Now, memory);

        
        memoryList.Add(memory);

        shortermMemory.Add(memory);
        if(shortermMemory.Count > 3)
		{
            CleanUpMemory();

        }

  //      foreach (var kvp in memoryStream)
		//{

		//	Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
		//}
	}


    public void Init()
	{
        APIAuthentication apiKey = new APIAuthentication("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");

        // This line gets your API key (and could be slightly different on Mac/Linux)
        api = new OpenAIAPI(apiKey);
    }

    private async void CleanUpMemory()
    {

        // Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;

        string memoryData = "";
        foreach(Memory smemory in shortermMemory)
		{
            memoryData += "Timestamp: " + smemory.timeStamp.ToString() + " | Content: " + smemory.action + "\n";

        }
        shortermMemory = new List<Memory>();
        userMessage.Content = memoryData;


        // Send the entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 50,
            Messages = new List<ChatMessage>() { userMessage }
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log("Summary the memory: " + responseMessage.Content);

        Memory compressedM = new Memory();
        compressedM.timeStamp = DateTime.Now.ToString();
        compressedM.action = responseMessage.Content;
        compressedMemory.Add(compressedM);

    }

    public void Reset()
	{
        //memoryStream = new Dictionary<DateTime, Memory>();
        memoryList = new List<Memory>();
        shortermMemory = new List<Memory>();
        compressedMemory = new List<Memory>();
    }
}
