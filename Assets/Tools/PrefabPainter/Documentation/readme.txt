Introduction
-------------------------------------------------
Prefab Painter allows you to paint prefabs in the scene

Usage
-------------------------------------------------

General

	* create container gameobject for the instantiated prefabs
	* create an editor gameobject add the PrefabPainter script to it
	* in the inspector assign the container and a prefab to the prefab painter editor settings
	* in scene view adjust the radius via ctrl + mouse wheel
	* start painting prefabs by clicking left mouse button and dragging the mouse


Gravity

	* paint prefabs, as describe in General
	* click "Add Rigidbody"
	* hit play mode
	* when gravity is applied, click "Copy Transforms"
	* exit play mode
	* hit Apply Copied Transforms
	* click Remove Rigidbody
	
Delete

	* Click "Remove Container Children" to quickly remove the children of the current container
	
Input
-------------------------------------------------

Precondition: gameobject with PrefabPainterEditor script must be selected

	* ctrl + mouse wheel: adjust the radius
	* mouse drag: instantiate gameobjects

	