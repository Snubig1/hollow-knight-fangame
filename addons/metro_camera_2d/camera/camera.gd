@tool
@icon("../icons/MetroCamera2D.svg")
class_name MetroCamera2D extends Camera2D

## [Camera2D] that implements zone-based custom behaviour.
##
## Uses user-defined [MetroCameraZone2D] nodes to define areas where the camera will have fixed
## bounds and property overrides. [br]
## Supports multiple movement modes (including interpolation) and optional mouse based look-ahead.

#region Signals ------------------------------------------------------------------------------------

## Emitted when entering in a zone.
signal zone_entered(zone: MetroCameraZone2D)

## Emitted when exiting a zone.
signal zone_exited(zone: MetroCameraZone2D)

#endregion
#region Enums --------------------------------------------------------------------------------------

## Defines what the camera should target for movement.
enum TargetMode {
	NODE,     ## Follows the position of the [Node2D] given in [member target_node].
	POSITION, ## Follows the user-defined position [member target_position].
}

## Defines how the camera should move to its target position.
enum MoveMode {
	INSTANT,  ## The camera is fixed to its target.
	LINEAR,   ## Moves towards its target position by a fixed delta.
	LERP,     ## Moves towards its target position using Linear Interpolation.
	SLERP     ## Moves towards its target position using Spherical Linear Interpolation.
}

## Defines how the camera should move to its target zoom.
enum ZoomMode {
	INSTANT,  ## Zoom is fixed to its target.
	LINEAR,   ## Moves towards its target zoom by a fixed delta.
	LERP      ## Moves towards its target zoom using Linear Interpolation.
}

#endregion
#region Constants ----------------------------------------------------------------------------------

const _ZONE_MANAGER = preload("zone_manager.gd")

#endregion
#region Exports ------------------------------------------------------------------------------------

## Defines what the camera should target for movement.
@export var target_mode: TargetMode = TargetMode.NODE:
	set = set_target_mode
## Node2D that the camera will follow.
@export var target_node: Node2D
## Global position that the camera will follow.
@export var target_position: Vector2:
	set = set_target_position

## Defines how the camera should move to its target position.
@export var move_mode: MoveMode = MoveMode.INSTANT:
	set = set_move_mode,
	get = get_move_mode
## Fixed amount used to move the camera towards its target.
@export var move_delta: float = 100.0:
	get = get_move_delta
## Factor used to interpolate the camera to its target.
@export var move_weight: float = 1.0:
	get = get_move_weight

## Defines how the camera should move to its target zoom.
@export var zoom_mode: ZoomMode = ZoomMode.INSTANT:
	set = set_zoom_mode,
	get = get_zoom_mode
## Fixed amount used to move the camera's zoom towards its target value.
@export var zoom_delta: float = 1.0:
	get = get_zoom_delta
## Factor used to interpolate the camera's zoom to its target value.
@export var zoom_weight: float = 1.0:
	get = get_zoom_weight

## Base zoom level of the camera.
@export var base_zoom: Vector2 = Vector2.ONE:
	set = set_base_zoom
## Base position offset of the camera.
@export var base_offset: Vector2:
	get = get_base_offset

@export_group("Mouse Look-ahead", "mouse_lookahead")
## Wether the camera should offset itself towards the mouse.
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var mouse_lookahead_enabled: bool = false:
	get = get_mouse_lookahead_enabled
## Minimum length of the look-ahead vector required to apply the offset.
@export var mouse_lookahead_deadzone: float = 30.0:
	get = get_mouse_lookahead_deadzone
## Minimum X and Y of the look-ahead offset.
@export var mouse_lookahead_min_offset: Vector2 = Vector2(-150, -100):
	get = get_mouse_lookahead_min_offset
## Maximum X and Y of the look-ahead offset.
@export var mouse_lookahead_max_offset: Vector2 = Vector2(150, 100):
	get = get_mouse_lookahead_max_offset

#endregion
#region Private Variables --------------------------------------------------------------------------

var _active_zone: MetroCameraZone2D = null:
	set = _set_active_zone
var _target_position_updated: bool = false
var _mouse_moved: bool = false
var _last_target_node_position: Vector2
var _target_camera_position: Vector2
var _target_camera_zoom: Vector2

#endregion
#region Public Methods -----------------------------------------------------------------------------

## Returns the current active zone or [code]null[/code] if there isn't one.
func get_active_zone() -> MetroCameraZone2D:
	return _active_zone

#endregion
#region Private Methods ----------------------------------------------------------------------------

func _init() -> void:
	if not Engine.is_editor_hint():
		_update.call_deferred()


func _ready() -> void:
	_target_camera_zoom = zoom


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		_mouse_moved = true


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	if _needs_update():
		_update()
	global_position = _get_moved_position(delta)
	zoom = _get_moved_zoom(delta)


func _update() -> void:
	var current_target_position: Vector2 = _get_target_position()
	_update_zone(current_target_position)
	_update_target_camera_position(current_target_position)


func _update_zone(current_target_position: Vector2) -> void:
	if _active_zone and _active_zone.contains(current_target_position):
		return
	_active_zone = null
	for zone in _ZONE_MANAGER.get_zones():
		if zone.contains(current_target_position):
			_active_zone = zone
			return


