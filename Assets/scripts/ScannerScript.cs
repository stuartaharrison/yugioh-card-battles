/*
 * Author: Stuart Harrison 
 * Date: February 2016
 * Class for controlling and handling the card scanning functionality within
 * the main menu scene
*/

using com.google.zxing.qrcode;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Threading;

public class ScannerScript : MonoBehaviour {

	private bool loop = true;
	private Color32[] c;
	private int W, H, WxH;
	private int x, y, z;
	private object scanResult = null;
	private sbyte[] d;
	private Thread scanningThread;
	private WebCamTexture cameraTexture;

	public object Result { get { return scanResult; } }
	public WebCamTexture CameraTexture { get { return cameraTexture; } }

	void Start () {
		OnEnable ();
	}

	//Called every frame and gets the camera input
	void Update () {
		if (cameraTexture != null) {
			c = cameraTexture.GetPixels32 ();
		}
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy() {
		OnDisable ();
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable() {
		//Setup the camera and QRCode scanning thread if they're not already
		//setup
		if (cameraTexture == null && scanningThread == null) {
			loop = true;
			cameraTexture = new WebCamTexture ();
			cameraTexture.Play ();
			W = cameraTexture.width;
			H = cameraTexture.height;
			WxH = W * H;
			scanningThread = new Thread (DecodeQR);
			scanningThread.Start ();
		}
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	void OnDisable() {
		//Stop all coroutines and join threads before closing down the gameobject
		if (cameraTexture != null && scanningThread != null) {
			loop = false;
			StopAllCoroutines ();
			scanningThread.Join ();
			scanningThread = null;
			//close down the webcame
			cameraTexture.Stop ();
			cameraTexture = null;
		}
	}

	/// <summary>
	/// Decodes the QRCode if one is found in the Webcam view
	/// </summary>
	void DecodeQR() {
		while (loop) {
			try {
				if (scanResult == null) {
					d = new sbyte[WxH];
					z = 0;
					for (y = H - 1; y >= 0; y--) {
						for (x = 0; x < W; x++) {
							d[z++] = (sbyte)(((int)c[y * W + x].r) << 16 | 
								((int)c[y * W + x].g) << 8 | 
								((int)c[y * W + x].b));
						}
					}
					//Use the library to decode the result of the scan
					scanResult = new QRCodeReader().decode(d, W, H).Text;
					Debug.Log("ScannerScript: scanResult: " + scanResult);
				}
			}
			catch {
				continue;
			}
		}
	}
}