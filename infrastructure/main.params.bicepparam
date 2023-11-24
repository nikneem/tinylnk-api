using './main.bicep'

param containerVersion = '0.1.33'
param integrationResourceGroupName = 'tinylnk-integration-ne'
param containerAppEnvironmentName = 'tinylnk-integration-ne-env'

param integrationEnvironment = {
  resourceGroupName: 'mvp-int-env'
  containerRegistryName: 'nvv54gsk4pteu'
  applicationInsights: 'mvp-int-env-ai'
  appConfiguration: 'mvp-int-env-appcfg'
  keyVault: 'mvp-int-env-kv'
  logAnalytics: 'mvp-int-env-log'
  serviceBus: 'mvp-int-env-bus'
}
