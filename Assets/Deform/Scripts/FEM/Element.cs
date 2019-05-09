using UnityEngine;
using System.Collections;
using System;

namespace Deform
{
    /// <summary>
    /// Element of FEM
    /// </summary>
    /// 
    [Serializable]
    public struct Element 
    {
        // Tetrahedron indices
        public int a;
        public int b;
        public int c;
        public int d;

        // Reference positions u1, u2, u3, u4 
        public Vector3 u1;
        public Vector3 u2;
        public Vector3 u3;
        public Vector3 u4;


        // matrix D_u, D_x, D_v
        // D_u = ( (u2-u1)T (u3-u1)T (u4-u1)T) 
        // matrix Beta = D_u^(-1)

        double[,] D_u;
        public double[,] D_x;
        double[,] D_v;
        double[,] beta;

        //double[,] F;
        // double[,] G; <-- I don't seem to need this???

        public Element(int p1, int p2, int p3, int p4, Vector3 _u1, Vector3 _u2, Vector3 _u3, Vector3 _u4)
        {
            a = p1;
            b = p2;
            c = p3;
            d = p4;

            u1 = _u1; 
            u2 = _u2;
            u3 = _u3;
            u4 = _u4; 

            D_u = new double[3, 3]; D_x = new double[3, 3]; D_v = new double[3, 3]; beta = new double[3, 3];
            // calculate D_u
            Vector3 u21 = u2 - u1; Vector3 u31 = u3 - u1; Vector3 u41 = u4 - u1;
            D_u[0, 0] = u21[0]; D_u[1, 0] = u21[1]; D_u[2, 0] = u21[2];
            D_u[0, 1] = u31[0]; D_u[1, 1] = u31[1]; D_u[2, 1] = u31[2];
            D_u[0, 2] = u41[0]; D_u[1, 2] = u41[1]; D_u[2, 2] = u41[2];

            // set D_x = D_u, beta = D_u
            //D_u.CopyTo(beta, 0); D_u.CopyTo(D_x, 0);
            beta = (double[,]) D_u.Clone();
            D_x = (double[,])D_u.Clone();

            
            // overwrite beta with inverse(D_u)
            alglib.matinvreport rep; int info;
            alglib.rmatrixinverse(ref beta, out info, out rep);

            // set D_v = 0
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    D_v[i, j] = 0;
                }
            }

            // Calculate F
            //F = new double[3, 3];
            //alglib.rmatrixgemm(3, 3, 3, 1, D_x, 0, 0, 0, beta, 0, 0, 0, 1, ref F, 0, 0);
        }
        public void Initialise()
        {
            D_u = new double[3, 3]; D_x = new double[3, 3]; D_v = new double[3, 3]; beta = new double[3, 3];
            // calculate D_u
            Vector3 u21 = u2 - u1; Vector3 u31 = u3 - u1; Vector3 u41 = u4 - u1;
            D_u[0, 0] = u21[0]; D_u[1, 0] = u21[1]; D_u[2, 0] = u21[2];
            D_u[0, 1] = u31[0]; D_u[1, 1] = u31[1]; D_u[2, 1] = u31[2];
            D_u[0, 2] = u41[0]; D_u[1, 2] = u41[1]; D_u[2, 2] = u41[2];

            // set D_x = D_u, beta = D_u
            //D_u.CopyTo(beta, 0); D_u.CopyTo(D_x, 0);
            beta = (double[,])D_u.Clone();
            D_x = (double[,])D_u.Clone();


            // overwrite beta with inverse(D_u)
            alglib.matinvreport rep; int info;
            alglib.rmatrixinverse(ref beta, out info, out rep);

            // set D_v = 0
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    D_v[i, j] = 0;
                }
            }

            // Calculate F
            //F = new double[3, 3];
            //alglib.rmatrixgemm(3, 3, 3, 1, D_x, 0, 0, 0, beta, 0, 0, 0, 1, ref F, 0, 0);
        }
        public void UpdateNodePositions(Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4)
        {
            Vector3 x21 = x2 - x1; Vector3 x31 = x3 - x1; Vector3 x41 = x4 - x1;
            D_x[0, 0] = x21[0]; D_x[1, 0] = x21[1]; D_x[2, 0] = x21[2];
            D_x[0, 1] = x31[0]; D_x[1, 1] = x31[1]; D_x[2, 1] = x31[2];
            D_x[0, 2] = x41[0]; D_x[1, 2] = x41[1]; D_x[2, 2] = x41[2];
        }

        public void UpdateNodeVelocities(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            Vector3 v21 = v2 - v1; Vector3 v31 = v3 - v1; Vector3 v41 = v4 - v1;
            D_v[0, 0] = v21[0]; D_v[1, 0] = v21[1]; D_v[2, 0] = v21[2];
            D_v[0, 1] = v31[0]; D_v[1, 1] = v31[1]; D_v[2, 1] = v31[2];
            D_v[0, 2] = v41[0]; D_v[1, 2] = v41[1]; D_v[2, 2] = v41[2];
        } 

        public void UpdateStrainForce(double lambda, double mu)
        {
            // F = deformation gradient = D_x * Beta
            double[,] F = new double[3, 3];
            alglib.rmatrixgemm(3, 3, 3, 1, D_x, 0, 0, 0, beta, 0, 0, 0, 1, ref F, 0, 0);
            // Decompose F = QA
            double[] w = new double[3];
            double[,] u = new double[3, 3];
            double[,] vt = new double[3, 3];
            alglib.rmatrixsvd(F, 3, 3, 2, 2, 2, out w, out u, out vt);

            // new F
            double[,] Q = new double[3, 3];
            alglib.rmatrixgemm(3, 3, 3, 1, u, 0, 0, 0, vt, 0, 0, 0, 1, ref Q, 0, 0); // Q = UV^T
            alglib.rmatrixgemm(3, 3, 3, 1, Q, 0, 0, 1, F, 0, 0, 0, 1, ref F, 0, 0); // F = Q^T * F

            // strain and stress
            double[,] strain = new double[3, 3];
            double[,] stress = new double[3, 3];
            double[,] F_t = new double[3, 3];
            double[,] I = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            alglib.rmatrixtranspose(3, 3, F, 0, 0, ref F_t, 0, 0);
            strain = MatrixUtil.MultiplyScalar3x3Matrix(MatrixUtil.AddSub3x3Matrix(F, F_t, true), 0.5);
            strain = MatrixUtil.AddSub3x3Matrix(strain, I, false);
            double lambdaTrace = lambda * MatrixUtil.Trace3x3Matrix(strain);
            double mu2 = 2 * mu;
            stress = MatrixUtil.AddSub3x3Matrix(
                                MatrixUtil.MultiplyScalar3x3Matrix(I, lambdaTrace),
                                MatrixUtil.MultiplyScalar3x3Matrix(strain, mu2), true);

            // calculate force, not using area weight but just 1/4?
            //Vector3 f1 = 
        }

        
    }
}