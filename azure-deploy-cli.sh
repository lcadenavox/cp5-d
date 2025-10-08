# Scripts Azure CLI para recursos DimDim

set -euo pipefail

# Vari�veis
resourceGroup=${RESOURCE_GROUP:-dimdim-rg}
location=${LOCATION:-brazilsouth}
appName=${APP_NAME:-dimdim-web-$RANDOM}
sqlServer=${SQL_SERVER_NAME:-dimdimsql$RANDOM}
sqlUser=${SQL_ADMIN_USER:-dimdimu}
sqlPass=${SQL_ADMIN_PASSWORD:-"P@ssw0rd!1234"}
sqlDb=${SQL_DB_NAME:-DimDimDb}
insights=${APPINSIGHTS_NAME:-dimdim-ai-$RANDOM}

# Login (se necess�rio)
# az login

# Grupo de recursos
az group create -n "$resourceGroup" -l "$location"

# Application Insights
az monitor app-insights component create -g "$resourceGroup" -l "$location" -a "$insights" --kind web -t web
aiConn=$(az monitor app-insights component show -g "$resourceGroup" -a "$insights" --query connectionString -o tsv)

echo "AI ConnectionString: $aiConn"

# SQL Server + Database
az sql server create -g "$resourceGroup" -l "$location" -n "$sqlServer" -u "$sqlUser" -p "$sqlPass"
# Liberar seu IP atual
myip=$(curl -s https://api.ipify.org)
az sql server firewall-rule create -g "$resourceGroup" -s "$sqlServer" -n AllowMyIP --start-ip-address "$myip" --end-ip-address "$myip"
# Permitir servi�os do Azure (inclui App Service)
az sql server firewall-rule create -g "$resourceGroup" -s "$sqlServer" -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
az sql db create -g "$resourceGroup" -s "$sqlServer" -n "$sqlDb" -e Basic

connStr="Server=tcp:$sqlServer.database.windows.net,1433;Initial Catalog=$sqlDb;Persist Security Info=False;User ID=$sqlUser;Password=$sqlPass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

echo "SQL ConnectionString: $connStr"

# App Service (Linux)
az appservice plan create -g "$resourceGroup" -n "$appName-plan" --sku B1 --is-linux
az webapp create -g "$resourceGroup" -p "$appName-plan" -n "$appName" -r "DOTNET|8.0"

# Configurar appsettings como vari�veis
# 1) Connection strings tipadas (portal > Connection strings)
az webapp config connection-string set -g "$resourceGroup" -n "$appName" \
  --settings SqlServer="$connStr" DefaultConnection="$connStr" \
  --connection-string-type SQLAzure

# 2) App Settings fallbacks
az webapp config appsettings set -g "$resourceGroup" -n "$appName" --settings \
  ConnectionStrings__SqlServer="$connStr" \
  ConnectionStrings__DefaultConnection="$connStr" \
  ApplicationInsights__ConnectionString="$aiConn" \
  APPLICATIONINSIGHTS_CONNECTION_STRING="$aiConn" \
  ASPNETCORE_ENVIRONMENT=Production

# Publicacao local em zip (deploy)
# Executar a partir da raiz do projeto (mesma pasta deste script)
dotnet publish ./cp5-d.csproj -c Release -o publish
(
  cd publish
  zip -qr ../app.zip .
)
az webapp deployment source config-zip -g "$resourceGroup" -n "$appName" --src app.zip

echo "Deploy conclu�do. URL: https://$appName.azurewebsites.net"
