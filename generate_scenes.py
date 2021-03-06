
import json
import argparse
import random
import os
import sys
import math
import numpy as np
import random
from pathlib import Path
from tqdm import tqdm
import copy

def camel_to_snake(s):
    return ''.join([' '+c.lower() if c.isupper() else c for c in s]).strip()


def generate_object_scene(args, metadata, skill_data, split='train'):
    """
    Generate scene

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
     }

    """

    obj2states = metadata['State']

    skill_name = skill_data["skill_name"]

    scenes = []
    j = 0

    pbar = tqdm(ncols=100)

    for template in skill_data['templates']:

        for i in range(args.n_repeat):
            objA = None

            for target_shape in metadata['Shape']:

                text = copy.deepcopy(template)

                if '<objA>' in template:
                    objA = target_shape
                    
                    if 'human' in objA:
                        objA = metadata["characters"][objA][0]
                        text = text.replace('<objA>', 'human')
                    else:
                        text = text.replace('<objA>', objA)

                text = camel_to_snake(text)

                assert skill_data['n_objects'] == 1

                objects = []

                for obj_id in range(skill_data['n_objects']):

                    scale = random.uniform(
                        args.min_object_scale,
                        args.max_object_scale)

                    obj = {
                        "id": obj_id,
                        "shape": objA,
                        "color": "plain",
                        "relation": None,
                        "scale": scale,
                        "texture": "plain",
                        "rotation": None,
                        "state": None
                    }

                    if target_shape in obj2states:
                        obj['state'] = obj2states[target_shape][0]

                    objects.append(obj)

                scene = {
                    'id': f"{skill_name}_{split}_{str(j).zfill(5)}",
                    'scene': "empty"
                }
                scene['text'] = text
                scene['skill'] = skill_name
                scene['split'] = split
                scene['objects'] = objects
                scenes.append(scene)
                j += 1

                desc = f'template: {template}'
                pbar.set_description(desc)
                pbar.update(1)
    pbar.close()

    return scenes


def generate_count_scene(args, metadata, skill_data, split='train'):
    obj2states = metadata['State']

    skill_name = skill_data["skill_name"]

    scenes = []
    j = 0

    pbar = tqdm(ncols=100)

    for template in skill_data['templates']:

        # "a photo of <N> <objA>s"

        for i in range(args.n_repeat):

            n_object_range = skill_data['n_objects'][0], skill_data['n_objects'][1]

            for n_obj in range(n_object_range[0], n_object_range[1]+1):

                objA = None

                for target_shape in metadata['Shape']:

                    text = copy.deepcopy(template)

                    if '<objA>' in template:
                        objA = target_shape
                        
                        if 'human' in objA:
                            objA = metadata["characters"][objA][0]
                            text = text.replace('<objA>', 'human')
                        else:
                            text = text.replace('<objA>', objA)

                    if '<N>' in template:
                        text = text.replace('<N>', str(n_obj))

                    text = camel_to_snake(text)

                    objects = []

                    for obj_id in range(n_obj):

                        scale = random.uniform(
                            args.min_object_scale,
                            args.max_object_scale)

                        obj = {
                            "id": obj_id,
                            "shape": objA,
                            "color": "plain",
                            "relation": None,
                            "scale": scale,
                            "texture": "plain",
                            "rotation": None,
                            "state": None
                        }

                        if target_shape in obj2states:
                            obj['state'] = obj2states[target_shape][0]

                        objects.append(obj)

                    scene = {
                        'id': f"{skill_name}_{split}_{str(j).zfill(5)}",
                        'scene': "empty"
                    }
                    scene['text'] = text
                    scene['skill'] = skill_name
                    scene['split'] = split
                    scene['objects'] = objects
                    scenes.append(scene)
                    j += 1

                    desc = f'template: {template}'
                    pbar.set_description(desc)
                    pbar.update(1)
    pbar.close()

    return scenes

def generate_color_scene(args, metadata, skill_data, split='train'):
    color_names = metadata['Color']

    obj2states = metadata['State']

    skill_name = skill_data["skill_name"]

    scenes = []
    j = 0

    pbar = tqdm(ncols=100)

    for template in skill_data['templates']:

        # "a photo of <N> <objA>s"

        for i in range(args.n_repeat):

            for color in color_names:

                objA = None

                for target_shape in metadata['Shape']:

                    text = copy.deepcopy(template)

                    if '<objA>' in template:
                        objA = target_shape
                        
                        if 'human' in objA:
                            objA = metadata["characters"][objA][0]
                            text = text.replace('<objA>', 'human')
                        else:
                            text = text.replace('<objA>', objA)


                    if '<color>' in template:
                        text = text.replace('<color>', color)

                    text = camel_to_snake(text)

                    n_obj = 1

                    objects = []

                    for obj_id in range(n_obj):

                        scale = random.uniform(
                            args.min_object_scale,
                            args.max_object_scale)

                        obj = {
                            "id": obj_id,
                            "shape": objA,
                            "color": color,
                            "relation": None,
                            "scale": scale,
                            # "texture": "plain",
                            "texture": "pure",
                            "rotation": None,
                            "state": None
                        }

                        if target_shape in obj2states:
                            obj['state'] = obj2states[target_shape][0]

                        objects.append(obj)

                    scene = {
                        'id': f"{skill_name}_{split}_{str(j).zfill(5)}",
                        'scene': "empty"
                    }
                    scene['text'] = text
                    scene['skill'] = skill_name
                    scene['split'] = split
                    scene['objects'] = objects
                    scenes.append(scene)
                    j += 1

                    desc = f'template: {template}'
                    pbar.set_description(desc)
                    pbar.update(1)
    pbar.close()

    return scenes

