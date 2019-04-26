uniform sampler2D texture;

varying vec3 position;
varying vec3 normal;

void main(void)
{
	gl_FragColor = gl_Color;
	//gl_FragColor = vec4(1.0, 0.5, 0.5, 1.0);
}
