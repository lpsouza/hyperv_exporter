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

At .NET core 2.1 developer workstation:

1. Clone this repository;
2. Inside the project folder run: `publish.cmd`;
   - If you need publish to 2012 R2 version, edit publish.cmd file and make the adjust.
3. Get all release files and send to Hyper-V Server;
4. On Hyper-V Server, in the release folder, run: `install_service.cmd`
5. Visit on the browser: [http://hyper-v-ip:9182/](http://hyper-v-ip:9182/)
