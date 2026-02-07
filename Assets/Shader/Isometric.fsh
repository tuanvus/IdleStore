#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
 varying vec2 v_texCoord; 
 varying vec4 v_fragmentColor; 
 
  void main()  {   

    vec4 Color = vec4(0.0, 0.0, 0.0, 0.0);
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	bool bOn = true;
	
	if (v_texCoord.x < 0.5 && v_texCoord.x + v_texCoord.y < 0.5)
		bOn = false;
	else if(v_texCoord.x > 0.5 && v_texCoord.x - v_texCoord.y > 0.5)
		bOn = false;
	else if (v_texCoord.x < 0.5 && v_texCoord.y - v_texCoord.x > 0.5)
		bOn = false;
	else if(v_texCoord.x > 0.5 && v_texCoord.x + v_texCoord.y > 1.5)
		bOn = false;
	
	if (bOn == true)
		gl_FragColor = v_orColor;
	else
		gl_FragColor = Color;
	
	
 } 

 