# BMDCubed
This program converts COLLADA (.dae) model files to the Binary MoDel (.bmd) format that first-party Nintendo GameCube and Wii games use.

It currently supports static meshes with 0 bones, and simple skinned meshes. There is an issue with complex skinned meshes not rendering correctly in-game. This issue is being researched.

BMDCubed supports the following vertex attributes:

* Normals
* 2 sets of Vertex Colors
* 8 sets of UV Coordinates

The default materials that are generated for the BMD file make use of textures, texture alpha, and vertex colors.
