using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	public GameObject gameOverPanel;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;			
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{			
			RestartScene();			
		}
	}

	public void RestartScene()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}

	public void GameOver()
	{
		Time.timeScale = 0;
		gameOverPanel.SetActive(true);
	}

	public static void DeformCharacterArea(PlayerCharacterController character, List<Vector3> newAreaVertices)
	{
		int newAreaVerticesCount = newAreaVertices.Count;
		if (newAreaVerticesCount > 0)
		{
			List<Vector3> areaVertices = character.territoryVertices;
			int startPoint = character.GetClosestAreaVertice(newAreaVertices[0]);
			int endPoint = character.GetClosestAreaVertice(newAreaVertices[newAreaVerticesCount - 1]);


            HashSet<Vector3> redundantVertices = new HashSet<Vector3>();
            int currentIndex = startPoint;
            while (currentIndex != endPoint)
            {
                redundantVertices.Add(areaVertices[currentIndex]);
                currentIndex = (currentIndex + 1) % areaVertices.Count;
            }
            redundantVertices.Add(areaVertices[endPoint]);

            float maxArea = float.MinValue;
            List<Vector3> maxAreaVertices = new List<Vector3>();

            for (int dir = -1; dir <= 1; dir += 2) // dir=-1 for counterclockwise, dir=1 for clockwise
            {
                List<Vector3> tempArea = new List<Vector3>(areaVertices);
                for (int i = 0; i < newAreaVerticesCount; i++)
                {
                    int insertIndex = startPoint + dir * i;
                    if (insertIndex < 0) insertIndex += tempArea.Count + 1;
                    tempArea.Insert(insertIndex, newAreaVertices[i]);
                }

                // Remove redundant vertices & calculate area size
                tempArea = tempArea.Except(redundantVertices).ToList();
                float areaSize = Mathf.Abs(tempArea.Take(tempArea.Count - 1).Select((p, i) =>
                    (tempArea[i + 1].x - p.x) * (tempArea[i + 1].z + p.z)).Sum() / 2f);

                // Update max area if needed
                if (areaSize > maxArea)
                {
                    maxArea = areaSize;
                    maxAreaVertices = tempArea;
                }
            }

            // Update character territory vertices with the area of greatest size
            character.territoryVertices = maxAreaVertices;
            character.UpdateArea();
        }
	}

    // https://codereview.stackexchange.com/questions/108857/point-inside-polygon-check
    public static bool IsPointInPolygon(Vector3 point, Vector3[] polygon)
    {
        int polygonLength = polygon.Length;
        bool inside = false;
        float pointX = point.x, pointZ = point.z; // Considering x and z coordinates for Vector3 points
        float startX, startZ, endX, endZ;
        Vector3 endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endZ = endPoint.z;

        int i = 0;
        while (i < polygonLength)
        {
            startX = endX;
            startZ = endZ;
            endPoint = polygon[i++];
            endX = endPoint.x;
            endZ = endPoint.z;

            inside ^= (endZ > pointZ ^ startZ > pointZ) && ((pointX - endX) < (pointZ - endZ) * (startX - endX) / (startZ - endZ));
        }
        return inside;
    }

    public static bool IsPointInsidePolygon(Vector3 point, Vector3[] vertices)
    {
        Vector3 normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

        Vector3 vector = point - vertices[0];
        float dotProduct = Vector3.Dot(normal, vector);

        // If dot product is positive, point is on the same side of the plane as the normal, hence inside
        return dotProduct >= 0;
    }

}