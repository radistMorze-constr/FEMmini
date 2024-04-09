#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

//uniform float maxDeflection;
//uniform float minDeflection;

out float color_factor;

void main(void)
{
    //color_factor = (aPosition.z - minDeflection) / (maxDeflection - minDeflection);
    color_factor = 1;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
