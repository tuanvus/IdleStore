#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 
 varying vec4 v_fragmentColor; 
 
 uniform float u_atime_1;
 
  void main()  {   
  
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if (v_orColor.a <= 0.7)
	{
		gl_FragColor = v_orColor;
		return;
	}
		
	float fR = 0.6 + 0.4 * u_atime_1;
	float fG = 0.8 + 0.2 * u_atime_1;
	float fB = 1.0;
	float fA = 0.9 + 0.1 * u_atime_1;
	float fBl = 3.0 - 2.0 * u_atime_1;
	
	vec4 v_fragmentColor_1 = vec4( fR, fG, fB, fA);
	
	vec4 v_orColor_1 = v_fragmentColor_1 * texture2D(CC_Texture0, v_texCoord);

	gl_FragColor = v_orColor_1;
	
	gl_FragColor *= fBl;
 } 
