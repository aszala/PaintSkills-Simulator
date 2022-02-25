using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimatedCharacter {

    public List<string> modelNames;
    public List<RuntimeAnimatorController> clips;

}

[System.Serializable]
public class AnimationSerializable {

    public List<AnimatedCharacter> characters;
    public List<string> clipNames;

}

[System.Serializable]
public class CharacterAnimationDict {
    public List<AnimationSerializable> animations;
}
