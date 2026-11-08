# Build e envio ao canal ALPHA

O app só é distribuído pelo canal **ALPHA** do Meta Horizon Developer Dashboard, com o `ovr-platform-util`.
App ID: **1398124376723979**. Package name: **`br.edu.ifg.laboratorioparalelepipedo`** (nunca mudar).

Scripts em `Assets/_Projeto/Scripts/Editor/Release/` (menu **Laboratório › Release**).

## Configuração local (fora do repositório)

Pasta `~/.laboratorioparalelepipedo/` (modo 700):

| Arquivo | Conteúdo |
|---|---|
| `laboratorioparalelepipedo-release.keystore` | Chave de assinatura (PKCS12, RSA 4096, alias `laboratorioparalelepipedo`) |
| `deploy.conf` (modo 600) | `KEYSTORE_PATH`, `KEY_ALIAS`, `AGE_GROUP`, `OVR_PLATFORM_UTIL`. Sem segredos. |

Segredos, **nunca** no repositório, em log ou em mensagem:

| Segredo | Variável de ambiente | Ou item do Keychain do macOS |
|---|---|---|
| Senha do keystore | `LABORATORIO_KEYSTORE_PASS` | `laboratorioparalelepipedo-keystore` |
| App Secret da Meta | `LABORATORIO_META_APP_SECRET` | `laboratorioparalelepipedo-app-secret` |

Os nomes são diferentes dos do Missão Coração de propósito: um app nunca usa o segredo do outro.

> **Backup do keystore.** Depois do primeiro envio, toda versão nova precisa ser assinada com a mesma chave.
> Guarde uma cópia do `.keystore` e da senha (Keychain › `laboratorioparalelepipedo-keystore`) num lugar seguro
> fora deste Mac. Sem eles, o app não recebe mais atualizações na loja.

`AGE_GROUP` (obrigatório para enviar): `CHILDREN`, `TEENS_AND_ADULTS` ou `MIXED_AGES`.

## Comandos

O Unity precisa estar fechado neste projeto.

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity

# 1. Gerar e conferir o APK (Builds/LaboratorioParalelepipedo-<versionCode>.apk), sem enviar
"$UNITY" -batchmode -nographics -projectPath . -buildTarget Android \
  -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.GerarRelease -quit -logFile Logs/release.log

# 2. Enviar um APK já gerado ao ALPHA (valida na Meta com --validate antes de enviar)
export LABORATORIO_META_APP_SECRET=...   # ou deixe no Keychain
"$UNITY" -batchmode -nographics -projectPath . -buildTarget Android \
  -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.EnviarApkAlpha \
  -apk Builds/LaboratorioParalelepipedo-<versionCode>.apk -quit -logFile Logs/envio.log
```

Também há `ValidarAlpha` (gera e só valida) e `GerarEEnviarAlpha` (gera e envia). Os envios ficam registrados
em `Builds/envios.txt`.

O `versionCode` é a data e hora UTC do build (`aaDDDHHmm`: ano, dia do ano, hora, minuto), sempre crescente.

## Lista de verificação do APK (automática)

O build é recusado se o APK tiver algum destes problemas (`RelatorioDoApk`, conferido com `aapt2`, `apksigner`
e uma varredura do `classes.dex` e do `global-metadata.dat` do IL2CPP):

- package name ou versionCode diferente do esperado; APK depurável; sem assinatura ou com certificado de debug;
- permissão proibida: internet e estado de rede/Wi-Fi, microfone, Bluetooth, câmera, rastreamento de mãos,
  olhos, rosto e corpo, cena e âncoras; qualquer permissão nova fora da lista aceita precisa de revisão;
- meta-data de telemetria (`com.oculus.telemetry.project_guid`);
- classes de anúncios, analytics ou Firebase no `classes.dex`;
- código dos módulos de rede do Meta SDK no IL2CPP (`Meta.XR.AI.AgentBridge`, `ImmersiveDebugger.DevAgent`,
  `BuildingBlocks.AIBlocks`, `MultiplayerBlocks`);
- `libISDKEngineTelemetry.so` ou bibliotecas de 32 bits; falta da arm64-v8a.

Como a internet fica de fora:

- `Assets/Plugins/Android/AndroidManifest.xml` remove (`tools:node="remove"`) as permissões que os pacotes
  pediriam;
- `FiltroDeAssemblies` tira do build os assemblies de rede do Meta SDK (`meta.xr.ai.agentbridge` e afins), sem
  mexer no pacote;
- os testes EditMode (`ReleaseTests`) conferem o manifesto, os pacotes e o filtro.
