using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public class GenerateScene : MonoBehaviour {

    public Vector2[] Triangle1, Triangle2, Triangle3;

    public List<Vector2> rectangleGrid;
    public float rectangleHalfWidth, rectangleHalfHeight;
    public float rectangleEdgeHalfWidth, rectangleEdgeHalfHeight;

    public Vector2 center;
    public float centerLength;

    private Rectangle centerCube;

    private List<Triangle> triangles;
    private List<Rectangle> gridRectangles;

    private float relationAngle = 45.0f;
    private float relationMaxLength = 8, relationMinLength = 2;

    private float nearDistance = 2, farDistance = 5;

    public Material unLit, semanticSky;
    
    public List<string> obj_names = new List<string>();
    public List<GameObject> objs = new List<GameObject>();

    private Dictionary<string, GameObject> name_obj_map = new Dictionary<string, GameObject>();


    public List<string> scene_names = new List<string>();
    public List<Material> scenes = new List<Material>();

    private Dictionary<string, Material> name_scene_map = new Dictionary<string, Material>();

    public List<string> texture_names = new List<string>();
    public List<Texture> textures = new List<Texture>();

    private Dictionary<string, Texture> name_texture_map = new Dictionary<string, Texture>();

    public List<string> color_names = new List<string>();
    public Color[] colors;

    public AnimationSerializable characterAnimations;

    public GameObject referenceCube;
    public GameObject parent;

    private Dictionary<string, Color> name_color_map = new Dictionary<string, Color>();

    private System.Random sysRandom = new System.Random();

    private List<Color> all_colors = new List<Color>();

    /*
     * Inital Prep needed before scene generation
     * Map values to related strings
     */
    private void Awake() {
        for (int i=0;i<255;i++) {
            all_colors.Add(new Color(i / 255.0f, Random.value, Random.value));
        }

        all_colors = all_colors.OrderBy(a => sysRandom.Next()).ToList();

        for (int i=0;i<obj_names.Count;i++) {
            name_obj_map.Add(obj_names[i], objs[i]);
        }

        for (int i = 0; i < color_names.Count; i++) {
            name_color_map.Add(color_names[i], colors[i]);
        }

        for (int i = 0; i < scene_names.Count; i++) {
            name_scene_map.Add(scene_names[i], scenes[i]);
        }

        texture_names.Add("plain");
        textures.Add(null);

        for (int i = 0; i < texture_names.Count; i++) {
            name_texture_map.Add(texture_names[i], textures[i]);
        }

        triangles = new List<Triangle>();
        triangles.Add(new Triangle(Triangle1));
        triangles.Add(new Triangle(Triangle2));
        triangles.Add(new Triangle(Triangle3));

        gridRectangles = new List<Rectangle>();

        for (int i=0;i<rectangleGrid.Count;i++) {
            Vector2 center = rectangleGrid[i];

            if (i == 0 || i == rectangleGrid.Count - 1) {
                gridRectangles.Add(new Rectangle(center, rectangleEdgeHalfWidth, rectangleEdgeHalfHeight, -7));
            } else {
                gridRectangles.Add(new Rectangle(center, rectangleHalfWidth, rectangleHalfHeight, -7));
            }
        }

        centerCube = new Rectangle(center, centerLength, centerLength, 0.5f);
    }

    public void generate(InputData data, int i, List<string> swaps) {
        StartCoroutine(spawnObjects(data, i, swaps));
       
    }

    /*
     * Create a scene with the given data
     * 
     * First get general scene information or use defaults
     * Then for each object
     * - Set the object properties based on the input
     * - Determine object positioning and check for collisions
     * Then render the scene
     * Then generate the segmentation map of the scene, where the each object is unqiue based on the R value from its RGB value.
     */
    public IEnumerator spawnObjects(InputData data, int imageID, List<string> swaps) {
        Dictionary<int, GameObject> id_pairs = new Dictionary<int, GameObject>();
        List<Rectangle> remainingGridSpots = new List<Rectangle>(gridRectangles);

        string scene;

        if (data.scene == null || data.scene.Length == 0) {
            scene = scene_names[sysRandom.Next(0, scene_names.Count)];
        } else if (data.scene.Contains("NOT")) {
            List<string> remaining_scenes = new List<string>();

            List<string> bad_ones = data.scene.Split('_').ToList<string>();

            foreach (string s in scene_names) {
                if (!bad_ones.Contains(s)) {
                    remaining_scenes.Add(s);
                }
            }

            scene = remaining_scenes[sysRandom.Next(0, remaining_scenes.Count)];
        } else {
            scene = data.scene;
        }


        if (!data.scene.Equals("empty")) {
            Material sceneMat = name_scene_map[scene];
            RenderSettings.skybox = sceneMat;
        } else {
            RenderSettings.skybox = semanticSky;
        }

        foreach (ObjectData od in data.objects) {
            if (od.shape == null || od.shape.Length == 0) {
                od.shape = obj_names[sysRandom.Next(0, obj_names.Count)];
            }

            RuntimeAnimatorController rac = null;

            string shape = od.shape;
            if (od.state != null && od.state.Length != 0) {
                if (od.shape.Contains("human")) {
                    string name = od.shape.Replace("human", "");

                    int index = -1;

                    for (int i=0;i<characterAnimations.characters.Count;i++) {
                        AnimatedCharacter ac = characterAnimations.characters[i];

                        if (ac.modelNames.Contains(name)) {
                            index = i;
                            break;
                        }
                    }

                    int animationIndex = characterAnimations.clipNames.IndexOf(od.state);

                    if (index == -1 || animationIndex == -1) {
                        throw new System.Exception("Cannot find model or animation: " + name + ", " + od.state);
                    }

                    RuntimeAnimatorController[] rac_copy = new RuntimeAnimatorController[characterAnimations.characters[index].clips.Count];
                    characterAnimations.characters[index].clips.CopyTo(rac_copy);
                    rac = rac_copy[animationIndex];
                } else {
                    shape = od.shape + "-" + od.state;
                }
            }

            GameObject x = Instantiate(name_obj_map[shape]);
            x.name = shape;

            if (rac != null) {
                x.name += "-" + od.state;
                x.GetComponent<Animator>().runtimeAnimatorController = rac;
            }

            x.name += od.id;

            x.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY
                                                    | RigidbodyConstraints.FreezeRotationZ| RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

            if (od.scale == 0) {
                x.transform.localScale *= Random.Range(1.0f, 3.0f);
            } else {
                x.transform.localScale *= od.scale;
            }

            x.transform.parent = parent.transform;

            if (od.rotation == null) {
                x.transform.rotation = Quaternion.Euler(new Vector3(x.transform.rotation.eulerAngles.x, Random.rotation.eulerAngles.y, x.transform.rotation.eulerAngles.z));
            } else  {
                x.transform.Rotate(od.rotation[0], od.rotation[1], od.rotation[2]);
            }


            for (int i = 0; i < 5; i++) {
                yield return new WaitForFixedUpdate();
            }

            Color color = new Color(0,0,0);

            if (od.color == null || od.color.Length == 0) {
                color = colors[sysRandom.Next(0, colors.Length)];
            } else if (od.color.Contains("NOT")) {
                List<Color> remaining_colors = new List<Color>();
                List<string> bad_colors = new List<string>();

                string[] bad_color_array = od.color.Split('_');

                for (int i=1;i<bad_color_array.Length;i++) {
                    bad_colors.Add(bad_color_array[i]);
                }

                foreach (string name in name_color_map.Keys) {
                    if (!bad_colors.Contains(name)) {
                        remaining_colors.Add(name_color_map[name]);
                    }
                }


                color = remaining_colors[sysRandom.Next(0, remaining_colors.Count)];
            } else if (!od.color.Equals("plain")) {
                color = name_color_map[od.color];
            }

            Texture texture;
            if (od.texture == null || od.texture.Length == 0) {
                texture = textures[sysRandom.Next(0, textures.Count)];
            } else if (od.texture.Contains("NOT")) {
                List<string> remaining_textures = new List<string>();

                List<string> bad_ones = od.texture.Split('_').ToList<string>();
                
                foreach (string t in texture_names) {
                    if (!bad_ones.Contains(t)) {
                        remaining_textures.Add(t);
                    }
                }

                texture = name_texture_map[remaining_textures[sysRandom.Next(0, remaining_textures.Count)]];
            } else {
                texture = name_texture_map[od.texture];
            }

            if (od.color == null)
                od.color = "";

            ObjectInfo oi = x.GetComponent<ObjectInfo>();

            if (!oi.hasMaterialOnChildren) {
                if (!od.color.Equals("plain"))
                    setMaterialColor(x, color);

                if (texture != null)
                    setMaterialTexture(x, texture);
            } else {
                if (!od.color.Equals("plain"))
                    TraverseHierarchy(x.transform, color);

                if (texture != null)
                    TraverseHierarchy(x.transform, texture);
            }

            Dictionary<string, Shape> relation_pairs = new Dictionary<string, Shape>();
            List<string> directions = new List<string>();

            if (od.relation != null && od.relation.Length != 0) {
                string[] relationInfo = od.relation.Split('_');

                GameObject relationObject = id_pairs[System.Convert.ToInt32(relationInfo[relationInfo.Length-1])];

                if (relationInfo[0].Equals("NOT")) {
                    directions.AddRange(new string[] { "left", "right", "behind", "front", "above", "near", "far" });

                    for (int i = 1; i < relationInfo.Length - 1; i++) {
                        directions.Remove(relationInfo[i]);
                    }
                } else {
                    for (int i = 0; i < relationInfo.Length - 1; i++) {
                        directions.Add(relationInfo[i]);
                    }
                }


                relationMinLength = od.scale * 2;

                if (directions.Contains("left")) {
                    if (data.objects.Count == 2) {
                        relationObject.transform.position = triangles[2].randomPoint();
                    }

                    /*
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x - relationMinLength, relationObject.transform.position.z);

                    relation_pairs.Add("left", new Triangle(relationObjectPos, 2, relationAngle, relationMaxLength));
                    */

                    relation_pairs.Add("left", triangles[0]);
                }
                
                if (directions.Contains("right")) {
                    if (data.objects.Count == 2) {
                        relationObject.transform.position = triangles[0].randomPoint();
                    }

                    /*
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x + relationMinLength, relationObject.transform.position.z);

                    relation_pairs.Add("right", new Triangle(relationObjectPos, 0, relationAngle, relationMaxLength));
                    */

                    relation_pairs.Add("right", triangles[2]);
                }
                
                if (directions.Contains("behind")) {
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x, relationObject.transform.position.z + relationMinLength);

                    relation_pairs.Add("behind", new Triangle(relationObjectPos, 1, relationAngle, relationMaxLength));
                }
                
                if (directions.Contains("front")) {
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x, relationObject.transform.position.z - relationMinLength);

                    relation_pairs.Add("front", new Triangle(relationObjectPos, 3, relationAngle, relationMaxLength));
                }

                if (directions.Contains("near")) {
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x, relationObject.transform.position.z);

                    relation_pairs.Add("near", new InvertedCircle(relationObjectPos, relationMinLength, nearDistance));
                }

                if (directions.Contains("far")) {
                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x, relationObject.transform.position.z);

                    relation_pairs.Add("far", new InvertedCircle(relationObjectPos, nearDistance, farDistance));
                }

                if (directions.Contains("above")) {
                    if (data.objects.Count == 2) {
                        relationObject.transform.position = new Vector3(0, -12, 0);
                    }

                    Vector2 relationObjectPos = new Vector2(relationObject.transform.position.x, relationObject.transform.position.z);
                    float depth = relationObject.transform.position.y;

                    relation_pairs.Add("above", new Rectangle(relationObjectPos, relationObject.GetComponent<BoxCollider>().bounds.extents.x,
                                                                relationObject.GetComponent<BoxCollider>().bounds.extents.z,
                                                                depth +
                                                                relationObject.GetComponent<BoxCollider>().bounds.extents.y +
                                                                x.GetComponent<BoxCollider>().bounds.extents.y +
                                                                relationObject.GetComponent<BoxCollider>().bounds.center.y +
                                                                relationMinLength/4));
                }
            }

            int collidedCount = 0;
            const int attemptCount = 100;

            Vector3 position;
            int rectangleGridIndex = -1;

            int attempts = 0;
            do {
                if (od.relation == null || od.relation.Length == 0) {
                    if (data.objects.Count == 1 || (data.skill.ToLower().Contains("spatial") && id_pairs.Count == 0)) {
                        position = new Vector3(0, 0.5f, 0);

                    } else {
                        if (data.skill.ToLower().Contains("count")) {
                            rectangleGridIndex = Random.Range(0, remainingGridSpots.Count);

                            Shape s = remainingGridSpots[rectangleGridIndex];

                            position = s.randomPoint();

                        } else {
                            Shape t = triangles[Random.Range(0, triangles.Count)];

                            position = t.randomPoint();
                        }
                    }

                } else {
                    string relation = directions[sysRandom.Next(0, directions.Count)];
                    Shape s = relation_pairs[relation];

                    position = s.randomPoint();
                }
                x.transform.position = position;

                yield return new WaitForFixedUpdate();


                for (int i = 0; i < parent.transform.childCount; i++) {
                    GameObject a = parent.transform.GetChild(i).gameObject;

                    if (!a.name.Equals(x.name)) {
                        Collider[] colliders = new Collider[a.GetComponent<ObjectInfo>().colliders.Count];

                        a.GetComponent<ObjectInfo>().colliders.CopyTo(colliders);
                        
                        collidedCount = a.GetComponent<ObjectInfo>().colliders.Count;

                        if (collidedCount > 0) {
                            break;
                        }
                    }
                }

                yield return new WaitForFixedUpdate();

                attempts++;

                if (attempts > attemptCount) {
                    break;
                }
            } while (collidedCount > 0);

            if (rectangleGridIndex != -1) {
                remainingGridSpots.RemoveAt(rectangleGridIndex);
            }

            x.transform.position = position;

            x.isStatic = true;

            id_pairs.Add(od.id, x);

            yield return new WaitForFixedUpdate();

        }

        for (int i = 0;i<5; i++) {
            yield return new WaitForFixedUpdate();
        }

        ConnectionInterface.takeScreenshot("image_" + data.id);

        GameObject[] gameObjects = new GameObject[parent.transform.childCount];

        for (int i = 0; i < parent.transform.childCount; i++) {
            gameObjects[i] = parent.transform.GetChild(i).gameObject;
        }

        foreach (string swap in swaps) {
            string[] split = swap.Split('-');

            if (split[2].Equals(data.id)) {

                int id1 = System.Convert.ToInt32(split[0]);
                int id2 = System.Convert.ToInt32(split[1]);

                gameObjects[id1].name = gameObjects[id1].name.Substring(0, gameObjects[id1].name.Length - 1) + id2;
                gameObjects[id2].name = gameObjects[id2].name.Substring(0, gameObjects[id2].name.Length - 1) + id1;

                GameObject temp = gameObjects[id1];

                gameObjects[id1] = gameObjects[id2];
                gameObjects[id2] = temp;
            }

        }

        for (int i = 0; i < gameObjects.Length; i++) {
            GameObject x = gameObjects[i];

            ObjectInfo oi = x.GetComponent<ObjectInfo>();

            char[] chars = x.name.ToCharArray();
            string sid = chars[chars.Length - 1] + "";
            int id = System.Convert.ToInt32(sid) + 1;

            Color c = new Color(id / 255.0f, all_colors[id].g, all_colors[id].b);

            BoundingBoxInfo.boundingBoxes[x.name] = new object[4] { id, System.Convert.ToInt32(c.g * 255), System.Convert.ToInt32(c.b * 255), x.name.Substring(0, x.name.Length-1).Split('-') };

            if (!oi.hasMaterialOnChildren) {
                setMaterialMaterial(x, c);
            } else {
                TraverseHierarchySetMaterial(x.transform, c);
            }
        }
        string boundingBoxOut = JsonConvert.SerializeObject(BoundingBoxInfo.boundingBoxes);
        BoundingBoxInfo.boundingBoxes.Clear();

        File.WriteAllText(ConnectionInterface.save_directory + "/segmentation_data_" + data.id + ".json", boundingBoxOut);
        
        RenderSettings.skybox = semanticSky;
        ConnectionInterface.takeScreenshot("segmentation_image_" + data.id);
        

        for (int i=0;i<parent.transform.childCount;i++) {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
        

    }

    private void TraverseHierarchySetMaterial(Transform b, Color c) {
        for (int i = 0; i < b.childCount; i++) {
            Transform child = b.GetChild(i);

            setMaterialMaterial(child.gameObject, c);

            TraverseHierarchySetMaterial(child, c);
        }
    }

    private void setMaterialMaterial(GameObject baseObj, Color c) {
        Renderer mr = baseObj.GetComponent<MeshRenderer>();

        if (mr == null) {
            mr = baseObj.GetComponent<SkinnedMeshRenderer>();

            if (mr == null)
                return;

        }

        Material[] mats = new Material[mr.materials.Length];

        for (int i = 0; i < mats.Length; i++) {
            Material m = new Material(unLit);
            m.color = c;

            mats[i] = m;
        }

        mr.materials = mats;

    }

    private void TraverseHierarchy(Transform b, Color color) {
        for (int i=0;i<b.childCount;i++) {
            Transform child = b.GetChild(i);

            setMaterialColor(child.gameObject, color);

            TraverseHierarchy(child, color);
        }
    }

    private void setMaterialColor(GameObject baseObj, Color color) {
        Renderer mr = baseObj.GetComponent<MeshRenderer>();

        if (mr == null) {
            mr = baseObj.GetComponent<SkinnedMeshRenderer>();

            if (mr == null)
                return;

        }

        Material[] mats = mr.materials;

        for (int i=0;i<mats.Length;i++) {
            Material m = mats[i];

            m.color = color;

            mats[i] = m;
        }
    }

    private void TraverseHierarchy(Transform b, Texture texture) {
        for (int i = 0; i < b.childCount; i++) {
            Transform child = b.GetChild(i);

            setMaterialTexture(child.gameObject, texture);

            TraverseHierarchy(child, texture);
        }
    }

    private void setMaterialTexture(GameObject baseObj, Texture texture) {
        Renderer mr = baseObj.GetComponent<MeshRenderer>();
        
        if (mr == null) {
            mr = baseObj.GetComponent<SkinnedMeshRenderer>();

            if (mr == null)
                return;

        }

        if (mr == null || texture == null)
            return;

        Material[] mats = mr.materials;

        for (int i = 0; i < mats.Length; i++) {
            Material m = mats[i];

            m.mainTexture = texture;

            mats[i] = m;
        }
    }
}
