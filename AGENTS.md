# YaeaY Account App — instruções públicas

## Estrutura

- `src/YaeaY.Account.Domain`: modelo e regras de domínio.
- `src/YaeaY.Account.Application`: casos de uso, contratos e validação de entrada.
- `src/YaeaY.Account.Infrastructure`: persistência e adaptadores técnicos.
- `src/YaeaY.Account.Presentation/YaeaY.Account.Presentation.Server`: API e host.
- `src/YaeaY.Account.Presentation/yaeay.account.presentation.client`: cliente Vue.
- `tests`: testes unitários de Domain, Application e Infrastructure.

## Regras de trabalho

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
