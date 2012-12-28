//
//  dominanti di colore.
//  I valori dei parametri vanno da -1 a +1
//  0 = nessuna correzione.
//

float Red   : register(C0);
float Green : register(C1);
float Blue  : register(C2);

sampler2D ourImage : register(s0);


float4 main(float2 locationInSource : TEXCOORD) : COLOR
{
	// create a variable to hold our color
	float4 color;

	// get the color of the current pixel
	color = tex2D( ourImage , locationInSource.xy);

	// color has four value that we can change
	// color.r, color.g, color.b, color.a

	// values are normalized, so 0 is no color, 1 is full color
	color.r += Red;
	color.g += Green;
	color.b += Blue;

	return color;
}