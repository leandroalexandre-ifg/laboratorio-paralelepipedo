using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Tira do build os assemblies de <see cref="RegrasDePrivacidade.AssembliesForaDoBuild"/>. Eles continuam no
    /// Editor (o pacote do SDK não muda), mas não chegam ao APK. Roda em todo build.
    /// </summary>
    public sealed class FiltroDeAssemblies : IFilterBuildAssemblies
    {
        public int callbackOrder => 0;

        public string[] OnFilterAssemblies(BuildOptions opcoes, string[] assemblies)
        {
            var mantidos = new List<string>(assemblies.Length);
            foreach (string caminho in assemblies)
            {
                if (EhForaDoBuild(caminho)) Debug.Log($"[Release] Fora do build: {Path.GetFileName(caminho)}");
                else mantidos.Add(caminho);
            }

            return mantidos.ToArray();
        }

        /// <summary>Se o assembly (caminho ou nome, com ou sem .dll) está na lista dos que ficam fora do build.</summary>
        public static bool EhForaDoBuild(string caminho)
        {
            string nome = Path.GetFileName(caminho ?? string.Empty);
            if (nome.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) nome = nome.Substring(0, nome.Length - 4);
            foreach (string proibido in RegrasDePrivacidade.AssembliesForaDoBuild)
                if (string.Equals(nome, proibido, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
