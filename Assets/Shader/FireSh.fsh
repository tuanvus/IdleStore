#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 

 
  void main()  {   
 
	vec4 v_fragmentColor = vec4( 1.0, 0.3, 0.1, 1.0);
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if (v_orColor.a <= 0.5)
	{
		gl_FragColor = v_orColor;
		return;
	}

	gl_FragColor = vec4(v_orColor.r, v_orColor.g, v_orColor.b, v_orColor.a);

	gl_FragColor.r *= 0.8;
    //gl_FragColor.g *= 2.5;
    //gl_FragColor.b *= 2.5;
 } 
