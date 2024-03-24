using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField]private Rigidbody rigidbody;
    [SerializeField] private Material playerBodyMaterial;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Color color;

    public ConvexHullGenerator conv;

    public List<PlayerCharacterController> attackedCharacters = new List<PlayerCharacterController>();
    public List<SphereCollider> trailColls = new List<SphereCollider>();


    public int startAreaPoints = 45;
    public float startAreaRadius = 3f;
    public float minPointDistance = 0.1f;
    public TerritoryArea territory;
    public GameObject territoryOutline;
    public List<Vector3> territoryVertices = new List<Vector3>();
    public List<Vector3> newTerritoryVertices = new List<Vector3>();

    private MeshRenderer territoryMeshRend;
    private MeshFilter territoryFilter;
    private MeshRenderer territoryOutlineMeshRend;
    private MeshFilter territoryOutlineFilter;
    public GameObject trailCollidersHolder;

    private Material territoryMaterial;

    public float sensitivity = 300f;
    public float turnTreshold = 15f;
    private Vector3 mouseStartPos;
    protected Vector3 curDir;
    protected Quaternion targetRot;

    public float speed = 2f;
    public float turnSpeed = 14f;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        trail.material.color = new Color(color.r, color.g, color.b, 0.65f);
        playerBodyMaterial.color = color * 1.35f;
    }

    private void Start()
    {
        InitializeCharacter();
    }


    public virtual void Update()
    {
        var mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = mousePos;
        }
        else if (Input.GetMouseButton(0))
        {
            float distance = (mousePos - mouseStartPos).magnitude;
            if (distance > turnTreshold)
            {
                if (distance > sensitivity)
                {
                    mouseStartPos = mousePos - (curDir * sensitivity / 2f);
                }

                var curDir2D = -(mouseStartPos - mousePos).normalized;
                curDir = new Vector3(curDir2D.x, 0, curDir2D.y);
            }
        }
        else
        {
            curDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        }

        var trans = transform;
        var transPos = trans.position;
        trans.position = Vector3.ClampMagnitude(transPos, 25f);
        bool isOutside =  !GameManager.IsPointInPolygon(new Vector3(transPos.x,transPos.y, transPos.z), territoryVertices.ToArray());
        int count = newTerritoryVertices.Count;

        if (isOutside)
        {
            if (count == 0 || !newTerritoryVertices.Contains(transPos) && (newTerritoryVertices[count - 1] - transPos).magnitude >= minPointDistance)
            {
                count++;
                newTerritoryVertices.Add(transPos);

                int trailCollsCount = trailColls.Count;
                float trailWidth = trail.startWidth;
                SphereCollider lastColl = trailCollsCount > 0 ? trailColls[trailCollsCount - 1] : null;
                if (!lastColl || (transPos - lastColl.center).magnitude > trailWidth)
                {
                    SphereCollider trailCollider = trailCollidersHolder.AddComponent<SphereCollider>();
                    trailCollider.center = transPos;
                    trailCollider.radius = trailWidth / 2f;
                    trailCollider.isTrigger = true;
                    trailCollider.enabled = false;
                    trailColls.Add(trailCollider);

                    if (trailCollsCount > 1)
                    {
                        trailColls[trailCollsCount - 2].enabled = true;
                    }
                }
            }

            if (!trail.emitting)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }
        else if (count > 0)
        {
            GameManager.DeformCharacterArea(this, newTerritoryVertices);

            foreach (var character in attackedCharacters)
            {
                List<Vector3> newCharacterAreaVertices = new List<Vector3>();
                foreach (var vertex in newTerritoryVertices)
                {
                    if (GameManager.IsPointInPolygon(new Vector3(vertex.x,vertex.y, vertex.z), territoryVertices.ToArray()))
                    //if (GameManager.IsPointInPolygon(new Vector2(vertex.x, vertex.z), Vertices2D(character.areaVertices)))
                    {
                        newCharacterAreaVertices.Add(vertex);
                    }
                }

                GameManager.DeformCharacterArea(character, newCharacterAreaVertices);
            }
            attackedCharacters.Clear();
            newTerritoryVertices.Clear();

            if (trail.emitting)
            {
                trail.Clear();
                trail.emitting = false;
            }
            foreach (var trailColl in trailColls)
            {
                Destroy(trailColl);
            }
            trailColls.Clear();
        }
    }

    public virtual void FixedUpdate()
    {
        rigidbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);

        if (curDir != Vector3.zero)
        {
            targetRot = Quaternion.LookRotation(curDir);
            if (rigidbody.rotation != targetRot)
            {
                rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRot, turnSpeed);
            }
        }
    }
    private void InitializeCharacter()
    {
        territory = new GameObject().AddComponent<TerritoryArea>();
        territory.name = "PlayerTerritory";
        territory.character = this;
        Transform territoryTrans = territory.transform;
        territoryFilter = territory.gameObject.AddComponent<MeshFilter>();
        territoryMeshRend = territory.gameObject.AddComponent<MeshRenderer>();
        territoryMeshRend.material = territoryMaterial;
        territoryMeshRend.material.color = color;

        territoryOutline = new GameObject();
        territoryOutline.name = "PlayerTerritoryOutline";
        Transform areaOutlineTrans = territoryOutline.transform;
        areaOutlineTrans.position += new Vector3(0, -0.495f, -0.1f);
        areaOutlineTrans.SetParent(territoryTrans);
        territoryOutlineFilter = territoryOutline.AddComponent<MeshFilter>();
        territoryOutlineMeshRend = territoryOutline.AddComponent<MeshRenderer>();
        territoryOutlineMeshRend.material = territoryMaterial;
        territoryOutlineMeshRend.material.color = new Color(color.r * .7f, color.g * .7f, color.b * .7f);

        float step = 360f / startAreaPoints;
        for (int i = startAreaPoints; i>= 0; i--)
        {
            territoryVertices.Add(transform.position + Quaternion.Euler(new Vector3(0, step * i, 0)) * Vector3.forward * startAreaRadius);
        }
        List<Vector3 > abc =ConvexHull.ComputeConvexHull(territoryVertices);
        Debug.Log(IsConvexPolygon(abc));
       // territoryVertices = ConvexHull.compute(territoryVertices,true);
       UpdateArea();

        trailCollidersHolder = new GameObject();
        trailCollidersHolder.transform.SetParent(territoryTrans);
        trailCollidersHolder.name = "PlayerTerritoryTrailCollidersHolder";
        trailCollidersHolder.layer = 8;
    }


    private void OnDrawGizmos()
    {
        // Draw lines between consecutive points
        Gizmos.color = Color.blue;
        for (int i = 0; i < territoryVertices.Count - 1; i++)
        {
            Gizmos.DrawLine(territoryVertices[i], territoryVertices[i + 1]);
        }

        // Draw line between last and first point to close the polygon
        if (territoryVertices.Count > 1)
        {
            Gizmos.DrawLine(territoryVertices[territoryVertices.Count - 1], territoryVertices[0]);
        }

        // Draw points
        Gizmos.color = Color.red;
        foreach (Vector3 point in territoryVertices)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }

    bool IsConvexPolygon(List<Vector3> points)
    {
        int n = points.Count;
        for (int i = 0; i < n; i++)
        {
            Vector3 current = points[i];
            Vector3 next = points[(i + 1) % n];
            Vector3 prev = points[(i + n - 1) % n];

            Vector3 edge1 = prev - current;
            Vector3 edge2 = next - current;

            Vector3 cross = Vector3.Cross(edge1, edge2);

            if (cross.y < 0) // Check if cross product points downwards
            {
                return false; // Polygon is not convex
            }
        }
        return true; // Polygon is convex
    }

    // Compute the cross product of vectors (p1-p0) and (p2-p0)
   
    public void UpdateArea()
    {
        if (territoryFilter)
        {
            Mesh areaMesh = GenerateMesh(territoryVertices, "PlayerTerritoryOutline");
            territoryFilter.mesh = areaMesh;
            territoryOutlineFilter.mesh = areaMesh;
            territory.mineMeshCollider.sharedMesh = areaMesh;
        }
    }

    private Mesh GenerateMesh(List<Vector3> vertices, string meshName)
    {
        List<int> ints = Triangulation.TriangulateConvexPolygon(vertices);
        Mesh newMesh = new Mesh()
        {
            vertices = vertices.ToArray(),
            triangles = ints.ToArray()
        };

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
    
        newMesh.name = meshName + "Mesh";

        return newMesh;
    }
    private Vector2[] GetVertices2D(List<Vector3> vertices)
    {
       
        List<Vector2> areaVertices2D = new List<Vector2>();
        foreach (Vector3 vertex in vertices)
        {
            areaVertices2D.Add(new Vector2(vertex.x, vertex.z));
        }

        return areaVertices2D.ToArray();
    }
    public int GetClosestAreaVertice(Vector3 fromPos)
    {
        int closest = -1;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < territoryVertices.Count; i++)
        {
            float dist = (territoryVertices[i] - fromPos).magnitude;
            if (dist < closestDist)
            {
                closest = i;
                closestDist = dist;
            }
        }

        return closest;
    }
    private void OnTriggerEnter(Collider other)
    {
        TerritoryArea characterArea = other.GetComponent<TerritoryArea>();
        if (characterArea && characterArea != territory && !attackedCharacters.Contains(characterArea.character))
        {
            attackedCharacters.Add(characterArea.character);
        }

        if (other.gameObject.layer == 8)
        {
            characterArea = other.transform.parent.GetComponent<TerritoryArea>();
            characterArea.character.DestroyPlayer();
            Debug.LogError("PLayer cross the line");
        }
    }

    public void DestroyPlayer()
    {
            GameManager.instance.GameOver();              
    }
}