func _update_target_camera_position(current_target_position: Vector2) -> void:
	if not _active_zone:
		return
	if mouse_lookahead_enabled:
		var lookahead_offset: Vector2 = (get_global_mouse_position() - _target_camera_position) * 0.5
		lookahead_offset = lookahead_offset.clamp(mouse_lookahead_min_offset, mouse_lookahead_max_offset)
		if lookahead_offset.length() >= mouse_lookahead_deadzone:
			current_target_position += lookahead_offset
	_target_camera_position = _active_zone.get_bounded_point(current_target_position + base_offset)


func _needs_update() -> bool:
	if mouse_lookahead_enabled and _mouse_moved:
		_mouse_moved = false
		return true
	match target_mode:
		TargetMode.POSITION:
			return _target_position_updated
		TargetMode.NODE when target_node and target_node.global_position != _last_target_node_position:
			_last_target_node_position = target_node.global_position
			return true
		TargetMode.NODE when not target_node:
			push_error("Missing target node")
	return false


func _get_target_position() -> Vector2:
	match target_mode:
		TargetMode.NODE:
			return target_node.global_position
		TargetMode.POSITION:
			return target_position
	push_error("Invalid target mode")
	return Vector2.ZERO


func _get_moved_position(delta: float) -> Vector2:
	match move_mode:
		MoveMode.LINEAR:
			return global_position.move_toward(_target_camera_position, move_delta * delta)
		MoveMode.LERP:
			return global_position.lerp(_target_camera_position, move_weight * delta)
		MoveMode.SLERP:
			return global_position.lerp(_target_camera_position, move_weight * delta)
	return _target_camera_position


func _get_moved_zoom(delta: float) -> Vector2:
	match zoom_mode:
		ZoomMode.LINEAR:
			return zoom.move_toward(_target_camera_zoom, zoom_delta * delta)
		ZoomMode.LERP:
			return zoom.lerp(_target_camera_zoom, zoom_weight * delta)
	return _target_camera_zoom


func _validate_property(property: Dictionary) -> void:
	match property.name:
		"target_node" when target_mode != TargetMode.NODE:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"target_position" when target_mode != TargetMode.POSITION:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"move_weight" when move_mode != MoveMode.LERP and move_mode != MoveMode.SLERP:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"move_delta" when move_mode != MoveMode.LINEAR:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom_weight" when zoom_mode != ZoomMode.LERP:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom_delta" when zoom_mode != ZoomMode.LINEAR:
			property.usage ^= PROPERTY_USAGE_EDITOR
		"offset":
			property.usage ^= PROPERTY_USAGE_EDITOR
		"zoom":
			property.usage ^= PROPERTY_USAGE_EDITOR
		"base_zoom":
			property.hint |= PROPERTY_HINT_LINK

#endregion
#region Setter & Getters ---------------------------------------------------------------------------

func set_target_mode(mode: TargetMode) -> void:
	target_mode = mode
	notify_property_list_changed()


func set_move_mode(mode: MoveMode) -> void:
	move_mode = mode
	notify_property_list_changed()


func set_target_position(pos: Vector2) -> void:
	target_position = pos
	_target_position_updated = true


func set_zoom_mode(mode: ZoomMode) -> void:
	zoom_mode = mode
	notify_property_list_changed()


func set_base_zoom(value: Vector2) -> void:
	if value != Vector2.ZERO:
		base_zoom = value
	if Engine.is_editor_hint():
		zoom = base_zoom


func get_move_mode() -> MoveMode:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.move_mode_overriden:
		return _active_zone.overrides.move_mode
	return move_mode

func get_move_delta() -> float:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.move_mode_overriden:
		return _active_zone.overrides.move_delta
	return move_delta


func get_move_weight() -> float:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.move_mode_overriden:
		return _active_zone.overrides.move_weight
	return move_weight


func get_zoom_mode() -> ZoomMode:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.zoom_mode_overriden:
		return _active_zone.overrides.zoom_mode
	return zoom_mode


func get_zoom_delta() -> float:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.zoom_mode_overriden:
		return _active_zone.overrides.zoom_delta
	return zoom_delta


func get_zoom_weight() -> float:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.zoom_mode_overriden:
		return _active_zone.overrides.zoom_weight
	return zoom_weight


func get_base_offset() -> Vector2:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.offset_overriden:
		return _active_zone.overrides.offset
	return base_offset


func get_mouse_lookahead_enabled() -> bool:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.mouse_lookahead_overriden:
		return _active_zone.overrides.mouse_lookahead_enabled
	return mouse_lookahead_enabled


func get_mouse_lookahead_deadzone() -> float:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.mouse_lookahead_overriden:
		return _active_zone.overrides.mouse_lookahead_deadzone
	return mouse_lookahead_deadzone


func get_mouse_lookahead_min_offset() -> Vector2:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.mouse_lookahead_overriden:
		return _active_zone.overrides.mouse_lookahead_min_offset
	return mouse_lookahead_min_offset


func get_mouse_lookahead_max_offset() -> Vector2:
	if _active_zone and _active_zone.overrides and _active_zone.overrides.mouse_lookahead_overriden:
		return _active_zone.overrides.mouse_lookahead_max_offset
	return mouse_lookahead_max_offset


func _set_active_zone(zone: MetroCameraZone2D) -> void:
	var old_zone: MetroCameraZone2D = _active_zone
	_active_zone = zone
	if old_zone:
		zone_exited.emit(old_zone)
		if old_zone.overrides and old_zone.overrides.zoom_overriden:
			_target_camera_zoom = base_zoom
	if _active_zone:
		zone_entered.emit(_active_zone)
		if _active_zone.overrides and _active_zone.overrides.zoom_overriden:
			_target_camera_zoom = _active_zone.overrides.zoom

#endregion
