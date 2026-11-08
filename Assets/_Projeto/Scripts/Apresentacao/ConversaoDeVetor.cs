using LaboratorioParalelepipedo.Nucleo;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>Conversão entre o <see cref="Vetor3"/> do núcleo (double) e o Vector3 do Unity (float).</summary>
    public static class ConversaoDeVetor
    {
        public static Vetor3 ParaNucleo(Vector3 v) => new Vetor3(v.x, v.y, v.z);

        public static Vector3 ParaUnity(Vetor3 v) => new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }
}
