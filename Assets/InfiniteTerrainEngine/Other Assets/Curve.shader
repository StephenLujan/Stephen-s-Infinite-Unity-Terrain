// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

//
// Curved.shader
//
// Author:
//       Devon O. <devon.o@onebyonedesign.com>
//
// Copyright (c) 2016 Devon O. Wolfgang
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


Shader "Custom/Curved"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert addshadow

        // Global Shader values
        uniform float2 _BendAmount;
        uniform float3 _BendOrigin;
        uniform float _BendFalloff;

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input
        {
              float2 uv_MainTex;
        };

        float4 Curve(float4 v)
        {
              //HACK: Considerably reduce amount of Bend
              _BendAmount *= .0001;

              float4 world = mul(unity_ObjectToWorld, v);

              float dist = length(world.xz-_BendOrigin.xz);

              dist = max(0, dist-_BendFalloff);

              // Distance squared
              dist = dist*dist;

              world.xy += dist*_BendAmount;
              return mul(unity_WorldToObject, world);
        }

        void vert(inout appdata_full v)
        {
              v.vertex = Curve(v.vertex);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
              fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
              o.Albedo = c.rgb;
              o.Alpha = c.a;
        }

        ENDCG
    }
 
      Fallback "Mobile/Diffuse"
}