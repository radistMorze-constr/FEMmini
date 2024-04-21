#version 330 core
layout (points) in;
layout (triangle_strip, max_vertices = 10) out;

in VS_OUT {
    float indicator;
} gs_in[];

void DrawArrow(vec4 position, float indicator)
{    
    if (indicator != 1) 
    {
        gl_Position = position;
        EmitVertex();
        gl_Position = position + vec4(-0.03, -0.05, -0.01, 0.0);
        EmitVertex();
        gl_Position = position + vec4(0.03, -0.05, -0.01, 0.0);
        EmitVertex();
        EndPrimitive();
    }
    if (indicator != 0) 
    {
        gl_Position = position;
        EmitVertex();
        gl_Position = position + vec4(-0.05, 0.03, -0.01, 0.0);
        EmitVertex();
        gl_Position = position + vec4(-0.05, -0.03, -0.01, 0.0);
        EmitVertex();
        EndPrimitive();
    }
}

void main() {    
    DrawArrow(gl_in[0].gl_Position, gs_in[0].indicator);
}