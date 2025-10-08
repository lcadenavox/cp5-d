<#
 Azure CLI deployment script (PowerShell) for DimDim (.NET 8 MVC)
 Provisions: Resource Group, Application Insights, Azure SQL Server+DB,
 App Service Plan + Web App, sets connection strings, and Zip Deploys the app.

 Usage (PowerShell):
   ./azure-deploy-cli.ps1 -ResourceGroup dimdim-rg -Location brazilsouth -AppName dimdim-web-1234 `
     -SqlServerName dimdimsql1234 -SqlAdminUser dimdimu -SqlAdminPassword 'P@ssw0rd!1234' -SqlDbName DimDimDb `
     -InsightsName dimdim-ai-1234

 If parameters are omitted, sensible defaults with randomness are used.
#>

param(
  [string]$ResourceGroup = "dimdim-rg",
  [string]$Location = "brazilsouth",
  [string]$AppName = "dimdim-web-$([System.Random]::new().Next(1000,9999))",
  [string]$SqlServerName = "dimdimsql$([System.Random]::new().Next(1000,9999))",
  [string]$SqlAdminUser = "dimdimu",
  [string]$SqlAdminPassword = "P@ssw0rd!1234",
  [string]$SqlDbName = "DimDimDb",
  [string]$InsightsName = "dimdim-ai-$([System.Random]::new().Next(1000,9999))",
  [switch]$SkipDeploy
)

set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "==> Azure login (if needed)" -ForegroundColor Cyan
# az login   # Uncomment if not already logged in

Write-Host "==> Create/ensure Resource Group: $ResourceGroup in $Location" -ForegroundColor Cyan
az group create -n $ResourceGroup -l $Location | Out-Null

Write-Host "==> Create Application Insights: $InsightsName" -ForegroundColor Cyan
az monitor app-insights component create -g $ResourceGroup -l $Location -a $InsightsName --kind web -t web | Out-Null
$aiConn = az monitor app-insights component show -g $ResourceGroup -a $InsightsName --query connectionString -o tsv
Write-Host "AI ConnectionString: $aiConn"

Write-Host "==> Create Azure SQL Server: $SqlServerName and DB: $SqlDbName" -ForegroundColor Cyan
az sql server create -g $ResourceGroup -l $Location -n $SqlServerName -u $SqlAdminUser -p $SqlAdminPassword | Out-Null

# Allow local IP and Azure services
$myip = (Invoke-RestMethod -Uri 'https://api.ipify.org')
az sql server firewall-rule create -g $ResourceGroup -s $SqlServerName -n AllowMyIP --start-ip-address $myip --end-ip-address $myip | Out-Null
az sql server firewall-rule create -g $ResourceGroup -s $SqlServerName -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 | Out-Null

az sql db create -g $ResourceGroup -s $SqlServerName -n $SqlDbName -e Basic | Out-Null

$connStr = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$SqlDbName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
Write-Host "SQL ConnectionString: $connStr"

Write-Host "==> Create App Service Plan and Web App: $AppName" -ForegroundColor Cyan
# Linux plan + .NET 8 runtime (use DOTNET|8.0 for current stack names)
az appservice plan create -g $ResourceGroup -n "$AppName-plan" --sku B1 --is-linux | Out-Null
az webapp create -g $ResourceGroup -p "$AppName-plan" -n $AppName -r "DOTNET|8.0" | Out-Null

Write-Host "==> Configure app settings (connection strings and environment)" -ForegroundColor Cyan
# 1) Set typed connection strings (visible em Configuration > Connection strings)
az webapp config connection-string set -g $ResourceGroup -n $AppName `
  --settings SqlServer="$connStr" DefaultConnection="$connStr" `
  --connection-string-type SQLAzure | Out-Null

# 2) Set app settings fallback keys (Program.cs lê essas variáveis se necessário)
az webapp config appsettings set -g $ResourceGroup -n $AppName --settings `
  ConnectionStrings__SqlServer="$connStr" `
  ConnectionStrings__DefaultConnection="$connStr" `
  ApplicationInsights__ConnectionString="$aiConn" `
  APPLICATIONINSIGHTS_CONNECTION_STRING="$aiConn" `
  ASPNETCORE_ENVIRONMENT=Production | Out-Null

if ($SkipDeploy) {
  Write-Host "==> SkipDeploy specified; provisioning complete." -ForegroundColor Yellow
  Write-Host ("URL: https://{0}.azurewebsites.net" -f $AppName)
  exit 0
}

Write-Host "==> Build and Zip Deploy application" -ForegroundColor Cyan
# Ensure we run from the project directory where this script resides
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Clean publish folder if exists
if (Test-Path publish) { Remove-Item -Recurse -Force publish }
dotnet publish ./cp5-d.csproj -c Release -o publish

if (Test-Path app.zip) { Remove-Item -Force app.zip }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path (Get-Location) 'publish'), (Join-Path (Get-Location) 'app.zip'))

az webapp deployment source config-zip -g $ResourceGroup -n $AppName --src app.zip | Out-Null

Write-Host "==> Deploy complete." -ForegroundColor Green
Write-Host ("URL: https://{0}.azurewebsites.net" -f $AppName)
