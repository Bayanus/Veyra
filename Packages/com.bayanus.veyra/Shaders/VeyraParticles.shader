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

            struct Particle { float3 position; float3 velocity; float age; float seed; float lifetime; };
            StructuredBuffer<Particle> Particles;
            float _ParticleSize;
            float _SizeRandomness;
            float4 _StartColor;
            float4 _EndColor;

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

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
                float t = saturate(p.age / max(0.001, p.lifetime));
                float sizeFactor = lerp(1.0 - _SizeRandomness, 1.0, hash11(p.seed + 91.3));
                Varyings o;
                o.positionCS = UnityWorldToClipPos(p.position);
                o.color = lerp(_StartColor, _EndColor, t);
                o.color.a *= 1.0 - t;
                o.size = _ParticleSize * sizeFactor * (1.0 + 2.0 * t);
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
