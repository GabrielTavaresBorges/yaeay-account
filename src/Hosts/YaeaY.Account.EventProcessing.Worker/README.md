# EventProcessing.Worker

Host interno para processar eventos publicados pela Outbox e manter read models do
Account. Ele não contém regras de negócio nem substitui as camadas de `src`.

Por padrão, RabbitMQ e o backfill estão desabilitados. A ativação exige configuração
explícita por ambiente, sem registrar connection strings, senhas ou tokens no
repositório.

## Configurações necessárias para integração

- `ConnectionStrings__ReadConnection`: conexão que pode ler `account_write` e gravar
  em `account_read`.
- `Messaging__RabbitMq__Enabled=true`: habilita o consumidor.
- `Messaging__RabbitMq__UserName` e `Messaging__RabbitMq__Password`: credenciais do
  broker fornecidas por secret do ambiente.
- `ReadModels__Rebuild__RebuildMyDataOnStartup=true`: executa uma única reconstrução
  de `UserMyData` ao iniciar o host. Desabilitar novamente após a conclusão.
- `ReadModels__Rebuild__RebuildAdministrationOnStartup=true`: reconstrói as projeções
  administrativas somente quando as tabelas administrativas de escrita e leitura já
  estiverem aplicadas.

O Worker declara a fila principal e a DLQ somente quando RabbitMQ está habilitado.
Uma falha de projeção é direcionada para `account.read-model.dead-letter` e requer
análise antes de qualquer reprocessamento.

## Garantia de entrega

O processamento local da Outbox (por exemplo, o envio do e-mail de confirmação) e a
publicação no RabbitMQ possuem confirmações separadas. Uma indisponibilidade do broker
não reenvia e-mails já processados: a publicação fica pendente em `OutboxMessages` até
receber confirmação do RabbitMQ. Eventos processados antes da ativação do broker também
ficam elegíveis para uma publicação única depois da evolução do schema da Outbox.
