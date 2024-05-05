#version 430

out vec4 outputColor;
uniform vec4 aColor;

void main()
{
	//outputColor = vec4(1.0, 0.0, 0.0, 1.0);
	outputColor = aColor;
}