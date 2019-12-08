# hyperv_exporter

Exporter for Prometheus developed in C# (.NET Core) for information on Hyper-V servers like:

- CPU usage
- Total and available memory
- Total and available disk
- Total traffic for each network interface
- Count of total VMs

Most of the information is obtained directly from Hyper-V Server performance counters.

## How to install

At .NET core 3.0 developer workstation:

1. Clone this repository;
2. Inside the project folder run: `publish.cmd`;
   - This batch script build two distinct versions (64 bits):
     - win10-x64: For Windows Server 2016 64 or late.
     - win81-x64: For Windows Server 2012 R2
   - The output folder is .\publish\*version*\
3. Get the correct version and copy to a folder on Hyper-V Server;
4. On Hyper-V Server, in the release folder, run: `install_service.ps1`

## Test if service is running

- Visit on the browser: [http://hyper-v-ip:9182/](http://hyper-v-ip:9182/)

## Sample config to `prometheus.yml`

```yml
scrape_configs:
  - job_name: 'HYPERV-SERVER'
    static_configs:
    - targets: [
      'HYPERV-SERVER:9182'
```
