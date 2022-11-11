## Underwater-Fish-Environment
 TODO: 
- SpawnerBoids.cs -> Replace X with L
- ADD zip with video background

# Overview

- The scene has 2 cameras - Fish Camera that only renders the fish prefabs and Background Camera that renders the video from the Video player object
- All the post-processing effects are set in the Post-processing effect object. The effects only apply to the Fish Camera output
- Waypoints object - after starting it holds all the waypoint objects that are used to create a bezier curve path
- Path - object that has the Path Creator and Generate Path scripts - these scripts are taken from the work of - [https://assetstore.unity.com/packages/tools/utilities/b-zier-path-creator-136082](https://assetstore.unity.com/packages/tools/utilities/b-zier-path-creator-136082). with more information here - [https://docs.google.com/document/d/1-FInNfD2GC-fVXO6KyeTSp9OSKst5AzLxDaBRb69b-Y/edit](https://docs.google.com/document/d/1-FInNfD2GC-fVXO6KyeTSp9OSKst5AzLxDaBRb69b-Y/edit)
- moverObj - contains the scripts for spawning the fish and parenting them to it and the path follower script that follows a randomly generated path at a certain speed
- lookAtObj is the object the fish are currently always looking at - this can be changed in the PathFollower script
- background Transparent - this object has a simple transparent colored shader together with a vignetted texture that more or less resembles the way the light in the videos vignettes the scenes

All the scripts can be found on:
 - Video player - SetVideoBackground script
 - Waypoints - GenerateGridWaypoints script
 - Path - PathCreator and GeneratePathExample script
 - moverObj - PathFollower and SpawnFish script
 - In the Prefab folder, the stickleback fish prefab has some of the other scripts - Rig builder script on the top object and on the FishMeshHolder - GetScreenSpaceBounds, FishName and GetFishBounds (Daniel's script)
 
 The fish animation is generated with the special Inverse Kinematic Animation library in Unity, so the stickback fish prefab is build in a specific way to enable the animation to be constructed. If you need to change it please let me know so I can give an overview.
