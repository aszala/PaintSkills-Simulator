import json
from os import stat
from unicodedata import name
import cv2
from tqdm import tqdm
import glob
import numpy as np
import argparse

from pathlib import Path

from multiprocessing import Pool

from pprint import pformat
import numba
from numba import jit

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--skill_name', type=str, default='object')
    parser.add_argument('--split', type=str, default='train')
    parser.add_argument('--metadata_path', type=str, default='data/metadata.json')
    args = parser.parse_args()
    print(args)

    skill_dir = Path("../../datasets/PaintSkills/").joinpath(args.skill_name)
    image_dir = skill_dir.joinpath("images")

    print(skill_dir)
    print(image_dir)

    scene_path = skill_dir.joinpath(f'scenes/{args.skill_name}_{args.split}.json')
    scene_data = json.load(open(scene_path))["data"]

    id2scene = {}
    for scene in scene_data:
        id2scene[scene['questionId'].split('_')[-1]] = scene

    print('id2scene size:', len(id2scene))

    metadata = json.load(open(args.metadata_path))

    # Category
    categories = []
    category_dict = {}
    for i, name in enumerate(metadata['Shape']):
        if name == 'human':
            name = 'humanSophie'

        categories.append({
            "id": i,
            "name": name
        })

        category_dict[name] = i

    json_files = glob.glob(f"{image_dir}/segmentation_data_{args.skill_name}_{args.split}_*.json")

    print(next(iter(json_files)))


    # images
    images = []
    desc = f"Skill: {args.skill_name} | Split: {args.split} - Images"
    for json_file in tqdm(json_files, desc=desc):
        with open(json_file, 'r') as f:
            data = json.load(f)

        segmentation_image = json_file.replace("segmentation_data_", "segmentation_image_").replace(".json", ".png")

        h, w = 720, 720

        image_path = segmentation_image.replace("segmentation_image_", "image_")
        file_name = Path(image_path).name
        image_id = file_name.split('.')[0].split('_')[-1]

        image_dict = {
            'file_name': file_name,
            'height': h,
            'width': w,
            'id': int(image_id),
        }
        images.append(image_dict)

    @jit(nopython=True)
    def computeBounds(im, count):
        temp_anno = []

        for i in range(count):
            temp_anno.append([1000000, 100000, 0, 0])

        for y in range(im.shape[0]):
            for x in range(im.shape[1]):
                idx = int(im[y, x, 2])
                if idx == 0:
                    continue

                if x < temp_anno[idx][0]:
                    temp_anno[idx][0] = x
                if x > temp_anno[idx][2]:
                    temp_anno[idx][2] = x

                if y < temp_anno[idx][1]:
                    temp_anno[idx][1] = y
                if y > temp_anno[idx][3]:
                    temp_anno[idx][3] = y
        return temp_anno

    # annotations
    box_id = 0
    annotations = []
    desc = f"Skill: {args.skill_name} | Split: {args.split} - Annotations"
    for json_file in tqdm(json_files, desc=desc):

        with open(json_file, 'r') as f:
            data = json.load(f)

        segmentation_image = json_file.replace("segmentation_data_", "segmentation_image_").replace(".json", ".png")

        im = cv2.imread(segmentation_image)
        h, w, _ = im.shape

        image_path = segmentation_image.replace(
            "segmentation_image_", "image_")
        file_name = Path(image_path).name
        image_id = file_name.split('.')[0].split('_')[-1]


        scene = id2scene[image_id]

        temp_anno = []
        temp_anno.append(
            {"name": "background", "all_points_x": [], "all_points_y": []})

        # data
        # {'airplane0': [1, 161, 131, ['airplane']],
        # 'bird-flying1': [2, 64, 93, ['bird', 'flying']]}
        for i, x in enumerate(data):
            try:
                state = data[x][3][1]
            except:
                state = None

            shape = data[x][3][0]

            try:
                assert scene['objects'][i]['shape'] == shape
            except AssertionError:
                print(pformat(scene, indent=4))
                print(scene['objects'][i]['shape'], shape)
                print(data)
                print(i)
                print(json_file)
                exit()

            temp_anno.append({
                "category_id": category_dict[shape],
                "image_id": int(image_id),
                "id": box_id,
                "shape": shape,
                "state": state,
                "iscrowd": 0,
                "all_points_x": [],
                "all_points_y": [],
                "color": scene['objects'][i]['color'],
                "texture": scene['objects'][i]['texture'],
                }
            )
            box_id += 1

        temp = computeBounds(im, len(temp_anno))
        for i in range(len(temp_anno)):
            temp_anno[i]["bbox"] = [temp[i][0],
                                    temp[i][1],
                                    temp[i][2] - temp[i][0],
                                    temp[i][3] - temp[i][1]]

            temp_anno[i]['area'] = temp_anno[i]["bbox"][2] * temp_anno[i]["bbox"][3]

        temp_anno = temp_anno[1:]
        annotations.extend(temp_anno)

    with open(skill_dir.joinpath(f"{args.skill_name}_{args.split}_bounding_boxes.json"), 'w') as f:
        json.dump({
            "annotations": annotations,
            "images": images,
            "categories": categories
        }, f, indent=4)
