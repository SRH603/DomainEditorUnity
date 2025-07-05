using Battlehub.Storage.EditorAttributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage
{
    public class ThumbnailUtil : MonoBehaviour, IThumbnailUtil
    {
        [Layer]
        public LayerMask ThumbnailLayer;

        public virtual int Layer
        {
            get { return ThumbnailLayer.value; }
            set { ThumbnailLayer.value = value; }
        }

        public bool AllowNullTexture = false;
        public Camera Camera;
        public bool DestroyScripts = true;
        public int SnapshotTextureWidth = 128;
        public int SnapshotTextureHeight = 128;
        public Vector3 DefaultPosition = new Vector3(0, 0, 0);
        public Vector3 DefaultRotation = new Vector3(26, 135, -24);
        public Vector3 DefaultScale = new Vector3(1, 1, 1);
        private Renderer m_materialSphere;

        public bool CreateLight = true;
        public Vector3 LightRotation = new Vector3(25, -10, 0);
        public Light Light;

        protected virtual void Awake()
        {
            if (Camera == null)
            {
                Camera = GetComponent<Camera>();
                if (Camera == null)
                {
                    Camera = gameObject.AddComponent<Camera>();
                    Camera.clearFlags = CameraClearFlags.SolidColor;
                    Camera.backgroundColor = new Color(0, 0, 0, 0);
                    Camera.orthographic = true;
                    Camera.farClipPlane = 5000;
                }
            }

            if (CreateLight && Light == null)
            {
                GameObject light = new GameObject("Light");
                light.transform.SetParent(transform, false);
                light.SetActive(false);

                Light = light.AddComponent<Light>();
                Light.type = LightType.Directional;
            }

            Camera.cullingMask = 1 << ThumbnailLayer.value;
            Camera.enabled = false;

            m_materialSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<Renderer>();
            m_materialSphere.name = "Material Sphere";
            m_materialSphere.transform.SetParent(transform, false);
            m_materialSphere.gameObject.SetActive(false);
            Destroy(m_materialSphere.GetComponent<Collider>());
        }

        protected virtual void Start()
        {
            if (ThumbnailLayer == 0)
            {
                Debug.LogWarning($"Please set the {nameof(ThumbnailUtil)}.ThumbnailLayer to a value other than the Default.");
            }
        }

        public virtual Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate = true)
        {
            GameObject go;
            if (obj is GameObject)
            {
                go = (GameObject)obj;
            }
            else if (obj is Material)
            {
                m_materialSphere.sharedMaterial = (Material)obj;
                go = m_materialSphere.gameObject;
                go.SetActive(true);
                instantiate = false;
            }
            else if (obj is Texture2D)
            {
                var srcTexture = (Texture2D)obj;
                var tex = srcTexture.isReadable ?
                    Instantiate((Texture2D)obj) :
                    TextureUtils.MakeReadable(srcTexture);

                TextureScaler.Scale(tex, SnapshotTextureWidth, SnapshotTextureHeight);
                return Task.FromResult(tex);
            }
            else
            {
                return Task.FromResult<Texture2D>(null);
            }

            Texture2D texture = TakeObjectSnapshot(
                go,
                null,
                DefaultPosition,
                Quaternion.Euler(DefaultRotation),
                DefaultScale, 1,
                instantiate: instantiate);

            if (texture == null && !AllowNullTexture)
            {
                texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, new Color(1, 1, 1, 0));
                texture.Apply();
            }

            if (obj is Material)
            {
                go.SetActive(false);
            }

            return Task.FromResult(texture);
        }

        public virtual Task<byte[]> EncodeToPngAsync(Texture2D texture)
        {
            return Task.FromResult((texture != null) ? texture.EncodeToPNG() : null);
        }

        private void SetLayerRecursively(GameObject o, int layer)
        {
            foreach (Transform t in o.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }

        protected Texture2D TakeObjectSnapshot(GameObject prefab, GameObject fallback, Vector3 position, Quaternion rotation, Vector3 scale, float previewScale, bool instantiate = true)
        {
            // validate properties
            if (Camera == null)
            {
                throw new System.InvalidOperationException("Object Image Camera must be set");
            }

            if (ThumbnailLayer < 0 || ThumbnailLayer > 31)
            {
                throw new System.InvalidOperationException("Object Image Layer must specify a valid layer between 0 and 31");
            }

            bool wasActive = prefab.activeSelf;
            Vector3 prevPosition = prefab.transform.position;
            Vector3 prevEuler = prefab.transform.eulerAngles;
            Vector3 prevScale = prefab.transform.localScale;
            int prevLayer = prefab.layer;

            GameObject go;
            Renderer[] renderers;
            Transform prevParent = null;
            if (instantiate)
            {
                // clone the specified game object so we can change its properties at will, and position the object accordingly

                // prefab.SetActive(false);

                go = Instantiate(prefab, position, rotation * Quaternion.Inverse(prefab.transform.rotation));
                if (DestroyScripts)
                {
                    var withRequireComponent = new List<MonoBehaviour>();
                    MonoBehaviour[] scripts = go.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = 0; i < scripts.Length; ++i)
                    {
                        var script = scripts[i];
                        if (script == null)
                        {
                            continue;
                        }

                        var type = script.GetType();
                        if (type.FullName.StartsWith("UnityEngine"))
                        {
                            scripts[i] = null;
                        }
                        else
                        {
                            var requireComponent = type.GetCustomAttributes<RequireComponent>();
                            if (requireComponent.Any())
                            {
                                withRequireComponent.Add(script);
                            }
                        }
                    }

                    for (int i = 0; i < withRequireComponent.Count; ++i)
                    {
                        var script = withRequireComponent[i];
                        if (script == null)
                        {
                            continue;
                        }

                        try
                        {
                            DestroyImmediate(script);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e.ToString());
                        }
                    }

                    for (int i = 0; i < scripts.Length; ++i)
                    {
                        var script = scripts[i];
                        if (script == null)
                        {
                            continue;
                        }

                        DestroyImmediate(script);
                    }

                }

                prefab.SetActive(wasActive);
                renderers = go.GetComponentsInChildren<Renderer>(false);
                if (renderers.Length == 0)
                {
                    if (fallback != null)
                    {
                        DestroyImmediate(go);
                        go = Instantiate(fallback, position, rotation);
                        renderers = new[] { fallback.GetComponentInChildren<Renderer>(false) };
                    }
                }
            }
            else
            {
                go = prefab;
                go.SetActive(true);

                prevParent = go.transform.parent;
                go.transform.SetParent(null, false);
                go.transform.position = position;
                go.transform.rotation = rotation;

                renderers = go.GetComponentsInChildren<Renderer>(false);
            }


            Texture2D texture = null;
            if (renderers.Length != 0)
            {
                go.transform.localScale = scale;

                Bounds bounds = TransformUtil.CalculateBounds(go.transform);
                float fov = Camera.fieldOfView * Mathf.Deg2Rad;
                float objSize = Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z);
                float distance = Mathf.Abs(objSize / Mathf.Sin(fov / 2.0f));

                go.transform.localScale = scale * previewScale;
                go.SetActive(true);
                for (int i = 0; i < renderers.Length; ++i)
                {
                    renderers[i].gameObject.SetActive(true);
                }

                position += bounds.center;

                Camera.transform.position = position - distance * Camera.transform.forward;
                Camera.orthographicSize = objSize;

                // set the layer so the render to texture camera will see the object 
                SetLayerRecursively(go, ThumbnailLayer);
                Camera.cullingMask = 1 << ThumbnailLayer.value;

                if (Light != null)
                {
                    Light.gameObject.SetActive(true);
                    Light.cullingMask = 1 << ThumbnailLayer.value;
                    Light.transform.eulerAngles = LightRotation;
                }

                // get a temporary render texture and render the camera
                Camera.targetTexture = RenderTexture.GetTemporary(SnapshotTextureWidth, SnapshotTextureHeight, 24);
                Camera.enabled = true;
                Camera.Render();
                Camera.enabled = false;

                if (Light != null)
                {
                    Light.gameObject.SetActive(false);
                }

                // activate the render texture and extract the image into a new texture
                RenderTexture saveActive = RenderTexture.active;
                RenderTexture.active = Camera.targetTexture;
                texture = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
                texture.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
                texture.Apply();

                RenderTexture.active = saveActive;

                // clean up after ourselves
                RenderTexture.ReleaseTemporary(Camera.targetTexture);
            }

            if (instantiate)
            {
                DestroyImmediate(go);
            }
            else
            {
                go.SetActive(wasActive);
                go.transform.SetParent(prevParent, false);
                go.transform.position = prevPosition;
                go.transform.eulerAngles = prevEuler;
                go.transform.localScale = prevScale;
                SetLayerRecursively(go, prevLayer);
            }

            return texture;
        }
    }

    public class TextureScaler
    {
        /// <summary>
        ///     Returns a scaled copy of given texture.
        /// </summary>
        /// <param name="tex">Source texure to scale</param>
        /// <param name="width">Destination texture width</param>
        /// <param name="height">Destination texture height</param>
        /// <param name="mode">Filtering mode</param>
        public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            GPUScale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
#if UNITY_2021_3_OR_NEWER
            result.Reinitialize(width, height);
#else
            result.Resize(width, height);
#endif
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        /// <summary>
        ///     Scales the texture data of the given texture.
        /// </summary>
        /// <param name="tex">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            GPUScale(tex, width, height, mode);

            // Update new texture
#if UNITY_2021_3_OR_NEWER
            tex.Reinitialize(width, height);
#else
            tex.Resize(width, height);
#endif
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true); //Remove this if you hate us applying textures for you :)
        }

        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        private static void GPUScale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}
