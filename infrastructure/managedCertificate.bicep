param hostname string
param location string
param managedEnvironmentName string

var certificateName = '${replace(hostname, '.', '-')}-cert'
resource managedEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: managedEnvironmentName
}

resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2023-05-01' = {
  name: certificateName
  parent: managedEnvironment
  location: location
  properties: {
    domainControlValidation: 'HTTP'
    subjectName: hostname
  }
}

output certificateResourceId string = managedCertificate.id
output certificateName string = certificateName
