// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

using Azure.Migrate.Explore.Models;

namespace Azure.Migrate.Explore.Common
{

    public class AzureIaaSCostCalculator
    {
        // IaaS datasets
        private List<SQL_IaaS_Instance_Rehost_Perf> SQL_IaaS_Instance_Rehost_Perf_List;
        private List<SQL_IaaS_Server_Rehost_Perf> SQL_IaaS_Server_Rehost_Perf_List;
        private List<WebApp_IaaS_Server_Rehost_Perf> WebApp_IaaS_Server_Rehost_Perf_List;
        private List<VM_IaaS_Server_Rehost_Perf> VM_IaaS_Server_Rehost_Perf_List;
        private HashSet<string> sqlUniqueDevMachines;
        private HashSet<string> sqlUniqueProdMachines;
        private HashSet<string> managementUniqueProdMachines;

        private bool IsCalculated;

        int sqlDevRowCount;
        int sqlProdRowCount;
        int webappDevRowCount;
        int webappProdRowCount;
        int vmserverDevRowCount;
        int vmserverProdRowCount;
        int managementTargetCount;

        private double SqlIaaSComputeCost;
        private double SqlIaaSDevComputeCost;
        private double SqlIaaSProdComputeCost;
        private double WebAppIaaSComputeCost;
        private double WebAppIaaSDevComputeCost;
        private double WebAppIaaSProdComputeCost;
        private double VmIaaSComputeCost;
        private double VmIaaSDevComputeCost;
        private double VmIaaSProdComputeCost;
        private double TotalIaaSComputeCost;

        private double SqlIaaSStorageCost;
        private double SqlIaaSDevStorageCost;
        private double SqlIaaSProdStorageCost;
        private double WebAppIaaSStorageCost;
        private double WebAppIaaSDevStorageCost;
        private double WebAppIaaSProdStorageCost;
        private double VmIaaSStorageCost;
        private double VmIaaSDevStorageCost;
        private double VmIaaSProdStorageCost;
        private double TotalIaaSStorageCost;

        private double SqlIaaSSecurityCost;
        private double WebAppIaaSSecurityCost;
        private double VmIaaSSecurityCost;
        private double TotalIaaSSecurityCost;

        private double SqlIaaSAhubSavings;
        private double VmIaaSAhubSavings;
        private double WebAppIaaSAhubSavings;
        private double TotalIaaSAhubSavings;

        private double BackupComputeCost;
        private double RecoveryComputeCost;
        public AzureIaaSCostCalculator()
        {
            SQL_IaaS_Instance_Rehost_Perf_List = new List<SQL_IaaS_Instance_Rehost_Perf>();
            SQL_IaaS_Server_Rehost_Perf_List = new List<SQL_IaaS_Server_Rehost_Perf>();
            WebApp_IaaS_Server_Rehost_Perf_List = new List<WebApp_IaaS_Server_Rehost_Perf>();
            VM_IaaS_Server_Rehost_Perf_List = new List<VM_IaaS_Server_Rehost_Perf>();
            sqlUniqueDevMachines = new HashSet<string>();
            sqlUniqueProdMachines = new HashSet<string>();
            managementUniqueProdMachines = new HashSet<string>();

            IsCalculated = false;

            sqlDevRowCount = 0;
            sqlProdRowCount = 0;
            webappDevRowCount = 0;
            webappProdRowCount = 0;
            vmserverDevRowCount = 0;
            vmserverProdRowCount = 0;
            managementTargetCount = 0;

            SqlIaaSComputeCost = 0.0;
            SqlIaaSDevComputeCost = 0.0;
            SqlIaaSProdComputeCost = 0.0;
            WebAppIaaSComputeCost = 0.0;
            WebAppIaaSDevComputeCost = 0.0;
            WebAppIaaSProdComputeCost = 0.0;
            VmIaaSComputeCost = 0.0;
            VmIaaSDevComputeCost = 0.0;
            VmIaaSProdComputeCost = 0.0;
            TotalIaaSComputeCost = 0.0;

            SqlIaaSStorageCost = 0.0;
            SqlIaaSDevStorageCost = 0.0;
            SqlIaaSProdStorageCost = 0.0;
            WebAppIaaSStorageCost = 0.0;
            WebAppIaaSDevStorageCost = 0.0;
            WebAppIaaSProdStorageCost = 0.0;
            VmIaaSStorageCost = 0.0;
            VmIaaSDevStorageCost = 0.0;
            VmIaaSProdStorageCost = 0.0;
            TotalIaaSStorageCost = 0.0;

            SqlIaaSSecurityCost = 0.0;
            WebAppIaaSSecurityCost = 0.0;
            VmIaaSSecurityCost = 0.0;
            TotalIaaSSecurityCost = 0.0;

            SqlIaaSAhubSavings = 0.0;
            VmIaaSAhubSavings = 0.0;
            WebAppIaaSAhubSavings = 0.0;
            TotalIaaSAhubSavings = 0.0;

            BackupComputeCost = 0.0;
            RecoveryComputeCost = 0.0;
        }

