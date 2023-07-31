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

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            return connection;
        }

        public void DeleteWebhook(int webhookId) {
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

            var creationTime = time.ToOffset(TimeSpan.FromHours(10)).DateTime;

            connection.Execute("INSERT INTO WebhookCalls (WebhookId, Time, ResponseCode) VALUES (@webhookId, @time, @responseCode)", new
            {
                webhookId,
                time = creationTime,
                responseCode
            });
        }

        public IEnumerable<WebhookResult> GetWebhookResults(int webhookId)
        {
            using var connection = CreateConnection();
            connection.Open();

            var results = connection.Query<WebhookResult>("SELECT Time, ResponseCode FROM WebhookCalls WHERE WebhookId = @webhookId", new
            {
                webhookId,
            });

            foreach (var result in results)
            {
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
