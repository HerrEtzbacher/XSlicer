using DefaultNamespace;
using UnityEngine;

public class MeshSlicerScaffolding : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private float minSliceSpeed = 0.01f;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float separationDistance = 1f; 
    [SerializeField] private CubeScore cubeScore;

    private Transform _activeSwordTransform;
    private Vector3 _origin;
    private Vector3 _normal;
    private Vector3 _previousSwordPosition;
    private Vector3 _lastSwingDirection;
    private bool _hasBeenSliced = false;
    private bool _isSwordInside = false;

    private void Awake()
    {
        if (cubeScore == null)
        {
            cubeScore = GetComponent<CubeScore>();
        }
    }

    private void Update()
    {
        if (!_isSwordInside || _activeSwordTransform == null) return;

        Vector3 swordMovement = _activeSwordTransform.position - _previousSwordPosition;
        _previousSwordPosition = _activeSwordTransform.position;

        if (swordMovement.magnitude < minSliceSpeed)
            return;

        Vector3 swingDir = swordMovement.normalized;
        _lastSwingDirection = swingDir;

        Vector3 planeNormal = Vector3.Cross(_activeSwordTransform.up, swingDir);
        
        if (Vector3.Dot(planeNormal, _activeSwordTransform.forward) < 0)
            planeNormal = -planeNormal;

        if (planeNormal.sqrMagnitude < 1e-4f || float.IsNaN(planeNormal.x))
        {
            planeNormal = Vector3.Cross(swordMovement, _activeSwordTransform.right);
            if (planeNormal.sqrMagnitude < 1e-4f || float.IsNaN(planeNormal.x))
            {
                Vector3 perpendicular = Vector3.Cross(swordMovement, Vector3.up);
                if (perpendicular.sqrMagnitude < 1e-4f)
                    perpendicular = Vector3.Cross(swordMovement, Vector3.forward);
                planeNormal = perpendicular;
            }
        }

        _normal = planeNormal.normalized;
        _origin = _activeSwordTransform.position;
    }

    private void SliceMesh()
    {
        if (_hasBeenSliced || _meshFilter == null) return;
        if (_normal.sqrMagnitude < 1e-6f) return;

        _hasBeenSliced = true;
        cubeScore?.AwardPoints();

        Vector3 objectCenter = _meshFilter.transform.position;
        float distanceToCenter = Vector3.Dot(objectCenter - _origin, _normal);
        _origin += _normal * distanceToCenter;

        Vector3 localOrigin = _meshFilter.transform.InverseTransformPoint(_origin);
        Vector3 localNormal = _meshFilter.transform.InverseTransformDirection(_normal).normalized;

        Mesh[] meshes = MeshSlicer.SliceMesh(_meshFilter.sharedMesh, localOrigin, localNormal);
        if (meshes == null || meshes.Length == 0) return;

        Material sourceMat = null;
        var srcRenderer = _meshFilter.GetComponent<MeshRenderer>();
        if (srcRenderer != null) sourceMat = srcRenderer.sharedMaterial;

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            GameObject part = new GameObject($"{gameObject.name}_part_{i}");
            
            part.transform.position = _meshFilter.transform.position;
            part.transform.rotation = _meshFilter.transform.rotation;
            part.transform.localScale = _meshFilter.transform.lossyScale;

            float directionMultiplier = (i == 0) ? 1f : -1f;
            Vector3 separationOffset = _normal * directionMultiplier * separationDistance;
            part.transform.position += separationOffset;

            var mf = part.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = part.AddComponent<MeshRenderer>();
            if (sourceMat != null) mr.sharedMaterial = sourceMat;

            var mc = part.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = true;

            var rb = part.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.AddForce(_normal * directionMultiplier * 2f, ForceMode.Impulse);
        }

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword")) return;
        
        if (_activeSwordTransform != null) return;

        // Assign the active sword dynamically
        _activeSwordTransform = other.transform;
        _previousSwordPosition = _activeSwordTransform.position;
        _isSwordInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Sword")) return;

        if (other.transform != _activeSwordTransform) return;

        _isSwordInside = false;
        SliceMesh();
        
        _activeSwordTransform = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        if (_normal.sqrMagnitude > 1e-6f)
        {
            Quaternion rot = Quaternion.LookRotation(_normal);
            Gizmos.matrix = Matrix4x4.TRS(_origin, rot, Vector3.one);
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawCube(Vector3.zero, new Vector3(2f, 2f, 0.01f));
            Gizmos.color = new Color(0, 1, 0, 1f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 2f, 0.01f));
            Gizmos.matrix = Matrix4x4.identity;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_origin, _origin + _normal * 2f);

        if (_lastSwingDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_origin, _origin + _lastSwingDirection * 2f);
            Gizmos.DrawWireSphere(_origin + _lastSwingDirection * 2f, 0.05f);
        }

        if (_activeSwordTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(_origin, _origin + _activeSwordTransform.forward * 1.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_activeSwordTransform.position, 0.1f);
        }

        if (_meshFilter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_meshFilter.transform.position, 0.1f); 
        }
    }
}