// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class RippleGrid : MonoBehaviour
// {
//     public int gridSize = 10;
//     public float rippleSpeed = .1f;
//     public float rippleMagnitude = 1f;
//     public GameObject[,] grid;
//     public Sprite sprite;
//     public Material meshMat;
//     private Vector3[,] originalPositions;
//     private Vector2 currentRipplePos = Vector2.zero;
//     private Vector2 lastRipplePos = Vector2.zero;
//     private float[,] rippleStrengths;
//     private GameObject player;

//     public MeshRenderer quadRenderer;

//     void Start()
//     {
//         gridSize = 100;
//         rippleSpeed = 1f;
//         rippleMagnitude = 10f;
//         grid = new GameObject[gridSize, gridSize];
//         originalPositions = new Vector3[gridSize, gridSize];
//         rippleStrengths = new float[gridSize, gridSize];
//         player = GameObject.FindWithTag("Player");

//         Vector3 playerPos = player.transform.position;
//         Vector3 centerPos = new Vector3(0f, 0f, 0f);
//         Vector3 offset = centerPos - (Vector3.one * (gridSize / 2f));

//         for (int x = 0; x < gridSize; x++)
//         {
//             for (int y = 0; y < gridSize; y++)
//             {
//                 GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
//                 obj.transform.localScale = Vector3.one * 0.5f;
//                 obj.transform.position = new Vector3(x, y, transform.position.z);
//                 quadRenderer = obj.GetComponent<MeshRenderer>();
//                 grid[x, y] = obj;
//                 originalPositions[x, y] = obj.transform.position;
//                 rippleStrengths[x, y] = 0f;
//             }
//         }

//         // Ripple(PlayerController.Instance.position, speed);
//         // InvokeRepeating("Ripple", 0f, .1f);
//     }

//     public void Ripple()
//     {
//         float speed = 0;
//         Vector2 pos = PlayerController.Instance.position;
//         if (pos != currentRipplePos)
//         {
//             lastRipplePos = currentRipplePos;
//             currentRipplePos = pos;

//             float magnitude = rippleMagnitude * (speed / 10f);

//             // Calculate the direction of the ripple
//             Vector2 direction = lastRipplePos - currentRipplePos;
//             if (direction != Vector2.zero)
//             {
//                 direction.Normalize();
//             }

//             for (int x = 0; x < gridSize; x++)
//             {
//                 for (int y = 0; y < gridSize; y++)
//                 {
//                     GameObject obj = grid[x, y];
//                     Vector2 gridPos = new Vector2(x, y);

//                     // Calculate the angle between the grid point and the ripple direction
//                     Vector2 diff = gridPos - lastRipplePos;
//                     float angle =
//                         Mathf.Atan2(direction.y, direction.x) - Mathf.Atan2(diff.y, diff.x);
//                     angle = Mathf.Repeat(angle, Mathf.PI * 2f);
//                     angle = angle > Mathf.PI ? angle - Mathf.PI * 2f : angle;

//                     float dist = Vector2.Distance(gridPos, lastRipplePos);

//                     float strength = magnitude / (1f + (dist * rippleSpeed));

//                     // Adjust the ripple strength based on the angle
//                     float angleMultiplier = Mathf.Cos(angle);
//                     strength *= angleMultiplier;

//                     if (strength > rippleStrengths[x, y])
//                     {
//                         rippleStrengths[x, y] = strength;
//                     }

//                     if (lastRipplePos != Vector2.zero)
//                     {
//                         float distToLast = Vector2.Distance(gridPos, lastRipplePos);
//                         float strengthToLast = rippleMagnitude / (1f + (distToLast * rippleSpeed));

//                         if (strengthToLast < rippleStrengths[x, y])
//                         {
//                             rippleStrengths[x, y] -= strengthToLast * Time.deltaTime;
//                             rippleStrengths[x, y] = Mathf.Max(0f, rippleStrengths[x, y]);
//                         }
//                     }

//                     float offsetMagnitude =
//                         rippleStrengths[x, y] * Mathf.Sin(Time.time * (speed + strength));
//                     Vector3 offset =
//                         new Vector3(
//                             Mathf.Sin(Time.time * strength),
//                             0f,
//                             Mathf.Cos(Time.time * strength)
//                         ) * offsetMagnitude;
//                     obj.transform.position = originalPositions[x, y] + offset;
//                 }
//             }
//         }
//     }

//     public void Update()
//     {
//         Ripple();
//     }
// }
