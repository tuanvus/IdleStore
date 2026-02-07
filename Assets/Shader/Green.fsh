#ifdef GL_ES 
 precision mediump float; 
#endif 
 
varying vec4 v_fragmentColor;	
varying vec2 v_texCoord;	

void main() {
	vec4 v_orColor = v_fragmentColor * texture2D(CC_Texture0, v_texCoord);
	
	if( v_orColor.a > 0.0 )
	{
		gl_FragColor = vec4(0, 1, 0, 1);
		return;
	}
	
	gl_FragColor = vec4(0, 0, 0, 0);
}