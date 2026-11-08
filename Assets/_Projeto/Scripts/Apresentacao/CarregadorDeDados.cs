using System;
using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Lê textos.json e desafios.json (TextAsset): o núcleo confere as chaves do JSON bruto
    /// (<see cref="VerificadorDeChaves"/>), o JsonUtility preenche os DTOs e o núcleo confere o conteúdo.
    /// Qualquer problema vira <see cref="DadosInvalidosException"/> com mensagem em português.
    /// </summary>
    public static class CarregadorDeDados
    {
        public static CatalogoDeTextos CarregarTextos(TextAsset arquivo) =>
            CarregarTextos(Conteudo(arquivo, "textos.json"), NomeDe(arquivo, "textos.json"));

        public static IReadOnlyList<Desafio> CarregarDesafios(TextAsset arquivo) =>
            CarregarDesafios(Conteudo(arquivo, "desafios.json"), NomeDe(arquivo, "desafios.json"));

        /// <exception cref="DadosInvalidosException">Se o JSON for inválido ou algum texto estiver errado.</exception>
        public static CatalogoDeTextos CarregarTextos(string json, string nomeDoArquivo = "textos.json")
        {
            var errosDeChave = VerificadorDeChaves.VerificarTextos(json, nomeDoArquivo);
            return Juntar(errosDeChave, nomeDoArquivo, () =>
                LeitorDeTextos.Ler(Desserializar<ArquivoDeTextosDto>(json, nomeDoArquivo), nomeDoArquivo));
        }

        /// <exception cref="DadosInvalidosException">Se o JSON for inválido ou algum desafio estiver errado.</exception>
        public static IReadOnlyList<Desafio> CarregarDesafios(string json, string nomeDoArquivo = "desafios.json")
        {
            var errosDeChave = VerificadorDeChaves.VerificarDesafios(json, nomeDoArquivo);
            return Juntar(errosDeChave, nomeDoArquivo, () =>
                LeitorDeDesafios.Ler(Desserializar<ArquivoDeDesafiosDto>(json, nomeDoArquivo), nomeDoArquivo));
        }

        /// <summary>Lê os dados e junta os erros de chave (primeiro) com os erros de conteúdo numa só exceção.</summary>
        private static T Juntar<T>(IReadOnlyList<string> errosDeChave, string nomeDoArquivo, Func<T> ler)
        {
            var erros = new List<string>(errosDeChave);
            T resultado = default;
            try
            {
                resultado = ler();
            }
            catch (DadosInvalidosException e)
            {
                erros.AddRange(e.Erros);
            }

            if (erros.Count > 0) throw new DadosInvalidosException(nomeDoArquivo, erros);
            return resultado;
        }

        private static T Desserializar<T>(string json, string nomeDoArquivo) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(json) ??
                       throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");
            }
            catch (ArgumentException e)
            {
                throw new DadosInvalidosException(nomeDoArquivo,
                    $"o JSON está mal formado ({e.Message}). Confira vírgulas, aspas e chaves.");
            }
        }

        private static string Conteudo(TextAsset arquivo, string nomePadrao)
        {
            if (arquivo == null)
                throw new DadosInvalidosException(nomePadrao, "o arquivo não foi ligado ao componente.");
            return arquivo.text;
        }

        private static string NomeDe(TextAsset arquivo, string nomePadrao) =>
            arquivo != null ? arquivo.name + ".json" : nomePadrao;
    }
}
