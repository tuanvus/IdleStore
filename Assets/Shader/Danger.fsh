
#ifdef GL_ES 
 precision mediump float; 
#endif 

varying vec4 v_fragmentColor;
varying vec2 v_texCoord;

uniform float u_ctime;
uniform int u_nteam;

  void main()  {   
 
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if (v_orColor.a <= 0.5)
	{
		gl_FragColor = v_orColor;
		return;
	}
		
	float value_1 = 1.0 + u_ctime * 1.7;
	float value_2 = 1.0 - u_ctime * 0.5;
	
	if(u_nteam == 0)
	{
		gl_FragColor = vec4(v_orColor.r * value_2, v_orColor.g * value_1, v_orColor.b * value_2, v_orColor.a);
	}
	else
	{
		gl_FragColor = vec4(v_orColor.r * value_1, v_orColor.g * value_2, v_orColor.b * value_2, v_orColor.a);
	}
	
	gl_FragColor *= 1.5;
 } 
