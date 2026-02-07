#ifdef GL_ES 
 precision mediump float; 
#endif 
 
 
varying vec2 v_texCoord; 

varying vec4 v_fragmentColor; 

void main()  
{   
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	gl_FragColor = vec4(v_orColor.r * 0.8, v_orColor.g * 0.7, v_orColor.b * 0.7, v_orColor.a);
} 
