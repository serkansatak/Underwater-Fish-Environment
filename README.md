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
<table>
<thead>
  <tr>
    <th colspan="2">Fish </th>
    <th>Data type/Unity encoding </th>
  </tr>
</thead>
<tbody>
  <tr>
    <td>Number of fish</td>
    <td>&lt;4, 50&gt;</td>
    <td>int</td>
  </tr>
  <tr>
    <td>Initial fish position</td>
    <td>[&lt;0, 1&gt;, &lt;0, 1&gt;, &lt;20, 60&gt;]</td>
    <td>Camera viewport encoding</td>
  </tr>
  <tr>
    <td>Initial fish rotation</td>
    <td>[0, &lt;-180, 180&gt;, 0]</td>
    <td>RPY encoding</td>
  </tr>
  <tr>
    <td>Fish scale</td>
    <td>[1, 1, 1] * &lt;0.5, 1&gt;</td>
    <td>Vector3 </td>
  </tr>
  <tr>
    <td>Fish material - Albedo</td>
    <td>[[1, 1, 1] * &lt;75, 225&gt;, 1]</td>
    <td>RGBA encoding</td>
  </tr>
  <tr>
    <td>Fish material - Metalic</td>
    <td>&lt;0.1, 0.5&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>Fish material - Metalic\Glossiness</td>
    <td>&lt;0.1, 0.5&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td colspan="2">Boid behaviour</td>
    <td></td>
  </tr>
  <tr>
    <td>K</td>
    <td>&lt;0.75, 1.25&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>S</td>
    <td>&lt;0.75, 1.25&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>M</td>
    <td>&lt;0.75, 1.25&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>L</td>
    <td>&lt;0.75, 1.25&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>No clumping area</td>
    <td>&lt;7.5, 12.5&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>Local area</td>
    <td>&lt;15, 25&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>Random direction</td>
    <td>[&lt;-1, 1&gt;, &lt;-1, 1&gt;, &lt;-1, 1&gt;]</td>
    <td>Vector3</td>
  </tr>
  <tr>
    <td>Random weight </td>
    <td>&lt;1, 10&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td colspan="2">Environment</td>
    <td></td>
  </tr>
  <tr>
    <td>Video background </td>
    <td>&lt;background_1.mp4, background_152.mp4&gt;</td>
    <td>NA</td>
  </tr>
  <tr>
    <td>Fog/Plain background colour </td>
    <td>[&lt;171, 191&gt;, &lt;192, 212&gt;, &lt;137, 157&gt;, &lt;151, 171&gt;]&nbsp;&nbsp;&nbsp;|| [[1, 1, 1] * &lt;75, 225&gt;, 1]</td>
    <td>RGBA colour encoding</td>
  </tr>
  <tr>
    <td>Fog intensity</td>
    <td>&lt;0.1, 0.8&gt;</td>
    <td>float</td>
  </tr>
  <tr>
    <td>Number of distractors </td>
    <td>&lt;50, 500&gt;</td>
    <td>int</td>
  </tr>
  <tr>
    <td>Initial distractor position </td>
    <td>[&lt;0, 1&gt;, &lt;0, 1&gt;, &lt;10, 50&gt;]</td>
    <td>Camera viewport encoding</td>
  </tr>
  <tr>
    <td>Distractor scale</td>
    <td>[1, 1, 1] * &lt;0.01, 1&gt;</td>
    <td>Vector3 </td>
  </tr>
  <tr>
    <td>Distractor colour</td>
    <td>[&lt;171, 191&gt;, &lt;192, 212&gt;, &lt;137, 157&gt;, &lt;151,&nbsp;&nbsp;&nbsp;171&gt;] </td>
    <td>RGBA colour encoding</td>
  </tr>
  <tr>
    <td>Distractor transparency</td>
    <td>&lt;0, 1&gt;</td>
    <td>float</td>
  </tr>
</tbody>
</table>
