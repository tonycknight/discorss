namespace Discorss

open System
open Microsoft.Extensions.Caching.Memory

module Caching = 
    let cacheOptions () =
        let options = new MemoryCacheEntryOptions()
        options

    let expiry (expiry: TimeSpan) (options: MemoryCacheEntryOptions) =
        options.AbsoluteExpirationRelativeToNow <- expiry
        options
        