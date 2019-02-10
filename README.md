# hyperv_exporter
Exporter for Prometheus developed in C# (.NET Core) for information on Hyper-V servers like:

- CPU usage
- Total and available memory
- Total and available disk
- Total traffic for each network interface
- Count of all VMs with integrity OK and Critical
- Count of all VMs that are running

Most of the information is obtained directly from Hyper-V Server performance counters.

## How to install

On the server run the following commands:

```cmd
git clone https://github.com/lpsouza/hyperv_exporter.git
cd hyperv_exporter
install_service.cmd
```

Note: You must have installed the GIT application on the server.
