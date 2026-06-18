@tool
@icon("../icons/MetroCamera2DOverride.svg")
class_name MetroCamera2DOverride extends Resource

## Defines properties that will override a [MetroCamera2D] when it enters a [MetroCameraZone2D].

#region Exports ------------------------------------------------------------------------------------

@export_group("Move Mode", "move")
## If set to [code]true[/code]. Overrides the move mode of the camera.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var move_mode_overriden: bool = false
## New move mode of the camera.
@export var move_mode: MetroCamera2D.MoveMode = MetroCamera2D.MoveMode.INSTANT:
	set = set_move_mode
## New movement delta. Only used when the move mode of the camera is set to 
## [enum MetroCamera2D.MoveMode.LINEAR].
@export var move_delta: float = 100.0
## New movement weight factor. Only used when the move mode of the camera is set to 
## [enum MetroCamera2D.MoveMode.LERP] or [enum MetroCamera2D.MoveMode.SLERP].
@export var move_weight: float = 1.0

@export_group("Zoom")
## If set to [code]true[/code]. Overrides the zoom level of the camera.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var zoom_overriden: bool = false
## New zoom of the camera.
@export var zoom: Vector2 = Vector2.ONE

@export_group("Zoom Mode", "zoom")
## If set to [code]true[/code]. Overrides the zoom mode of the camera.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var zoom_mode_overriden: bool = false
## New zoom mode of the camera.
@export var zoom_mode: MetroCamera2D.ZoomMode = MetroCamera2D.ZoomMode.INSTANT:
	set = set_zoom_mode
## New zoom delta. Only used when the zoom mode of the camera is set to 
## [enum MetroCamera2D.ZoomMode.LINEAR].
@export var zoom_delta: float = 100.0
## New zoom weight factor. Only used when the zoom mode of the camera is set to 
## [enum MetroCamera2D.MoveMode.LERP].
@export var zoom_weight: float = 1.0

@export_group("Offset")
## If set to [code]true[/code]. Overrides the offset of the camera.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var offset_overriden: bool = false
## New offset to apply to the camera.
@export var offset: Vector2

@export_group("Mouse Look-ahead", "mouse_lookahead")
## If set to [code]true[/code]. Overrides the mouse look-ahead properties of the camera.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var mouse_lookahead_overriden: bool = false
## Wether mouse look-ahead is enabled.
@export var mouse_lookahead_enabled: bool = true
## New look-ahead deadzone.
@export var mouse_lookahead_deadzone: float = 10.0
## New look-ahead minimum offset.
@export var mouse_lookahead_min_offset: Vector2 = Vector2(-100, -100)
## New look-ahead maximum offset.
@export var mouse_lookahead_max_offset: Vector2 = Vector2(100, 100)

#endregion
#region Private Methods ----------------------------------------------------------------------------

func _validate_property(property: Dictionary) -> void:
	match property.name:
		"move_weight" when (move_mode != MetroCamera2D.MoveMode.LERP and
			move_mode != MetroCamera2D.MoveMode.SLERP):
			property.usage ^= PROPERTY_USAGE_EDITOR
		"move_delta" when move_mode != MetroCamera2D.MoveMode.LINEAR:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom_weight" when zoom_mode != MetroCamera2D.ZoomMode.LERP:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom_delta" when zoom_mode != MetroCamera2D.ZoomMode.LINEAR:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom":
			property.hint |= PROPERTY_HINT_LINK

#endregion
#region Setter & Getters ---------------------------------------------------------------------------

func set_move_mode(mode: MetroCamera2D.MoveMode) -> void:
	move_mode = mode
	notify_property_list_changed()


func set_zoom_mode(mode: MetroCamera2D.ZoomMode) -> void:
	zoom_mode = mode
	notify_property_list_changed()

#endregion
