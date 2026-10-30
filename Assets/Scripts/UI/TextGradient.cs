using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScalingShmup.UI
{
    // Colours a UI Text (or other Graphic) with a top-to-bottom gradient, e.g. the chrome/sunset
    // look of 80s arcade titles. Put it above any Shadow or Outline on the object, so their copies
    // keep their own flat colour.
    [RequireComponent(typeof(Graphic))]
    public class TextGradient : BaseMeshEffect
    {
        [SerializeField] private Color _top = new Color(1f, 0.95f, 0.3f);
        [SerializeField] private Color _bottom = new Color(1f, 0.2f, 0.6f);

        private readonly List<UIVertex> _vertices = new List<UIVertex>();

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            _vertices.Clear();
            vh.GetUIVertexStream(_vertices);

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (UIVertex vertex in _vertices)
            {
                minY = Mathf.Min(minY, vertex.position.y);
                maxY = Mathf.Max(maxY, vertex.position.y);
            }

            float height = Mathf.Max(maxY - minY, 0.0001f);

            for (int i = 0; i < _vertices.Count; i++)
            {
                UIVertex vertex = _vertices[i];
                Color gradient = Color.Lerp(_bottom, _top, (vertex.position.y - minY) / height);
                vertex.color = gradient * (Color)vertex.color;
                _vertices[i] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(_vertices);
        }
    }
}
