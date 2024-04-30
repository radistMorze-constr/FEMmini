#version 430 core
layout (points) in;
layout (line_strip, max_vertices = 5) out;

struct LoadSSBO {
    float value;
    float angle;
};

// readonly SSBO containing the data
//layout(binding = 5, std430) readonly buffer ssbo1 {
layout(std430) readonly buffer ssbo1 {
    LoadSSBO data[];
};

vec4 RotateVec(vec4 position, float angle)
{    
    float sina = sin(angle);
    float cosa = cos(angle);

    vec4 pos = vec4(-position.x * sina + position.y * cosa, -position.y * sina - position.x * cosa, 0, 0.0);
    //vec4 pos = vec4(position.x * cosa - position.y * sina, position.y * cosa + position.x * sina, 0, 0.0);
    return pos;
}

vec4 DrawArrow(vec4 position, float multiple, float angle)
{
    gl_Position = position + multiple * RotateVec(vec4(-0.02, 0.05, 0, 0.0), angle);
    EmitVertex();   
    gl_Position = position;
    EmitVertex();
    gl_Position = position + multiple * RotateVec(vec4(0.02, 0.05, 0, 0.0), angle);
    EmitVertex();
    gl_Position = position;
    EmitVertex();
    gl_Position = position + multiple * RotateVec(vec4( 0.0,  0.2, 0, 0.0), angle);
    EmitVertex();
    EndPrimitive();
    return gl_Position;
}

void main() {    
    float multiple = data[gl_PrimitiveIDIn].value;
    float angle = data[gl_PrimitiveIDIn].angle;

    DrawArrow(gl_in[0].gl_Position, multiple, angle);
}