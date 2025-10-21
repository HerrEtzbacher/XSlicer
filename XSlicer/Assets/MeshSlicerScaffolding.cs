using DefaultNamespace;
using UnityEngine;

public class MeshSlicerScaffolding : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField] 
    private Vector3 _origin;    
    [SerializeField] 
    private Vector3 _normal;

    public void SliceMesh()
    {
        Mesh[] meshes = MeshSlicer.SliceMesh(_meshFilter.sharedMesh, _origin, _normal);
        for (int index = 0; index < meshes.Length; index++)
        {
            Mesh mesh = meshes[index];
            GameObject submesh = Instantiate(this.gameObject);
            submesh.gameObject.transform.position += (2* transform.right);
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(_normal), Vector3.one);
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawCube(_origin, new Vector3(2, 2, 0.01f));
        Gizmos.color = new Color(0, 1, 0, 1f);
        Gizmos.DrawWireCube(_origin, new Vector3(2, 2, 0.01f));

        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < _meshFilter.sharedMesh.normals.Length; i++)
        {
            Vector3 normal = _meshFilter.sharedMesh.normals[i];
            Vector3 vertex = _meshFilter.sharedMesh.vertices[i];
            Gizmos.DrawLine(vertex, vertex + normal);
        }
    }
}