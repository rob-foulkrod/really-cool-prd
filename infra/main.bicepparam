using './main.bicep'

param location = 'northcentralus'
param resourceSuffix = 'prd'

// This will be set by the deployment workflow via -c parameter
// param managedIdentityResourceId = '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identityName}'

param appServicePlanSku = 'B1'
