using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public int highScore;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    // Start is called before the first frame update
    void Awake()
    {
        if(gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        Load();
    }

    private void OnDisable()
    {
        Save();
    }
    public void Save()
    {
        //Create a binary formatter which can read binary files
        BinaryFormatter formatter = new BinaryFormatter();
        //Create a route from the program to the file
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat",FileMode.Create);
        //Create a copy save data
        SaveData data = new SaveData();
        data = saveData;
        //Actually save the data in the file
        formatter.Serialize(file, data);
        file.Close();

    }
    public void Load()
    { 
         if(File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            //Create binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
