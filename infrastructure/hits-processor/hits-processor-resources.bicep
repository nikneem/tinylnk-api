param containerVersion string
param integrationResourceGroupName string
param containerAppEnvironmentName string
param containerRegistryName string
param serviceBusName string
param location string

var systemName = 'tinylnk-api'
var defaultResourceName = '${systemName}-ne'

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppEnvironmentName
  scope: resourceGroup(integrationResourceGroupName)
}
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
  scope: resourceGroup(integrationResourceGroupName)
}
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusName
  scope: resourceGroup(integrationResourceGroupName)
  resource queue 'queues' existing = {
    name: 'hitsprocessorqueue'
  }
}

var serviceBusEndpoint = '${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBus.apiVersion).primaryConnectionString

resource hitsStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: uniqueString(defaultResourceName)
  resource hitsTableStorageService 'tableServices' existing = {
    name: 'default'
    resource table 'tables' = {
      name: 'hits'
    }
  }
}

resource hitsProcessorJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'tinylnk-jobs-hits-processor'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppEnvironment.id
    configuration: {
      secrets: [
        {
          name: 'servicebus-connection-string'
          value: serviceBusConnectionString
        }
        {
          name: 'container-registry-secret'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
      replicaTimeout: 60
      replicaRetryLimit: 1
      triggerType: 'Event'
      eventTriggerConfig: {
        replicaCompletionCount: 1
        parallelism: 1
        scale: {
          minExecutions: 0
          maxExecutions: 10
          pollingInterval: 30
          rules: [
            {
              name: 'azure-servicebus-queue-rule'
              type: 'azure-servicebus'
              metadata: any(
                {
                  queueName: serviceBus::queue.name
                  connection: 'servicebus-connection-string'
                }
              )
              auth: [
                {
                  secretRef: 'servicebus-connection-string'
                  triggerParameter: 'connection'
                }
              ]
            }
          ]
        }
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.name
          passwordSecretRef: 'container-registry-secret'
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${containerRegistry.properties.loginServer}/tinylnk-jobs-hitsprocessor:${containerVersion}'
          name: 'hits-processor'
          env: [
            {
              name: 'ServiceBusConnection'
              secretRef: 'servicebus-connection-string'
            }
            {
              name: 'QueueName'
              secretRef: serviceBus::queue.name
            }
            {
              name: 'StorageAccountName'
              value: hitsStorageAccount.name
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

resource storageTableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}
module storageTableDataContributorRoleAssignment 'roleAssignment.bicep' = {
  name: 'storageTableDataContributorRoleAssignment'
  params: {
    principalId: hitsProcessorJob.identity.principalId
    roleDefinitionId: storageTableDataContributorRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}
