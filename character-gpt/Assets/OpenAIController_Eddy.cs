using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenAIController_Eddy : MonoBehaviour
{
    [SerializeField] MemoryDataBase memoryDB;
    [TextArea(4,100)]
    public string initialPrompt;
    public TMP_Text textField;
    public TMP_Text textField_NPC;
    public Transform textFieldBG_NPC;
    public TMP_InputField inputField;
    public Button okButton;

    private OpenAIAPI api;
    private List<ChatMessage> messages;



    // Start is called before the first frame update
    void Start()
    {
        APIAuthentication apiKey = new APIAuthentication("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");

        // This line gets your API key (and could be slightly different on Mac/Linux)
        api = new OpenAIAPI(apiKey);
        StartConversation();
        okButton.onClick.AddListener(() => GetResponse());

        memoryDB.Init();
    }

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Return) && okButton.enabled)
		{
            GetResponse();
        }
    }

	private void StartConversation()
    {
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, initialPrompt)
        };

        inputField.text = "";
        string startString = "You have just approached the palace gate where a knight guards the gate.";
        textField.text = startString;
        Debug.Log(startString);
    }

    private async void GetResponse()
    {
        if (inputField.text.Length < 1)
        {
            return;
        }

        // Disable the OK button
        okButton.enabled = false;

        // Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        if (userMessage.Content.Length > 100)
        {
            // Limit messages to 100 characters
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add the message to the list
        messages.Add(userMessage);

        // Update the text field with the user message
        textField.text = string.Format("You: {0}", userMessage.Content);

        // Clear the input field
        inputField.text = "";

        memoryDB.AddNewMemory("Player said:" + userMessage.Content);
        // Send the entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 50,
            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        // Add the response to the list of messages
        messages.Add(responseMessage);

        // Update the text field with the response
        textField.text = string.Format("You: {0}\n\nFriend Soldier: {1}", userMessage.Content, responseMessage.Content);

        memoryDB.AddNewMemory("AI said:" + responseMessage.Content);

        textField_NPC.text = responseMessage.Content;
        var s = textFieldBG_NPC.localScale;
        print(textField_NPC.textBounds.size.y);
        s.y = textField_NPC.textBounds.size.y * 10;
        textFieldBG_NPC.localScale = s;
        // Re-enable the OK button
        okButton.enabled = true;
    }

	private void OnApplicationQuit()
	{
        memoryDB.Reset();
	}
}
