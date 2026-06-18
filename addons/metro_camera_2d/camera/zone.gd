@tool
@icon("../icons/MetroCameraZone2D.svg")
class_name MetroCameraZone2D extends Node2D

## Exposes an area to be covered by a [MetroCamera2D].
##
## Zones are used to bound the camera's movement and can override its behaviour.[br]
## To function, a zone needs to define its [member authority] are and camera [member bounds]
## (which can be assigned to the same object).
## [br][br]
## [b]Note[/b]: Both [member authority] and [member bounds] accepts [CollisionShape2D] nodes.
## However, only the following shapes are supported: [CircleShape2D], [RectangleShape2D], 
## [CapsuleShape2D], [ConvexPolygonShape2D], [ConcavePolygonShape2D]. [br]
## Assigning any other shape will result in the camera bounds being ignored.

#region Constants ----------------------------------------------------------------------------------

const DEFAULT_AUTHORITY_COLOR: Color = Color("3bbc6214")
const DEFAULT_BOUNDS_COLOR: Color = Color("a5a7ed14")
const _ZONE_MANAGER = preload("zone_manager.gd")

#endregion
#region Exports ------------------------------------------------------------------------------------

## That area of the zone. To be considered active, the camera's target needs to
## be within this area. [br]
@export_node_path("Polygon2D", "CollisionShape2D") 
var authority: NodePath:
	set = set_authority

## The boundaries of the camera. While the target is inside [member authority],
## the camera will be bounds to the node defined
@export_node_path("Polygon2D", "CollisionShape2D", "Path2D", "Marker2D") 
var bounds: NodePath:
	set = set_bounds

## Specify settings that will override the camera's defaults when the zone becomes active.
@export var overrides: MetroCamera2DOverride

@export_group("Editor Tools")
@export_tool_button("Set areas color", "ColorRect")
@warning_ignore("unused_private_class_variable")
var _debug_color_btn: Callable = _set_debug_colors
@export_tool_button("Set areas name", "Label")
@warning_ignore("unused_private_class_variable")
var _rename_areas_btn: Callable = _rename_areas

#endregion
#region Private Variables --------------------------------------------------------------------------

var _authority: Node2D
var _bounds: Node2D
var _authority_aabb: Rect2
var _contains: Callable
var _get_bounded_point: Callable
var _global_bounds_polygon: PackedVector2Array
var _global_authority_polygon: PackedVector2Array

#endregion
#region Public Methods -----------------------------------------------------------------------------

## Returns wether the given point is inside the zone's [member authority]. Input is expected in
## global coordinates.
func contains(point: Vector2) -> bool:
	return _authority_aabb.has_point(point) and _contains.call(point)


## Returns the point inside the zone's [member bounds] that is the closest to [param point]. Input
## and output are in global coordinates.
func get_bounded_point(point: Vector2) -> Vector2:
	return _get_bounded_point.call(point)

#endregion
#region Private Methods ----------------------------------------------------------------------------

func _ready() -> void:
	set_authority(authority)
	set_bounds(bounds)
	if not Engine.is_editor_hint():
		_ZONE_MANAGER.add_zone(self)


func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE and not Engine.is_editor_hint() :
		_ZONE_MANAGER.remove_zone(self)


func _polygon_2d_contains(point: Vector2) -> bool:
	return Geometry2D.is_point_in_polygon(point, _global_authority_polygon)


func _shape_2d_contains(point: Vector2, shape: Shape2D) -> bool:
	if shape is RectangleShape2D:
		# At this point it is already confirmed to be inside _authority_aabb
		# so we know it is contained within the rectangle shape
		return true
	if shape is CircleShape2D:
		return Geometry2D.is_point_in_circle(point, _authority.global_position, shape.radius)
	if shape is ConvexPolygonShape2D or shape is ConcavePolygonShape2D:
		return Geometry2D.is_point_in_polygon(point, _global_authority_polygon)
	if shape is CapsuleShape2D:
		point = _authority.to_local(point)
		var half_height: float = max(0.0, (shape.height / 2.0) - shape.radius)
		var closest_y: float = clamp(point.y, -half_height, half_height)
		var closest_center: Vector2 = Vector2(0, closest_y)
		return point.distance_squared_to(closest_center) <= (shape.radius * shape.radius)
	push_error("Authority Shape2D %s is not supported." % shape.get_class())
	return false


func _polygon_2d_get_bounded_point(point: Vector2) -> Vector2:
	return _polygon_get_bounded_point(point)


func _shape_2d_get_bounded_point(point: Vector2, shape: Shape2D) -> Vector2:
	if shape is RectangleShape2D:
		var extents: Vector2 = shape.size / 2.0
		var clamped_local: Vector2 = _bounds.to_local(point).clamp(-extents, extents)
		return _bounds.to_global(clamped_local)
	if shape is CircleShape2D:
		var offset: Vector2 = point - _bounds.global_position
		var clamped_offset: Vector2 = offset.limit_length(shape.radius)
		return _bounds.global_position + clamped_offset
	if shape is ConcavePolygonShape2D or shape is ConvexPolygonShape2D:
		return _polygon_get_bounded_point(point)
	if shape is CapsuleShape2D:
		point = _bounds.to_local(point)
		var half_spine: float = max(0.0, (shape.height / 2.0) - shape.radius)
		var anchor: Vector2 = Vector2(0, clamp(point.y, -half_spine, half_spine))
		var clamped_offset: Vector2 = (point - anchor).limit_length(shape.radius)
		return _bounds.to_global(anchor + clamped_offset)
	push_error("Bounds Shape2D %s is not supported." % shape.get_class())
	return point


