#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 
 varying vec4 v_fragmentColor; 
 
  void main()  {   

	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	gl_FragColor = vec4(v_orColor.r, v_orColor.g, v_orColor.b, v_orColor.a);
 } 
