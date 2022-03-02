using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GouraudShader : IShader
{
    private FileReader _model;
    private Matrix4x4 _viewport;
    private Vector3 _light_dir;
    private Vector3 _varyingIntensity;
    private Matrix4x4 _varyingUv;
    public Matrix4x4 VaryingTri { get; private set; }
    private Texture2D _texture;
    public GouraudShader(FileReader filereader, Matrix4x4 viewMatrix, Vector3 lightDir, Texture2D texture2D)
    {
        _model = filereader;
        _viewport = viewMatrix;
        _varyingIntensity = new Vector3();
        _light_dir = lightDir;
        _varyingUv = new Matrix4x4();
        VaryingTri = new Matrix4x4();
        _texture = texture2D;
    }
    public Vector4 Vertex(int iface, int textureIndex, int normalIndex, int nthVert) {
        Vector4 glVertex = new Vector4(_model.VerticesList[iface][0],_model.VerticesList[iface][1],_model.VerticesList[iface][2],1); 
        glVertex = OurGl.M2v(_viewport * OurGl.V2m(glVertex));;
        Vector3 normal = new Vector3(_model.NormalesList[normalIndex][0], _model.NormalesList[normalIndex][1], _model.NormalesList[normalIndex][2]).normalized;
        var ity = ScalarMultyply(normal, _light_dir);
        _varyingIntensity[nthVert] = Math.Max(0,ity);
        VaryingTri = SetColumn(VaryingTri,nthVert, glVertex);
        _varyingUv = SetColumn(_varyingUv, nthVert,  new Vector4(_model.TextureVerticesList[textureIndex][0] * _texture.width,_model.TextureVerticesList[textureIndex][1] * _texture.height));
        if (nthVert == 2 && ity != 0)
        {
            Vector4 shuffle;
            if (VaryingTri.m10 > VaryingTri.m11)
            {
                VaryingTri = ChangeColumn(VaryingTri, 0, 1);
                _varyingUv = ChangeColumn(_varyingUv, 0, 1);
                (_varyingIntensity.x, _varyingIntensity.y) = (_varyingIntensity.y, _varyingIntensity.x);
            }
            if (VaryingTri.m10 > VaryingTri.m12)
            {
                VaryingTri = ChangeColumn(VaryingTri, 0, 2);
                _varyingUv = ChangeColumn(_varyingUv, 0, 2);
                (_varyingIntensity.x, _varyingIntensity.z) = (_varyingIntensity.z, _varyingIntensity.x);
            }
            if (VaryingTri.m11 > VaryingTri.m12)
            {
                VaryingTri = ChangeColumn(VaryingTri, 1, 2);
                _varyingUv = ChangeColumn(_varyingUv, 1, 2);
                (_varyingIntensity.y, _varyingIntensity.z) = (_varyingIntensity.z, _varyingIntensity.y);
            }
        }
        return glVertex;
    }
    public Color Fragment(Vector3 bar)
    {
        var uv =   _varyingUv.MultiplyVector(bar);
        float intensity = ScalarMultyply(_varyingIntensity,bar);  
        var color = _texture.GetPixel((int)uv.x,(int)uv.y);
        color = new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1);
        return color; 
    }
    private float ScalarMultyply(Vector3 v1, Vector3 v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }
    private Matrix4x4 SetColumn(Matrix4x4 matrix,int index, Vector4 column)
    {
        matrix[0, index] = column.x;
        matrix[1, index] = column.y;
        matrix[2, index] = column.z;
        matrix[3, index] = column.w;
        return matrix;
    }
    private Vector4 GetColumn(Matrix4x4 matrix,int index)
    {
        Vector4 column;
        column.x = matrix[0, index];
        column.y = matrix[1, index];
        column.z = matrix[2, index];
        column.w = matrix[3, index];
        return column;
    }
    private Matrix4x4 ChangeColumn(Matrix4x4 matrix,int index1,int index2)
    {
        var column = GetColumn(matrix, index1);
        matrix[0, index1] = matrix[0, index2];
        matrix[1, index1] = matrix[1, index2];
        matrix[2, index1] = matrix[2, index2];
        matrix[3, index1] = matrix[3, index2];
        matrix[0, index2] = column.x;
        matrix[1, index2] = column.y;
        matrix[2, index2] = column.z;
        matrix[3, index2] = column.w;
        return matrix;
    }
}
public interface IShader
{
    public Matrix4x4 VaryingTri { get; } 
    public Vector4 Vertex(int iface, int textureIndex, int normalIndex, int nthVert);
    public Color Fragment(Vector3 bar);
}