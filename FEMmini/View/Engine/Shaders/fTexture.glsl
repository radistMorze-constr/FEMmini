#version 330

uniform sampler2D uSampler;
uniform vec4 uColor;

in vec2 vTexCoord;

uniform sampler2D texture0;

void main()
{
    // Texel color look up based on interpolated UV value in vTexCoord
    //vec4 c = texture2D(uSampler, vTexCoord);
    // Tint the textured area, and leave transparent area as defined by the texture
    //vec3 r = vec3(c) * (1.0-uColor.a) + vec3(uColor) * uColor.a;
    //vec4 result = vec4(r, c.a);
    //gl_FragColor = result;

    //gl_FragColor = vec4(1, 0, 0, 1);

    vec4 color = texture(texture0, vTexCoord);
    if (color.a > 0) {
        //gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
        gl_FragColor = color;
    }
    else {
        discard;
        //gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
}