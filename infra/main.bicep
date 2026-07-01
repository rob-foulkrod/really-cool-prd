// ============================================================
// Really Cool PRD — Infrastructure as Code (Bicep)
// 
// Provisions:
//  - App Service Plan (B1 Standard for slot support)
//  - Web App (Linux, .NET 10.0)
//  - Staging slot for blue-green deployment
//  - Health check configuration
// ============================================================

param location string = resourceGroup().location
param environment string = 'production'

@description('Unique suffix for resource names (e.g., "prd", "staging")')
param resourceSuffix string = 'prd'

@description('App Service Plan SKU (B1 = Standard with slot support)')
param appServicePlanSku string = 'B1'

var resourceBaseName = 'really-cool-${resourceSuffix}'
var appServicePlanName = 'asp-${resourceBaseName}'
var webAppName = 'app-${resourceBaseName}'
var stagingSlotName = 'staging'
var healthCheckPath = '/health'
var healthCheckInterval = 60  // seconds
var healthCheckThreshold = 3  // consecutive failures before unhealthy

// ============================================================
// App Service Plan
// ============================================================
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

// ============================================================
// Web App (Production slot)
// ============================================================
resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'

  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      
      // Configure health check probe
      healthCheckPath: healthCheckPath
      
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
}

// ============================================================
// Health Check Configuration
// ============================================================
resource healthCheck 'Microsoft.Web/sites/config@2024-04-01' = {
  name: '${webApp.name}/healthCheckPath'
  properties: {
    healthCheckPath: healthCheckPath
  }
}

// ============================================================
// Staging Slot (for blue-green deployment)
// ============================================================
resource stagingSlot 'Microsoft.Web/sites/slots@2024-04-01' = {
  name: '${webApp.name}/${stagingSlotName}'
  location: location
  kind: 'app,linux'

  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      healthCheckPath: healthCheckPath
      
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Staging'
        }
      ]
    }
  }
}

// ============================================================
// Slot Swap Configuration (swap slots when ready)
// ============================================================
resource slotSwapConfig 'Microsoft.Web/sites/config@2024-04-01' = {
  name: '${webApp.name}/slotConfigNames'
  properties: {
    appSettingNames: [
      'ASPNETCORE_ENVIRONMENT'
    ]
  }
}

// ============================================================
// Outputs
// ============================================================
output webAppId string = webApp.id
output webAppName string = webApp.name
output webAppDefaultHostName string = webApp.properties.defaultHostName
output stagingSlotName string = stagingSlot.name
output appServicePlanId string = appServicePlan.id
