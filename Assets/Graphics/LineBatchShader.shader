Shader "Cytoid/InstancedLine"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required for instancing
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float distanceAlongLine : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required for instancing
            };

            UNITY_INSTANCING_BUFFER_START(PerInstanceProps)
                // x = tileCount, y = clipStart, z = clipEnd, w = unused
                UNITY_DEFINE_INSTANCED_PROP(float4, _TilingProps)
            UNITY_INSTANCING_BUFFER_END(PerInstanceProps)

            uniform fixed4 _Color;
            uniform sampler2D _BaseMap;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(float4(-v.vertex.y, v.vertex.x, 0, 1));
                o.distanceAlongLine = v.vertex.x + 0.5;
                o.color = _Color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 tileProps = UNITY_ACCESS_INSTANCED_PROP(PerInstanceProps, _TilingProps);
                float tileCount = tileProps.x;
                float clipStart = tileProps.y;
                float clipEnd = tileProps.z;
                float normalizedDistance = frac(i.distanceAlongLine * tileCount);

                fixed4 texColor = tex2D(_BaseMap, float2(0.5, normalizedDistance));

                if (i.distanceAlongLine < clipStart || i.distanceAlongLine > clipEnd)
                {
                    discard;
                }

                return texColor * i.color;
            }
            ENDCG
        }
    }
}