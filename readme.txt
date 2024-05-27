Team members: (names and student IDs)
Camiel Schilham 0687839
Micha Jenniskens 2480891
Aidan Bruinsma 7098812

Tick the boxes below for the implemented features. Add a brief note only if necessary, e.g., if it's only partially working, or how to turn it on.

Formalities:
[x] This readme.txt
[x] Cleaned (no obj/bin folders)

Minimum requirements implemented:
[x] Camera: position and orientation controls, field of view in degrees
Controls: W, A, S, D for back-forth-left-right, Z and X for up-down, Q and E for yaw, arrow keys for pitch and roll
[x] Primitives: plane, sphere
[x] Lights: at least 2 point lights, additive contribution, shadows without "acne"
[x] Diffuse shading: (N.L), distance attenuation
[x] Phong shading: (R.V) or (N.H), exponent
[x] Diffuse color texture: only required on the plane primitive, image or procedural, (u,v) texture coordinates
[x] Mirror reflection: recursive
[x] Debug visualization: sphere primitives, rays (primary, shadow, reflected, refracted)

Bonus features implemented:
[ ] Triangle primitives: single triangles or meshes
[ ] Interpolated normals: only required on triangle primitives, 3 different vertex normals must be specified
[ ] Spot lights: smooth falloff optional
[x] Glossy reflections: not only of light sources but of other objects
[ ] Anti-aliasing
[x] Parallelized: using parallel-for
[x] Textures: on all implemented primitives
[x] Bump or normal mapping: on all implemented primitives
[x] Environment mapping: sphere or cube map, without intersecting actual sphere/cube/triangle primitives
[ ] Refraction: also requires a reflected ray at every refractive surface, recursive
[ ] Area lights: soft shadows
[ ] Acceleration structure: bounding box or hierarchy, scene with 5000+ primitives
Note: [provide one measurement of speed/time with and without the acceleration structure]
[ ] GPU implementation: using a fragment shader, CUDA, OptiX, RTX, DXR, or [fill in other method]

Notes:
Sources:
https://raytracing.github.io/books/RayTracingInOneWeekend.html
https://raytracing.github.io/books/RayTracingTheNextWeek.html
https://opentk.net/learn
https://opentk.net/learn/chapter2/3-materials.html
