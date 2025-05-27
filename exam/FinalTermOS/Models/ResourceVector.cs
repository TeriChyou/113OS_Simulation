// Models/ResourceVector.cs
using System;
using System.Linq;
using System.Text; // For StringBuilder

namespace FinalTermOS.Models
{
    public class ResourceVector
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }

        public ResourceVector(int a, int b, int c, int d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        // Helper constructor for cloning or creating from an array
        public ResourceVector(int[] values)
        {
            if (values == null || values.Length != 4)
                throw new ArgumentException("Resource vector must have exactly 4 values (A, B, C, D).");
            A = values[0];
            B = values[1];
            C = values[2];
            D = values[3];
        }

        // Overload + operator for vector addition
        public static ResourceVector operator +(ResourceVector v1, ResourceVector v2)
        {
            return new ResourceVector(v1.A + v2.A, v1.B + v2.B, v1.C + v2.C, v1.D + v2.D);
        }

        // Overload - operator for vector subtraction
        public static ResourceVector operator -(ResourceVector v1, ResourceVector v2)
        {
            return new ResourceVector(v1.A - v2.A, v1.B - v2.B, v1.C - v2.C, v1.D - v2.D);
        }

        // Helper method to check if current vector is less than or equal to another vector (component-wise)
        public bool LessThanOrEqual(ResourceVector other)
        {
            return this.A <= other.A && this.B <= other.B && this.C <= other.C && this.D <= other.D;
        }

        // Convert to int array
        public int[] ToArray()
        {
            return new int[] { A, B, C, D };
        }

        public override string ToString()
        {
            return $"({A}, {B}, {C}, {D})";
        }
    }
}