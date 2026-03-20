// using UnityEngine;
//
// namespace ColorBlast.Gameplay
// {
//     [CreateAssetMenu(fileName = "BlockProperties", menuName = "ColorBlast/BlockProperties")]
//     public class BlockProperties : ScriptableObject
//     {
//         private const float BaseSizeX = 0.45f;
//         private const float BaseSizeY = 0.4f;
//
//         private const float BaseSpacingX = -0.01f;
//         private const float BaseSpacingY = -0.15f;
//
//         [SerializeField] private Block prefab;
//
//         [Header("Block Size")]
//         [SerializeField] private float sizeX = 0.45f;
//         [SerializeField] private float sizeY = 0.4f;
//
//         public float SizeX => sizeX;
//         public float SizeY => sizeY;
//         public float SpacingX => BaseSpacingX * (sizeX / BaseSizeX);
//         public float SpacingY => BaseSpacingY * (sizeY / BaseSizeY);
//
//         public Vector2 SpriteBoundSize { get; private set; }
//
// #if UNITY_EDITOR
//         private void OnValidate()
//         {
//             CacheSpriteBoundSize();
//         }
//
//         private void CacheSpriteBoundSize()
//         {
//             if (prefab == null)
//             {
//                 Debug.LogWarning($"[{name}] Block prefab is not assigned.", this);
//                 SpriteBoundSize = Vector2.zero;
//                 return;
//             }
//
//             var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
//             if (spriteRenderer == null || spriteRenderer.sprite == null)
//             {
//                 Debug.LogWarning($"[{name}] Block prefab has no valid SpriteRenderer or Sprite.", this);
//                 SpriteBoundSize = Vector2.zero;
//                 return;
//             }
//
//             var sprite = spriteRenderer.sprite;
//             var spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
//             var spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
//
//             SpriteBoundSize = new Vector2(spriteWidth * sizeX, spriteHeight * sizeY);
//         }
// #endif
//     }
// }

