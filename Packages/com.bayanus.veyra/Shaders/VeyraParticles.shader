Shader "Veyra/Particles"
{
    Properties
    {
        _ParticleSize ("Particle Size", Float) = 0.06
        _StartColor ("Start Color", Color) = (1,1,1,1)
        _EndColor ("End Color", Color) = (1,0.2,0.02,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
            #include "UnityIndirect.cginc"

            struct Particle { float3 position; float3 velocity; float age; float seed; };
            StructuredBuffer<Particle> Particles;
            float _ParticleSize;
            float4 _StartColor;
            float4 _EndColor;
            float _Lifetime;

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR0;
                float size : PSIZE;
            };

            Varyings Vert(uint svVertexID : SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                Particle p = Particles[GetIndirectVertexID(svVertexID)];
                float t = saturate(p.age / max(0.001, _Lifetime));
                Varyings o;
                o.positionCS = UnityWorldToClipPos(p.position);
                o.color = lerp(_StartColor, _EndColor, t);
                o.size = _ParticleSize * (1.0 + 2.0 * t);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
