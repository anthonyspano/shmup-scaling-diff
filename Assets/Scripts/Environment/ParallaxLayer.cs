using System.Collections.Generic;
using UnityEngine;

namespace ScalingShmup.Environment
{
    // Scrolls one background layer left and loops it seamlessly. Copies of the layer's sprite sit
    // edge to edge as children; once the layer has moved one sprite width it snaps back by exactly
    // that width, which is invisible because a copy now occupies the starting spot.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("Units per second. Give distant layers lower speeds than near ones.")]
        [SerializeField] private float _speed = 1f;
        [Tooltip("Copies overlap by this much (in the layer's local units) to hide seams between tiles.")]
        [SerializeField, Min(0f)] private float _overlap = 0.01f;
        [Tooltip("Defaults to Camera.main. Used to work out how many copies cover the screen.")]
        [SerializeField] private Camera _camera;

        private readonly List<Transform> _copies = new List<Transform>();
        private SpriteRenderer _spriteRenderer;
        private float _startX;
        private float _tileStep;
        private float _cameraAspect;
        private float _cameraSize;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _startX = transform.localPosition.x;
            _tileStep = _spriteRenderer.sprite.bounds.size.x - _overlap;
            CollectExistingCopies();
            LayoutCopies();
        }

        private void Update()
        {
            if (_camera != null && (_camera.aspect != _cameraAspect || _camera.orthographicSize != _cameraSize))
            {
                LayoutCopies();
            }

            Vector3 position = transform.localPosition;
            position.x -= _speed * Time.deltaTime;

            // Local scale converts the sprite's width into the parent's units, which localPosition uses.
            float wrapDistance = _tileStep * transform.localScale.x;

            if (wrapDistance > 0f)
            {
                while (position.x <= _startX - wrapDistance)
                {
                    position.x += wrapDistance;
                }
            }

            transform.localPosition = position;
        }

        // Existing children showing the same sprite (e.g. "far (1)") are reused as copies.
        private void CollectExistingCopies()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out SpriteRenderer childRenderer) && childRenderer.sprite == _spriteRenderer.sprite)
                {
                    _copies.Add(child);
                }
            }
        }

        // Places copies to the left and right of the layer so the screen is covered at every point
        // of the scroll cycle, at any resolution or aspect ratio.
        private void LayoutCopies()
        {
            int copiesLeft = 0;
            int copiesRight = 1;

            if (_camera != null && _camera.orthographic)
            {
                _cameraAspect = _camera.aspect;
                _cameraSize = _camera.orthographicSize;

                float worldStep = _tileStep * transform.lossyScale.x;
                float halfCamera = _cameraSize * _cameraAspect;
                float halfTile = worldStep * 0.5f;
                float offsetFromCamera = transform.parent != null
                    ? transform.parent.TransformPoint(new Vector3(_startX, 0f, 0f)).x - _camera.transform.position.x
                    : _startX - _camera.transform.position.x;

                copiesLeft = Mathf.Max(0, Mathf.CeilToInt((offsetFromCamera + halfCamera - halfTile) / worldStep));
                copiesRight = Mathf.Max(1, Mathf.CeilToInt((halfCamera - offsetFromCamera + halfTile) / worldStep));
            }

            int needed = copiesLeft + copiesRight;

            while (_copies.Count < needed)
            {
                _copies.Add(CreateCopy());
            }

            for (int i = 0; i < _copies.Count; i++)
            {
                bool inUse = i < needed;
                _copies[i].gameObject.SetActive(inUse);

                if (inUse)
                {
                    // Right-hand copies come first so an existing hand-placed child stays on the right.
                    int slot = i < copiesRight ? i + 1 : -(i - copiesRight + 1);
                    _copies[i].localPosition = new Vector3(slot * _tileStep, 0f, 0f);
                    _copies[i].localRotation = Quaternion.identity;
                    _copies[i].localScale = Vector3.one;
                }
            }
        }

        private Transform CreateCopy()
        {
            var copy = new GameObject($"{name} (copy)");
            copy.transform.SetParent(transform, false);

            var copyRenderer = copy.AddComponent<SpriteRenderer>();
            copyRenderer.sprite = _spriteRenderer.sprite;
            copyRenderer.color = _spriteRenderer.color;
            copyRenderer.flipX = _spriteRenderer.flipX;
            copyRenderer.flipY = _spriteRenderer.flipY;
            copyRenderer.sharedMaterial = _spriteRenderer.sharedMaterial;
            copyRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            copyRenderer.sortingOrder = _spriteRenderer.sortingOrder;

            return copy.transform;
        }
    }
}
