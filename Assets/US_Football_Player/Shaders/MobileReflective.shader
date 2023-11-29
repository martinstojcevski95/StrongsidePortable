Shader "Mobile Reflective" {
    Properties {  
      _MainTex ("Texture", 2D) = "white" {} 
      _Cube ("Cubemap", CUBE) = "" {}
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
 
    }  
    SubShader {  
      Tags { "RenderType" = "Opaque" }  
      CGPROGRAM  
      #pragma surface surf Lambert  
      struct Input
      {  
          float2 uv_MainTex;  
          float3 worldRefl;
          float3 viewDir;
      };  
      sampler2D _MainTex;   
      samplerCUBE _Cube;
      float _RimPower;
      float4 _RimColor; 
      
      void surf (Input IN, inout SurfaceOutput o) {  
		o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * .55;
		//half rim = saturate(dot (normalize(IN.viewDir), o.Normal));
		half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
		//o.Emission = texCUBE (_Cube, IN.worldRefl).rgb * _RimColor.rgb * pow(rim,_RimPower);
		o.Emission = texCUBE (_Cube, IN.worldRefl).rgb * _RimColor.rgb * 3 * pow (rim, _RimPower); 
      }  
      ENDCG  
    }   
    Fallback "Diffuse"  
}