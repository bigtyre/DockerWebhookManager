using Dapper;
using MySql.Data.MySqlClient;

namespace DockerRegistryUI.Controllers
{
    public class WebhookRepository
    {
        public WebhookRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private string ConnectionString { get; }

        private static readonly TimeSpan _storageTimeZone = TimeSpan.FromHours(10);

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            return connection;
        }

        public void DeleteWebhook(int webhookId)
        {
            using var connection = CreateConnection();
            connection.Open();

            connection.Execute("DELETE FROM Webhooks WHERE Id = @id", new
            {
                id = webhookId.ToString(),
            });
        }
        public void CreateWebhook(string repositoryName, Uri url)
        {
            using var connection = CreateConnection();
            connection.Open();

            connection.Execute("INSERT INTO Webhooks (RepositoryName, Url) VALUES (@repositoryName, @url)", new
            {
                repositoryName,
                url = url.ToString()
            });
        }

        public void CreateWebhookCallHistory(int webhookId, DateTimeOffset time, int responseCode)
        {
            using var connection = CreateConnection();
            connection.Open();

            var creationTime = time.ToOffset(_storageTimeZone).DateTime;

            connection.Execute("INSERT INTO WebhookCalls (WebhookId, Time, ResponseCode) VALUES (@webhookId, @time, @responseCode)", new
            {
                webhookId,
                time = creationTime,
                responseCode
            });
        }

        public IEnumerable<WebhookResult> GetWebhookResults(int? webhookId = null, DateTime? startDate = null)
        {
            using var connection = CreateConnection();
            connection.Open();

            var conditions = new List<string>();
            if (webhookId.HasValue)
            {
                conditions.Add("`WebhookId` = @webhookId");
            }

            if (startDate.HasValue)
            {
                conditions.Add("`Time` >= @startDate");
            }

            // IMPORTANT - Ensure all conditions use parameters, rather than directly including the value,
            // to prevent SQL injection attacks. This is important since we are dynamically building the query.
            string condition = "";
            if (conditions.Any())
            {
                condition = "WHERE " + string.Join("AND", conditions);
            }

            var displayTimeZone = TimeSpan.FromHours(10);

            var results = connection.Query<WebhookResult>(
                $@"SELECT 
                    `WebhookId`,
                    `Time`, 
                    `ResponseCode` 
                FROM `WebhookCalls` 
                {condition}", new
                {
                    webhookId,
                    startDate,
                }
            );

            foreach (var result in results)
            {
                var time = new DateTimeOffset(result.Time.DateTime, _storageTimeZone);

                result.Time = time.ToOffset(displayTimeZone);
                yield return result;
            }
        }

        public IEnumerable<Webhook> GetWebhooksByRepositoryName(string repositoryName)
        {
            using var connection = CreateConnection();
            connection.Open();

            var results = connection.Query<Webhook>(
                "SELECT Id, RepositoryName, Url " +
                "FROM Webhooks " +
                "WHERE RepositoryName = @repositoryName",
                new
                {
                    repositoryName
                }
            );

            foreach (var row in results)
            {
                yield return row;
            }
        }

        public IEnumerable<Webhook> GetWebhooks()
        {
            using var connection = CreateConnection();
            connection.Open();

            var results = connection.Query<Webhook>(
                "SELECT Id, RepositoryName, Url " +
                "FROM Webhooks"
            );

            foreach (var row in results)
            {
                yield return row;
            }
        }
    }
}
