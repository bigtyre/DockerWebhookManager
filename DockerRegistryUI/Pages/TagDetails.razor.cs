using DockerRegistryUI.Data;
using Microsoft.AspNetCore.Components;
using TrivyAPIClient;

namespace DockerRegistryUI.Pages
{
    public partial class TagDetails : ComponentBase
    {
        [Parameter]
        public string? RepositoryName { get; set; }

        [Parameter]
        public string? TagName { get; set; }



        [Inject] public DockerImageUrlBuilder DockerImageUrlBuilder { get; set; } = default!;
        [Inject] public VulnerabilityScanner VulnerabilityScanner { get; set; } = default!;
        [Inject] public VulnerabilityScanRepository ScanRepository { get; set; } = default!;

        private readonly List<Vulnerability> _vulnerabilities = [];

        private DateTime? _scanTime;
        public DateTime? ScanTime
        {
            get => _scanTime; private set
            {
                if (_scanTime != value)
                {
                    _scanTime = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning; private set
            {
                if (_isScanning != value)
                {
                    _isScanning = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        private bool _hasScanned;
        public bool HasScanned
        {
            get => _hasScanned;
            private set
            {
                if (_hasScanned != value)
                {
                    _hasScanned = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            var scanUrl = DockerImageUrlBuilder.GetImageUrl(RepositoryName, TagName).WithoutScheme();
            var latestScan = await ScanRepository.GetLatestScanWithVulnerabilitiesAsync(scanUrl);
            if (latestScan is not null)
            {
                await PopulateFromSavedScan(latestScan);
            }

            await base.OnParametersSetAsync();
        }

        private async Task PopulateFromSavedScan(SavedVulnerabilityScan latestScan)
        {
            ScanTime = latestScan.ScanDate;
            _vulnerabilities.Clear();
            _vulnerabilities.AddRange(latestScan
                .Vulnerabilities
                .Select(c => new Vulnerability()
                {
                    Severity = c.Severity,
                    PkgName = c.PackageName,
                    Description = c.Description?.DescriptionText
                })
                .SortBySeverity()
            );
            HasScanned = true;
            await InvokeAsync(StateHasChanged);
        }

        private static readonly Dictionary<string, string> _severityClasses = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Critical", "severity-critical" },
                { "High", "severity-high" },
                { "Medium", "severity-medium" },
                { "Low", "severity-low" },
                { "Unknown", "severity-unknown" },
            };

        public string GetSeverityCssClass(string severity)
        {
            if (_severityClasses.TryGetValue(severity, out var result))
                return result;

            return "";
        }

        public async void UpdateVulnerabilities()
        {
            try
            {
                if (RepositoryName is null || TagName is null) return;

                if (IsScanning)
                    return;

                IsScanning = true;
                try
                {
                    var result = await VulnerabilityScanner.ScanAsync(RepositoryName, TagName);

                    var vulnerabilities = result
                        .Results
                        .SelectMany(c => c.Vulnerabilities)
                        .SortBySeverity()
                    .ToList();

                    _vulnerabilities.Clear();
                    _vulnerabilities.AddRange(vulnerabilities);
                    await InvokeAsync(StateHasChanged);

                    HasScanned = true;
                }
                finally
                {
                    IsScanning = false;
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
