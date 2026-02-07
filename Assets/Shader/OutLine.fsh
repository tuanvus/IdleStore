#ifdef GL_ES 
	precision lowp float; 
#endif 

varying vec4 v_fragmentColor;
varying vec2 v_texCoord;

uniform vec3  u_vfragment;

void main()
{

    float radius = 0.0015;
    float threshold = 5.0;
    float fLine  = radius * threshold;

    vec4 accum = vec4(0.0);
    vec4 normal = vec4(0.0);
    vec4 Color = vec4(u_vfragment.r, u_vfragment.g, u_vfragment.b, 0.1);

    normal =  texture2D(CC_Texture0, vec2(v_texCoord.x, v_texCoord.y));

	if ( normal.r != 0.0 && normal.g != 0.0  && normal.b != 0.0 )
    {
		gl_FragColor = normal;
		return;
    }

    if ( normal.a > 0.5 )
    {
		gl_FragColor = normal;

		return;
    }
	
    accum = texture2D(CC_Texture0, vec2( v_texCoord.x - fLine , v_texCoord.y - fLine ));

	if ( accum.a != 0.0 )
    {
		gl_FragColor = Color;
		return;
    }



    accum = texture2D(CC_Texture0, vec2( v_texCoord.x + fLine , v_texCoord.y - fLine ));

   if (  accum.a != 0.0)
    {
		gl_FragColor = Color;
		return;
    }

	
    accum = texture2D(CC_Texture0, vec2(v_texCoord.x + fLine , v_texCoord.y + fLine ));

   if ( accum.a != 0.0)
    {
		gl_FragColor = Color;
		return;
    }

	

    accum = texture2D(CC_Texture0, vec2(v_texCoord.x - fLine , v_texCoord.y + fLine ));

    if ( accum.a != 0.0)
	{
		gl_FragColor = Color;
		return;
    }

	
	gl_FragColor = vec4(0.0,0.0,0.0, 0.0);

}
