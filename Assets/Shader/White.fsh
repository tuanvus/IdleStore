#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 
 varying vec4 v_fragmentColor; 
 
  void main()  {   

	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if( v_orColor.a == 0.0 )
		gl_FragColor = v_orColor;
	else
		gl_FragColor = vec4(1, 1, 1, v_orColor.a);
 } 

 