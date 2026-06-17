using UnityEngine;
using UnityEditor;
using System.IO;

public class DevTools
{
    // Dodaje nową zakładkę na samej górze Unity obok File, Edit, Assets...
    [MenuItem("Narzędzia Testera/Skasuj Zapis Gry")]
    public static void DeleteSaveData()
    {
        string savePath = Application.persistentDataPath + "/savegame.sav";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Pomyślnie usunięto plik zapisu!");
        }
        else
        {
            Debug.LogWarning("Brak pliku zapisu do usunięcia.");
        }
    }
}