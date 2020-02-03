using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using dotnet_lib_prometheus;
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
                result += Prometheus.CreateMetricDescription(
                    counterHypervCpuTotalRunTimeName,
                    "gauge",
                    "Virtual processor usage percentage in guest and hypervisor code."
                );
                result += Prometheus.CreateMetric(
                    counterHypervCpuTotalRunTimeName,
                    counterHypervCpuTotalRunTime.NextValue().ToString(),
                    string.Empty
                );
                #endregion

                #region hyperv_memory_total_bytes
                string counterComputerInfoTotalPhysicalMemoryName = "hyperv_memory_total_bytes";
                ComputerInfo computerInfo = new ComputerInfo();
                result += Prometheus.CreateMetricDescription(
                    counterComputerInfoTotalPhysicalMemoryName,
                    "gauge",
                    "Total physical memory."
                );
                result += Prometheus.CreateMetric(
                    counterComputerInfoTotalPhysicalMemoryName,
                    computerInfo.TotalPhysicalMemory.ToString(),
                    string.Empty
                );
                #endregion

                #region hyperv_memory_avaliable_bytes
                string counterMemoryAvaliableBytesName = "hyperv_memory_avaliable_bytes";
                PerformanceCounter counterMemoryAvaliableBytes = new PerformanceCounter("Memory", "Available Bytes");
                counterMemoryAvaliableBytes.NextValue();
                System.Threading.Thread.Sleep(1000);
                result += Prometheus.CreateMetricDescription(
                    counterMemoryAvaliableBytesName,
                    "gauge",
                    "Total memory avaliable in bytes."
                );
                result += Prometheus.CreateMetric(
                    counterMemoryAvaliableBytesName,
                    counterMemoryAvaliableBytes.NextValue().ToString(),
                    string.Empty
                );
                #endregion

                #region hyperv_network_adapter_bytes_total_sec
                string counterNetworkAdapterBytesTotalSecName = "hyperv_network_adapter_bytes_total_sec";
                PerformanceCounterCategory categoryNetworkAdapterBytesTotalSec = PerformanceCounterCategory.GetCategories().FirstOrDefault(a => a.CategoryName == "Network Adapter");
                string[] instanceNetworkAdapterBytesTotalSecNames = categoryNetworkAdapterBytesTotalSec.GetInstanceNames();
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                result += Prometheus.CreateMetricDescription(
                    counterNetworkAdapterBytesTotalSecName,
                    "gauge",
                    "Network adapter traffic (bytes total/sec)."
                );
                foreach (string instanceName in instanceNetworkAdapterBytesTotalSecNames)
                {
                    try
                    {
                        PerformanceCounter counterNetworkAdapterBytesTotalSec = new PerformanceCounter("Network Adapter", "Bytes Total/sec", instanceName);
                        if (counterNetworkAdapterBytesTotalSec.RawValue > 0)
                        {
                            string name = networkInterfaces.AsEnumerable().Where(a => GenerateSlug(a.Description) == GenerateSlug(instanceName)).FirstOrDefault().Name;
                            counterNetworkAdapterBytesTotalSec.NextValue();
                            System.Threading.Thread.Sleep(1000);
                            result += Prometheus.CreateMetric(
                                counterNetworkAdapterBytesTotalSecName,
                                counterNetworkAdapterBytesTotalSec.NextValue().ToString(),
                                "{adapter=\"" + GenerateSlug(name) + "\"}"
                            );
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("EXCEPTION: {0}",ex.Message);
                        // throw;
                    }
                }
                #endregion

                #region hyperv_logical_disk_total_megabytes and hyperv_logical_disk_avaliable_megabytes
                string counterDriveInfoTotalSizeName = "hyperv_logical_disk_total_megabytes";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                result += Prometheus.CreateMetricDescription(
                    counterDriveInfoTotalSizeName,
                    "gauge",
                    "Total disk space."
                );
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        result += Prometheus.CreateMetric(
                            counterDriveInfoTotalSizeName,
                            (d.TotalSize / 1024 / 1024).ToString(),
                            "{disk=\"" + GenerateSlug(d.Name) + "\"}"
                        );
                    }
                }

                string counterLogicalDiskFreeMegabytesName = "hyperv_logical_disk_avaliable_megabytes";
                result += Prometheus.CreateMetricDescription(
                    counterLogicalDiskFreeMegabytesName,
                    "gauge",
                    "Logical disk free space (in MB)."
                );
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        result += Prometheus.CreateMetric(
                            counterLogicalDiskFreeMegabytesName,
                            (d.AvailableFreeSpace / 1024 / 1024).ToString(),
                            "{disk=\"" + GenerateSlug(d.Name) + "\"}"
                        );
                    }
                }
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
