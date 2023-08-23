targetScope = 'subscription'

param containerVersion string
param integrationResourceGroupName string
param containerAppEnvironmentName string
param containerRegistryName string
param serviceBusName string
param location string = deployment().location

var systemName = 'tinylnk-api'
var defaultResourceName = '${systemName}-ne'

resource targetResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: defaultResourceName
  location: location
}

module hitsProcessorModule 'hits-processor-resources.bicep' = {
  name: 'hits-processor-module'
  scope: targetResourceGroup
  params: {
    containerVersion: containerVersion
    location: location
    serviceBusName: serviceBusName
    integrationResourceGroupName: integrationResourceGroupName
    containerAppEnvironmentName: containerAppEnvironmentName
    containerRegistryName: containerRegistryName
  }
}
