# PaintSkills Simulator
* Authors: [Jaemin Cho](https://j-min.io), [Abhay Zala](https://aszala.com/), and [Mohit Bansal](https://www.cs.unc.edu/~mbansal/) (UNC Chapel Hill)
* [Paper](https://arxiv.org/abs/2202.04053)

Instructions on how to build and edit the PaintSkills Simulator.

The simulator was developed in Unity 2019.4.12f1.

## Generating data
Either download the pre-built simulator [here](https://drive.google.com/file/d/1opcJJNweB1DZOY4-bP99h5v4wO8e5rvT/view?usp=sharing) or the build it yourself from the source.

Place the built simulator as a zip called `linux_build.zip`.
The zip should contain a folder called `Linux` and the simulator build should be within this folder.

First build the docker image with
```
docker build -t objectsim:latest .
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

Then expand the drop down for the property you wish to modify or add too. There will be 2 drop downs for each property, one for named `{property}` and the other `{property}_names`. Whenever you add a new value to the property list, also add the cooresponding string name that should be used to refer to it from the JSON input.

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

Open `scripts/GenerateScene.cs`. There are 2 places that need to be updated. First, Line 289 (at the time of this document's creation) you need to update this line:
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
