# PaintSkills Simulator

Instructions on how to build and edit [PaintSkills Simulator](https://arxiv.org/abs/2202.04053) as described in the paper:

[**DALL-Eval: Probing the Reasoning Skills and Social Biases of Text-to-Image Generative Transformers**](https://arxiv.org/abs/2202.04053)
<br>
 <a href='https://j-min.io'>Jaemin Cho</a>,
 <a href='https://aszala.com/'>Abhay Zala</a>,
 <a href='https://www.cs.unc.edu/~mbansal/'>Mohit Bansal</a>
 (UNC Chapel Hill)
 <br>

The simulator was developed in [Unity 2019.4.12f1](https://unity3d.com/unity/whats-new/2019.4.12).

## Generating data
Either download the pre-built simulator [here](https://drive.google.com/file/d/1opcJJNweB1DZOY4-bP99h5v4wO8e5rvT/view?usp=sharing) or build it yourself from the source.

Place the built simulator as a zip called `linux_build.zip`.
The zip should contain a folder called `Linux` and the simulator build should be within this folder.

First, build the docker image with
```
docker build -t paintskillssim:latest .
```
Then generate the JSON input files for each skill {object, color, count, spatial}.
```
bash generate_scenes.sh {skill} 10 scene
```
Then generate the images for each skill
```
bash generate_scenes.sh {skill} 10 image
```
Then finally generate the bounding boxes for each image with
```
bash generate_scenes.sh {skill} 10 box
```

Example Scene Generation:

<img src="https://user-images.githubusercontent.com/22106429/155769162-57fb93cf-2a22-49da-82a5-f5171fd6f632.png" width="500px">

Each "scene" must include the various scene properties and a list of objects.
Given a lits of these "scenes", the simulator will generate a series of images fitting the properties defined.

Here is the scene format:
```json
[
 {
  "scene": "background environment for the image (e.g. empty)",
  "skill": "which skill you are trying to generate for (e.g. object)",
  "text": "the text prompt for this scene (e.g. a photo of two airplanes)",
  "id": "unique id for this scene (e.g. object_train_00001)",
  "objects": [
    {
     "id": "unique identifier number for this object (e.g. 0)",
     "shape": "object to be used (e.g. airplane)",
     "relation": "relation to any other objects based on id number (e.g. 'right_0' means right of object with id 0)",
     "color": "color of the object (e.g. red)",
     "scale": "size of the object (e.g. 2.5)",
     "texture": "texture of object (e.g. plain)",
     "rotation": "xyz rotation values for the object in degrees (e.g. [100, 50, 20])",
     "state": "the special state of the object (e.g. 'standing' for a dog)"
    },
    ...
   ]
 },
 ...
]
```

## Build
Go to `File` -> `Build Settings`.

Ensure `Scenes/Main` is included in "Scenes In Build".

Then follow these steps:
 - Set the platform to `PC, Mac & Linux Standalone`.
 - Select `Linux` for the target platform.
 - Select `x86_64` as the Architecture
 - Enable Development Build if you want debugging information
 - All other checkboxes should be empty.
 - Select `Default` for the Compression Method.


Now you can click build and then select the file location you wish to save the build.

## Simulator Modification
After making a modification you should update `generate_scenes.py` as needed.

### Non-Model Modification / Addition
Select the `SceneManager` Object in the Main scene. This object controls all properties of the simulator.

Then expand the drop down for the property you wish to modify or add to. There will be 2 drop downs for each property, one for named `{property}` and the other `{property}_names`. Whenever you add a new value to the property list, also add the corresponding string name that should be used to refer to it from the JSON input.

### Model Modification / Addition

#### Model Creation
Follow these steps:
 - Add your 3D model to the project.
 - Place it into the scene.
 - Scale the model down so it roughly fits within one of the reference cubes (1x1x1) in the scene.
 - Remove all colliders on the object and its children.
 - Add a `Box Coliider` to the parent object and scale it to fit the model
 - Add a `Rigidbody` to the parent object and under constraints, Check all `Freeze Position` and `Freeze Rotation` options.
 - Add the `Object Info` script to the parent object. If your model has materials on its childern objects, then select the `Has Material On Children` property. Otherwise you can leave it unchecked.
 - Save your object as a prefab by dragging it from the Hierarchy view to the Project View.

Select the `SceneManager` Object in the Main scene. This object controls all properties of the simulator.

Then expand the drop down for objs and obj_names. Add your model prefab and the reference name string to each list respectively. 

#### Model Modification
Find the model you wish to update and edit the prefab for that model. Ensure you are actually modifying the prefab, if you are not, then it will not update.


### Spatial Relation Modification / Addition
To modify or add new spatial relations, you need to edit the code files.

Open `scripts/GenerateScene.cs`. There are 2 places that need to be updated. First, Line 300 (at the time of this document's creation) you need to update this line:
```c#
directions.AddRange(new string[] { "left", "right", "behind", "front", "above", "near", "far" });
```
Add or remove any spatial relations you want. Then slightly below this, you will find a series of `if` statements. Add your own or modify the existing one for the relation you want.

When making your own, you can follow the pattern the others follow. Get the position of the `relationObject` and then add your relation to the `relation_pairs` Dictionary. The key should be your relation as a string and then the value should be a new shape object.

# Reference
Please cite our paper if you use our dataset in your works:
```bibtex

@article{Cho2022DallEval,
  title         = {DALL-Eval: Probing the Reasoning Skills and Social Biases of Text-to-Image Generative Transformers},
  author        = {Jaemin Cho and Abhay Zala and Mohit Bansal},
  year          = {2022},
  archivePrefix = {arXiv},
  primaryClass  = {cs.CV},
  eprint        = {2202.04053}
}
```
