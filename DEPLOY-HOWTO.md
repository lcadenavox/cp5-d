# HowTo: Deploy DimDim (.NET 8 MVC) na Azure com SQL Database e Application Insights

Pré-requisitos
- Azure CLI instalada e logada (az login)
- PowerShell 7+ (Windows) ou Bash (WSL/Linux/macOS)
- Dotnet SDK 8 instalado
- Repositório GitHub (opcional se usar GitHub Actions)

Arquitetura
- App Service (Linux ou Windows)
- Azure SQL Database (PaaS)
- Application Insights

Passos resumidos
1) Banco de dados
- Execute o script `Database/ddl.sql` no Azure SQL (ou crie via migrations do EF: `dotnet ef database update`).
- Crie o servidor e o DB com CLI (script abaixo) e libere IP local.

2) Aplicação (.NET 8)
- Configure `ConnectionStrings:SqlServer` no `appsettings.json` ou em `App Service > Configuration`.
- Configure a connection string do Application Insights em `ApplicationInsights:ConnectionString`.

3) Deploy
- Opção A: Azure CLI com Zip Deploy (PowerShell em Windows: `azure-deploy-cli.ps1`; Bash: `azure-deploy-cli.sh`)
- Opção B: GitHub Actions com publish profile

4) Monitoramento
- Habilitado via pacote `Microsoft.ApplicationInsights.AspNetCore` + connection string.

Scripts Azure CLI (exemplo)

Opção Windows (PowerShell)

1. Abra um PowerShell na pasta do projeto (`cp5-d`).
2. Execute:

```
./azure-deploy-cli.ps1 -ResourceGroup dimdim-rg -Location brazilsouth -AppName dimdim-web-1234 `
  -SqlServerName dimdimsql1234 -SqlAdminUser dimdimu -SqlAdminPassword 'P@ssw0rd!1234' -SqlDbName DimDimDb `
  -InsightsName dimdim-ai-1234
```

Ao final, anote a URL mostrada. Para parâmetros omitidos, o script gera nomes randômicos.

Opção Bash (WSL/Linux/macOS)

resourceGroup=dimdim-rg
location=brazilsouth
appName=dimdim-web-$RANDOM
sqlServer=dimdimsql$RANDOM
sqlUser=dimdimu
sqlPass='P@ssw0rd!1234'
sqlDb=DimDimDb
insights=dimdim-ai-$RANDOM

az group create -n $resourceGroup -l $location
az monitor app-insights component create -g $resourceGroup -l $location -a $insights --kind web -t web
aiConn=$(az monitor app-insights component show -g $resourceGroup -a $insights --query connectionString -o tsv)

az sql server create -g $resourceGroup -l $location -n $sqlServer -u $sqlUser -p $sqlPass
myip=$(curl -s https://api.ipify.org)
az sql server firewall-rule create -g $resourceGroup -s $sqlServer -n AllowMyIP --start-ip-address $myip --end-ip-address $myip
az sql db create -g $resourceGroup -s $sqlServer -n $sqlDb -e Basic

connStr="Server=tcp:$sqlServer.database.windows.net,1433;Initial Catalog=$sqlDb;Persist Security Info=False;User ID=$sqlUser;Password=$sqlPass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az appservice plan create -g $resourceGroup -n $appName-plan --sku B1 --is-linux
az webapp create -g $resourceGroup -p $appName-plan -n $appName -r "DOTNETCORE|8.0"

az webapp config appsettings set -g $resourceGroup -n $appName --settings \
  ConnectionStrings__SqlServer="$connStr" \
  ApplicationInsights__ConnectionString="$aiConn"

# Deploy (Zip)
dotnet publish ./cp5-d.csproj -c Release -o publish
(cd publish && zip -r ../app.zip .)
az webapp deployment source config-zip -g $resourceGroup -n $appName --src app.zip

# Aplicar DDL no Azure SQL (opcional se usar migrations)
# Use Azure Data Studio/SSMS ou sqlcmd com o arquivo Database/ddl.sql

GitHub Actions (arquivo .github/workflows/azure-webapp.yml)
- build dotnet, upload artefato, deploy com publish profile

Testes/Validação para o vídeo
- CRUD em Lojas, Carros e Vendedores mostrando persistência no Azure SQL (mostre as linhas inseridas/alteradas/deletadas após cada operação)
- Verificar requisições/telemetria no Application Insights (Transactions/Failures/Traces)

API JSON (se usar API)
- Envie exemplos JSON de requisições e respostas para GET/POST/PUT/DELETE em um arquivo `api-examples.json` na raiz.
