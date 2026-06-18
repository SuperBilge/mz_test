using UnityEngine;

namespace MZ.Field
{
    public abstract class FieldEntity : MonoBehaviour
    {
        [HideInInspector] public int id;
        [HideInInspector] public Vector2Int position;
        [SerializeField] private Renderer mainRenderer;

        public Color EntityColor => mainRenderer.material.color;

        public void Init(int i, int x, int y, Color clr)
        {
            id = i;
            position = new Vector2Int(x, y);
            transform.localPosition = new Vector3(position.x, 0, position.y);
            mainRenderer.material.SetColor("_Color", clr);
        }
    }
}