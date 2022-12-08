namespace Olive.Dashboards.PowerBI
{
    using Microsoft.PowerBI.Api;
    using Microsoft.Rest;
    using Olive.Dashboards.PowerBI.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Olive;
    using Microsoft.PowerBI.Api.Models;
    using Olive.Dashboards.PowerBI.Services;

    public class EmbedService
    {
        private readonly string uri;
        private readonly AzureAdService azureAdService;

        PowerBIClient client = null;

        public EmbedService(AzureAdService azureAdService, string uri = "https://api.powerbi.com")
        {
            this.uri = uri;
            this.azureAdService = azureAdService;
            if (azureAdService == null)
            {
                throw new Exception("azureAdService is required");
            }

            client = GetClientAsync().GetAwaiter().GetResult();
        }

        async Task<PowerBIClient> GetClientAsync()
        {
            string? token = null;
            try
            {
                token = await azureAdService.GetAccessToken();
            }
            catch
            {
                throw new Exception("Error on generating access token on Azure, please check the Azure service configuration");
            }

            TokenCredentials tokenCredentials = new TokenCredentials(token, "Bearer");

            PowerBIClient? client = null;
            try
            {
                client = new PowerBIClient(new Uri(uri), tokenCredentials);
            }
            catch
            {
                throw new Exception("Error on initialzing, please check the URI and token credentials configuration");
            }
            return client;
        }

        #region Report
        public async Task<EmbedParams> EmbedReport(Guid workspaceId, Guid reportId, EmbedToken embedToken)
        {
            if (DateTime.Now > embedToken.Expiration)
            {
                throw new Exception("Report token time is expired");
            }

            // Get report info
            Report? report = null;
            try
            {
                report = await client.Reports.GetReportInGroupAsync(workspaceId, reportId);
            }
            catch
            {
                throw new Exception("Report not found, Workspace Id, or Report Id is wrong");
            }

            var embedReports = new List<EmbedReport>
            {
                new EmbedReport
                {
                    Id = report.Id,
                    Name = report.Name,
                    EmbedUrl = report.EmbedUrl
                }
            };

            return new EmbedParams
            {
                EmbedReport = embedReports,
                Type = Constants.EmbedParamTypes.Report,
                EmbedToken = embedToken
            };
        }

        public async Task<EmbedParams> EmbedReport(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId)
        {
            EmbedToken embedToken;

            // Get report info
            Report? report = null;
            try
            {
                report = client.Reports.GetReportInGroup(workspaceId, reportId);
            }
            catch
            {
                throw new Exception("Report not found, Workspace Id or Report Id is wrong");
            }

            //  Check if dataset is present for the corresponding report
            //  If isRDLReport is true then it is a RDL Report 
            var isRDLReport = report.DatasetId.IsEmpty();

            if (isRDLReport)
            {
                // Get Embed token for RDL Report
                embedToken = await GetEmbedTokenForRDLReport(workspaceId, reportId);
            }
            else
            {
                var datasetIds = new List<Guid>() { Guid.Parse(report.DatasetId) };

                if (additionalDatasetId != Guid.Empty)
                {
                    datasetIds.Add(additionalDatasetId);
                }

                embedToken = await GetEmbedTokenForReport(workspaceId, datasetIds, reportId);
            }

            if (DateTime.Now > embedToken.Expiration)
            {
                throw new Exception("Report token time is expired");
            }

            var embedReports = new List<EmbedReport>
            {
                new EmbedReport
                {
                    Id = report.Id,
                    Name = report.Name,
                    EmbedUrl = report.EmbedUrl
                }
            };

            return new EmbedParams
            {
                EmbedReport = embedReports,
                Type = Constants.EmbedParamTypes.Report,
                EmbedToken = embedToken
            };
        }

        /// <summary>
        /// Gets Embed token fro single report, multiple datasetIds and optional workspace
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="datasetIds"></param>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public async Task<EmbedToken> GetEmbedTokenForReport(Guid workspaceId, IList<Guid> datasetIds, Guid reportId)
        {
            if (datasetIds == null)
            {
                Report? report = null;
                try
                {
                    report = await client.Reports.GetReportInGroupAsync(workspaceId, reportId);
                }
                catch
                {
                    throw new Exception("Report not found, Workspace Id or Report Id is wrong");
                }
                datasetIds = new List<Guid>() { Guid.Parse(report.DatasetId) };
            }

            var tokenRequest = new GenerateTokenRequestV2(
                reports: new List<GenerateTokenRequestV2Report> { new GenerateTokenRequestV2Report(reportId) },
                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),
                targetWorkspaces: workspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(workspaceId) } : null
                );

            return await client.EmbedToken.GenerateTokenAsync(tokenRequest);
        }

        /// <summary>
        /// Get Embed Token for RDP Report
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="reportId"></param>
        /// <param name="accessLevel"></param>
        /// <returns>EmbedToken</returns>
        public async Task<EmbedToken> GetEmbedTokenForRDLReport(Guid workspaceId, Guid reportId, string accessLevel = "view")
        {
            // Generate token request for RDL Report
            var tokenRequest = new GenerateTokenRequest(accessLevel: accessLevel);

            // Get Embed token
            return await client.Reports.GenerateTokenInGroupAsync(workspaceId, reportId, tokenRequest);
        }

        /// <summary>
        /// Refreshes the datase
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> RefreshDatasetAsync(Guid workspaceId, string datasetId, DatasetRefreshRequest datasetRefreshRequest = null)
        {
            if (datasetRefreshRequest == null)
            {
                datasetRefreshRequest = new DatasetRefreshRequest()
                {
                    ApplyRefreshPolicy = false,
                    CommitMode = DatasetCommitMode.Transactional,
                    EffectiveDate = DateTime.UtcNow,
                    MaxParallelism = 1,
                    NotifyOption = NotifyOption.NoNotification,
                    Objects = null,
                    RetryCount = 3,
                    Type = DatasetRefreshType.Full
                };
            }
            try
            {
                await client.Datasets.RefreshDatasetInGroupAsync(workspaceId, datasetId, datasetRefreshRequest);
                return true;
            }
            catch
            {
                throw new Exception("Error on refreshing dataset, please make sure workspaceId or datasetId is correct");
            }
        }

        /// <summary>
        /// Gets refresh history of dataset
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IList<Refresh>> GetDatasetRefreshHistoryAsync(Guid workspaceId, string datasetId)
        {
            Refreshes? refreshes = null;
            try
            {
                refreshes = await client.Datasets.GetRefreshHistoryInGroupAsync(workspaceId, datasetId);
            }
            catch
            {
                throw new Exception("Error on getting history of dataset refresh, please make sure workspaceId or datasetId is set correctly");
            }
            return refreshes.Value;
        }
        #endregion

        #region Dashbaord
        /// <summary>
        /// Returns dashboard
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="dashboardId"></param>
        /// <param name="tokenAccessLevel"></param>
        /// <returns></returns>
        public async Task<EmbedParams> EmbedDashboard(Guid workspaceId, Guid dashboardId, TokenAccessLevel tokenAccessLevel)
        {
            EmbedToken embedToken = null;
            Dashboard dashboard = null;

            try
            {
                dashboard = await client.Dashboards.GetDashboardInGroupAsync(workspaceId, dashboardId);
            }
            catch
            {
                throw new Exception("Error on finding dashboard, please check workspace Id and dashboard Id");
            }
            embedToken = await GetEmbedTokenForDashboard(workspaceId, dashboardId, tokenAccessLevel);

            var embedDashboard = new EmbedDashboard()
            {
                EmbedUrl = dashboard.EmbedUrl,
                Id = dashboard.Id,
                Name = dashboard.DisplayName
            };

            return new EmbedParams
            {
                EmbedToken = embedToken,
                Type = Constants.EmbedParamTypes.Dashboard,
                EmbedDashboard = embedDashboard
            };
        }

        public async Task<EmbedParams> EmbedDashboard(Guid workspaceId, Guid dashbaordId, EmbedToken embedToken)
        {
            Dashboard dashboard = null;
            try
            {
                dashboard = await client.Dashboards.GetDashboardInGroupAsync(workspaceId, dashbaordId);
            }
            catch
            {
                throw new Exception("Error on finding dashboard, please check workspace Id, and dashboard Id");
            }

            if (DateTime.Now > embedToken.Expiration)
            {
                throw new Exception("Dashboard token time is expired");
            }

            var embedDashboard = new EmbedDashboard()
            {
                EmbedUrl = dashboard.EmbedUrl,
                Id = dashboard.Id,
                Name = dashboard.DisplayName
            };

            return new EmbedParams
            {
                EmbedDashboard = embedDashboard,
                EmbedToken = embedToken,
                Type = Constants.EmbedParamTypes.Dashboard
            };
        }

        public async Task<EmbedToken> GetEmbedTokenForDashboard(Guid workspaceId, Guid dashboardId, TokenAccessLevel tokenAccessLevel, string datasetId = null)
        {
            EmbedToken embedToken = null;
            try
            {
                embedToken = await client.Dashboards.GenerateTokenInGroupAsync(workspaceId, dashboardId, new GenerateTokenRequest()
                {
                    AccessLevel = tokenAccessLevel,
                    DatasetId = datasetId
                });
            }
            catch
            {
                throw new Exception("Error on generating token, please make sure you set the access level correctly");
            }
            return embedToken;
        }
        #endregion
    }
}