        public bool IsCalculationComplete()
        {
            return IsCalculated;
        }

        public double GetTotalStorageCost()
        {
            return TotalIaaSStorageCost;
        }

        public double GetTotalComputeCost()
        {
            return TotalIaaSComputeCost;
        }
        
        public double GetTotalAhubSavings()
        {
            return TotalIaaSAhubSavings;
        }

        public double GetTotalSecurityCost()
        {
            return TotalIaaSSecurityCost;
        }

        public double GetSqlIaaSDevComputeCost()
        {
            return SqlIaaSDevComputeCost;
        }

        public double GetSqlIaaSDevStorageCost()
        {
            return SqlIaaSDevStorageCost;
        }

        public double GetSqlIaaSProdComputeCost()
        {
            return SqlIaaSProdComputeCost;
        }

        public double GetSqlIaaSProdStorageCost()
        {
            return SqlIaaSProdStorageCost;
        }

        public double GetWebAppIaaSDevComputeCost()
        {
            return WebAppIaaSDevComputeCost;
        }

        public double GetWebAppIaaSDevStorageCost()
        {
            return WebAppIaaSDevStorageCost;
        }

        public double GetWebAppIaaSProdComputeCost()
        {
            return WebAppIaaSProdComputeCost;
        }

        public double GetWebAppIaaSProdStorageCost()
        {
            return WebAppIaaSProdStorageCost;
        }

        public double GetVmIaaSDevComputeCost()
        {
            return VmIaaSDevComputeCost;
        }

        public double GetVmIaaSDevStorageCost()
        {
            return VmIaaSDevStorageCost;
        }

        public double GetVmIaaSProdComputeCost()
        {
            return VmIaaSProdComputeCost;
        }

        public double GetVmIaaSProdStorageCost()
        {
            return VmIaaSProdStorageCost;
        }

        public double GetTotalBackupComputeCost()
        {
            return BackupComputeCost;
        }
        public double GetTotalRecoveryComputeCost()
        {
            return RecoveryComputeCost;
        }

        public int GetsqlIaaSDevMachineIdCount()
        {
            return sqlUniqueDevMachines.Count;
        }

        public int GetsqlIaaSProdMachineIdCount()
        {
            return sqlUniqueProdMachines.Count;
        }

        public int GetsqlIaaSDevMachinesCountTarget()
        {
            return sqlDevRowCount;
        }

        public int GetsqlIaaSProdMachinesCountTarget()
        {
            return sqlProdRowCount;
        }

        public int GetwebappIaaSDevMachinesCount()
        {
            return webappDevRowCount;
        }

        public int GetwebappIaaSProdMachinesCount()
        {
            return webappProdRowCount;
        }

        public int GetvmserverIaaSDevMachinesCount()
        {
            return vmserverDevRowCount;
        }

        public int GetvmserverIaaSProdMachinesCount()
        {
            return vmserverProdRowCount;
        }

        public int GetManagementSourceMachinesCount()
        {
            return managementUniqueProdMachines.Count;
        }

        public int GetManagementTargetMachinesCount()
        {
            return managementTargetCount;
        }

        public void SetParameters(List<SQL_IaaS_Instance_Rehost_Perf> sql_IaaS_Instance_Rehost_Perf_List,
                                  List<SQL_IaaS_Server_Rehost_Perf> sql_IaaS_Server_Rehost_Perf_List,
                                  List<WebApp_IaaS_Server_Rehost_Perf> webApp_IaaS_Server_Rehost_Perf_List,
                                  List<VM_IaaS_Server_Rehost_Perf> vm_IaaS_Server_Rehost_Perf_List)
        {
            SQL_IaaS_Instance_Rehost_Perf_List = sql_IaaS_Instance_Rehost_Perf_List;
            SQL_IaaS_Server_Rehost_Perf_List = sql_IaaS_Server_Rehost_Perf_List;
            WebApp_IaaS_Server_Rehost_Perf_List = webApp_IaaS_Server_Rehost_Perf_List;
            VM_IaaS_Server_Rehost_Perf_List = vm_IaaS_Server_Rehost_Perf_List;
        }

