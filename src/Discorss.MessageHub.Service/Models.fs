namespace Discorss.MessageHub.Service

open System
open Discorss.Messaging

type MessageHubMessage =
    { id: Guid
      priority: decimal
      messageType: string
      content: string
      created: DateTimeOffset
      ttl: DateTimeOffset option }

    static member empty() =
        { MessageHubMessage.id = Guid.NewGuid()
          priority = 0M
          messageType = ""
          content = null
          created = DateTimeOffset.UtcNow
          ttl = None }

    static member ofQueueMessage(msg:QueueMessage)=
        { MessageHubMessage.id = msg.id; priority = msg.priority; 
            messageType = msg.messageType;
            content = msg.content;
            created = msg.created;
            ttl = msg.ttl }

    static member toQueueMessage(msg:MessageHubMessage)=
        { QueueMessage.id = msg.id; priority = msg.priority; 
            messageType = msg.messageType;
            content = msg.content;
            created = msg.created;
            ttl = msg.ttl }