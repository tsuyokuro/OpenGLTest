// simple.vert

varying vec4 position;
varying vec3 normal;

void main(void)
{
	position = gl_ModelViewMatrix * gl_Vertex;
	normal = normalize(gl_NormalMatrix * gl_Normal);
	
	gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;

	vec3 light = normalize((gl_LightSource[0].position * position.w - gl_LightSource[0].position.w * position).xyz);
	float diffuse = max(dot(light, normal), 0.0);

	vec3 view = -normalize(position.xyz);
	vec3 halfway = normalize(light + view);
	float specular = pow(max(dot(normal, halfway), 0.0), gl_FrontMaterial.shininess);
	gl_FrontColor = gl_FrontLightProduct[0].diffuse * diffuse
		+ gl_FrontLightProduct[0].specular * specular
		+ gl_FrontLightProduct[0].ambient;

	gl_Position = ftransform();
}