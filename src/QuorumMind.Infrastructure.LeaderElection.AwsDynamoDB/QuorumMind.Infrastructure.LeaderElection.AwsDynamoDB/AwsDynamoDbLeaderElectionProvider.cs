using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.AwsDynamoDB;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

public class AwsDynamoDbLeaderElectionProvider : ILeaderElectionProvider
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly AwsDynamoDBLeaderElectionOptions _options;
    private readonly ILogger<AwsDynamoDbLeaderElectionProvider> _logger;
    private readonly string _expirationAttr = "ExpiresAt";

    public AwsDynamoDbLeaderElectionProvider(
        IAmazonDynamoDB dynamoDb,
        AwsDynamoDBLeaderElectionOptions options,
        ILogger<AwsDynamoDbLeaderElectionProvider> logger)
    {
        _dynamoDb = dynamoDb;
        _options = options;
        _logger = logger;
        RenewalInterval = _options.CheckInterval;
    }

    public  TimeSpan RenewalInterval { get; set; }

    public override async Task<bool> TryAcquireLeadershipAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(_options.Ttl).ToUnixTimeSeconds();

        var request = new PutItemRequest
        {
            TableName = _options.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["LockKey"] = new AttributeValue { S = _options.LockKey },
                ["OwnerId"] = new AttributeValue { S = _options.InstanceId },
                [_expirationAttr] = new AttributeValue { N = expiresAt.ToString() }
            },
            ConditionExpression = "attribute_not_exists(LockKey) OR #e < :now",
            ExpressionAttributeNames = { ["#e"] = _expirationAttr },
            ExpressionAttributeValues = {
                [":now"] = new AttributeValue { N = now.ToUnixTimeSeconds().ToString() }
            }
        };

        try
        {
            await _dynamoDb.PutItemAsync(request, cancellationToken);
            _logger.LogInformation("Leadership acquired for {LockKey} by {InstanceId}", _options.LockKey, _options.InstanceId);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            _logger.LogDebug("Failed to acquire leadership for {LockKey}. Another instance is leader.", _options.LockKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while trying to acquire leadership for {LockKey}", _options.LockKey);
            return false;
        }
    }

    public override async Task<bool> TryRenewLeadershipAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(_options.Ttl).ToUnixTimeSeconds();

        var request = new UpdateItemRequest
        {
            TableName = _options.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["LockKey"] = new AttributeValue { S = _options.LockKey }
            },
            UpdateExpression = "SET #e = :expiresAt",
            ConditionExpression = "OwnerId = :ownerId",
            ExpressionAttributeNames = { ["#e"] = _expirationAttr },
            ExpressionAttributeValues = {
                [":expiresAt"] = new AttributeValue { N = expiresAt.ToString() },
                [":ownerId"] = new AttributeValue { S = _options.InstanceId }
            }
        };

        try
        {
            await _dynamoDb.UpdateItemAsync(request, cancellationToken);
            _logger.LogInformation("Leadership renewed for {LockKey} by {InstanceId}", _options.LockKey,
                _options.InstanceId);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            _logger.LogWarning("Failed to renew leadership for {LockKey}. Another instance may have taken over.",
                _options.LockKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while renewing leadership for {LockKey}", _options.LockKey);
            return false;
        }
        finally
        {
            await Task.Delay(RenewalInterval, cancellationToken);
        }
    }
}
