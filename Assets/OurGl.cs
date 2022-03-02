using System;
using System.Threading;
using UnityEngine;

public static class OurGl
{
    private const float Depth = 255f;
    private static readonly int Width = (int)ApptimeScreen.GetScreenSize().x;
    private static readonly int Height = (int)ApptimeScreen.GetScreenSize().y;
    
    private static Vector3 Barycentric(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        Vector3 vecX = new Vector3(C.x - A.x, B.x - A.x, A.x - P.x);
        Vector3 vecY = new Vector3(C.y - A.y, B.y - A.y, A.y - P.y);
        Vector3 u = Vector3.Cross(vecX, vecY);
        if (Math.Abs(u.z)>1e-2) 
            return new Vector3(1-(u.x+u.y)/(u.z+1), u.y/(u.z+1), u.x/(u.z+1));
        return new Vector3(-1,1,1);
    }
    public static int[] Triangle(Vector4[] pts, IShader shader,  int[] zBuffer)
    {
        Vector2 boxMin = new Vector2( float.MaxValue,  float.MaxValue);
        Vector2 boxMax = new Vector2( - float.MaxValue, - float.MaxValue);
        for (int i=0; i<3; i++) {
            boxMin.x = Math.Max(0, Math.Min(boxMin.x, pts[i].x ));
            boxMin.y = Math.Max(0, Math.Min(boxMin.y, pts[i].y ));
            boxMax.x = Math.Min(Width-1,Math.Max(boxMax.x, pts[i].x ));
            boxMax.y = Math.Min(Height-1,Math.Max(boxMax.x, pts[i].y ));
        }
        Vector2 P;
        Color color;
        float fragDepth;
        Vector3 bcClip;
        for (P.x=(int)boxMin.x; P.x<=(int)boxMax.x; P.x++) {
            for (P.y=(int)boxMin.y; P.y<=(int)boxMax.y; P.y++) {
                Vector3 bcScreen = Barycentric(
                    new Vector2((int)pts[0].x,(int)pts[0].y),
                    new Vector2((int)pts[1].x,(int)pts[1].y),
                    new Vector2((int)pts[2].x,(int)pts[2].y),
                    P);
                if (bcScreen.x<0 || bcScreen.y<0 || bcScreen.z<0) continue;
                bcClip = new Vector3(bcScreen.x / pts[0].w, bcScreen.y / pts[1].w, bcScreen.z / pts[2].w);
                bcClip = bcClip * 1 / (bcClip.x + bcClip.y + bcClip.z);
                fragDepth = bcClip.x * shader.VaryingTri.m20 + bcClip.y * shader.VaryingTri.m21 + bcClip.z  * shader.VaryingTri.m22;
                var zBufferIndex = (int)P.x + (int)P.y * Width;
                if (zBuffer[zBufferIndex] >= (int)fragDepth) continue;
                zBuffer[zBufferIndex] = (int)fragDepth;
                color = shader.Fragment(bcClip);
                ApptimeScreen.SetPixel((int)P.x, (int)P.y, color); 
            }
        }
        return zBuffer;
    }
    
