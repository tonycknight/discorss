namespace Discorss.Messaging

open System

[<Obsolete("TODO:")>]
type QueueMessage =
    { id: Guid
      priority: decimal
      messageType: string
      content: string
      created: DateTimeOffset
      ttl: DateTimeOffset option }

    static member empty() =
        { QueueMessage.id = Guid.NewGuid()
          priority = 0M
          messageType = ""
          content = null
          created = DateTimeOffset.UtcNow
          ttl = None }
