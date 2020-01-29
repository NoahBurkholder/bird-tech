/// Bird by Example codex thumbnailing script. Takes candid photos of birds during gameplay.
/// 
/// Noah James Burkholder 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


/// <summary>
/// This class takes passport photos for each bird using a secondary invisible camera.
/// Pictures are then exported to disk with an ID that ties them to a bird.
/// These thumbnails will then be used in a type of Pokedex system, but for birds.
/// </summary>
public class Thumbnailer : MonoBehaviour
{
    // Singleton code...

    private static Thumbnailer _instance;

    public static Thumbnailer instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (Thumbnailer)FindObjectOfType(typeof(Thumbnailer));

                if (_instance == null)
                {
                    Debug.LogError("An instance of " + typeof(Thumbnailer) +
                        " is needed in the scene, but there is none.");
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Subclass for queuing photo ops with a sort of signup sheet.
    /// </summary>
    public class ThumbnailData
    {
        public Bird bird;
        public Transform lockOn;
        public string label;
        public ThumbnailData(Bird b, string l, Transform t)
        {
            bird = b;
            lockOn = t;
            label = l;
        }
    }

    // For the camera capture -> image exportation pipeline.
    public RenderTexture rt; // Directly written to from camera.
    public Texture2D t2d; // Processes rendered picture to be ready for export.

    // Thumbnail dimensions.
    private readonly int THUMB_WIDTH = 500;
    private readonly int THUMB_HEIGHT = 500;

    // Reference to the invisible camera which teleports around taking photos.
    public Camera thumbCamera;

    // Variables for the camera offset.
    private Transform lockOn;
    private Vector3 positionOffset = Vector3.forward;
    private Vector3 rotationOffset = (Vector3.up * 180);

    // Photo op queue. TODO: Check that bird is still alive.
    private List<ThumbnailData> photoQueue; 
    public void Awake()
    {
        // Initialize values.
        photoQueue = new List<ThumbnailData>();
        rt.width = THUMB_WIDTH;
        rt.height = THUMB_HEIGHT;
        rt.format = RenderTextureFormat.DefaultHDR;

        // Start listening for photo bookings.
        StartCoroutine(SnapRoutine());
    }

    /// <summary>
    /// A coroutine which makes a constant trickle of photoshoots for birds.
    /// </summary>
    public IEnumerator SnapRoutine()
    {
        while (true) {
            if (GameManager.IsInGameState(GameManager.GameState.Playing)) // While the game is playing...
            {
                if (photoQueue.Count > 0) // If there are birds waiting to have their photo taken... 
                {
                    if (thumbCamera != null) // And the camera is assigned...
                    {
                        // Make a random offset.
                        positionOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), Random.Range(0.8f, 3f));
                        rotationOffset = new Vector3(Mathf.Atan2(positionOffset.y, positionOffset.z), Mathf.Atan2(-positionOffset.x, -positionOffset.z), Random.Range(-0.5f, 0.5f)) / Mathf.Deg2Rad;
                        positionOffset += Vector3.up * (positionOffset.z * -0.1f);

                        // Make the camera a child of the lock-on point.
                        lockOn = photoQueue[0].lockOn;
                        thumbCamera.transform.SetParent(lockOn);

                        // Assign the new offset to the camera within the parent's transform matrix.
                        thumbCamera.transform.localPosition = positionOffset;
                        thumbCamera.transform.localEulerAngles = rotationOffset;

                        // Make sure the thumb camera prints to the correct RenderTexture.
                        thumbCamera.targetTexture = rt;
                        
                        // Wait a half second to avoid over-burdening the CPU.
                        yield return GameManager.Wait05;

                        // Now that it's had time to catch up, render to the texture.
                        thumbCamera.Render();

                        // Wait another frame.
                        yield return GameManager.WaitFrame;

                        // Initialize the Texture2D with a certain format and dimension.
                        t2d = new Texture2D(THUMB_WIDTH, THUMB_HEIGHT, TextureFormat.RGBAFloat, false, true);
                        
                        // Make this rendertexture the active one.
                        RenderTexture.active = rt;

                        // Read from the active RenderTexture to the Texture2D.
                        t2d.ReadPixels(new Rect(0, 0, THUMB_WIDTH, THUMB_HEIGHT), 0, 0, false);
                        t2d.Apply();

                        // Save to disk.
                        SaveThumbnail(photoQueue[0].label);

                        // Release active RenderTexture.
                        RenderTexture.active = null;

                        yield return GameManager.WaitFrame;

                        // Dispose of Texture2D properly.
                        Destroy(t2d);

                        // Bump the queue.
                        photoQueue.Remove(photoQueue[0]);
                        
                    }
                }
            }
            yield return GameManager.Wait05; // Wait half a second...
        }
    }

    /// <summary>
    /// Used elsewhere in the game to queue a bird for a headshot.
    /// </summary>
    public static void QueueSnap (Bird bird, string path, Transform lockOn) {
        Debug.Log("Bird " + path + " queued for headshot.");
        ThumbnailData newSnap = new ThumbnailData(bird, path, lockOn);
        instance.photoQueue.Add(newSnap);
    }

    /// <summary>
    /// Saves the bird's headshot to the disk to save between sessions.
    /// </summary>
    public void SaveThumbnail(string path)
    {
        File.WriteAllBytes(path, t2d.EncodeToPNG());
    }
}
