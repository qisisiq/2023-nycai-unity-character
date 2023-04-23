using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MemoryScript : MonoBehaviour
{
    [SerializeField]
    MemoryDataBase memoryDB;

    [SerializeField]
    TextMeshProUGUI sTM;
    [SerializeField]
    TextMeshProUGUI lTM;
    [SerializeField]
    TextMeshProUGUI cTM;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string stringDisplay = "";
        foreach(MemoryDataBase.Memory m in memoryDB.shortermMemory)
		{
            stringDisplay = m.action + "\n" + stringDisplay;
		}
        sTM.text = stringDisplay;

        stringDisplay = "";
        foreach (MemoryDataBase.Memory m in memoryDB.memoryList)
        {
            stringDisplay = m.timeStamp + " " + m.action + "\n" + stringDisplay;
        }
        lTM.text = stringDisplay;

        stringDisplay = "";
        foreach (MemoryDataBase.Memory m in memoryDB.compressedMemory)
        {
            stringDisplay = m.action + "\n" + stringDisplay;
        }
        cTM.text = stringDisplay;
    }
}
