# DimDim – .NET 8 MVC (Azure SQL + Application Insights)

Aplicação web MVC (.NET 8) para o estudo de caso DimDim, com persistência em Azure SQL Database (PaaS) e monitoração via Application Insights. Inclui scripts de deploy com Azure CLI (PowerShell e Bash) e um guia completo de implantação.

## Visão geral
- Framework: .NET 8, ASP.NET Core MVC
- Persistência: EF Core (SQL Server) → Azure SQL Database
- Monitoração: Application Insights
- Deploy: Azure CLI (scripts `azure-deploy-cli.ps1` e `azure-deploy-cli.sh`)

## Estrutura relevante
- `Program.cs`: Configuração do pipeline, EF Core e Application Insights
- `Data/` e `Models/`: DbContext e entidades (`Loja`, `Carro`, `Vendedor`)
- `Controllers/` + `Views/`: CRUD completo
- `Database/ddl.sql`: Script DDL com tabelas, PK e FKs (master-detail)
- `azure-deploy-cli.ps1` e `azure-deploy-cli.sh`: Scripts de provisionamento e deploy
- `DEPLOY-HOWTO.md`: Passo a passo detalhado de implantação
- `docs/ENTREGA_CHECKLIST.md`: Checklist de entrega e template do PDF
- `api-examples.json`: Exemplos JSON dos objetos (caso use API)

## Executar localmente
Requisitos: .NET SDK 8, SQL local (LocalDB opcional) e conexão definida em `appsettings.json`.

1. Restaurar e compilar
2. Executar a aplicação

Observação: Em produção (App Service), a connection string e a conexão do Application Insights são definidas como App Settings pelo script de deploy.

## Deploy (resumo)
Use o PowerShell no Windows:

1. Faça login na Azure (se necessário): `az login`
2. Execute o script:
   `./azure-deploy-cli.ps1 -ResourceGroup dimdim-rg -Location brazilsouth -AppName dimdim-web-1234 -SqlServerName dimdimsql1234 -SqlAdminUser dimdimu -SqlAdminPassword 'P@ssw0rd!1234' -SqlDbName DimDimDb -InsightsName dimdim-ai-1234`
3. Ao final, acesse a URL: `https://<appname>.azurewebsites.net`

Detalhes completos em `DEPLOY-HOWTO.md`.

## Monitoração
A aplicação usa `Microsoft.ApplicationInsights.AspNetCore` e coleta telemetria por padrão. Configure `ApplicationInsights:ConnectionString` no App Service para habilitar em produção.

## Licença
Uso acadêmico.
