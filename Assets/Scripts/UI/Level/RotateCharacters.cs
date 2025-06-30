using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RotateCharacters : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float rotationAngle = 30f; // ÿ���ַ�����ת�Ƕ�

    void Start()
    {
        // ���TextMesh Pro����Ƿ����
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        // ȷ���ı��Ѿ�����
        textMeshPro.ForceMeshUpdate();

        // ��ȡ�ı���Ϣ
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        // ����ÿ���ַ�
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // ����ַ��Ƿ�ɼ�
            if (!charInfo.isVisible)
                continue;

            // ��ȡ�ַ��Ķ�������
            int vertexIndex = charInfo.vertexIndex;

            // ��ȡ�ַ��Ķ���
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // �����ַ����ĵ�
            Vector3 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            // ������ת����
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationAngle), Vector3.one);

            // Ӧ����ת
            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3] - charMidBasline) + charMidBasline;
        }

        // ��������
        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    void Update()
    {
        // ���TextMesh Pro����Ƿ����
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        // ȷ���ı��Ѿ�����
        textMeshPro.ForceMeshUpdate();

        // ��ȡ�ı���Ϣ
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        // ����ÿ���ַ�
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // ����ַ��Ƿ�ɼ�
            if (!charInfo.isVisible)
                continue;

            // ��ȡ�ַ��Ķ�������
            int vertexIndex = charInfo.vertexIndex;

            // ��ȡ�ַ��Ķ���
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // �����ַ����ĵ�
            Vector3 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            // ������ת����
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationAngle), Vector3.one);

            // Ӧ����ת
            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2] - charMidBasline) + charMidBasline;
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3] - charMidBasline) + charMidBasline;
        }

        // ��������
        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
