
#ifdef GL_ES 
 precision mediump float; 
#endif 


varying vec4 v_fragmentColor;
varying vec2 v_texCoord;

uniform float u_ctime_1;
 	
void main()			
{
	vec4 normal = vec4(0.0);

    normal =  texture2D(CC_Texture0, vec2(v_texCoord.x, v_texCoord.y));
	
    if (normal.a != 1.0)
    {
		gl_FragColor = normal;
		return;
    }

	normal.a = 0.1;
	
	
	gl_FragColor = normal;	
 } 
