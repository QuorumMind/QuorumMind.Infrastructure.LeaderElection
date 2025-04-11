using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using QuorumMind.Infrastructure.LeaderElection.AwsDynamoDB;
using QuorumMind.Infrastructure.LeaderElection.Consul;
using QuorumMind.Infrastructure.LeaderElection.Redis;
using QuorumMind.Infrastructure.LeaderElection.ExampleApp.Worker;
using QuorumMind.Infrastructure.LeaderElection.GCP.Firestore;

var builder = WebApplication.CreateBuilder(args);

SetupGCPFirestoreLeaderElection(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<ReminderWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();

void SetupRedisLeaderElection(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddLeaderElectionRedis(cfg =>
    {
        cfg.ConnectionString = webApplicationBuilder.Configuration.GetConnectionString("Redis");
        cfg.LeaderKey = "quorum-mind-leader-key";
        cfg.InstanceId = Environment.MachineName;
        cfg.LeaseTime = TimeSpan.FromSeconds(10);
        cfg.RenewTimeSpan = TimeSpan.FromSeconds(8);
    });
}

void SetupConsulLeaderElection(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddLeaderElectionConsul(cfg =>
    {  
        cfg.ConsulAddress = webApplicationBuilder.Configuration.GetConnectionString("Consul");
        cfg.LeaderKey = "quorum-mind-leader-key";
        cfg.InstanceId = Environment.MachineName;
        cfg.LeaseTime = TimeSpan.FromSeconds(10);
        cfg.RenewalInterval = TimeSpan.FromSeconds(2);
    });
}

void SetupAwsDynamoDBLeaderElection(WebApplicationBuilder webApplicationBuilder)
{
    var dynamoConfig = new AmazonDynamoDBConfig
    {
        RegionEndpoint = RegionEndpoint.USEast2
    };

    var credentials = new BasicAWSCredentials("PLACEHOLDER", "PLACEHOLDER");
    var dynamoClient = new AmazonDynamoDBClient(credentials, dynamoConfig);
    builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoClient);

    webApplicationBuilder.Services.AddLeaderElectionAws(cfg =>
    {
        cfg.TableName = "LeaderElection";
        cfg.LockKey = "my-service-lock";
        cfg.InstanceId = Environment.MachineName;
        cfg.Ttl = TimeSpan.FromSeconds(5);
        cfg.CheckInterval = TimeSpan.FromSeconds(2);
    });
}

void SetupGCPFirestoreLeaderElection(WebApplicationBuilder webApplicationBuilder)
{
    var credential = GoogleCredential.FromFile("<<PATH TO SERVICE ACCOUNT FILE>>");
    var builder = new FirestoreClientBuilder
    {
        Credential = credential,
    };
    var firestoreDB = FirestoreDb.Create("<<YOUR PROJECT ID>>", builder.Build());
    

    webApplicationBuilder.Services.AddSingleton<FirestoreDb>(firestoreDB);

    webApplicationBuilder.Services.AddLeaderElectionFirestore(cfg =>
    {
        cfg.LockKey = "my-service-lock";
        cfg.InstanceId = Environment.MachineName;
        cfg.Ttl = TimeSpan.FromSeconds(5);
        cfg.CheckInterval = TimeSpan.FromSeconds(2);
    });
}
