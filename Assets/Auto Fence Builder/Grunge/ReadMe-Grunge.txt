Grunge & Dirt

Most of the textures in AFB are quite clean without too much grunge & dirt;  this is because it’s easy to add grunge, but difficult to remove if you wanted a clean texture.

One way to add grunge  is to add a grunge texture to the Material using the Standard Shader. 
There are some examples in the Grunge folder. Drag one of the grunge maps (or use your own) on to the ‘Detail Albedo x2’ slot. The strength of this can then be modified by adding one of the Opacity maps to the ‘Detail Mask’ slot. While this isn’t the most efficient way to add grunge, it’s a good way to quickly audition different grunge textures.

You can see an example of this on the preset ‘ConcreteDirtyOldWall’ where the Rail/Wall A uses a material with an added grunge detail map. 
If you view its material (OldConcreteWall), you can change the Detail Mask to Opacity90 and see the effect.

The ideal solution is to modify the color/albedo map directly and add the grunge there, along with any corresponding metal/smooth/normal maps. 