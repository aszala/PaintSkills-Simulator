using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConnectionInterface : MonoBehaviour {

	public static int imagesSaved = 0;

	public static int dimension = 720;
	public static string save_directory;

	public GenerateScene sceneManager;
	private static Camera mainCamera;

	/*
	 * 
	 * Starting point
	 * Read in parameters and begin generation process
	 * 
	 */
	void Start() {
		mainCamera = Camera.main;

		print("Initializing");

		save_directory = Application.dataPath + "/../SimOutput/";

		string[] args = Environment.GetCommandLineArgs();

		for (int i = 0; i < args.Length; i++) {
			if (args[i].Contains("--help")) {
				print("--image-size: square dimensions of output image; DEFAULT: " + dimension);
				print("--savepath: (absolute) directory to save the images in; DEFAULT: " + save_directory);
				print("--input: data for sim to process. required; DEFAULT: none");

				Application.Quit();
			}
		}

		InputDataContainer data = null;

		for (int i = 0; i < args.Length; i++) {
			if (args[i].Contains("--savepath")) {
				save_directory = args[i + 1];
			} else if (args[i].Contains("--input")) {
				data = JsonConvert.DeserializeObject<InputDataContainer>(File.ReadAllText(args[i + 1]));
			} else if (args[i].Contains("--image-size")) {
				dimension = Convert.ToInt32(args[i + 1]);
			}
		}

		print("Done.");

		// Uncomment to load file by hardcoded path
		//data = JsonConvert.DeserializeObject<InputDataContainer>(File.ReadAllText("./test.json"));

		List<string> swaps = new List<string>();

		for (int i = 0; i < data.data.Count; i++) {
			for (int j=0; j < data.data[i].objects.Count;j++) {
				string relation = data.data[i].objects[j].relation;

				if (relation != null && relation.Contains("below")) {
					string[] relationInfo = relation.Split('_');

					int idx = System.Convert.ToInt32(relationInfo[relationInfo.Length - 1]);

					ObjectData temp = data.data[i].objects[j];

					data.data[i].objects[j] = data.data[i].objects[idx];
					data.data[i].objects[idx] = temp;

					data.data[i].objects[j].id = j;
					data.data[i].objects[idx].id = idx;

					data.data[i].objects[j].relation = relation.Replace("below", "above");
					swaps.Add(j + "-" + idx + "-" + data.data[i].id);
					data.data[i].objects[idx].relation = null;
				}

			}
		}

		StartCoroutine(processData(data, swaps));
	}

	/*
	 * Generate/render scene in the data
	 * Wait for current scene to be rendered before starting next scene
	 */
	private IEnumerator processData(InputDataContainer data, List<string> swaps) {
		int totalData = data.data.Count;

		print("Processing data");
		for (int i = 0; i < totalData; i++) {
			sceneManager.generate(data.data[i], i, swaps);
			yield return new WaitUntil(() => imagesSaved == 2);
			imagesSaved = 0;
			float percent = i * 1.0f / totalData;
			print("Percent Done: " + percent);
		}
		print("Done...");

		Application.Quit();
    }

	/*
	 * Take a screenshot of the generated scene and save to image file
	 */
	public static void takeScreenshot(string name) {
		RenderTexture rt = new RenderTexture(dimension, dimension, 24);
		rt.Create();

		mainCamera.targetTexture = rt;

		Texture2D screenshot = new Texture2D(dimension, dimension, TextureFormat.RGB24, false);

		mainCamera.Render();

		RenderTexture.active = rt;
		screenshot.ReadPixels(new Rect(0, 0, dimension, dimension), 0, 0);
		screenshot.Apply();

		byte[] bytes = screenshot.EncodeToPNG();

		Destroy(RenderTexture.active);
		RenderTexture.active = null;

		mainCamera.targetTexture.Release();
		Destroy(mainCamera.targetTexture);

		mainCamera.targetTexture = null;
		RenderTexture.active = null;

		if (rt != null) {
			rt.Release();
			Destroy(rt);
		}


		Destroy(screenshot);

		File.WriteAllBytes(save_directory + "/" + name + ".png", bytes);

		imagesSaved++;
	}

}
