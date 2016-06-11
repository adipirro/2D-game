//http://answers.unity3d.com/questions/542646/3d-text-strokeoutline.html

using UnityEngine;
using System.Collections;

public class TextOutline : MonoBehaviour
{

    public float pixelSize = 1;
    public Color outlineColor = Color.black;
    private TextMesh textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        for (int i = 0; i < 8; i++)
        {
            GameObject outline = new GameObject("outline", typeof(TextMesh));
            outline.transform.parent = transform;
            outline.transform.localScale = new Vector3(1, 1, 1);

            //Added for 2D sorting
            outline.GetComponent<Renderer>().sortingLayerID = transform.GetComponent<Renderer>().sortingLayerID;
            outline.GetComponent<Renderer>().sortingOrder = transform.GetComponent<Renderer>().sortingOrder;

            MeshRenderer otherMeshRenderer = outline.GetComponent<MeshRenderer>();
            otherMeshRenderer.material = new Material(meshRenderer.material);
            otherMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //otherMeshRenderer.castShadows = false;
            otherMeshRenderer.receiveShadows = false;
        }
    }

    void LateUpdate()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        outlineColor.a = textMesh.color.a;

        // copy attributes
        for (int i = 0; i < transform.childCount; i++)
        {

            TextMesh other = transform.GetChild(i).GetComponent<TextMesh>();
            other.color = outlineColor;
            other.text = textMesh.text;
            other.alignment = textMesh.alignment;
            other.anchor = textMesh.anchor;
            other.characterSize = textMesh.characterSize;
            other.font = textMesh.font;
            other.fontSize = textMesh.fontSize;
            other.fontStyle = textMesh.fontStyle;
            other.richText = textMesh.richText;
            other.tabSize = textMesh.tabSize;
            other.lineSpacing = textMesh.lineSpacing;
            other.offsetZ = textMesh.offsetZ;

            Vector3 pixelOffset = GetOffset(i) * pixelSize;
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint + pixelOffset);
            other.transform.position = worldPoint;
        }
    }

    Vector3 GetOffset(int i)
    {
        switch (i % 8)
        {
            case 0: return new Vector3(0, 1, 0);
            case 1: return new Vector3(1, 1, 0);
            case 2: return new Vector3(1, 0, 0);
            case 3: return new Vector3(1, -1, 0);
            case 4: return new Vector3(0, -1, 0);
            case 5: return new Vector3(-1, -1, 0);
            case 6: return new Vector3(-1, 0, 0);
            case 7: return new Vector3(-1, 1, 0);
            default: return Vector3.zero;
        }
    }
}