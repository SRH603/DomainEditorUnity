using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RotateCharacters : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float rotationAngle = 30f; // 每个字符的旋转角度

    void Start()
    {
        // 检查TextMesh Pro组件是否存在
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        // 确保文本已经生成
        textMeshPro.ForceMeshUpdate();

        // 获取文本信息
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        // 遍历每个字符
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 检查字符是否可见
            if (!charInfo.isVisible)
                continue;

            // 获取字符的顶点索引
            int vertexIndex = charInfo.vertexIndex;

            // 获取字符的顶点
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // 计算字符中心点
            Vector3 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            // 创建旋转矩阵
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationAngle), Vector3.one);

            // 应用旋转
            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3] - charMidBasline) + charMidBasline;
        }

        // 更新网格
        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    void Update()
    {
        // 检查TextMesh Pro组件是否存在
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        // 确保文本已经生成
        textMeshPro.ForceMeshUpdate();

        // 获取文本信息
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        // 遍历每个字符
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 检查字符是否可见
            if (!charInfo.isVisible)
                continue;

            // 获取字符的顶点索引
            int vertexIndex = charInfo.vertexIndex;

            // 获取字符的顶点
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // 计算字符中心点
            Vector3 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            // 创建旋转矩阵
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationAngle), Vector3.one);

            // 应用旋转
            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3] - charMidBasline) + charMidBasline;
        }

        // 更新网格
        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
