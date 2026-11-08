using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Ponto ou vetor no espaço, em C# puro (o núcleo não usa UnityEngine).
    /// Convenção do núcleo: x = comprimento (a), y = altura (c), z = largura (b, da frente para o fundo).
    /// </summary>
    public readonly struct Vetor3 : IEquatable<Vetor3>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vetor3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Comprimento do vetor.</summary>
        public double Comprimento => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>Distância entre dois pontos.</summary>
        public static double Distancia(Vetor3 p, Vetor3 q) => (q - p).Comprimento;

        public static Vetor3 operator +(Vetor3 p, Vetor3 q) => new Vetor3(p.X + q.X, p.Y + q.Y, p.Z + q.Z);

        public static Vetor3 operator -(Vetor3 p, Vetor3 q) => new Vetor3(p.X - q.X, p.Y - q.Y, p.Z - q.Z);

        public bool Equals(Vetor3 outro) => X.Equals(outro.X) && Y.Equals(outro.Y) && Z.Equals(outro.Z);

        public override bool Equals(object obj) => obj is Vetor3 outro && Equals(outro);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = X.GetHashCode();
                h = (h * 397) ^ Y.GetHashCode();
                return (h * 397) ^ Z.GetHashCode();
            }
        }

        public override string ToString() => $"({X}; {Y}; {Z})";
    }
}
