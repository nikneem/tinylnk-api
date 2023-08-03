param containerVersion string
param location string

var systemName = 'tinylnk-api'
var defaultResourceName = '${systemName}-we'

var tables = [
  'shortlinks'
]

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
