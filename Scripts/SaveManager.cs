using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData currentSave;

    private byte[] GetDynamicKey()
    {
        string part1 = "JpW1@3";
        string part2 = "aCk1410!";
        string part3 = "Sup3rT4jn3";
        string part4 = "H4sl0_Xx";

        string fullKey = part3 + part2 + part4 + part1;

        return Encoding.UTF8.GetBytes(fullKey);
    }

    private byte[] GetDynamicIV()
    {
        int[] shiftedAscii = new int[] {
            77, 111, 113, 55, 21, 12, 102, 13, 99, 8, 1, 11, 102, 44, 49, 42
        };

        string result = "";
        for (int i = 0; i < shiftedAscii.Length; i++)
        {
            result += (char)shiftedAscii[i];
        }

        return Encoding.UTF8.GetBytes(result);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }

#if UNITY_EDITOR
        if (currentSave == null && !File.Exists(Application.persistentDataPath + "/savegame.sav"))
        {
            Debug.LogWarning("DEV MOCK: Tworzê sztuczny zapis, ¿eby móc testowaæ od œrodka!");
            currentSave = new SaveData();
            currentSave.savedPlayerMaxHP = 15;
            currentSave.savedPlayerHP = 15;
            currentSave.savedPlayerCoins = 500;
            currentSave.savedTokensAttack = 5;
            currentSave.savedTokensDefense = 5;
        }
#endif
    }

    public void SaveToFile(SaveData dataToSave)
    {
        string json = JsonUtility.ToJson(dataToSave);

        string encryptedJson = Encrypt(json);

        string savePath = Application.persistentDataPath + "/savegame.sav";
        File.WriteAllText(savePath, encryptedJson);
        Debug.Log("Gra zaszyfrowana i zapisana!");
    }

    public bool LoadFromFile()
    {
        string savePath = Application.persistentDataPath + "/savegame.sav";
        if (File.Exists(savePath))
        {
            string encryptedJson = File.ReadAllText(savePath);

            string decryptedJson = Decrypt(encryptedJson);

            currentSave = JsonUtility.FromJson<SaveData>(decryptedJson);
            Debug.Log("Zapis odszyfrowany i za³adowany!");
            return true;
        }
        return false;
    }
    public void DeleteSaveData()
    {
        currentSave = null;
        string savePath = Application.persistentDataPath + "/savegame.sav";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Plik zapisu zosta³ fizycznie usuniêty z dysku.");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono pliku zapisu do usuniêcia w œcie¿ce: " + savePath);
        }
    }

    private string Encrypt(string plainText)
    {
        byte[] bText = Encoding.UTF8.GetBytes(plainText);
        Aes aes = Aes.Create();
        aes.Key = GetDynamicKey();
        aes.IV = GetDynamicIV();

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(bText, 0, bText.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    private string Decrypt(string cipherText)
    {
        byte[] bText = Convert.FromBase64String(cipherText);
        Aes aes = Aes.Create();
        aes.Key = GetDynamicKey();
        aes.IV = GetDynamicIV();

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using (MemoryStream ms = new MemoryStream(bText))
        {
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}