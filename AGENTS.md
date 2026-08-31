# YaeaY Account App — instruções públicas

## Estrutura

- `src/YaeaY.Account.Domain`: modelo e regras de domínio.
- `src/YaeaY.Account.Application`: casos de uso, contratos e validação de entrada.
- `src/YaeaY.Account.Infrastructure`: persistência e adaptadores técnicos.
- `src/YaeaY.Account.Presentation/YaeaY.Account.Presentation.Server`: API e host.
- `src/YaeaY.Account.Presentation/yaeay.account.presentation.client`: cliente Vue.
- `tests`: testes unitários de Domain, Application e Infrastructure.

`App` é exclusivamente o repositório da solução. Não armazene aqui Docker Compose,
Dockerfiles, Nginx, manifests, templates de ambiente, scripts de publicação, runbooks,
Specs ou qualquer configuração operacional da VM. Esses artefatos pertencem ao
contexto privado `Account/Infrastructure` e `Account/Automation`.

O único artefato de automação permitido no App é a configuração declarativa exigida
pela plataforma de hospedagem, como `.github/workflows/ci.yml`. Ela pode conter CI e
um despacho mínimo para CD em runner privado, desde que não inclua paths internos,
infraestrutura, scripts de publicação ou secrets. As regras de branches, pull
requests, versionamento e a execução privada pertencem a
`Account/Automation/SourceControl/Standard.md` e `Account/Automation/Deployment`.

## Regras de trabalho

- Produza textos de interface, títulos, descrições e comentários de revisão em português do Brasil, com codificação UTF-8 correta.

- Inspecione código e testes próximos antes de alterar comportamento.
- Preserve alterações locais preexistentes e não reverta trabalho do usuário.
- Mantenha dependências apontando para dentro: Domain não depende das demais camadas;
  Application depende do Domain; Infrastructure e Presentation implementam ou usam
  contratos das camadas internas.
- Não introduza secrets, credenciais, dados pessoais reais ou detalhes internos de
  infraestrutura no repositório.
- Não altere contrato público, schema, dependência de produção ou arquitetura sem
  explicitar o impacto.
- Não execute commit, push, SQL real ou deploy sem autorização específica.
- Nunca trabalhe diretamente em `develop` ou `main`: use uma branch curta derivada de
  `develop` e entregue-a por pull request para `develop`.
- Valide primeiro o menor escopo afetado e depois a camada ou solução aplicável.

## Comandos de validação

Na raiz do repositório:

```powershell
dotnet build YaeaY.Account.slnx
dotnet test YaeaY.Account.slnx
```

No diretório do cliente:

```powershell
npm.cmd run type-check
npm.cmd run build
```

Não declare uma mudança concluída sem informar comandos executados, aprovações,
falhas, testes ignorados e validações não realizadas.
