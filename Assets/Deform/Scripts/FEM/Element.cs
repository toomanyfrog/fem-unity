using UnityEngine;
using System.Collections;

namespace Deform
{
    /// <summary>
    /// Element of FEM
    /// </summary>
    public class Element : MonoBehaviour
    {
        // u1, u2, u3, u4 
        // matrix D_u, D_x, D_v
        // D_u = ( (u2-u1)T (u3-u1)T (u4-u1)T) 
        // matrix Beta = D_u^(-1)
    }
}