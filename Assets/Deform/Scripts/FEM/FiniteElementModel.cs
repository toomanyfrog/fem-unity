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
        public TetMesh tm;

        private Mesh mesh;
        private MeshCollider meshCollider;

        public Vector3[] meshVertices;
        private Vector3[] meshNormals;

        private float mappingMaxSearchDistance = 0.00001f;

        public int rigidbodyCount;
        [HideInInspector]
        public Rigidbody[] rigidBodies;

        
        public int[] rbMappings; // mesh vertex to rigidbody
        public int[] vnMappings; // mesh vertex to node mapping

        //DEBUGGING
        public int[] normalCount;
        public int[] tetCount;


        void CreateMappings()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            meshVertices = mesh.vertices;
            meshNormals = mesh.normals;
            rbMappings = new int[mesh.vertexCount];
            vnMappings = new int[mesh.vertexCount];
            normalCount = new int[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 v = meshVertices[i];
                bool mappingFound1 = false;
                bool mappingFound2 = false;

                float minDistance1 = 100000.0f;
                float minDistance2 = 100000.0f;
                int minId1 = 0; int minId2 = 0;

                for (int j = 0; j < rigidbodyCount; j++)
                {
                    float dist = Vector3.Distance(v, transform.InverseTransformPoint(rigidBodies[j].position));
                    if (dist < minDistance1)
                    {
                        minDistance1 = dist;
                        minId1 = j;
                    }
                }

                for (int j = 0; j < tm.pointsCount; j++)
                {
                    float dist = Vector3.Distance(v, transform.InverseTransformPoint(tm.nodesPositions[j]));
                    if (dist < minDistance2)
                    {
                        minDistance2 = dist;
                        minId2 = j;
                    }
                }

                if (minDistance1 < this.mappingMaxSearchDistance)
                {
                    rbMappings[i] = minId1;
                    mappingFound1 = true;
                }
                if (minDistance2 < this.mappingMaxSearchDistance)
                {
                    vnMappings[i] = minId2;
                    mappingFound2 = true;
                }

                if (!mappingFound2)
                {
                    Debug.LogError("MappingMissing: " + i);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            CreateMappings();
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].Initialise();
            }

            elements = new Element[tm.tetrasCount];
            for (int i = 0; i < tm.tetrasCount; i++)
            {
                Vector3 u1 = tm.nodesPositions[tm.tetras[i].pA];
                Vector3 u2 = tm.nodesPositions[tm.tetras[i].pB];
                Vector3 u3 = tm.nodesPositions[tm.tetras[i].pC];
                Vector3 u4 = tm.nodesPositions[tm.tetras[i].pD];
                elements[i] = new Element(tm.tetras[i].pA, tm.tetras[i].pB, tm.tetras[i].pC, tm.tetras[i].pD,
                                                u1, u2, u3, u4);
            }

            //HOW MANY TETRAHEDRONS DO I HAVE
            tetCount = new int[tm.pointsCount];
            for (int i = 0; i < tetCount.Length; i++)
            {
                tetCount[i] = 0;
            }   
            for (int i = 0; i < tm.tetrasCount; i++)
            {
                Tetrahedron t = tm.tetras[i];
                tetCount[t.pA]++;
                tetCount[t.pB]++;
                tetCount[t.pC]++;
                tetCount[t.pD]++;
            }
        }

        private void FixedUpdate()
        {
            normalCount = new int[mesh.vertexCount];
            for (int i=0; i<normalCount.Length; i++)
            {
                normalCount[i] = 0;
            }
            for (int i = 0; i < elements.Length; i++)
            {
                // moving verts to rigidbody pos
                // m_vertices[x] = transform.InverseTransformPoint(m_msm.m_rigidBodies[mappings[x]].position);

                // update element node positions (element.xi) according to mesh vert positions
                // Calculate force on node -> Unity moves rigidbody -> move mesh verts to rigidbody position
                Element e = elements[i]; 
                Vector3 x1 = rigidBodies[e.a].position;
                Vector3 x2 = rigidBodies[e.b].position;
                Vector3 x3 = rigidBodies[e.c].position;
                Vector3 x4 = rigidBodies[e.d].position;
                /*
                
                Vector3 x1 = transform.TransformPoint(tm.nodesPositions[e.a]);
                Vector3 x2 = transform.TransformPoint(tm.nodesPositions[e.b]);
                Vector3 x3 = transform.TransformPoint(tm.nodesPositions[e.c]);
                Vector3 x4 = transform.TransformPoint(tm.nodesPositions[e.d]);
                
    */
                Vector3 x32 = x3 - x2; Vector3 x42 = x4 - x2; Vector3 n1 = Vector3.Cross(x32, x42).normalized;
                Vector3 x31 = x3 - x1; Vector3 x41 = x4 - x1; Vector3 n2 = Vector3.Cross(x31, x41).normalized;
                Vector3 x21 = x2 - x1; Vector3 n3 = Vector3.Cross(x21, x41).normalized;
                Vector3 n4 = Vector3.Cross(x21, x31).normalized;
                
                // check direction because my tetrahedralised mesh is handmade sdgjklhgjklg
                if (Vector3.Dot(n1, x21) < 0) n1 = -n1;
                if (Vector3.Dot(n2, x32) < 0) n2 = -n2;
                if (Vector3.Dot(n3, -x32) < 0) n3 = -n3;
                if (Vector3.Dot(n4, -x41) < 0) n4 = -n4;

                // reverse normals (toward center of tetra)
                //n1 = -n1; n2 = -n2; n3 = -n3; n4 = -n4;
                normalCount[e.a]++;
                normalCount[e.b]++;
                normalCount[e.c]++;
                normalCount[e.d]++;

                Debug.DrawLine(x1, x1 + n1 * 0.5f, Color.magenta);
                Debug.DrawLine(x2, x2 + n2 * 0.5f, Color.magenta);
                Debug.DrawLine(x3, x3 + n3 * 0.5f, Color.magenta);
                Debug.DrawLine(x4, x4 + n4 * 0.5f, Color.magenta);


                e.UpdateNodePositions(x1, x2, x3, x4);
                e.UpdateStrainForce(0.3333f * 0.1f, 0.5f * 0.1f);

                double[] force1 = new double[3];
                double[] force2 = new double[3];
                double[] force3 = new double[3];
                double[] force4 = new double[3];


                //Vector3 wsNormal = transform.TransformVector(-meshNormals[e.a]);
                double[] normal = new double[] { 0, 0, 0 };

                normal = new double[] { n1[0], n1[1], n1[2] };
                alglib.rmatrixgemv(3, 3, 1, e.Qsigma, 0, 0, 0, normal, 0, 1, ref force1, 0);
                Vector3 f1 = new Vector3((float)force1[0] / 4, (float)force1[1] / 4, (float)force1[2] / 4);
                rigidBodies[rbMappings[e.a]].AddForce(f1);


                //wsNormal = transform.TransformVector(-meshNormals[e.b]);
                normal = new double[] { n2[0], n2[1], n2[2] };
                alglib.rmatrixgemv(3, 3, 1, e.Qsigma, 0, 0, 0, normal, 0, 1, ref force2, 0);
                Vector3 f2 = new Vector3((float)force2[0] / 4, (float)force2[1] / 4, (float)force2[2] / 4);
                rigidBodies[rbMappings[e.b]].AddForce(f2);

                //wsNormal = transform.TransformVector(-meshNormals[e.c]);
                normal = new double[] { n3[0], n3[1], n3[2] };

                //Debug.DrawLine(x3, x3 - wsNormal, Color.magenta);

                alglib.rmatrixgemv(3, 3, 1, e.Qsigma, 0, 0, 0, normal, 0, 1, ref force3, 0);
                Vector3 f3 = new Vector3((float)force3[0] / 4, (float)force3[1] / 4, (float)force3[2] / 4);

                rigidBodies[rbMappings[e.c]].AddForce(f3);

                //wsNormal = transform.TransformVector(-meshNormals[e.d]);

                //Debug.DrawLine(x4, x4 - wsNormal, Color.magenta);

                normal = new double[] { n4[0], n4[1], n4[2] };
                alglib.rmatrixgemv(3, 3, 1, e.Qsigma, 0, 0, 0, normal, 0, 1, ref force4, 0);
                Vector3 f4 = new Vector3((float)force4[0] / 4, (float)force4[1] / 4, (float)force4[2] / 4);
                rigidBodies[rbMappings[e.d]].AddForce(f4);

                Debug.DrawLine(x1, x1 + f1, Color.red);
                Debug.DrawLine(x2, x2 + f2, Color.red);
                Debug.DrawLine(x3, x3 + f3, Color.red);
                Debug.DrawLine(x4, x4 + f4, Color.red);

            }
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                //mesh.vertices[i] = transform.InverseTransformPoint(rigidBodies[mappings[i]].position);
                meshVertices[i] = transform.InverseTransformPoint(rigidBodies[rbMappings[i]].position); //vert move to rb
                tm.nodesPositions[vnMappings[i]] = meshVertices[i]; //node move to vert
            }

            mesh.vertices = meshVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }

        }
    }
}