using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavingSystemJson : MonoBehaviour
{
    string path;

    private void Awake()
    {
        path = Application.persistentDataPath + " /SavingSystem.json ";
    }
    //hier sla je op:
    public void SaveData()
    {
        //GrabManagers
        SaveData data = new SaveData();
        data.playerWeapons = MainPlayer.Instance.AddWeaponToSave();
        data.finalScore = ScoreSystem.Instance._totalScore;
        data.enemyUtils = EnemyUtils.Instance;
        data.coinSystem = CoinSystem.Instance;
        //saveManagers
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }
    //hier laad je het
    public void LoadData()//load the data for settings
    {
        //GrabManagers
        if (File.Exists(path))
        {
            //loadManagers
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            MainPlayer.Instance.AddWeaponFromSave(data.playerWeapons);
            CoinSystem.Instance= data.coinSystem;
            ScoreSystem.Instance._totalScore = data.finalScore;
            EnemyUtils.Instance= data.enemyUtils;
        }
        else
        {
            Debug.LogWarning("No file found");
        }
    }
    public void ClearData()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            Debug.LogWarning("No data to clear");
        }
    }
}
