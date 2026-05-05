namespace Discorss

open System.Threading.Tasks

module Task =
    let ofResult value = Task.FromResult value
