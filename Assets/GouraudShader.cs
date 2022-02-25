using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GouraudShader : IShader
{
    private FileReader model;
    private Matrix4x4 Viewport;
    private Matrix4x4 Projection;
    private Matrix4x4 ModelView;
    private Vector3 light_dir;
    Vector3 varyingIntensity; // written by vertex shader, read by fragment shader
    public GouraudShader(FileReader filereader, Matrix4x4 viewport, Matrix4x4 projection, Matrix4x4 modelView, Vector3 lightDir)
    {
        model = filereader;
        Viewport = viewport;
        Projection = projection;
        ModelView = modelView;
        varyingIntensity = default;
        light_dir = lightDir;
    }
    public Vector4 Vertex(int iface, int nthvert) {
        Vector4 gl_Vertex = new Vector4(model.VerticesList[iface][0],model.VerticesList[iface][1],model.VerticesList[iface][2],0); // read the vertex from .obj file
        gl_Vertex = Viewport*Projection*ModelView*gl_Vertex;     // transform it to screen coordinates
        Vector3 normal = new Vector3(model.NormalesList[iface][0], model.NormalesList[iface][1], model.NormalesList[iface][2]);
        varyingIntensity[nthvert] = Math.Max(0, ScalarMultyply(normal,light_dir)); // get diffuse lighting intensity
        return gl_Vertex;
    }

    public bool Fragment(Vector3 bar, Color color) {
        float intensity = ScalarMultyply(varyingIntensity,bar);   // interpolate intensity for the current pixel
        color = new Color(255, 255, 255)*intensity; // well duh
        return false;                              // no, we do not discard this pixel
    }
    private float ScalarMultyply(Vector3 v1, Vector3 v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }
}
public interface IShader
{
}