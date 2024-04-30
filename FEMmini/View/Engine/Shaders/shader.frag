#version 430

out vec4 outputColor;

in float color_factor;

void main()
{
    float green_factor, blue_factor = 0;
    if (color_factor <= 0.5) {
        green_factor = 0;
        blue_factor = color_factor;
    }
    else {
        green_factor = color_factor - 0.5;
        blue_factor = 0.5;
    }
    //outputColor = vec4(0.0, 1.0 - 2*green_factor, 0.0 + 2*blue_factor, 1.0);
    outputColor = vec4(0.0, 1.0, 0.0, 1.0);
}