        public void Calculate()
        {
            CalculateSqlIaaSCost();
            CalculateWebAppIaaSCost();
            CalculateVMIaaSCost();

            TotalIaaSComputeCost = SqlIaaSComputeCost + WebAppIaaSComputeCost + VmIaaSComputeCost;
            TotalIaaSStorageCost = SqlIaaSStorageCost + WebAppIaaSStorageCost + VmIaaSStorageCost;
            TotalIaaSSecurityCost = SqlIaaSSecurityCost + WebAppIaaSSecurityCost + VmIaaSSecurityCost;
            TotalIaaSAhubSavings = SqlIaaSAhubSavings + WebAppIaaSAhubSavings + VmIaaSAhubSavings;

            managementTargetCount = vmserverProdRowCount + sqlProdRowCount + webappProdRowCount;

            IsCalculated = true;
        }

        private void CalculateSqlIaaSCost()
        {
            double nonAhubCost = 0.0;
            foreach (var sqlInstance in SQL_IaaS_Instance_Rehost_Perf_List)
            {
                if (sqlInstance.Environment.Equals("Dev"))
                {
                    SqlIaaSDevComputeCost += sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year == 0 ? sqlInstance.MonthlyComputeCostEstimate_AHUB : sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlIaaSDevStorageCost += sqlInstance.MonthlyStorageCostEstimate;
                    nonAhubCost += sqlInstance.MonthlyComputeCostEstimate_RI3year == 0 ? sqlInstance.MonthlyComputeCostEstimate : sqlInstance.MonthlyComputeCostEstimate_RI3year;

                    sqlDevRowCount += 1;

                    if (!sqlUniqueDevMachines.Contains(sqlInstance.MachineId))
                        sqlUniqueDevMachines.Add(sqlInstance.MachineId);
                }
                else
                {
                    SqlIaaSProdComputeCost += sqlInstance.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlIaaSProdStorageCost += sqlInstance.MonthlyStorageCostEstimate;
                    BackupComputeCost += sqlInstance.MonthlyAzureBackupCostEstimate;
                    RecoveryComputeCost += sqlInstance.MonthlyAzureSiteRecoveryCostEstimate;
                    nonAhubCost += sqlInstance.MonthlyComputeCostEstimate_RI3year;

                    sqlProdRowCount += 1;

                    if (!sqlUniqueProdMachines.Contains(sqlInstance.MachineId))
                        sqlUniqueProdMachines.Add(sqlInstance.MachineId);

                    if (!managementUniqueProdMachines.Contains(sqlInstance.MachineId))
                        managementUniqueProdMachines.Add(sqlInstance.MachineId);
                }
                SqlIaaSSecurityCost += sqlInstance.MonthlySecurityCostEstimate;
            }

            foreach (var sqlServer in SQL_IaaS_Server_Rehost_Perf_List)
            {
                if (sqlServer.Environment.Equals("Dev"))
                {
                    SqlIaaSDevComputeCost += sqlServer.MonthlyComputeCostEstimate_AHUB_RI3year == 0 ? sqlServer.MonthlyComputeCostEstimate_AHUB : sqlServer.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlIaaSDevStorageCost += sqlServer.MonthlyStorageCostEstimate;               
                    nonAhubCost += sqlServer.MonthlyComputeCostEstimate_RI3year == 0 ? sqlServer.MonthlyComputeCostEstimate : sqlServer.MonthlyComputeCostEstimate_RI3year;

                    sqlDevRowCount += 1;

                    if (!sqlUniqueDevMachines.Contains(sqlServer.MachineId))
                        sqlUniqueDevMachines.Add(sqlServer.MachineId);
                }
                else
                {
                    SqlIaaSProdComputeCost += sqlServer.MonthlyComputeCostEstimate_AHUB_RI3year;
                    SqlIaaSProdStorageCost += sqlServer.MonthlyStorageCostEstimate;
                    BackupComputeCost += sqlServer.MonthlyAzureBackupCostEstimate;
                    RecoveryComputeCost += sqlServer.MonthlyAzureSiteRecoveryCostEstimate;
                    nonAhubCost += sqlServer.MonthlyComputeCostEstimate_RI3year;

                    sqlProdRowCount += 1;

                    if (!sqlUniqueProdMachines.Contains(sqlServer.MachineId))
                        sqlUniqueProdMachines.Add(sqlServer.MachineId);

                    if (!managementUniqueProdMachines.Contains(sqlServer.MachineId))
                        managementUniqueProdMachines.Add(sqlServer.MachineId);
                }

                SqlIaaSSecurityCost += sqlServer.MonthlySecurityCostEstimate;
            }
            SqlIaaSComputeCost = SqlIaaSDevComputeCost + SqlIaaSProdComputeCost;
            SqlIaaSStorageCost = SqlIaaSDevStorageCost + SqlIaaSProdStorageCost;
            SqlIaaSAhubSavings = nonAhubCost - SqlIaaSComputeCost;

        }

