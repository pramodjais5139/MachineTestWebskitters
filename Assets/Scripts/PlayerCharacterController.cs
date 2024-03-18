using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private Material playerBodyMaterial;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Color color;

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
        trans.position = Vector3.ClampMagnitude(transPos, 25.5f);
       bool isOutside = !GameManager.IsPointInPolygon(new Vector3(transPos.x, transPos.z), GetVertices2D(territoryVertices));
      //  bool isOutside = !GameManager.IsPointInsidePolygon(transPos, GetVertices3D(territoryVertices));
        int count = newTerritoryVertices.Count;
        Debug.Log(isOutside);
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
                    if (GameManager.IsPointInsidePolygon(vertex, GetVertices3D(character.territoryVertices)))
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
        for (int i = 0; i < startAreaPoints; i++)
        {
            territoryVertices.Add(transform.position + Quaternion.Euler(new Vector3(0, step * i, 0)) * Vector3.forward * startAreaRadius);
        }
        UpdateArea();

        trailCollidersHolder = new GameObject();
        trailCollidersHolder.transform.SetParent(territoryTrans);
        trailCollidersHolder.name = "PlayerTerritoryTrailCollidersHolder";
        trailCollidersHolder.layer = 8;
    }


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
        Triangulator3D tr = new Triangulator3D(GetVertices3D(vertices));
        int[] indices = tr.Triangulate();
        Debug.Log(indices.Length);
        Mesh newMesh = new Mesh();
       newMesh.vertices = vertices.ToArray();
        newMesh.triangles = indices;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        newMesh.name = meshName + "Mesh";
       
        return newMesh;
    }

    private Vector3[] GetVertices3D(List<Vector3> vertices)
    {
        List<Vector3> areaVertices2D = new List<Vector3>();
        foreach (Vector3 vertex in vertices)
        {
            areaVertices2D.Add(new Vector3(vertex.x,vertex.y, vertex.z));
        }

        return areaVertices2D.ToArray();
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
        //  GameManager.instance.GameOver();              
    }
}
