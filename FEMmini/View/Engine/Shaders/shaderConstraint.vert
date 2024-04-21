#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in float aConstraintType;
    
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out VS_OUT {
    float indicator;
} vs_out;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    vs_out.indicator = aConstraintType;
}