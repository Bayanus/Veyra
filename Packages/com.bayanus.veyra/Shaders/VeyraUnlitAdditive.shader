Shader "Veyra/UnlitAdditive"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Attributes { float4 vertex : POSITION; float4 color : COLOR; };
            struct Varyings { float4 pos : SV_POSITION; float4 color : COLOR; };
            float4 _Color;
            Varyings vert(Attributes v) { Varyings o; o.pos = UnityObjectToClipPos(v.vertex); o.color = v.color * _Color; return o; }
            float4 frag(Varyings i) : SV_Target { return i.color; }
            ENDHLSL
        }
    }
}
