#version 330 core
layout (lines) in;
layout (line_strip, max_vertices = 50) out;

void DrawArrow(vec4 position)
{    
    gl_Position = position + vec4(-0.02, 0.05, 0, 0.0);
    EmitVertex();   
    gl_Position = position;
    EmitVertex();
    gl_Position = position + vec4(0.02, 0.05, 0, 0.0);
    EmitVertex();
    gl_Position = position;
    EmitVertex();
    gl_Position = position + vec4( 0.0,  0.2, 0, 0.0);
    EmitVertex();
    EndPrimitive();
}

void DrawLine(vec4 position1, vec4 position2)
{    
    gl_Position = position1 + vec4( 0.0,  0.2, 0, 0.0);
    EmitVertex();   
    gl_Position = position2 + vec4( 0.0,  0.2, 0, 0.0);
    EmitVertex();
    EndPrimitive();
}

float FindCoordinate(float x1, float x2, float h) 
{
    float x = (x1 + h * x2) / (1 + h);
    return x;
}

vec4 FindPoint(vec4 position1, vec4 position2, float h) 
{
    float x = FindCoordinate(position1[0], position2[0], h);
    float y = FindCoordinate(position1[1], position2[1], h);
    float z = FindCoordinate(position1[2], position2[2], h);
    return vec4(x, y, z, 1);
}

void main() {    
    DrawArrow(gl_in[0].gl_Position);
    DrawArrow(gl_in[1].gl_Position);
    vec4 position1 = FindPoint(gl_in[0].gl_Position, gl_in[1].gl_Position, 2);
    vec4 position2 = FindPoint(gl_in[0].gl_Position, gl_in[1].gl_Position, 0.4);
    DrawArrow(position1);
    DrawArrow(position2);
    DrawLine(gl_in[0].gl_Position, gl_in[1].gl_Position);
}