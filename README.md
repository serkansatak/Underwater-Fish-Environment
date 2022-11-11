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

# Overview of randomized parameters 
|                **Fish**            |                                                                                    | Data type/Unity encoding  |
|:----------------------------------:|------------------------------------------------------------------------------------|---------------------------|
| Number of fish                     | <4, 50>                                                                            | int                       |
| Initial fish position              | [<0, 1>, <0, 1>, <20, 60>]                                                         | Camera viewport encoding  |
| Initial fish rotation              | [0, <-180, 180>, 0]                                                                | RPY encoding              |
| Fish scale                         | [1, 1, 1] * <0.5, 1>                                                               | Vector3                   |
| Fish material - Albedo             | [[1, 1, 1] * <75, 225>, 1]                                                         | RGBA encoding             |
| Fish material - Metalic            | <0.1, 0.5>                                                                         | float                     |
| Fish material - Metalic\Glossiness | <0.1, 0.5>                                                                         | float                     |
<br/>
|           **Boid behaviour**       |                                                                                    | Data type/Unity encoding  |
| K                                  | <0.75, 1.25>                                                                       | float                     |
| S                                  | <0.75, 1.25>                                                                       | float                     |
| M                                  | <0.75, 1.25>                                                                       | float                     |
| L                                  | <0.75, 1.25>                                                                       | float                     |
| No clumping area                   | <7.5, 12.5>                                                                        | float                     |
| Local area                         | <15, 25>                                                                           | float                     |
| Random direction                   | [<-1, 1>, <-1, 1>, <-1, 1>]                                                        | Vector3                   |
| Random weight                      | <1, 10>                                                                            | float                     |
<br/>
|             **Environment**        |                                                                                    | Data type/Unity encoding  |
| Video background                   | <background_1.mp4, background_152.mp4>                                             | NA                        |
| Fog/Plain background colour        | [<171, 191>, <192, 212>, <137, 157>, <151, 171>]   \|\| [[1, 1, 1] * <75, 225>, 1] | RGBA colour encoding      |
| Fog intensity                      | <0.1, 0.8>                                                                         | float                     |
| Number of distractors              | <50, 500>                                                                          | int                       |
| Initial distractor position        | [<0, 1>, <0, 1>, <10, 50>]                                                         | Camera viewport encoding  |
| Distractor scale                   | [1, 1, 1] * <0.01, 1>                                                              | Vector3                   |
| Distractor colour                  | [<171, 191>, <192, 212>, <137, 157>, <151,   171>]                                 | RGBA colour encoding      |
| Distractor transparency            | <0, 1>                                                                             | float                     |
