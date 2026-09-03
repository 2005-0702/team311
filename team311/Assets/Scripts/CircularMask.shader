Shader "UI/CircularMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,1)
        _Center ("Center (viewport 0-1)", Vector) = (0.5,0.5,0,0)
        _HoleRadius ("Hole Radius", Range(0,1.5)) = 1
        _Softness ("Softness", Range(0.0001,0.5)) = 0.02
        _OverlayAlpha ("Overlay Alpha", Range(0,1)) = 0

        // UI標準プロパティ（Maskとの併用や描画順のために必要）
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float2 _Center;
            float _HoleRadius;
            float _Softness;
            float _OverlayAlpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                // FadeImageはRectTransformで画面全体に貼られている前提なので
                // 頂点のtexcoord(0-1)をそのままビューポート座標として使う
                o.screenUV = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 diff = i.screenUV - _Center;
                diff.x *= aspect; // 画面のアスペクト比を補正して円をゆがませない

                float dist = length(diff);

                float edge0 = max(_HoleRadius - _Softness, 0);
                float edge1 = _HoleRadius;
                // dist < edge0 : 穴の内側（見える／透明）
                // dist > edge1 : 穴の外側（オーバーレイで覆う）
                float mask = smoothstep(edge0, edge1, dist);

                fixed4 col = _Color;
                col.a = mask * _OverlayAlpha * texColor.a;
                return col;
            }
            ENDCG
        }
    }
}
