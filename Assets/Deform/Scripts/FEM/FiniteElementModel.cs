using UnityEngine;
using System.Collections;

namespace Deform
{
    /// <summary>
    /// Updates the FEM rigidbodies and parameters
    /// </summary>
    public class FiniteElementModel : MonoBehaviour
    {
        TetMesh meshModel; // in U space


        public float stiffness = 1000.0f;

        public float damping = 10.0f;

        public int rigidBodiesCount = 0;


        [HideInInspector]
        public Rigidbody[] m_rigidBodies;

        [HideInInspector]
        public SpringJoint[] m_springJoints;


        // Use this for initialization
        void Start()
        {
            double[,] mat = new double[4, 4];
            alglib.rmatrixrndorthogonal(4, out mat);
            Debug.Log(mat);
        }

        
    }
}