        private void CalculateWebAppIaaSCost()
        {
            double nonAhubCost = 0.0;
            foreach (var webappServer in WebApp_IaaS_Server_Rehost_Perf_List)
            {
                if (webappServer.Environment.Equals("Dev"))
                {
                    WebAppIaaSDevComputeCost += webappServer.MonthlyComputeCostEstimate_AHUB_RI3year == 0 ? webappServer.MonthlyComputeCostEstimate_AHUB : webappServer.MonthlyComputeCostEstimate_AHUB_RI3year;
                    WebAppIaaSDevStorageCost += webappServer.MonthlyStorageCostEstimate;
                    nonAhubCost += webappServer.MonthlyComputeCostEstimate_RI3year == 0 ? webappServer.MonthlyComputeCostEstimate : webappServer.MonthlyComputeCostEstimate_RI3year;

                    webappDevRowCount += 1;
                }
                else
                {
                    WebAppIaaSProdComputeCost += webappServer.MonthlyComputeCostEstimate_AHUB_RI3year;
                    WebAppIaaSProdStorageCost += webappServer.MonthlyStorageCostEstimate;
                    BackupComputeCost += webappServer.MonthlyAzureBackupCostEstimate;
                    RecoveryComputeCost += webappServer.MonthlyAzureSiteRecoveryCostEstimate;
                    nonAhubCost += webappServer.MonthlyComputeCostEstimate_RI3year;

                    webappProdRowCount += 1;

                    if (!managementUniqueProdMachines.Contains(webappServer.MachineId))
                        managementUniqueProdMachines.Add(webappServer.MachineId);
                }
                WebAppIaaSSecurityCost += webappServer.MonthlySecurityCostEstimate;
            }
            WebAppIaaSComputeCost = WebAppIaaSDevComputeCost + WebAppIaaSProdComputeCost;
            WebAppIaaSStorageCost = WebAppIaaSDevStorageCost + WebAppIaaSProdStorageCost;
            WebAppIaaSAhubSavings = nonAhubCost - WebAppIaaSComputeCost;
        }

        private void CalculateVMIaaSCost()
        {
            double nonAhubCost = 0.0;
            foreach (var vm in VM_IaaS_Server_Rehost_Perf_List)
            {
                if (vm.Environment.Equals("Dev"))
                {
                    VmIaaSDevComputeCost += vm.MonthlyComputeCostEstimate_AHUB_RI3year == 0 ? vm.MonthlyComputeCostEstimate_AHUB : vm.MonthlyComputeCostEstimate_AHUB_RI3year;
                    VmIaaSDevStorageCost += vm.MonthlyStorageCostEstimate;
                    nonAhubCost += vm.MonthlyComputeCostEstimate_RI3year == 0 ? vm.MonthlyComputeCostEstimate : vm.MonthlyComputeCostEstimate_RI3year;

                    vmserverDevRowCount += 1;
                }
                else
                {
                    VmIaaSProdComputeCost += vm.MonthlyComputeCostEstimate_AHUB_RI3year;
                    VmIaaSProdStorageCost += vm.MonthlyStorageCostEstimate;
                    BackupComputeCost += vm.MonthlyAzureBackupCostEstimate;
                    RecoveryComputeCost += vm.MonthlyAzureSiteRecoveryCostEstimate;
                    nonAhubCost += vm.MonthlyComputeCostEstimate_RI3year;

                    vmserverProdRowCount += 1;

                    if (!managementUniqueProdMachines.Contains(vm.MachineId))
                        managementUniqueProdMachines.Add(vm.MachineId);
                }
                VmIaaSSecurityCost += vm.MonthlySecurityCostEstimate; ;
            }
            VmIaaSComputeCost = VmIaaSDevComputeCost + VmIaaSProdComputeCost;
            VmIaaSStorageCost = VmIaaSDevStorageCost + VmIaaSProdStorageCost;
            VmIaaSAhubSavings = nonAhubCost - VmIaaSComputeCost;
        }
    }
}
