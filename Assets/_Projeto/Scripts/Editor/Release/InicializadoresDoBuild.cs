using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Lê e limpa o RuntimeInitializeOnLoads.json do player: a lista dos métodos [RuntimeInitializeOnLoadMethod] que o
    /// Unity chama ao abrir o app. O <see cref="FiltroDeAssemblies"/> tira assemblies do build depois de essa lista ser
    /// montada, e ela ficava apontando para eles (na fase SubsystemRegistration, a mesma em que o Input System e o
    /// mapa de ações do OVRInput se preparam). Aqui saem as entradas de assemblies que não estão no build.
    /// </summary>
    public static class InicializadoresDoBuild
    {
        [Serializable]
        private sealed class Lista
        {
            public Inicializador[] root = new Inicializador[0];
        }

        [Serializable]
        private sealed class Inicializador
        {
            public string assemblyName;
            public string nameSpace;
            public string className;
            public string methodName;
            public int loadTypes;
            public bool isUnityClass;
        }

        [Serializable]
        private sealed class Assemblies
        {
            public string[] names = new string[0];
            public int[] types = new int[0];
        }

        /// <summary>Tira as entradas cujo assembly <paramref name="foraDoBuild"/> diz que saiu; devolve o JSON novo.</summary>
        public static string Limpar(string json, Func<string, bool> foraDoBuild, out int removidos)
        {
            var lista = JsonUtility.FromJson<Lista>(json) ?? new Lista();
            var mantidos = new List<Inicializador>(lista.root.Length);
            foreach (var item in lista.root)
                if (!foraDoBuild(item.assemblyName)) mantidos.Add(item);
            removidos = lista.root.Length - mantidos.Count;
            lista.root = mantidos.ToArray();
            return JsonUtility.ToJson(lista);
        }

        /// <summary>
        /// Assemblies citados no RuntimeInitializeOnLoads.json que não estão no ScriptingAssemblies.json (os que o
        /// app tem). A lista precisa sair vazia.
        /// </summary>
        public static List<string> Orfaos(string inicializadores, string assemblies)
        {
            var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string nome in (JsonUtility.FromJson<Assemblies>(assemblies) ?? new Assemblies()).names)
                existentes.Add(nome.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? nome.Substring(0, nome.Length - 4) : nome);

            var orfaos = new List<string>();
            foreach (var item in (JsonUtility.FromJson<Lista>(inicializadores) ?? new Lista()).root)
                if (!existentes.Contains(item.assemblyName) && !orfaos.Contains(item.assemblyName))
                    orfaos.Add(item.assemblyName);
            return orfaos;
        }
    }
}
