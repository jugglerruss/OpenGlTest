using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "FileReader", menuName = "ScriptableObjects/FileReader", order = 1) ]
public class FileReader : ScriptableObject
{
    [SerializeField] private string _fileName= "/african_head.obj";
    private List<string> _textLines = new List<string>();
    public List<float[]> VerticesList { get; private set; }
    public List<float[]> TextureVerticesList { get; private set; }
    public List<float[]> NormalesList { get; private set; }
    public List<int[]> FacetsList { get; private set; }
    public Action OnFileRead;
    
    private bool IsFileExist(string path)
    {
        return File.Exists(Application.dataPath + path);
    }
    public void Load()
    {
        VerticesList = new List<float[]>();
        TextureVerticesList = new List<float[]>();
        NormalesList = new List<float[]>();
        FacetsList = new List<int[]>();
        var folder = "/Object";
        if (IsFileExist(folder + _fileName))
        {
            StreamReader sr = new StreamReader(Application.dataPath + folder + _fileName);
            while ( !sr.EndOfStream )
                _textLines.Add(sr.ReadLine());
            sr.Close();
            Debug.Log("ReadFile");
            ReadFile();
        }
    }
    private void ReadFile()
    {
        VerticesList.Add(new float[] { 0, 0, 0 });
        TextureVerticesList.Add(new float[] { 0, 0, 0 });
        NormalesList.Add(new float[] { 0, 0, 0 });
        foreach (var line in _textLines)
        {
            if (line.Length == 0)
                continue;
            if (GetData(line,"v ", ' ', VerticesList))
                continue;
            if (GetData(line,"vt  ", ' ', TextureVerticesList))
                continue;
            if (GetData(line,"vn  ", ' ', NormalesList))
                continue;
            GetData(line, "f ", '/', FacetsList);
        }
        OnFileRead?.Invoke();
    }
    bool GetData(string line,string startString, char splitChar, List<float[]> resultList)
    {
        if (!line.StartsWith(startString)) return false;
        float[] verticesFloat = new float[3];
        var splitLine = line.Substring(startString.Length).Split(splitChar);
        int i = 0;
        foreach (var word in splitLine)
        {
            verticesFloat[i] = float.Parse(word, System.Globalization.CultureInfo.InvariantCulture);
            i++;
        }
        resultList.Add(new [] { verticesFloat[0], verticesFloat[1], verticesFloat[2] });
        return true;
    } 
    bool GetData(string line,string startString, char splitChar, List<int[]> resultList)
    {
        if (!line.StartsWith(startString)) return false;
        List<int> facetsInt = new List<int>();
        var splitLine = line.Substring(startString.Length).Split();
        foreach (var stringWhithSlashes in splitLine)
        {
            foreach (var word in stringWhithSlashes.Split(splitChar))
            {
                facetsInt.Add(int.Parse(word));
            }
        }
        resultList.Add(facetsInt.ToArray());
        return true;
    }
}
