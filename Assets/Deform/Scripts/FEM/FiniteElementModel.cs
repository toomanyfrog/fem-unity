﻿using UnityEngine;
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

        private float mappingMaxSearchDistance = 0.00001f;

        public int rigidbodyCount;
        [HideInInspector]
        public Rigidbody[] rigidBodies;

        [HideInInspector] // mapping from mesh vertex indices to rigidbodies <-- map when create model instance
        public int[] mappings;


        void CreateMappings()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            meshVertices = mesh.vertices;
            meshNormals = mesh.normals;
            mappings = new int[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 v = meshVertices[i];
                bool mappingFound = false;

                float minDistance = 100000.0f;
                int minId = 0;

                for (int j = 0; j < rigidbodyCount; j++)
                {
                    float dist = Vector3.Distance(v, transform.InverseTransformPoint(rigidBodies[j].position));
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        minId = j;
                    }
                }

                if (minDistance < this.mappingMaxSearchDistance)
                {
                    mappings[i] = minId;
                    mappingFound = true;
                }

                if (!mappingFound)
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
        }

        private void FixedUpdate()
        {
            for (int i=0; i<elements.Length; i++)
            {
                // moving verts to rigidbody pos
                // m_vertices[i] = transform.InverseTransformPoint(m_msm.m_rigidBodies[mappings[i]].position);

                // update element node positions (element.xi) according to mesh vert positions
                // Calculate force on node -> Unity moves rigidbody -> move mesh verts to rigidbody position

                Vector3 x1 = mesh.vertices[elements[i].a];
                Vector3 x2 = mesh.vertices[elements[i].b];
                Vector3 x3 = mesh.vertices[elements[i].c];
                Vector3 x4 = mesh.vertices[elements[i].d];
                elements[i].UpdateNodePositions(x1, x2, x3, x4);
                elements[i].UpdateStrainForce(1, 1);
            }
        }

    }
}