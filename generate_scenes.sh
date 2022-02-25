
# 'object'
skill_name=$1
N_process=$2
task=$3

output_dir="./$skill_name"

output_dir=`realpath $output_dir`

scene_output_dir="$output_dir/scenes"
image_output_dir="$output_dir/images"

docker_image="latest"
echo $docker_image

# split='train'
# for split in val test train

min_object_scale=8
max_object_scale=15

# for split in val
# for split in val test train
# for split in val test
for split in val train
# for split in train
do
    if [[ $skill_name = 'count' ]];then

        min_object_scale=8
        max_object_scale=12

        if [[ $split = 'train' ]];then
            n_repeat=300
        else
            n_repeat=30
        fi

    elif [[ $skill_name = 'object' ]];then
        if [[ $split = 'train' ]];then
            n_repeat=1000
        else
            n_repeat=50
        fi
    elif [[ $skill_name = 'color' ]];then
        if [[ $split = 'train' ]];then
            n_repeat=200
        else
            n_repeat=20
        fi
    elif [[ $skill_name = 'spatial' ]];then

        if [[ $split = 'train' ]];then
            n_repeat=16
            # n_repeat=3
        else
            n_repeat=2
            # n_repeat=1
        fi
    fi

    case $task in
        'scene')
            # Generate scene config
            python generate_scenes.py \
                --skill_name $skill_name \
                --split $split \
                --n_repeat $n_repeat \
                --world_size $N_process \
                --output_dir $scene_output_dir \
                --min_object_scale $min_object_scale \
                --max_object_scale $max_object_scale
            ;;

        'image')
            # Generate images
            echo $docker_image
            for i in `seq 1 $N_process`
            do
                docker run --rm -i -d -t \
                    --name "$skill_name"_"$split"_"$i" \
                    -v "$scene_output_dir/splits:/home/sim/scenes/" \
                    -v "$image_output_dir:/home/sim/output/" \
                    paintskillssim:$docker_image \
                    --input /home/sim/scenes/"$skill_name"_"$split"_"$i".json \
                    --savepath "/home/sim/output/"
            done
            ;;
        'box')
            # Generate bounding box
            python generate_bounding_boxes.py \
                --skill_name $skill_name \
                --split $split
            ;;
        *)
            echo -n "task unknown"
            ;;
    esac
done
