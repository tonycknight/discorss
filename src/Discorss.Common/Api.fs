namespace Discorss

open Giraffe

module Api=
    let heartbeatRoute : HttpHandler = route "/heartbeat"      >=> json [ "OK" ]

