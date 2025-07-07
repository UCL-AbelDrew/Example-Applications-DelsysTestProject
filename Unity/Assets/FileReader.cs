using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Reads a text file from the StreamingAssets folder.
/// Example usage: FileReader fileReader = new FileReader();
/// Example: string content = fileReader.ReadFile("/example.txt");
/// </summary>
public class FileReader
{
    public string ReadFile(string fileName)

    {
        string filePath = Application.streamingAssetsPath+fileName;

        try
        {
            string fileContent = System.IO.File.ReadAllText(filePath);
          //  Debug.Log(fileContent); 
            return fileContent;
          
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error reading file: " + e.Message);
        }
            return null;
    }
}
