using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// No projeto Gradle gerado, tira do RuntimeInitializeOnLoads.json os inicializadores dos assemblies que o
    /// <see cref="FiltroDeAssemblies"/> deixou fora do build (ver <see cref="InicializadoresDoBuild"/>).
    /// </summary>
    public sealed class LimparInicializadoresDoBuild : IPostGenerateGradleAndroidProject
    {
        public const string CaminhoNoProjeto = "src/main/assets/bin/Data/RuntimeInitializeOnLoads.json";

        public int callbackOrder => 10001;

        public void OnPostGenerateGradleAndroidProject(string caminho)
        {
            string arquivo = Path.Combine(caminho, CaminhoNoProjeto.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(arquivo))
            {
                Debug.LogWarning($"[Release] {CaminhoNoProjeto} não encontrado no projeto Gradle.");
                return;
            }

            string limpo = InicializadoresDoBuild.Limpar(File.ReadAllText(arquivo), FiltroDeAssemblies.EhForaDoBuild,
                out int removidos);
            if (removidos == 0) return;
            File.WriteAllText(arquivo, limpo);
            Debug.Log($"[Release] {removidos} inicializador(es) de assemblies fora do build retirados do RuntimeInitializeOnLoads.json.");
        }
    }
}
