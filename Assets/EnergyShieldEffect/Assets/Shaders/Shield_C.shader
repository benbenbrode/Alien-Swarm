// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SDQQ1234/Shield_C"
{
	Properties
	{
		_MainColor("主颜色", Color) = (1,1,1,0)
		_UV1Tex("颜色贴图(UV1)", 2D) = "white" {}
		_LightStrength("颜色强度", Range( 0 , 5)) = 0
		_FresneslPower("边缘光宽度", Range( 0 , 20)) = 5
		_FresnelScale("边缘光亮度", Range( 0 , 2)) = 1
		_UV2MoveTex("扫光贴图(UV2)", 2D) = "black" {}
		_MoveHeight("盾片偏移高度", Range( 0 , 0.1)) = 0
		_ScanningOffsetY("扫光的Y轴偏移", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			float4 vertexColor : COLOR;
			float2 uv2_texcoord2;
		};

		uniform sampler2D _UV2MoveTex;
		uniform float _ScanningOffsetY;
		uniform float _MoveHeight;
		uniform float _LightStrength;
		uniform float4 _MainColor;
		uniform sampler2D _UV1Tex;
		uniform float4 _UV1Tex_ST;
		uniform float _FresnelScale;
		uniform float _FresneslPower;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult46 = (float2(0.0 , _ScanningOffsetY));
			float2 uv2_TexCoord45 = v.texcoord1.xy + appendResult46;
			float4 tex2DNode31 = tex2Dlod( _UV2MoveTex, float4( uv2_TexCoord45, 0, 0.0) );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( tex2DNode31.a * _MoveHeight ) * ase_vertexNormal );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_UV1Tex = i.uv_texcoord * _UV1Tex_ST.xy + _UV1Tex_ST.zw;
			float4 tex2DNode1 = tex2D( _UV1Tex, uv_UV1Tex );
			o.Emission = ( _LightStrength * ( _MainColor * tex2DNode1 ) ).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV4 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode4 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV4, _FresneslPower ) );
			float2 appendResult46 = (float2(0.0 , _ScanningOffsetY));
			float2 uv2_TexCoord45 = i.uv2_texcoord2 + appendResult46;
			float4 tex2DNode31 = tex2D( _UV2MoveTex, uv2_TexCoord45 );
			o.Alpha = ( fresnelNode4 + ( tex2DNode1.a * ( i.vertexColor.a + tex2DNode31.a ) ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
7;29;1352;692;1961.783;-42.8242;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;53;-1475.903,-101.3191;Float;False;1543.756;628.0752;Comment;10;45;46;47;7;5;6;4;9;38;40;透明计算;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1463.596,443.4977;Float;False;Property;_ScanningOffsetY;扫光的Y轴偏移;7;0;Create;False;0;0;False;0;0;-0.27;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1178.22,415.9159;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;52;-853.8297,606.8077;Float;False;1126.186;362.5776;Comment;5;43;34;44;35;48;偏移距离;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-1032.944,372.0574;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;7;-765.6887,182.3533;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-803.8298,703.0266;Float;False;Property;_MoveHeight;盾片偏移高度;6;0;Create;False;0;0;False;0;0;0.0361;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;31;-803.0677,357.1494;Float;True;Property;_UV2MoveTex;扫光贴图(UV2);5;0;Create;False;0;0;False;0;None;79991f2173577ae42bae714f087a1a00;True;1;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;51;-811.1866,-609.7252;Float;False;889.3976;475.2693;Comment;5;2;1;3;41;42;每个多边形块贴图和颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;34;-753.6542,790.3853;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-878.9628,-11.76244;Float;False;Property;_FresnelScale;边缘光亮度;4;0;Create;False;0;0;False;0;1;0.4;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-396.9622,656.8077;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-761.1866,-364.4558;Float;True;Property;_UV1Tex;颜色贴图(UV1);1;0;Create;False;0;0;False;0;None;4d40e273b3dcefd44a4f0c326f7a34db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-713.8574,-559.7252;Float;False;Property;_MainColor;主颜色;0;0;Create;False;0;0;False;0;1,1,1,0;0.5631488,0.6521059,0.9117647,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-442.0876,366.9581;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-876.1015,86.01781;Float;False;Property;_FresneslPower;边缘光宽度;3;0;Create;False;0;0;False;0;5;3.2;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-318.3406,-382.2813;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-404.1616,-486.673;Float;False;Property;_LightStrength;颜色强度;2;0;Create;False;0;0;False;0;0;2.75;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;4;-581.7739,-3.35681;Float;False;Standard;TangentNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-292.5886,253.8584;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-133.1622,697.9286;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;48;15.3554,696.8735;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-86.14764,188.2413;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-90.78935,-349.7853;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;50;289.1,-2.6;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SDQQ1234/Shield_C;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;46;1;47;0
WireConnection;45;1;46;0
WireConnection;31;1;45;0
WireConnection;44;0;31;4
WireConnection;44;1;43;0
WireConnection;40;0;7;4
WireConnection;40;1;31;4
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;4;2;6;0
WireConnection;4;3;5;0
WireConnection;38;0;1;4
WireConnection;38;1;40;0
WireConnection;35;0;44;0
WireConnection;35;1;34;0
WireConnection;48;0;35;0
WireConnection;9;0;4;0
WireConnection;9;1;38;0
WireConnection;42;0;41;0
WireConnection;42;1;3;0
WireConnection;50;2;42;0
WireConnection;50;9;9;0
WireConnection;50;11;48;0
ASEEND*/
//CHKSM=D1FE2425FAE5A295E549E6101A273BC01926C8B8