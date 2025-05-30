// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Common
{
    public class AzurePaaSCostCalculator
    {
        // PaaS datasets
        private List<SQL_MI_PaaS> SQL_MI_PaaS_List;
        private List<WebApp_PaaS> WebApp_PaaS_List;
        HashSet<string> WebappUniqueDevMachines;
        HashSet<string> WebappUniqueProdMachines;
        HashSet<string> SqlUniqueDevMachines;
        HashSet<string> SqlUniqueProdMachines;

        private bool IsCalculated;

        private int SqlDevRowCount;
        private int SqlProdRowCount;
        private int WebAppDevRowCount;
        private int WebAppProdRowCount;

        private double SqlPaasComputeCost;
        private double SqlPaasDevComputeCost;
        private double SqlPaasProdComputeCost;
        private double WebappPaasComputeCost;
        private double WebappPaasDevComputeCost;
        private double WebappPaasProdComputeCost;
        private double TotalPaasComputeCost;

        private double SqlPaasStorageCost;
        private double SqlPaasDevStorageCost;
        private double SqlPaasProdStorageCost;
        private double WebappPaasStorageCost;
        private double WebappPaasDevStorageCost;
        private double WebappPaasProdStorageCost;
        private double TotalPaasStorageCost;

        private double SqlPaasSecurityCost;
        private double WebappPaasSecurityCost;
        private double TotalPaasSecurityCost;

        private double SqlPaasAhubSavings;
        private double TotalPaasAhubSavings;

        public AzurePaaSCostCalculator()
        {
            SQL_MI_PaaS_List = new List<SQL_MI_PaaS>();
            WebApp_PaaS_List = new List<WebApp_PaaS>();
            WebappUniqueDevMachines = new HashSet<string>();
            WebappUniqueProdMachines = new HashSet<string>();
            SqlUniqueDevMachines = new HashSet<string>();
            SqlUniqueProdMachines = new HashSet<string>();

            IsCalculated = false;

            SqlDevRowCount = 0;
            SqlProdRowCount = 0;
            WebAppDevRowCount = 0;
            WebAppProdRowCount = 0;

            SqlPaasComputeCost = 0.0;
            SqlPaasDevComputeCost = 0.0;
            SqlPaasProdComputeCost = 0.0;
            WebappPaasComputeCost = 0.0;
            WebappPaasDevComputeCost = 0.0;
            WebappPaasProdComputeCost = 0.0;
            TotalPaasComputeCost = 0.0;

            SqlPaasStorageCost = 0.0;
            SqlPaasDevStorageCost = 0.0;
            SqlPaasProdStorageCost = 0.0;
            WebappPaasStorageCost = 0.0;
            WebappPaasDevStorageCost = 0.0;
            WebappPaasProdStorageCost = 0.0;
            TotalPaasStorageCost = 0.0;

            SqlPaasSecurityCost = 0.0;
            WebappPaasSecurityCost = 0.0;
            TotalPaasSecurityCost = 0.0;

            SqlPaasStorageCost = 0.0;

            SqlPaasAhubSavings = 0.0;
            TotalPaasAhubSavings = 0.0;
        }

        public void Calculate()
        {
            CalculateSqlPaaSCost();
            CalculateWebAppPaaSCost();

            TotalPaasComputeCost = SqlPaasComputeCost + WebappPaasComputeCost;
            TotalPaasStorageCost = SqlPaasStorageCost + WebappPaasStorageCost;
            TotalPaasSecurityCost = SqlPaasSecurityCost + WebappPaasSecurityCost;
            TotalPaasAhubSavings = SqlPaasAhubSavings;

            IsCalculated = true;
        }

        public bool IsCalculationComplete()
        {
            return IsCalculated;
        }

        public double GetTotalSecurityCost()
        {
            return TotalPaasSecurityCost;
        }

        public double GetTotalAhubSavings()
        {
            return TotalPaasAhubSavings;
        }

        public double GetTotalComputeCost()
        {
            return TotalPaasComputeCost;
        }

        public double GetTotalStorageCost()
        {
            return TotalPaasStorageCost;
        }

        public double GetSqlPaaSDevComputeCost()
        {
            return SqlPaasDevComputeCost;
        }

        public double GetSqlPaaSProdComputeCost()
        {
            return SqlPaasProdComputeCost;
        }

        public double GetSqlPaaSDevStorageCost()
        {
            return SqlPaasDevStorageCost;
        }

        public double GetSqlPaaSProdStorageCost()
        {
            return SqlPaasProdStorageCost;
        }

        public double GetWebAppPaaSDevComputeCost()
        {
            return WebappPaasDevComputeCost;
        }
        public double GetWebAppPaaSProdComputeCost()
        {
            return WebappPaasProdComputeCost;
        }
        public double GetWebAppPaaSDevStorageCost()
        {
            return WebappPaasDevStorageCost;
        }

        public double GetWebAppPaaSProdStorageCost()
        {
            return WebappPaasProdStorageCost;
        }

        public int GetWebAppPaaSDevMachineIdCount()
        {
            return WebappUniqueDevMachines.Count;
        }

        public int GetWebAppPaaSProdMachineIdCount()
        {
            return WebappUniqueProdMachines.Count;
        }

        public int GetSqlPaaSDevMachineIdCount()
        {
            return SqlUniqueDevMachines.Count;
        }

        public int GetSqlPaaSProdMachineIdCount()
        {
            return SqlUniqueProdMachines.Count;
        }

        public int GetSqlPaaSDevMachinesCountTarget()
        {
            return SqlDevRowCount;
        }

        public int GetSqlPaaSProdMachinesCountTarget()
        {
            return SqlProdRowCount;
        }

        public int GetWebAppDevRowCount()
        {
            return WebAppDevRowCount;
        }

        public int GetWebAppProdRowCount()
        {
            return WebAppProdRowCount;
        }

        private void CalculateSqlPaaSCost()
        {
            double nonAhubCost = 0.0;

            foreach (var sqlInstance in SQL_MI_PaaS_List)
            {
                if (sqlInstance.Environment.Equals("Dev"))
                {
                    SqlPaasDevComputeCost += sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year == 0 ? sqlInstance.MonthlyComputeCostEstimate_AHUB : sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlPaasDevStorageCost += sqlInstance.MonthlyStorageCostEstimate;
                    nonAhubCost += sqlInstance.MonthlyComputeCostEstimate_RI3year == 0 ? sqlInstance.MonthlyComputeCostEstimate : sqlInstance.MonthlyComputeCostEstimate_RI3year;

                    SqlDevRowCount += 1;

                    if (!SqlUniqueDevMachines.Contains(sqlInstance.MachineId))
                        SqlUniqueDevMachines.Add(sqlInstance.MachineId);
                }
                else
                {
                    SqlPaasProdComputeCost += sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlPaasProdStorageCost += sqlInstance.MonthlyStorageCostEstimate;
                    nonAhubCost += sqlInstance.MonthlyComputeCostEstimate_RI3year;

                    SqlProdRowCount += 1;

                    if (!SqlUniqueProdMachines.Contains(sqlInstance.MachineId))
                        SqlUniqueProdMachines.Add(sqlInstance.MachineId);
                }
                SqlPaasSecurityCost += sqlInstance.MonthlySecurityCostEstimate;
            }
            SqlPaasComputeCost = SqlPaasDevComputeCost + SqlPaasProdComputeCost;
            SqlPaasStorageCost = SqlPaasDevStorageCost + SqlPaasProdStorageCost;
            SqlPaasAhubSavings = nonAhubCost - SqlPaasComputeCost;
        }

        private void CalculateWebAppPaaSCost()
        {
            foreach (var webapp in WebApp_PaaS_List)
            {
                if (webapp.Environment.Equals("Dev"))
                {
                    WebappPaasDevComputeCost += webapp.MonthlyComputeCostEstimate_ASP3year == 0 ? webapp.MonthlyComputeCostEstimate : webapp.MonthlyComputeCostEstimate_ASP3year;

                    WebAppDevRowCount += 1;

                    if (!WebappUniqueDevMachines.Contains(webapp.MachineId))
                        WebappUniqueDevMachines.Add(webapp.MachineId);
                }
                else
                {
                    WebappPaasProdComputeCost += webapp.MonthlyComputeCostEstimate_ASP3year == 0 ? webapp.MonthlyComputeCostEstimate : webapp.MonthlyComputeCostEstimate_ASP3year;

                    WebAppProdRowCount += 1;

                    if (!WebappUniqueProdMachines.Contains(webapp.MachineId))
                        WebappUniqueProdMachines.Add(webapp.MachineId);
                }

                WebappPaasSecurityCost += webapp.MonthlySecurityCostEstimate;
            }
            WebappPaasComputeCost = WebappPaasDevComputeCost + WebappPaasProdComputeCost;
        }

        public void SetParameters(List<SQL_MI_PaaS> sql_MI_PaaS_List, List<WebApp_PaaS> webApp_PaaS_List)
        {
            SQL_MI_PaaS_List = sql_MI_PaaS_List;
            WebApp_PaaS_List = webApp_PaaS_List;
        }
    }
}
