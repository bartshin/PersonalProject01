Shader "Unlit/Grow"
{
  Properties
  {
    /*
     *  x: change size 
     *  y: moving axis by time (boolean)
     *  z: 
     *  w: change alpha
     */
    _AnimateValues("Animate Values", Vector) = (0, 0, 0, 0)
    _Color("Color", Vector) = (1, 1, 1, 1)
  }
  SubShader
  {
    Tags {
      "Queue"="Transparent"
    }
    Blend SrcAlpha OneMinusSrcAlpha

    LOD 100

      Pass
      {
        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members alpha)
#pragma exclude_renderers d3d11
        #pragma vertex vert
        #pragma fragment frag
        
        #include "UnityCG.cginc"

        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };

        struct v2f
        {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        };

        #define CIRCLE_LINE_WIDTH 0.007
        #define AXIS_LINE_WIDTH 0.005
        #define AXIS_LENGTH 0.05
        #define DEFUALT_AXIS_OFFSET 0.2
        #define AXIS_MOVING_DIST 0.05
        sampler2D _MainTexture;
        float4 _AnimateValues;
        float4 _Color;

        v2f vert (appdata v)
        {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
          float centerDist = distance(i.uv, fixed2(0.5, 0.5));
          // circle
          fixed4 color = _Color;
          color.w = step(abs(centerDist - _AnimateValues.x), CIRCLE_LINE_WIDTH);

          // axis
          float axisOffset = _AnimateValues.y * cos(_Time.y * 5) * AXIS_MOVING_DIST + DEFUALT_AXIS_OFFSET;
          
          fixed2 axisDist = fixed2(abs(i.uv.x - 0.5), abs(i.uv.y - 0.5));
          float isAxis = step(abs(axisDist.x - AXIS_LINE_WIDTH), AXIS_LINE_WIDTH) 
          + step(abs(axisDist.y - AXIS_LINE_WIDTH), AXIS_LINE_WIDTH);

          isAxis *= step(
            abs((_AnimateValues.x + axisOffset) - centerDist), 
            AXIS_LENGTH);

          color.w += isAxis;

          color.w *= _AnimateValues.w;
          return color;
        }
        ENDCG
      }
  }
}
