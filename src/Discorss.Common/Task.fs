namespace Discorss

open System
open System.Threading.Tasks

module Task =
    let ofResult value = Task.FromResult value

    let delay (duration: TimeSpan) = Task.Delay duration
