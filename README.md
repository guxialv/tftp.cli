# tftp.cli

impl tftp client: [rfc1350](https://www.rfc-editor.org/rfc/rfc1350)

Usage:

```powershell
# Generate config file
Tftp.cli.exe --gen

# Run with config file
Tftp.cli.exe --run
```

# Default config file:
```
LocalIp=0.0.0.0
RemoteIp=127.0.0.1
RemotePort=69
BlockSize=512
TimeoutSeconds=5
Operation=Download
RemoteFile=test
LocalFile=xxx\download\test
```
