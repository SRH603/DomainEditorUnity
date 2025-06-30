using UnityEngine;                     //  ★ 新建文件
[ExecuteAlways]
public class MeshDebugGizmos : MonoBehaviour
{
    public bool showNormals = true;
    public bool showTriIndex = false;
    public float normalLength = 0.1f;
    
    Mesh mesh;
    void OnEnable()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf) mesh = mf.sharedMesh;
    }

    void OnDrawGizmos()
    {
        if (!mesh) return;

        // 1️⃣ 绘制法线
        if (showNormals)
        {
            Gizmos.color = Color.green;
            var verts = mesh.vertices;
            var norms = mesh.normals;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(verts[i]);
                Vector3 worldDir = transform.TransformDirection(norms[i]);
                Gizmos.DrawLine(worldPos, worldPos + worldDir * normalLength);
            }
        }

        // 2️⃣ 在 Scene 视图标出三角索引（选做）
        if (showTriIndex)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p0 = transform.TransformPoint(mesh.vertices[mesh.triangles[i]]);
                Vector3 p1 = transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
                Vector3 p2 = transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);
                Vector3 center = (p0 + p1 + p2) / 3f;
                //UnityEditor.Handles.Label(center, i.ToString());
            }
        }
    }
}