param containerVersion string
param location string
param integrationResourceGroupName string
param containerAppEnvironmentName string
param containerRegistryName string

var systemName = 'tinylnk-api'
var defaultResourceName = '${systemName}-we'
var containerRegistryPasswordSecretRef = 'container-registry-password'

var tables = [
  'shortlinks'
]

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppEnvironmentName
  scope: resourceGroup(integrationResourceGroupName)
}
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
  scope: resourceGroup(integrationResourceGroupName)
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
        transport: 'http2'
        corsPolicy: {
          allowedOrigins: [
            'https://localhost:4200'
            'https://app.tinylnk.nl'
          ]
        }
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
