param serviceBusName string
param queueNames array
param topicName string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusName
  resource topic 'topics' = {
    name: topicName
    resource subscription 'subscriptions' = [for queue in queueNames: {
      name: '${queue}sub'
      properties: {
        forwardTo: queue
      }
    }]

  }
}