def generate_spatial_scene(args, metadata, skill_data, split='train'):
    
    relation_names = metadata['Relation']

    obj2states = metadata['State']

    skill_name = skill_data["skill_name"]

    scenes = []
    j = 0

    pbar = tqdm(ncols=100)

    for template in skill_data['templates']:

        # "a photo of <N> <objA>s"

        for i in range(args.n_repeat):
            for relation in relation_names:

                objA = None

                for objA_shape in metadata['Shape']:

                    for objB_shape in metadata['Shape']:

                        text = copy.deepcopy(template)

                        if '<objA>' in template:
                            objA = objA_shape
                            
                            if 'human' in objA:
                                objA = metadata["characters"][objA][0]
                                text = text.replace('<objA>', 'human')
                            else:
                                text = text.replace('<objA>', objA)

                        if '<objB>' in template:
                            objB = objB_shape
                            
                            if 'human' in objB:
                                objB = metadata["characters"][objB][0]
                                text = text.replace('<objB>', 'human')
                            else:
                                text = text.replace('<objB>', objB)

                        if '<rel>' in template:
                            text = text.replace('<rel>', relation)

                            text = text.replace('left', 'left to')
                            text = text.replace('right', 'right to')


                        text = camel_to_snake(text)


                        n_obj = 2

                        objects = []

                        for obj_id in range(n_obj):

                            scale = random.uniform(
                                args.min_object_scale,
                                args.max_object_scale)

                            if obj_id == 0:
                                _relation = None
                                shape = objA_shape
                            else:
                                _relation = f"{relation}_0"
                                shape = objB_shape

                            obj = {
                                "id": obj_id,
                                "shape": shape,
                                "color": "plain",
                                "relation": _relation,
                                "scale": scale,
                                "texture": "plain",
                                "rotation": None,
                                "state": None
                            }

                            if shape == 'human':
                                obj['shape'] = metadata["characters"][shape][0]

                            if shape in obj2states:
                                obj['state'] = obj2states[shape][0]

                            objects.append(obj)

                        scene = {
                            'id': f"{skill_name}_{split}_{str(j).zfill(5)}",
                            'scene': "empty"
                        }
                        scene['text'] = text
                        scene['skill'] = skill_name
                        scene['split'] = split
                        scene['objects'] = objects
                        scenes.append(scene)
                        j += 1

                        desc = f'template: {template}'
                        pbar.set_description(desc)
                        pbar.update(1)
    pbar.close()

    return scenes

def main(args):
    if args.use_gpu:
        if args.distributed:
            print(f'Use GPU #{args.rank}')

    output_dir = Path(args.output_dir)

    if not output_dir.is_dir():
        output_dir.mkdir(parents=True)

    with open(args.metadata_path) as f:
        metadata = json.load(f)

    skill_config_path = Path(args.skill_config_dir)
    with open(skill_config_path.joinpath(args.skill_name).with_suffix('.json')) as f:
        skill_config = json.load(f)

    split = args.split

    if args.skill_name == 'object':
        scenes = generate_object_scene(
            args,
            metadata,
            skill_config,
            split
        )
    elif args.skill_name == 'count':
        scenes = generate_count_scene(
            args,
            metadata,
            skill_config,
            split
        )
    elif args.skill_name == 'color':
        scenes = generate_color_scene(
            args,
            metadata,
            skill_config,
            split
        )
    elif args.skill_name == 'spatial':
        scenes = generate_spatial_scene(
            args,
            metadata,
            skill_config,
            split
        )
    print(f'Generated {split} scenes: # {len(scenes)}')

    scene_file_name = f'{args.skill_name}_{split}.json'
    scene_path = output_dir.joinpath(scene_file_name)

    with open(scene_path, 'w') as f:
        json.dump({"data": scenes}, f, indent=4)
    print(f'Dumped {split} scene json file at {scene_path}')

    if args.world_size > 1:
        output_dir.joinpath('splits').mkdir(exist_ok=True)
        for i in range(1, args.world_size+1):
            scene_file_name = f'{args.skill_name}_{split}_{i}.json'
            scene_path = output_dir.joinpath('splits', scene_file_name)

            scene_subset = scenes[i % args.world_size::args.world_size]
            with open(scene_path, 'w') as f:
                json.dump({"data": scene_subset}, f, indent=4)
        print(f'Dumped {args.world_size} scene json files')


def get_parser():
    parser = argparse.ArgumentParser()

    parser.add_argument('--use_gpu', action='store_true')
    parser.add_argument('--scene_only', action='store_true')
    parser.add_argument('--render_only', action='store_true')

    parser.add_argument('--distributed', action='store_true')
    parser.add_argument('--rank', type=int, default=-1)
    parser.add_argument('--world_size', type=int, default=1)

    parser.add_argument('--metadata_path', type=str,
                        default='./data/metadata.json')

    parser.add_argument('--skill_config_dir', type=str,
                        default='./data/skills/')
    parser.add_argument('--output_dir', type=str, default='./output/')


    parser.add_argument('--skill_name', type=str, default='object')
    parser.add_argument('--split', type=str, default='train')

    parser.add_argument('--n_repeat', type=int, default=1)

    # Scene parameters
    parser.add_argument('--min_object_scale', type=int, default=4)
    parser.add_argument('--max_object_scale', type=int, default=10)

    return parser


if __name__ == '__main__':
    parser = get_parser()
    args = parser.parse_args()

    if args.rank in [0, -1]:
        print(args)
    main(args)
