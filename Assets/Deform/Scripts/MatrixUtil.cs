using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatrixUtil
{
    public static double[,] AddSub3x3Matrix(double[,] a, double[,] b, bool add)
    {
        double[,] c = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (add) { c[i, j] = a[i, j] + b[i, j]; }
                else { c[i, j] = a[i, j] - b[i, j]; }
            }
        }
        return c;
    }

    public static double[,] MultiplyScalar3x3Matrix(double[,] a, double s)
    {
        double[,] c = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                c[i, j] = a[i, j] * s;
            }
        }
        return c;
    }

    public static double Trace3x3Matrix(double[,] a)
    {
        double sum = 0;
        for (int i = 0; i < 3; i++)
        {
            sum += a[i, i];
        }
        return sum;
    }
}
