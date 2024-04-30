#version 430 core
layout (lines) in;
layout (line_strip, max_vertices = 50) out;

struct LoadSSBO {
    float value;
    float angle;
};

// readonly SSBO containing the data
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

void DrawLine(vec4 position1, vec4 position2)
{    
    gl_Position = position1;
    EmitVertex();   
    gl_Position = position2;
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
    return vec4(x, y, z, 1.0);
}
    
void main() {
    float multiple = data[gl_PrimitiveIDIn].value;
    float angle = data[gl_PrimitiveIDIn].angle;

    vec4 position3 = DrawArrow(gl_in[0].gl_Position, multiple, angle);
    vec4 position4 = DrawArrow(gl_in[1].gl_Position, multiple, angle);

    vec4 position5 = FindPoint(gl_in[0].gl_Position, gl_in[1].gl_Position, 2);
    vec4 position6 = FindPoint(gl_in[0].gl_Position, gl_in[1].gl_Position, 0.4);
    DrawArrow(position5, multiple, angle);
    DrawArrow(position6, multiple, angle);
    DrawLine(position3, position4);
}