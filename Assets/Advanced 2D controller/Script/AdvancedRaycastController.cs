using UnityEngine;
namespace Advanced2DPlayerController
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class AdvancedRaycastController : MonoBehaviour
    {


        [SerializeField]
        protected int horizontalRayCount = 4;
        [SerializeField]
        protected int verticalRayCount = 4;
        [SerializeField]
        protected LayerMask collisionMask;


        readonly protected float skinWidth = 0.015f;

        protected RaycastOrigins raycastOrigins;


        protected float horizontalRaySpacing;
        protected float verticalRaySpacing;

        protected BoxCollider2D boxCollider;

        public BoxCollider2D BoxCollider
        {
            get
            {
                return boxCollider;
            }

            set
            {
                boxCollider = value;
            }
        }

        protected struct RaycastOrigins
        {
            public Vector2 topLeft, topRight, bottomLeft, bottomRight;
        }


        public virtual void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            CalculateRaySpacing();
        }

        #region Raycast
        protected void UpdateRaycastOrigins()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        protected void CalculateRaySpacing()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }
        #endregion



    }
}