func _path_2d_get_bounded_point(point: Vector2, path: Path2D) -> Vector2:
	return path.to_global(path.curve.get_closest_point(_bounds.to_local(point)))


func _marker_2d_get_bounded_point(_point: Vector2, marker: Marker2D) -> Vector2:
	return marker.global_position


func _polygon_get_bounded_point(point: Vector2) -> Vector2:
	if Geometry2D.is_point_in_polygon(point, _global_bounds_polygon):
		return point
	
	var polygon_size := _global_bounds_polygon.size()
	var closest := _global_bounds_polygon[0]
	var closest_dist := point.distance_squared_to(closest)
	var curr_closest: Vector2
	var curr_dist: float
	var a: Vector2
	var b: Vector2
	
	for i in range(polygon_size):
		a = _global_bounds_polygon[i]
		b = _global_bounds_polygon[(i + 1) % polygon_size]
		curr_closest = Geometry2D.get_closest_point_to_segment(point, a, b)
		curr_dist = point.distance_squared_to(curr_closest)
		if curr_dist < closest_dist:
			closest = curr_closest
			closest_dist = curr_dist
	
	return closest


func _set_debug_colors() -> void:
	var color: Color
	if _authority:
		color = DEFAULT_AUTHORITY_COLOR
		if _authority is Polygon2D:
			_authority.color = color
		elif _authority is CollisionShape2D:
			_authority.debug_color = color
	if _bounds:
		color = DEFAULT_BOUNDS_COLOR
		if _bounds == _authority:
			color = color.blend(DEFAULT_AUTHORITY_COLOR)
			color.a = DEFAULT_BOUNDS_COLOR.a
		if _bounds is Polygon2D:
			_bounds.color = color
		elif _bounds is CollisionShape2D:
			_bounds.debug_color = color 
		elif _bounds is Path2D:
			color.a = 1.0
			_bounds.self_modulate = color
		elif _bounds is Marker2D:
			color.a = 1.0
			_bounds.self_modulate = color


func _rename_areas() -> void:
	if _authority and _authority == _bounds:
		_authority.name = "Authority&Bounds"
		authority = get_path_to(_authority)
		bounds = authority
		return
	if _authority:
		_authority.name = "Authority"
		authority = get_path_to(_authority)
	if _bounds:
		_bounds.name = "Bounds"
		bounds = get_path_to(_bounds)


static func _get_global_polygon(points: PackedVector2Array, parent: Node2D) -> PackedVector2Array:
	var global_points: PackedVector2Array = PackedVector2Array()
	var size: int = points.size()
	global_points.resize(size)
	for i in range(size):
		global_points[i] = parent.to_global(points[i])
	return global_points

#endregion
#region Setter & Getters ---------------------------------------------------------------------------

func set_authority(path: NodePath) -> void:
	authority = path
	if not has_node(path):
		return
	_authority = get_node(path)
	
	if _authority is Polygon2D:
		_authority_aabb = Rect2()
		_global_authority_polygon = _get_global_polygon(_authority.polygon, _authority)
		for point in _global_authority_polygon:
			_authority_aabb = _authority_aabb.expand(point)
		_contains = _polygon_2d_contains
	elif _authority is CollisionShape2D:
		_authority_aabb = _authority.global_transform * _authority.shape.get_rect()
		_contains = _shape_2d_contains.bind(_authority.shape)
		if _authority.shape is ConvexPolygonShape2D:
			_global_authority_polygon = _get_global_polygon(_authority.shape.points, _authority)
		elif _authority.shape is ConcavePolygonShape2D:
			_global_authority_polygon = _get_global_polygon(_authority.shape.segments, _authority)
	elif Engine.is_editor_hint():
		return
	else:
		push_error("Invalid authority node: %s" % _authority)


func set_bounds(path: NodePath) -> void:
	bounds = path
	if not has_node(path):
		return
	_bounds = get_node(path)
	
	if _bounds is Polygon2D:
		_global_bounds_polygon = _get_global_polygon(_bounds.polygon, _bounds)
		_get_bounded_point = _polygon_2d_get_bounded_point
	elif _bounds is CollisionShape2D:
		_get_bounded_point = _shape_2d_get_bounded_point.bind(_bounds.shape)
		if _bounds.shape is ConvexPolygonShape2D:
			_global_bounds_polygon = _get_global_polygon(_bounds.shape.points, _bounds)
		elif _bounds.shape is ConcavePolygonShape2D:
			_global_bounds_polygon = _get_global_polygon(_bounds.shape.segments, _bounds)
	elif _bounds is Path2D:
		_get_bounded_point = _path_2d_get_bounded_point.bind(_bounds)
	elif _bounds is Marker2D:
		_get_bounded_point = _marker_2d_get_bounded_point.bind(_bounds)
	elif Engine.is_editor_hint():
		return
	else:
		push_error("Invalid bounds node: %s" % _bounds)

#endregion
