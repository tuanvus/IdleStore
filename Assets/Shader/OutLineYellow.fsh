#ifdef GL_ES 
	precision lowp float; 
#endif 

varying vec4 v_fragmentColor;
varying vec2 v_texCoord;

void main()
{

    float radius = 0.001;
    float threshold = 6.0;
    float fLine  = radius * threshold;

    vec4 accum = vec4(0.0);
    vec4 normal = vec4(0.0);
    vec4 Color = vec4(1.0, 1.0, 1.0, 0.4);

    normal =  texture2D(CC_Texture0, vec2(v_texCoord.x, v_texCoord.y));
	
    if (normal.a == 0.5)
    {
		gl_FragColor = Color;
		return;
    }

    gl_FragColor = normal;

}
