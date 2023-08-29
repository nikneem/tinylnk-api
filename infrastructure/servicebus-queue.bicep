param serviceBusName string
param queueNames array

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusName
  resource queues 'queues' = [for queue in queueNames: {
    name: queue
  }]
}
