#version 330 core
layout (points) in;
layout (line_strip, max_vertices = 5) out;

void DrawArrow(vec4 position)
{    
    gl_Position = position + vec4(-0.02, 0.05, -0.01, 0.0);
    EmitVertex();   
    gl_Position = position;
    EmitVertex();
    gl_Position = position + vec4(0.02, 0.05, -0.01, 0.0);
    EmitVertex();
    gl_Position = position;
    EmitVertex();
    gl_Position = position + vec4( 0.0,  0.2, -0.01, 0.0);
    EmitVertex();
    EndPrimitive();
}

void main() {    
    DrawArrow(gl_in[0].gl_Position);
}