**Olive.Dashboards.PowerBI**
This documentation provides an overview of the Power BI embedding service and Azure AD authentication for accessing Power BI resources.

**Class:** `AzureAdService`

**Description:**
This class is responsible for generating an Azure Active Directory (AAD) authentication token for Power BI services. It supports both `masteruser` and `serviceprincipal` authentication modes.

**Methods:**
- `Task<string> GetAccessToken()`: Retrieves an AAD access token using either `masteruser` credentials or a `service principal`.

**Usage:**
- Call `GetAccessToken()` to obtain an access token that can be used to authenticate API requests to Power BI.

---

**Embed Service Documentation**

**Class:** `EmbedService`

**Description:**
This class provides functionality to embed Power BI reports and dashboards using Azure authentication.

**Constructor:**
- `EmbedService(AzureAdService azureAdService, string uri = "https://api.powerbi.com")`

**Methods:**
- `Task<EmbedParams> EmbedReport(Guid workspaceId, Guid reportId, EmbedToken embedToken)`: Embeds a report using an existing embed token.
- `Task<EmbedParams> EmbedReport(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId)`: Embeds a report and generates an embed token.
- `Task<EmbedToken> GetEmbedTokenForReport(Guid workspaceId, IList<Guid> datasetIds, Guid reportId)`: Retrieves an embed token for a report.
- `Task<bool> RefreshDatasetAsync(Guid workspaceId, string datasetId, DatasetRefreshRequest datasetRefreshRequest = null)`: Refreshes a Power BI dataset.
- `Task<IList<Refresh>> GetDatasetRefreshHistoryAsync(Guid workspaceId, string datasetId)`: Retrieves the refresh history of a dataset.
- `Task<EmbedParams> EmbedDashboard(Guid workspaceId, Guid dashboardId, TokenAccessLevel tokenAccessLevel)`: Embeds a Power BI dashboard.
- `Task<EmbedToken> GetEmbedTokenForDashboard(Guid workspaceId, Guid dashboardId, TokenAccessLevel tokenAccessLevel, string datasetId = null)`: Retrieves an embed token for a dashboard.

**Usage:**
- Initialize `EmbedService` with an instance of `AzureAdService`.
- Use `EmbedReport` or `EmbedDashboard` to obtain the necessary embed details.
- Use `RefreshDatasetAsync` to refresh datasets as needed.