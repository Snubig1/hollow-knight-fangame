shader_type canvas_item;

uniform vec2 frameAmount;
uniform vec2 currentFrame;



void vertex() {
}

void fragment() {
	vec2 frameSize = 1.0/frameAmount;
	COLOR = texture(TEXTURE, vec2(UV.x * frameSize.x + fract(TIME)*4.0 *frameSize.x, UV.y * frameSize.y + frameSize.y));
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
