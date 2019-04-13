
uniform sampler2D tex;

void main()
{
	vec4 a = texture2D(tex, gl_TexCoord[0].st);
	vec4 color = gl_Color;
	color[3] = color[3] * a[3];
	gl_FragColor = color;
}