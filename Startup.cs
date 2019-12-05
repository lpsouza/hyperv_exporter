using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using NickStrupat;

namespace hyperv_exporter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.Run(async (context) =>
            {
                string result = string.Empty;

                #region hyperv_cpu_total_run_time
                string counterHypervCpuTotalRunTimeName = "hyperv_cpu_total_run_time";
                PerformanceCounter counterHypervCpuTotalRunTime = new PerformanceCounter("Hyper-V Hypervisor Logical Processor", "% Total Run Time", "_Total");
                counterHypervCpuTotalRunTime.NextValue();
                System.Threading.Thread.Sleep(1000);
                result += string.Format("# HELP {0} Virtual processor usage percentage in guest and hypervisor code\n", counterHypervCpuTotalRunTimeName);
                result += string.Format("# TYPE {0} gauge\n", counterHypervCpuTotalRunTimeName);
                result += string.Format("{0} {1}\n", counterHypervCpuTotalRunTimeName, counterHypervCpuTotalRunTime.NextValue());
                #endregion

                #region hyperv_memory_total_bytes
                string counterComputerInfoTotalPhysicalMemoryName = "hyperv_memory_total_bytes";
                ComputerInfo computerInfo = new ComputerInfo();
                result += string.Format("# HELP {0} Total physical memory\n", counterComputerInfoTotalPhysicalMemoryName);
                result += string.Format("# TYPE {0} gauge\n", counterComputerInfoTotalPhysicalMemoryName);
                result += string.Format("{0} {1}\n", counterComputerInfoTotalPhysicalMemoryName, computerInfo.TotalPhysicalMemory);
                #endregion

                #region hyperv_memory_avaliable_bytes
                string counterMemoryAvaliableBytesName = "hyperv_memory_avaliable_bytes";
                PerformanceCounter counterMemoryAvaliableBytes = new PerformanceCounter("Memory", "Available Bytes");
                counterMemoryAvaliableBytes.NextValue();
                System.Threading.Thread.Sleep(1000);
                result += string.Format("# HELP {0} Total memory avaliable in bytes\n", counterMemoryAvaliableBytesName);
                result += string.Format("# TYPE {0} gauge\n", counterMemoryAvaliableBytesName);
                result += string.Format("{0} {1}\n", counterMemoryAvaliableBytesName, counterMemoryAvaliableBytes.NextValue());
                #endregion

                #region hyperv_network_adapter_bytes_total_sec
                string counterNetworkAdapterBytesTotalSecName = "hyperv_network_adapter_bytes_total_sec";
                PerformanceCounterCategory categoryNetworkAdapterBytesTotalSec = PerformanceCounterCategory.GetCategories().FirstOrDefault(a => a.CategoryName == "Network Adapter");
                string[] instanceNetworkAdapterBytesTotalSecNames = categoryNetworkAdapterBytesTotalSec.GetInstanceNames();
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                result += string.Format("# HELP {0} Network adapter traffic (bytes total/sec)\n", counterNetworkAdapterBytesTotalSecName);
                result += string.Format("# TYPE {0} gauge\n", counterNetworkAdapterBytesTotalSecName);
                foreach (string instanceName in instanceNetworkAdapterBytesTotalSecNames)
                {
                    PerformanceCounter counterNetworkAdapterBytesTotalSec = new PerformanceCounter("Network Adapter", "Bytes Total/sec", instanceName);
                    if (counterNetworkAdapterBytesTotalSec.RawValue > 0)
                    {
                        string name = networkInterfaces.AsEnumerable().Where(a => GenerateSlug(a.Description) == GenerateSlug(instanceName)).FirstOrDefault().Name;
                        counterNetworkAdapterBytesTotalSec.NextValue();
                        System.Threading.Thread.Sleep(1000);
                        result += string.Format("{0}{2} {1}\n", counterNetworkAdapterBytesTotalSecName, counterNetworkAdapterBytesTotalSec.NextValue(), "{adapter=\"" + GenerateSlug(name) + "\"}");
                    }
                }
                #endregion

                #region hyperv_logical_disk_total_megabytes
                string counterDriveInfoTotalSizeName = "hyperv_logical_disk_total_megabytes";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                result += string.Format("# HELP {0} Total disk space\n", counterDriveInfoTotalSizeName);
                result += string.Format("# TYPE {0} gauge\n", counterDriveInfoTotalSizeName);
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        result += string.Format("{0}{2} {1}\n", counterDriveInfoTotalSizeName, d.TotalSize / 1024 / 1024, "{disk=\"" + GenerateSlug(d.Name) + "\"}");
                    }
                }
                #endregion

                #region hyperv_logical_disk_avaliable_megabytes
                string counterLogicalDiskFreeMegabytesName = "hyperv_logical_disk_avaliable_megabytes";
                PerformanceCounterCategory categoryLogicalDiskFreeMegabytes = PerformanceCounterCategory.GetCategories().FirstOrDefault(a => a.CategoryName == "LogicalDisk");
                string[] instanceLogicalDiskFreeMegabytesNames = categoryLogicalDiskFreeMegabytes.GetInstanceNames();
                result += string.Format("# HELP {0} Logical disk free space (in MB)\n", counterLogicalDiskFreeMegabytesName);
                result += string.Format("# TYPE {0} gauge\n", counterLogicalDiskFreeMegabytesName);
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        string instanceName = instanceLogicalDiskFreeMegabytesNames.Where(a => GenerateSlug(a) == GenerateSlug(d.Name)).FirstOrDefault();
                        PerformanceCounter counterLogicalDiskFreeMegabytes = new PerformanceCounter("LogicalDisk", "Free Megabytes", instanceName);
                        if (counterLogicalDiskFreeMegabytes.RawValue > 0)
                        {
                            counterLogicalDiskFreeMegabytes.NextValue();
                            System.Threading.Thread.Sleep(1000);
                            result += string.Format("{0}{2} {1}\n", counterLogicalDiskFreeMegabytesName, counterLogicalDiskFreeMegabytes.NextValue(), "{disk=\"" + GenerateSlug(instanceName) + "\"}");
                        }
                    }
                }
                #endregion

                #region hyperv_vms_ok
                string counterHypervHealthOkName = "hyperv_vms_ok";
                PerformanceCounter counterHypervHealthOk = new PerformanceCounter("Hyper-V Virtual Machine Health Summary", "Health Ok");
                counterHypervHealthOk.NextValue();
                System.Threading.Thread.Sleep(1000);
                result += string.Format("# HELP {0} Count of virtual machines with OK status health\n", counterHypervHealthOkName);
                result += string.Format("# TYPE {0} gauge\n", counterHypervHealthOkName);
                result += string.Format("{0} {1}\n", counterHypervHealthOkName, counterHypervHealthOk.NextValue());
                #endregion

                #region hyperv_vms_running
                string counterHypervCountRunningVmsName = "hyperv_vms_running";
                PerformanceCounterCategory categoryHypervCountRunningVms = PerformanceCounterCategory.GetCategories().FirstOrDefault(a => a.CategoryName == "Hyper-V Dynamic Memory VM");
                result += string.Format("# HELP {0} Count total VMs in Hyper-V\n", counterHypervCountRunningVmsName);
                result += string.Format("# TYPE {0} gauge\n", counterHypervCountRunningVmsName);
                result += string.Format("{0} {1}\n", counterHypervCountRunningVmsName, categoryHypervCountRunningVms.GetInstanceNames().Count());
                #endregion

                await context.Response.WriteAsync(result);
            });
        }
        public static string GenerateSlug(string phrase)
        {
            var s = phrase.ToLower();
            s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            s = Regex.Replace(s, @"\s", "_");
            return s;
        }
    }
}
