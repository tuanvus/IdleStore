
#ifdef GL_ES 
 precision mediump float; 
#endif 


varying vec4 v_fragmentColor;
varying vec2 v_texCoord;

uniform float u_ctime_1;
 
  void main()  {   
 
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if (v_orColor.a <= 0.5)
	{
		gl_FragColor = v_orColor;
		return;
	}

	float bright    = 1.0 + u_ctime_1 * 0.4;

	v_orColor.r = v_orColor.r + (1.0 - v_orColor.r) * u_ctime_1 * 0.5;
	v_orColor.g = v_orColor.g + (1.0 - v_orColor.g) * u_ctime_1 * 0.5;
	v_orColor.b = v_orColor.b + (1.0 - v_orColor.b) * u_ctime_1 * 0.5;

	gl_FragColor = v_orColor * bright;

	//float bright    = 1.0 + u_ctime_1* 0.75;
	//float al        = 1.0 - u_ctime_1* 0.5;

	//gl_FragColor = v_orColor * bright;
	
	//gl_FragColor = vec4(v_orColor.r * bright, v_orColor.g * bright, v_orColor.b * bright, 1.0);
 } 
