param (
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "mongodb://host.docker.internal:27017",
    
    [Parameter(Mandatory=$false)]
    [string]$Microbroker = "http://host.docker.internal:8080",

    [Parameter(Mandatory=$false)]
    [string]$DbName = "discorss",

    [Parameter(Mandatory=$false)]
    [string]$FeedIngestionFrequency = "00:05:00",

    [Parameter(Mandatory=$false)]
    [string]$LogLevel = "Trace"
)

docker run -it --rm -p 8081:8081 discorss --Discorss:microbrokerServiceUrl=$Microbroker --Discorss:mongoConnection=$ConnectionString --Discorss:mongoDbName=$DbName --Discorss:feedIngestionFrequency=$FeedIngestionFrequency  --Logging:LogLevel:Discorss=$LogLevel --Logging:Console:LogLevel:Discorss=$LogLevel
