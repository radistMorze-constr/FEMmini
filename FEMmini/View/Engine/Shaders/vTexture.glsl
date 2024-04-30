#version 430

layout(location = 1) in vec3 aPosition;
layout(location = 2) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    //gl_Position = uVPMatrix * uModelMatrix * vec4(aPosition, 1.0);
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    //gl_Position = vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
}