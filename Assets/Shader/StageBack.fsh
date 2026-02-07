#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
varying vec2 v_texCoord; 
varying vec4 v_fragmentColor; 
 
 
uniform vec3  u_vfragment;
uniform float u_fAlpha;
 
  void main()  {   

	vec4 v_fragmentColor = vec4( u_vfragment.r, u_vfragment.g, u_vfragment.b, 1.0);
	v_fragmentColor *= u_fAlpha;
	v_fragmentColor *= 0.25;
	
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	gl_FragColor = vec4(v_orColor.r, v_orColor.g, v_orColor.b, v_orColor.a);

 } 

 