# Dependência química por cocaína: ambiente simulado em Realidade Virtual

Aplicação Unity de **Realidade Virtual** (RV) desenvolvida como Trabalho de Conclusão de Curso (TCC II) de **Engenharia de Software** na **Universidade do Vale do Taquari — Univates** (Lajeado/RS, novembro de 2025).

| | |
|---|---|
| **Autor** | Júlio César Schmidt |
| **Orientador** | Me. Fabrício Pretto |
| **Título** | Dependência química por cocaína: desenvolvimento e avaliação de um ambiente simulado por meio de realidade virtual |
| **Repositório** | https://github.com/julioschmidt/tcc-unity |

> **Atenção:** o conteúdo simula oferta e consumo de substâncias (cocaína/crack) com fins de pesquisa clínica. Não é um jogo recreativo. Uso em contexto experimental deve ocorrer sob supervisão profissional e com aprovação ética.

---

## Sobre o projeto

A dependência por cocaína e derivados (como o crack) é um problema grave de saúde pública. Tratamentos convencionais enfrentam altas taxas de recaída, e ainda não há fármaco com eficácia consistente contra a **fissura** (*craving*).

Este protótipo usa RV acessível (**smartphone + Google Cardboard / VR Box**) para expor pacientes com **Transtorno por Uso de Estimulantes (TUE)** a cenários realistas de risco — rua noturna e balada — com ofertas implícitas e explícitas de drogas, em ambiente **controlado e seguro**.

A validação foi feita com pacientes internados no **Hospital Bruno Born** (Lajeado–RS), sob acompanhamento médico, com coleta de:

- escolhas do usuário no aplicativo (log CSV);
- frequência cardíaca (oxímetro, equipe clínica);
- Escala de Ansiedade de Hamilton (HAM-A) pré/pós sessão;
- questionários e depoimentos qualitativos.

**Protocolo ético:** CAAE `91204925.0.0000.5310`, parecer nº `7.777.080` (Plataforma Brasil).

### Objetivo geral

Desenvolver e avaliar um ambiente virtual imersivo que simule contextos de consumo de substâncias ilícitas, induzindo *craving* de forma controlada, como base para pesquisas futuras no tratamento do TUE.

### Hipótese

A exposição imersiva e controlada a ambientes virtuais de oferta/consumo (ênfase em cocaína) é capaz de induzir *craving* e respostas emocionais/fisiológicas mensuráveis.

### Principais achados (resumo)

Nas sessões (~15 min), a RV induziu fissura e reações fisiológicas/emocionais de forma segura. Houve elevação da frequência cardíaca sobretudo em cenas explícitas e ofertas; não foram relatados enjoo ou desconforto típicos de RV. A ferramenta mostrou potencial como apoio experimental à prevenção de recaída — **não** como instrumento terapêutico pronto para uso clínico isolado.

---

## Experiência do usuário

Fluxo em **primeira pessoa**, locomoção por **joystick** (gamepad) e interação por **gaze** (olhar fixo ~2 s em botões SIM/NÃO).

### Cena 1 — Rua (`Assets/Scenes/Rua.unity`)

Ambiente urbano noturno, iluminação baixa, sons de rua. O usuário percorre uma rua com **seis encontros**:

| # | Situação | Painel SIM/NÃO |
|---|----------|----------------|
| 1 | Personagem fala sobre a dificuldade de ficar sem “pó” | Não |
| 2 | Casal comenta festa e convida o usuário | Sim |
| 3 | Catador pede dinheiro em fissura | Não |
| 4 | Dois homens no banco convidam a “dar um teco” | Sim |
| 5 | Homem isolado fumando cachimbo de crack | Não |
| 6 | Traficante oferece drogas | Sim |

Ao aproximar-se do prédio da balada, a transição automática leva à segunda cena (`PortalTrigger` + fade).

### Cena 2 — Balada (`Assets/NightClub/Scenes/Balada.unity`)

Ambiente interno com pista, bar e mesa de pôquer; luzes e música eletrônica. Progressão roteirizada: após cada encontro, teleporte suave para o próximo ponto (`SceneFlowController`).

| # | Situação | Painel SIM/NÃO |
|---|----------|----------------|
| 1 | Funcionária na entrada sugere pedir algo “a mais” | Sim |
| 2 | Personagem no banheiro consumindo cocaína | Não |
| 3 | Duas mulheres no bar convidam a “deixar a noite mais divertida” | Sim |
| 4 | Mesa de pôquer com drogas; convite a participar | Sim |
| 5 | Pista de dança (sem interação direta) | Não |
| 6 | Funcionária na saída agradece e convida a voltar | — |

Ao final do fluxo, fade para preto e encerramento da sessão.

---

## Stack técnica

| Tecnologia | Uso | Versão (projeto) |
|------------|-----|------------------|
| **Unity** | Motor 3D / RV | `6000.0.48f1` (Unity 6) |
| **C#** | Lógica de jogo e interação | — |
| **Google Cardboard XR Plugin** | Estéreo + tracking via smartphone | `com.google.xr.cardboard` (git) |
| **XR Interaction Toolkit** | Infraestrutura XR | `3.0.8` |
| **Input System** | Joystick / teclado (fallback) | `1.14.0` |
| **TextMesh Pro** | UI do modal | incluso |
| **Mixamo** | Personagens e animações | assets |

Alvo de execução: **Android** + óculos tipo Cardboard/VR Box (baixo custo, portátil).

---

## Arquitetura da solução

**Externa (sessão clínica):** smartphone Android no VR Box, joystick Bluetooth, espelhamento de tela (TV para a equipe), oxímetro operado pela equipe médica.

**Aplicação (Unity):**

