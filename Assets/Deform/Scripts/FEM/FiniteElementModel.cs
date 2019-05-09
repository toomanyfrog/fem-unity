using UnityEngine;
using System.Collections;

namespace Deform
{
    /// <summary>
    /// Updates the FEM rigidbodies and parameters
    /// </summary>
    public class FiniteElementModel : MonoBehaviour
    {
        public Element[] elements;

        private Mesh mesh;
        private MeshCollider meshCollider;

        private Vector3[] meshVertices;
        private Vector3[] meshNormals;

        private float maxSearchDistance = 0.00001f;

        public int rigidbodyCount;
        [HideInInspector]
        public Rigidbody[] rigidBodies;

        [HideInInspector] // mapping from mesh vertex indices to element node indices <-- map when create model instance
        public int[] mappings;


        // Use this for initialization
        void Start()
        {
            double[,] mat = new double[4, 4];
            alglib.rmatrixrndorthogonal(4, out mat);
            Debug.Log(mat);
        }

        
    }
}