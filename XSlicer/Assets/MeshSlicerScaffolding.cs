using DefaultNamespace;
using UnityEngine;

public class MeshSlicerScaffolding : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Transform _cutPlaneTransform;
    [SerializeField] private Vector3 _origin;
    [SerializeField] private Vector3 _normal;
    [SerializeField] private float minSliceSpeed = 0.01f;
    [SerializeField] private bool showDebugGizmos = true;

    private Vector3 _previousSwordPosition;
    private Vector3 _lastSwingDirection;
    private bool _hasBeenSliced = false;
    private bool _isSwordInside = false;

    private void Start()
    {
        if (_cutPlaneTransform != null)
            _previousSwordPosition = _cutPlaneTransform.position;
    }

    private void Update()
    {
        if (!_isSwordInside || _cutPlaneTransform == null) return;

        Vector3 swordMovement = _cutPlaneTransform.position - _previousSwordPosition;
        _previousSwordPosition = _cutPlaneTransform.position;

        if (swordMovement.magnitude < minSliceSpeed)
            return;

        Vector3 swingDir = swordMovement.normalized;
        _lastSwingDirection = swingDir;

        Vector3 planeNormal = Vector3.Cross(_cutPlaneTransform.up, swingDir);
        if (Vector3.Dot(planeNormal, _cutPlaneTransform.forward) < 0)
            planeNormal = -planeNormal;

        if (planeNormal.sqrMagnitude < 1e-4f || float.IsNaN(planeNormal.x))
        {
            planeNormal = Vector3.Cross(swordMovement, _cutPlaneTransform.right);
            if (planeNormal.sqrMagnitude < 1e-4f || float.IsNaN(planeNormal.x))
            {
                Vector3 perpendicular = Vector3.Cross(swordMovement, Vector3.up);
                if (perpendicular.sqrMagnitude < 1e-4f)
                    perpendicular = Vector3.Cross(swordMovement, Vector3.forward);
                planeNormal = perpendicular;
            }
        }

        _normal = planeNormal.normalized;
        _origin = _cutPlaneTransform.position;
    }

    private void SliceMesh()
    {
        if (_hasBeenSliced || _meshFilter == null) return;
        if (_normal.sqrMagnitude < 1e-6f) return;

        _hasBeenSliced = true;

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

            var mf = part.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = part.AddComponent<MeshRenderer>();
            if (sourceMat != null) mr.sharedMaterial = sourceMat;

            var mc = part.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = true;

            var rb = part.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword")) return;
        _isSwordInside = true;
        if (_cutPlaneTransform != null)
            _previousSwordPosition = _cutPlaneTransform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Sword")) return;
        _isSwordInside = false;
        SliceMesh();
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

        if (_cutPlaneTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(_origin, _origin + _cutPlaneTransform.forward * 1.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_cutPlaneTransform.position, 0.1f);
        }

        if (_meshFilter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_meshFilter.transform.position, 0.1f);
        }
    }
}
