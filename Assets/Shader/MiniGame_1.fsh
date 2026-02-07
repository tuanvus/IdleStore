#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 
 varying vec4 v_fragmentColor; 
 
 uniform float u_atime_1;
 
  void main()  {   
  
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
		
	float fR = 0.54 - 0.05 * u_atime_1;
	float fG = 0.25 - 0.054 * u_atime_1;
	float fB = 0.32 - 0.058 * u_atime_1;

	
	vec4 v_fragmentColor_1 = vec4( fR, fG, fB, 1.0);
	
	vec4 v_orColor_1 = v_fragmentColor_1 * texture2D(CC_Texture0, v_texCoord);

	gl_FragColor = v_orColor_1;
 } 