    //old Triangle
    public static void PaintTriangle(Texture2D texture, Vector3[] v, Vector2[] uv, float[] ity, int[] zBuffer)
    {
        v[0] = new Vector3((int)v[0].x, (int)v[0].y, (int)v[0].z);
        v[1] = new Vector3((int)v[1].x, (int)v[1].y, (int)v[1].z);
        v[2] = new Vector3((int)v[2].x, (int)v[2].y, (int)v[1].z);
        if (v[0].y == v[1].y && v[0].y == v[2].y) return;
        if (v[0].y > v[1].y){(v[0], v[1]) = (v[1], v[0]); (uv[0], uv[1]) = (uv[1], uv[0]); (ity[0], ity[1]) = (ity[1], ity[0]);}
        if (v[0].y > v[2].y){(v[0], v[2]) = (v[2], v[0]); (uv[0], uv[2]) = (uv[2], uv[0]); (ity[0], ity[2]) = (ity[2], ity[0]);}
        if (v[1].y > v[2].y){(v[1], v[2]) = (v[2], v[1]); (uv[1], uv[2]) = (uv[2], uv[1]); (ity[1], ity[2]) = (ity[2], ity[1]);}
            
        int totalHeight = (int)(v[2].y - v[0].y);
        for (int i = 0; i < totalHeight; i++)
        {
            bool isSecondHalf = i > v[1].y - v[0].y || v[1].y == v[0].y;
            int segmentHeight = (int)(isSecondHalf ? (v[2].y - v[1].y) : (v[1].y - v[0].y));
            float alpha = (float)i / totalHeight;
            if(segmentHeight == 0)
                continue;
            float beta = (float)(i - (isSecondHalf ? v[1].y - v[0].y : 0)) / segmentHeight;
            Vector3 a = v[0] + (v[2] - v[0]) * alpha;
            a = new Vector3((int)a.x, (int)a.y, (int)a.z);
            Vector3 b = isSecondHalf ? v[1] +(v[2] - v[1]) * beta : v[0] + (v[1]-v[0]) * beta;
            b = new Vector3((int)b.x, (int)b.y, (int)b.z);
            Vector2 uvA =                uv[0] + (uv[2] - uv[0]) * alpha;
            Vector2 uvB = isSecondHalf ? uv[1] + (uv[2] - uv[1]) * beta : uv[0] + (uv[1] - uv[0])*beta;
            float ityA =                ity[0] + (ity[2] - ity[0]) * alpha;
            float ityB = isSecondHalf ? ity[1] + (ity[2] - ity[1]) * beta : ity[0] + (ity[1] - ity[0])*beta;
            if (a.x > b.x)
            {
                (a, b) = (b, a);
                (uvA, uvB) = (uvB, uvA);
                (ityA, ityB) = (ityB, ityA);
            }
            for (int j = (int)a.x; j <= (int)b.x; j++)
            {
                float phi = b.x == a.x ? 1f : (j - a.x) / (b.x - a.x);
                Vector3 P = a + (b - a) * phi;
                P = new Vector3((int)P.x, (int)P.y, (int)P.z);
                var uvP = uvA + (uvB - uvA) * phi;
                var ityP = ityA + (ityB - ityA) * phi;
                int idx = (int)(P.x + P.y * Width);
                if(P.x >= Width||P.y>=Height||P.x<0||P.y<0) continue;
                if (zBuffer[idx] < P.z)
                {
                    zBuffer[idx] = (int)P.z;
                    var color = texture.GetPixel((int)(uvP.x), (int)(uvP.y));
                    color = new Color( color.r * ityP, color.g * ityP, color.b *  ityP);
                    ApptimeScreen.SetPixel((int)P.x, (int)P.y, color);
                }
            }
        }
    }
    public static Matrix4x4 LookAt(Vector3 eye, Vector3 center, Vector3 up) {
        Vector3 z = (eye-center).normalized;
        Vector3 x = Vector3.Cross(up,z).normalized;
        Vector3 y = Vector3.Cross(z,x).normalized;
        Matrix4x4 minv = Matrix4x4.identity;
        Matrix4x4 Tr   = Matrix4x4.identity;
        
        minv.m00 = x[0];
        minv.m10 = y[0];
        minv.m20 = z[0];
        Tr.m03 = -center[0];
        minv.m01 = x[1];
        minv.m11 = y[1];
        minv.m21 = z[1];
        Tr.m13 = -center[1];
        minv.m02 = x[2];
        minv.m12 = y[2];
        minv.m22 = z[2];
        Tr.m23 = -center[2];
        
        return minv*Tr;
    }
    public static Vector4 M2v(Matrix4x4 m)
    {
        return new Vector4(m.m00 / m.m30, m.m10 / m.m30, m.m20 / m.m30, m.m30);
    }
    public static  Matrix4x4 V2m(Vector4 v)
    {
        Matrix4x4 m = Matrix4x4.zero;
        m.m00 = v.x;
        m.m10 = v.y;
        m.m20 = v.z;
        m.m30 = 1f;
        return m;
    }
    public static Matrix4x4 Viewport(int x, int y, int w, int h)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m03 = x + w/2f;
        m.m13 = y + h/2f;
        m.m23 = Depth/2f;

        m.m00 = w/2f;
        m.m11 = h/2f;
        m.m22 = Depth/2f;
        return m;
    }
    private static void PaintLine(int x0, int y0, int x1, int y1, Color color)
    {
        bool isSteeping = false;
        if (Math.Abs(x0 - x1) < Math.Abs(y0 - y1))
        {
            isSteeping = true;
            (x0, y0) = (y0, x0);
            (x1, y1) = (y1, x1);
        }
        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }
        int dx = x1 - x0;
        int dy = y1 - y0;
        int derror2 = Math.Abs(dy)*2;
        int error2 = 0;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            if(isSteeping)
                ApptimeScreen.SetPixel(y, x, color);
            else
                ApptimeScreen.SetPixel(x, y, color);
            error2 += derror2;
            if (error2 > dx)
            {
                y += y1 > y0 ? 1 : -1;
                error2 -= dx*2;
            }
        }
    }
}