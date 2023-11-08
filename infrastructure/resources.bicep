param containerVersion string
param location string
param integrationResourceGroupName string
param containerAppEnvironmentName string
param containerRegistryName string
param applicationInsightsName string
param serviceBusName string

var systemName = 'tinylnk-api'
var functionAppName = 'tinylnk-func'
var defaultResourceName = '${systemName}-ne'
var functionAppResourceName = '${functionAppName}-ne'
var containerRegistryPasswordSecretRef = 'container-registry-password'

var tables = [
  'shortlinks'
]
var hitsQueueNames = [
  'hitsprocessorqueue'
  'hitscumulatorqueue'
]
var queueNames = [
  'hitscalculationscompleted'
]

var apexHostName = 'tinylnk.nl'
var apiHostName = 'api.tinylnk.nl'
var serviceBusEndpoint = '${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBus.apiVersion).primaryConnectionString
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppEnvironmentName
  scope: resourceGroup(integrationResourceGroupName)
  resource apexCert 'managedCertificates' existing = {
    name: '${replace(apexHostName, '.', '-')}-cert'
  }
  resource apiCert 'managedCertificates' existing = {
    name: '${replace(apiHostName, '.', '-')}-cert'
  }
}
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
  scope: resourceGroup(integrationResourceGroupName)
}
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup(integrationResourceGroupName)
}
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusName
  scope: resourceGroup(integrationResourceGroupName)
}
module serviceBusQueueModules 'servicebus-queue.bicep' = {
  name: 'serviceBusQueueModules'
  scope: resourceGroup(integrationResourceGroupName)
  params: {
    serviceBusName: serviceBus.name
    queueNames: queueNames
  }
}
module serviceBusHitsQueueModules 'servicebus-queue.bicep' = {
  name: 'serviceBusHitsQueueModules'
  scope: resourceGroup(integrationResourceGroupName)
  params: {
    serviceBusName: serviceBus.name
    queueNames: hitsQueueNames
  }
}
module serviceBusTopicsModule 'servicebus-topic.bicep' = {
  dependsOn: [ serviceBusHitsQueueModules ]
  name: 'serviceBusTopicsModule'
  scope: resourceGroup(integrationResourceGroupName)
  params: {
    serviceBusName: serviceBus.name
    topicName: 'hits'
    queueNames: hitsQueueNames
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: uniqueString(defaultResourceName)
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}
resource storageAccountTableService 'Microsoft.Storage/storageAccounts/tableServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}
resource storageAccountTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2022-09-01' = [for table in tables: {
  name: table
  parent: storageAccountTableService
}]

resource apiContainerApp 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: '${defaultResourceName}-ca'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppEnvironment.id
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      dapr: {
        enabled: true
        appId: defaultResourceName
        appPort: 80
        appProtocol: 'http'
      }
      ingress: {
        external: true
        targetPort: 80
        transport: 'http'
        corsPolicy: {
          allowedOrigins: [
            'https://localhost:4200'
            'https://app.tinylnk.nl'
          ]
          allowCredentials: true
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
            'OPTIONS'
          ]
        }
        customDomains: [
          {
            name: 'tinylnk.nl'
            bindingType: 'SniEnabled'
            certificateId: containerAppEnvironment::apexCert.id
          }
          {
            name: 'api.tinylnk.nl'
            bindingType: 'SniEnabled'
            certificateId: containerAppEnvironment::apiCert.id
          }
        ]
      }
      secrets: [
        {
          name: containerRegistryPasswordSecretRef
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
      maxInactiveRevisions: 1
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.properties.adminUserEnabled ? containerRegistry.name : null
          passwordSecretRef: containerRegistryPasswordSecretRef
        }
      ]

    }
    template: {
      containers: [
        {
          name: defaultResourceName
          image: '${containerRegistry.properties.loginServer}/${systemName}:${containerVersion}'
          env: [
            {
              name: 'Azure__StorageAccountName'
              value: storageAccount.name
            }
            {
              name: 'Azure__ServiceBusName'
              value: serviceBus.name
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 6
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '30'
              }
            }
          }
        ]
      }
    }
  }
}

resource functionContainerApp 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: '${functionAppResourceName}-ca'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppEnvironment.id
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      dapr: {
        enabled: true
        appId: defaultResourceName
        appPort: 80
        appProtocol: 'http'
      }
      ingress: {
        external: true
        targetPort: 80
        transport: 'http'
        corsPolicy: {
          allowedOrigins: [
            'https://localhost:4200'
            'https://app.tinylnk.nl'
          ]
          allowCredentials: true
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
            'OPTIONS'
          ]
        }
      }
      secrets: [
        {
          name: containerRegistryPasswordSecretRef
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'servicebusconnectionstring'
          value: serviceBusConnectionString
        }
        {
          name: 'storageaccountconnectionstring'
          value: storageAccountConnectionString
        }
      ]
      maxInactiveRevisions: 1
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.properties.adminUserEnabled ? containerRegistry.name : null
          passwordSecretRef: containerRegistryPasswordSecretRef
        }
      ]

    }
    template: {
      containers: [
        {
          name: functionAppResourceName
          image: '${containerRegistry.properties.loginServer}/${functionAppName}:${containerVersion}'
          env: [
            {
              name: 'AzureWebJobsStorage'
              secretRef: 'storageaccountconnectionstring'
            }
            {
              name: 'FUNCTIONS_WORKER_RUNTIME'
              value: 'dotnet-isolated'
            }
            {
              name: 'StorageAccountName'
              value: storageAccount.name
            }
            {
              name: 'ServiceBus'
              secretRef: 'servicebusconnectionstring'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
    }
  }
}

resource serviceBusDataSenderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
}
resource serviceBusDataReceiverRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
}

module serviceBusDataSenderRoleAssignment 'roleAssignment.bicep' = {
  name: 'serviceBusDataSenderRoleAssignment'
  scope: resourceGroup(integrationResourceGroupName)
  params: {
    principalId: apiContainerApp.identity.principalId
    roleDefinitionId: serviceBusDataSenderRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}

module serviceBusDataReceiverRoleAssignment 'roleAssignment.bicep' = {
  name: 'serviceBusDataReceiverRoleAssignment'
  scope: resourceGroup(integrationResourceGroupName)
  params: {
    principalId: functionContainerApp.identity.principalId
    roleDefinitionId: serviceBusDataReceiverRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}
module storageTableDataContributorRoleAssignment 'roleAssignment.bicep' = {
  name: 'storageTableDataContributorRoleAssignment'
  params: {
    principalId: apiContainerApp.identity.principalId
    roleDefinitionId: storageTableDataContributorRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}
module functionStorageTableDataContributorRoleAssignment 'roleAssignment.bicep' = {
  name: 'functionStorageTableDataContributorRoleAssignment'
  params: {
    principalId: functionContainerApp.identity.principalId
    roleDefinitionId: storageTableDataContributorRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}
