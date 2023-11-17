# Docker Registry Webhook Manager

This is a simple Blazor app used to provide some of the same webhook capabilities as DockerHub when you are self-hosting a docker registry. 

To use it you must configure your Docker Registry to send push events to this project's receiving endpoint. Upon receiving a push notification this system will record it and execute any webhooks registered against the pushed repository.

```
notifications:
  endpoints:
    - name: "Publish to Webhooks system (Docker Registry UI)"
      url:  "https://webhooks.example.com/api/webhook"
      timeout: 500ms
      threshold: 5
      backoff: 1s
      events:
        - push
```

This app does not have its own authentication at this stage. If you choose to use it you do so at your own risk. Currently only Basic auth is supported for connecting to the registry.

The following environment variables should be configured. Note the double underscores in the key names.

| Key                       | Description                                                         |
|---------------------------|---------------------------------------------------------------------|
| `Registry__Uri`           | The URL of the Docker registry. Used to retrieve the list of repositories. |
| `Registry__Username`      | Username to use when connecting to the Docker registry              |
| `Registry__Password`      | Password to use when connecting to the Docker registry              |
| `MySqlConnectionString`   | Connection string for the MySQL server for storing webhook registrations and call history. |
