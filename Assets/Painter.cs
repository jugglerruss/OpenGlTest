using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class Painter : MonoBehaviour
{
    [SerializeField] private float _scale = 300;
    [SerializeField] private float _scaleLight = 0.7f;
    [SerializeField] private float _cameraDistance = 3f;
    [SerializeField] private Vector3 _eye = new Vector3(-1,0.3f,1);
    [SerializeField] private Vector3 _center = new Vector3(0,0,0);
    [SerializeField] private Vector3 _lightDir = new Vector3(-1,1,1);
    [SerializeField] private FileReader _fileReader;
    [SerializeField] private Texture2D _texture;
    [SerializeField] private Text _textFps;
    
    private bool _isReading;
    private bool _isReady;
    private int _width;
    private int _height;
    private int _timeApplyPixelChanges;
    private Matrix4x4 _modelView;
    private float _timeStart;
    private int[] _zBuffer;
    private Color[] _colorBuffer;
    private Vector3 _eyeStart;
    private void Start() {
        _fileReader.OnFileRead += RepaintObject;
        _width = (int)ApptimeScreen.GetScreenSize().x;
        _height = (int)ApptimeScreen.GetScreenSize().y;
        _zBuffer = new int[_width * _height];
        _colorBuffer = new Color[_width * _height];
        _eyeStart = _eye;
    }
    private void Update()
    { 
        var fps = (1f/Time.unscaledDeltaTime);
        _textFps.text = fps.ToString();
        if (!_isReady) 
            return;
        CameraMove(); 
        RepaintObject();
    }
    private void CameraMove()
    {
        if (_eye.x < _eyeStart.x && _eye.z >= _eyeStart.z)
            _eye += new Vector3(0.1f, 0, 0);
        else
        {
            if (_eye.x <= _eyeStart.x-1 && _eye.z > _eyeStart.z-1)
                _eye += new Vector3(0, 0, 0.1f);
            else
            {
                if (_eye.z > _eyeStart.z-1)
                    _eye += new Vector3(0, 0, -0.1f);
                else if (_eye.x > _eyeStart.x-1)
                    _eye += new Vector3(-0.1f, 0, 0);
                else
                    _eye += new Vector3(0, 0, 0.1f);
            }
        }
    }
    public void LoadFile()
    {
        if (_isReady)
        {
            return;
        }
        if (_isReading)
        {
            return;
        }
        _isReading = true;
        _fileReader.Load();
    }
    private void RepaintObject()
    {
        Debug.Log("RepaintObject");
        _isReading = false;
        _isReady = false;
        _timeStart = (int)(Time.realtimeSinceStartup * 1000);
       // Debug.Log("_timeStart " + _timeStart);
        //ApptimeScreen.Clear();
        PaintObject();
        var timeAfterPaintObject = (int)(Time.realtimeSinceStartup * 1000);
       // Debug.Log("timeAfterPaintObject " + (timeAfterPaintObject-timeClear));
        ApptimeScreen.ApplyPixelChanges();
        _timeApplyPixelChanges = (int)(Time.realtimeSinceStartup * 1000);
      // Debug.Log("timeApplyPixelChanges " + (_timeApplyPixelChanges-timeAfterPaintObject));
        _isReady = true;
    }
    private void PaintObject()
    {
        _modelView = OurGl.LookAt(_eye, _center, new Vector3(0, 1, 0));
        var projection  = Matrix4x4.identity;
        var viewPort    = OurGl.Viewport(_width/8,_height/8,_width*3/4,_height*3/4);
        projection.m32 = -1f / (_eye - _center).magnitude;
        Matrix4x4 z = (viewPort*projection*_modelView);
        var lightDir =(_modelView* _lightDir).normalized;
        
        var timeSrartFillArrays = (int)(Time.realtimeSinceStartup * 1000);
        Debug.Log("timeSrartFillArrays " + (timeSrartFillArrays-_timeStart));
        for (int i=0; i<_zBuffer.Length; i++)
        {
            _zBuffer[i] = int.MinValue;
        }
        for (int i=0; i<_colorBuffer.Length; i++)
        {
            _colorBuffer[i] = Color.black;
        }
        var timeEndFillArrays = (int)(Time.realtimeSinceStartup * 1000);
        Debug.Log("timeEndFillArrays " + (timeEndFillArrays-timeSrartFillArrays));
        foreach (var facesSet in _fileReader.FacetsList)
        {
            Vector4[] screenCoords  = new Vector4[3];
            GouraudShader shader = new GouraudShader(_fileReader, z,lightDir, _texture);
            for (int j = 0; j < 3; j++)
            {
                var k = j * 3;
                shader.Vertex(facesSet[k],facesSet[k+1],facesSet[k+2], j);
            }
            
            for (int j = 0; j < 3; j++)
            {
                screenCoords[j] = shader.VaryingTri.GetColumn(j);
            }
            OurGl.Triangle(screenCoords, shader, _zBuffer, _colorBuffer);
        }
        
        ApptimeScreen.SetPixelsSimple(_colorBuffer);
    }
}