1. Cenas 3D (rua / balada) com NPCs, áudio espacial e ambiente.
2. Scripts C# controlam proximidade, falas, modal de escolha, teleporte/fade e log.
3. Interação por *gaze* (raycast a partir da câmera VR) nos botões do modal.
4. Escolhas gravadas em CSV no armazenamento persistente do app.

```
Paciente (Cardboard + joystick)
        │
        ▼
┌───────────────────────────────┐
│  Unity App (Android)          │
│  Rua ──portal──► Balada       │
│  NPCs → áudio → modal gaze    │
│  CSV: Aceitou / Negou         │
└───────────────────────────────┘
        │
        ▼
Equipe (TV espelhada + oxímetro + escalas)
```

---

## Estrutura do repositório

```
Assets/
  Scenes/Rua.unity              # Cena 1 (build index 0)
  NightClub/Scenes/Balada.unity # Cena 2 (build index 1)
  scripts/                      # Gaze, modal, movimento, CSV, portal, narrador…
  festa/scripts/                # Fluxo da balada (SceneFlowController, NPCTrigger…)
  Characters/                   # Avatares e animações
  items/                        # Props (cocaína, cachimbo, etc.)
  sounds/                       # Áudios de ambiente e falas
  NightClub/                    # Pack visual da balada
  Neon Pack/, Low poly city…    # Ambientes / props
Packages/manifest.json
ProjectSettings/
```

Pastas geradas pelo Unity (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`) **não** entram no Git — o editor as recria ao abrir o projeto.

### Scripts principais

| Script | Papel |
|--------|--------|
| `MotionObjectController` | Locomoção (CharacterController + gamepad/teclado) |
| `CustomGazeInputModule` | Raycast de olhar a partir da câmera VR |
| `GazeButton` | Clique por gaze (~2 s) com feedback visual |
| `CustomVRModal` (`VrModal.cs`) | Painel SIM/NÃO, congela fluxo e dispara CSV |
| `CSVHandler` | Persiste `Timestamp,Choice` em `modal_choices.csv` |
| `NarratorManager` | Áudio 2D (narrador) e 3D (NPC) |
| `NPCTrigger` / `NPCModalTrigger` | Proximidade → fala → modal → avanço |
| `SceneFlowController` | Teleportes com fade entre pontos da balada |
| `PortalTrigger` | Transição Rua → Balada |
| `FadeController` | Fade in/out de tela |

---

## Como abrir o projeto

1. Instale o **Unity Hub** e o editor **6000.0.48f1** (ou compatível da série Unity 6).
2. Hub → **Add** → selecione esta pasta.
3. Na primeira abertura, aguarde a regeneração de `Library/` (pode demorar).
4. Confirme no **Build Settings** as cenas:
   - `Assets/Scenes/Rua.unity`
   - `Assets/NightClub/Scenes/Balada.unity`
5. Para testar no editor: XR Mock HMD / play mode; para experimento: build **Android** com Cardboard XR configurado.

### Build Android (resumo)

1. File → Build Settings → Android.
2. Player Settings: API mínima compatível com o Cardboard XR Plugin; orientação landscape.
3. XR Plug-in Management: loader Cardboard ativo.
4. Build e instale o `.apk` no aparelho; use com VR Box + joystick Bluetooth.

### Logs de escolha

Arquivo gerado em runtime:

`Application.persistentDataPath/modal_choices.csv`

Formato:

```csv
Timestamp,Choice
2025-09-19 20:03:11,Aceitou
2025-09-19 20:04:02,Negou
```

No Android, `persistentDataPath` tipicamente fica sob o armazenamento interno do app (acessível via `adb` ou ferramentas de arquivo com permissão).

---

## Requisitos funcionais (implementados)

| ID | Descrição |
|----|-----------|
| RF01 | Ambiente virtual de rua escura realista |
| RF02 | Avatares com ofertas implícitas/explícitas |
| RF03 | Falas audíveis dos avatares |
| RF04 | Painel de escolha com SIM e NÃO |
| RF05 | Registro das escolhas do usuário |
| RF06 | Ambiente virtual de balada |
| RF07 | Variação de luminosidade |
| RF08 | Variação sonora do ambiente |

**Não funcionais (alvo):** smartphones de médio custo + Cardboard; estabilidade sem travamentos; logs; latência de interação baixa; minimizar enjoo de RV.

---

## Limitações e trabalhos futuros

Conforme o monógrafo:

- Sessões curtas; curva de imersão sugere benefício de durações maiores.
- Cenários domésticos (casa/quarto) poderiam aumentar validade ecológica.
- FC medida com oxímetro manual — ideal integrar sensores em tempo real.
- Polimento de animações e naturalidade das interações.
- Amostras maiores e acompanhamento longitudinal.

---

## Aviso ético e de conteúdo

Este software foi desenvolvido para **pesquisa acadêmica** com pacientes maiores de 18 anos, estáveis, internados, com TCLE e supervisão médica. Contém representações explícitas de uso e oferta de drogas.

**Não use** com pessoas em situação de vulnerabilidade sem protocolo ético e equipe clínica. O autor e a instituição não se responsabilizam por uso fora do contexto previsto.

---

## Citação sugerida

```
SCHMIDT, Júlio César. Dependência química por cocaína: desenvolvimento e
avaliação de um ambiente simulado por meio de realidade virtual. 2025.
Monografia (Bacharelado em Engenharia de Software) — Universidade do Vale do
Taquari - Univates, Lajeado, 2025.
```

---

## Licença e assets

Código de autoria do TCC: consulte o autor para termos de uso/redistribuição.

Pacotes de terceiros (NightClub, Neon Pack, NPCs, Mixamo, TextMesh Pro, Google Cardboard SDK, etc.) mantêm as licenças originais dos respectivos fornecedores — verifique antes de republicar ou comercializar builds.
