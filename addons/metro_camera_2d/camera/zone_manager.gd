extends RefCounted

static var _instance: RefCounted = new()

var zones: Dictionary[MetroCameraZone2D, Variant]


static func add_zone(zone: MetroCameraZone2D) -> void:
	if not _instance.zones.has(zone):
		_instance.zones[zone] = zone


static func remove_zone(zone: MetroCameraZone2D) -> bool:
	return _instance.zones.erase(zone)


static func get_zones() -> Array[MetroCameraZone2D]:
	return _instance.zones.keys()


static func get_instance() -> RefCounted:
	return _instance
