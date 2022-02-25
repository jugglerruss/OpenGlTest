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
            if (GetVertices())
                continue;
            if (GetNormales())
                continue;
            if (GetTextureVertices())
                continue;
            GetFaces();
            
            bool GetVertices()
            {
                if (line[0] == 'v' && line[1] == ' ')
                {
                    var spaceIndex = 0;
                    string[] verticesString = new string[3];
                    float[] verticesFloat = new float[3];
                    List<char>[] chars = new List<char>[3];
                    foreach (var word in line)
                    {
                        if (word == 'v')
                            continue;
                        if (word == ' ')
                        {
                            spaceIndex++;
                            chars[spaceIndex - 1] = new List<char>();
                        }
                        chars[spaceIndex - 1].Add(word);
                    }
                    for (int i = 0; i < verticesString.Length; i++)
                    {
                        verticesString[i] = new String(chars[i].ToArray());
                        verticesFloat[i] = float.Parse(verticesString[i], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    VerticesList.Add(new float[] { verticesFloat[0], verticesFloat[1], verticesFloat[2] });
                    return true;
                }
                return false;
            }
            bool GetNormales()
            {
                if (line[0] == 'v' && line[1] == 'n')
                {
                    var spaceIndex = 0;
                    string[] verticesString = new string[3];
                    float[] verticesFloat = new float[3];
                    List<char>[] chars = new List<char>[3];
                    foreach (var word in line)
                    {
                        if (word == 'v' || word == 'n')
                            continue;
                        if (word == ' ')
                        {
                            if( spaceIndex - 1 == 0 && chars[spaceIndex - 1].Count == 0)
                                continue;
                            spaceIndex++;
                            chars[spaceIndex - 1] = new List<char>();
                            continue;
                        }
                        chars[spaceIndex - 1].Add(word);
                    }
                    for (int i = 0; i < verticesString.Length; i++)
                    {
                        verticesString[i] = new String(chars[i].ToArray());
                        verticesFloat[i] = float.Parse(verticesString[i], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    NormalesList.Add(new float[] { verticesFloat[0], verticesFloat[1], verticesFloat[2] });
                    return true;
                }
                return false;
            }
            bool GetTextureVertices()
            {
                if (line[0] == 'v' && line[1] == 't')
                {
                    var spaceIndex = 0;
                    string[] verticesString = new string[3];
                    float[] verticesFloat = new float[3];
                    List<char>[] chars = new List<char>[3];
                    foreach (var word in line)
                    {
                        if (word == 'v' || word == 't')
                            continue;
                        if (word == ' ')
                        {
                            if( spaceIndex - 1 == 0 && chars[spaceIndex - 1].Count == 0)
                                continue;
                            spaceIndex++;
                            chars[spaceIndex - 1] = new List<char>();
                            continue;
                        }
                        chars[spaceIndex - 1].Add(word);
                    }
                    for (int i = 0; i < verticesString.Length; i++)
                    {
                        verticesString[i] = new String(chars[i].ToArray());
                        verticesFloat[i] = float.Parse(verticesString[i], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    TextureVerticesList.Add(new float[] { verticesFloat[0], verticesFloat[1], verticesFloat[2] });
                    return true;
                }
                return false;
            }

            void GetFaces()
            {
                if (line[0] == 'f' && line[1] == ' ')
                {
                    var spaceIndex = 0;
                    var slashIndex = 1;
                    string facetsString;
                    List<int> facetsInt = new List<int>();
                    List<char>[][] chars = new List<char>[3][];
                    foreach (var word in line)
                    {
                        if (word == 'f')
                            continue;
                        if (word == ' ')
                        {
                            spaceIndex++;
                            slashIndex = 1;
                            chars[spaceIndex - 1] = new List<char>[3];
                            chars[spaceIndex - 1][slashIndex - 1] = new List<char>();
                        }
                        else if (word == '/')
                        {
                            slashIndex++;
                            chars[spaceIndex - 1][slashIndex - 1] = new List<char>();
                        }
                        else
                        {
                            chars[spaceIndex - 1][slashIndex - 1].Add(word);
                        }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var charsToString = chars[i][j].ToArray();
                            facetsString = new String(charsToString);
                            facetsInt.Add(int.Parse(facetsString));
                        }
                    }
                    FacetsList.Add(facetsInt.ToArray());
                    return;
                }
            }
        }
        OnFileRead?.Invoke();
    }
}
