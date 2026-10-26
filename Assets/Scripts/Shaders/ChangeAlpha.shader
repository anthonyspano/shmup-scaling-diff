Shader "Custom/ChangeAlpha"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Alpha ("Alpha Value", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Alpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // tex2D returns the color4 of the pixel
                fixed4 col = tex2D(_MainTex, i.texcoord);
                col.a *= _Alpha; // Set alpha to variable

                // discard transparent pixels
                if(col.a < 0.1) discard;

                return col;
            }
            ENDCG
        }
    }
}
