using UnityEngine;
using System;
using System.IO;

public class CSVHandler : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugLogs = true;
    public void SaveChoiceToCSV(string choice)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "modal_choices.csv");

        // Linha com timestamp + escolha
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{choice}";

        // Se o arquivo não existir ainda, escreve o cabeçalho
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,Choice\n");
        }

        // Acrescenta a nova linha
        File.AppendAllText(filePath, line + "\n");

        if (showDebugLogs)
        {
            Debug.Log($"[VRModalLogger] Escolha registrada: {line} (Arquivo: {filePath})");
        }
    }
}
